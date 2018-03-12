using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace Kernel.Core{

	/// <summary>
	/// 类名 : AB数据关系
	/// 作者 : Canyon / 龚阳辉
	/// 日期 : 2018-03-12 13:40
	/// 功能 : 
	/// </summary>
	public class ABDataDependence {
		static string[] ignoreFiles = {
			".manifest",
			".meta",
			".cs",
			".fnt",
		};

		static string[] mustFiles = {
			".prefab",
			".ttf",
			".fontsettings",
		};

		static public bool _IsIn(string fp,string[] arrs){
			if (arrs == null || arrs.Length <= 0)
				return false;
			string fpTolower = fp.ToLower ();
			
			for (int i = 0; i < arrs.Length; i++) {
				if(fpTolower.Contains (arrs [i])){
					return true;
				}
			}
			return false;
		}

		static public bool _IsIgnoreFile(string fp){
			return _IsIn(fp,ignoreFiles);
		}

		static public bool _IsMustFile(string fp){
			return _IsIn(fp,mustFiles);
		}

		public string m_res = "";
		public bool m_isMustAB = false;
		public int m_nUseCount = 0;
		public List<string> m_lDependences = new List<string>();

		// 被谁引用了?
		public List<string> m_lBeDeps = new List<string>();

		public ABDataDependence(){}

		public ABDataDependence(string objAssetPath,bool isMust){
			Init (objAssetPath, isMust);
		}

		public ABDataDependence(Object obj,bool isMust){
			Init (obj, isMust);
		}

		public void Init(string objAssetPath,bool isMust){
			// 相对于objAssetPath;
			Object obj = AssetDatabase.LoadAssetAtPath<Object>(objAssetPath);
			Init (obj,isMust);
		}

		public void Init(Object obj,bool isMust){
			if (obj == null) {
				return;
			}
			this.m_isMustAB = isMust;
			this.m_nUseCount = 1;

			this.m_res = AssetDatabase.GetAssetPath (obj);
			if (_IsIgnoreFile (this.m_res)) {
				this.m_isMustAB = false;
				this.m_nUseCount = -99999999;
				return;
			}

			if (_IsMustFile (this.m_res)) {
				this.m_isMustAB = true;
			}

			string[] deps = AssetDatabase.GetDependencies (m_res,false);
			if (deps == null || deps.Length <= 0)
				return;
			foreach (var item in deps) {
				m_lDependences.Add (item);	
			}
		}

		public int GetUsedCount(){
			if (m_isMustAB)
				return 99999999;

			return m_nUseCount;
		}

		public void AddBeDeps(string beDeps){
			m_lBeDeps.Add (beDeps);
		}

		public override string ToString ()
		{
			return string.Format ("m_res = [{0}],m_isMustAB = [{1}],m_nUseCount = [{2}],m_lDependences count = [{3}]",
				m_res,m_isMustAB,m_nUseCount,m_lDependences.Count);
		}

	}

	public static class MgrABDataDependence{

		static public Dictionary<string,ABDataDependence> m_dic = new Dictionary<string, ABDataDependence> ();

		static ABDataDependence _GetData(string key){
			if (m_dic.ContainsKey (key)) {
				return m_dic [key];
			}
			return null;
		}

		static public void ClearAll(){
			m_dic.Clear ();
		}

		static public void Init(string objAssetPath,bool isMust,string beDeps = ""){
			// 相对于objAssetPath;
			Object obj = AssetDatabase.LoadAssetAtPath<Object>(objAssetPath);
			Init (obj,isMust,beDeps);
		}

		static public void Init(Object obj,bool isMust,string beDeps = ""){
			if (obj == null) {
				return;
			}
			string resPath = AssetDatabase.GetAssetPath (obj);
			if (ABDataDependence._IsIgnoreFile (resPath))
				return;
			
			ABDataDependence _data = _GetData(resPath);
			if (_data != null) {
				if (!string.IsNullOrEmpty (beDeps)) {
					_data.AddBeDeps (beDeps);
				}
				_data.m_nUseCount++;
			} else {
				_data = new ABDataDependence (obj,isMust);
				m_dic.Add (resPath, _data);

				foreach (var item in _data.m_lDependences) {
					Init (item,false,resPath);
				}
			}
		}

		static public int GetCount(string objAssetPath){
			Object obj = AssetDatabase.LoadAssetAtPath<Object>(objAssetPath);
			return GetCount (obj);
		}

		static public int GetCount(Object obj){
			if (obj == null)
				return -1;
			
			string resPath = AssetDatabase.GetAssetPath (obj);
			ABDataDependence _data = _GetData(resPath);
			if (_data == null)
				return 0;
			
			return _data.GetUsedCount();
		}

		static public void PrintDic(){
			foreach (var item in m_dic) {
				Debug.Log (item);
			}
		}

		static public void WriteDicToString(int limitCount = int.MaxValue){
			System.Text.StringBuilder builder = new System.Text.StringBuilder();
			foreach (var item in m_dic.Values) {
				if (item.GetUsedCount () > limitCount) {
					builder.Append (item).Append ("\n");
				}
			}
			GameFile.CreateFolder (GameFile.m_dirRes);
			System.IO.File.WriteAllText (GameFile.m_dirRes + "build_res.txt", builder.ToString ());
		}
	}
}