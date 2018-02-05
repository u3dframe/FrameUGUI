using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// 类名 : 记录主包资源
/// 作者 : Canyon / 龚阳辉
/// 日期 : 2018-01-02 16:55
/// 功能 : 
/// </summary>
public class MgrRecordMainRes : MonoBehaviour {
	#if UNITY_EDITOR
	string _m_path = "";

	Dictionary<string,string> _m_data = new Dictionary<string,string> ();

	void Awake(){
		instance = this;
		_m_path = Kernel.GameFile.m_fpMainRecordRes;
		FileInfo finfo = new FileInfo (_m_path);
		if (finfo.Exists) {
			string v = File.ReadAllText (_m_path);
			string[] arrs = v.Split ("\r\n\t".ToCharArray (), System.StringSplitOptions.RemoveEmptyEntries);
			foreach (var item in arrs) {
				AddABFile (item);
			}
		}
		Debug.Log (" ==== ReordMainRes Count = " + _m_data.Count);
	}

	void OnGUI(){
		float x = Screen.width - 150;
		if (GUI.Button (new Rect (x, 30, 120, 30), "保存记录")) {
			_Save ();
		}
	}

	void _Save(){
		System.Text.StringBuilder build = new System.Text.StringBuilder ();
		foreach (var item in _m_data.Keys) {
			build.Append (item).Append ("\r\n");
		}
		File.WriteAllText (_m_path, build.ToString ());
		UnityEditor.AssetDatabase.Refresh ();
	}
	#endif

	public void AddABFile(string abName){
		#if UNITY_EDITOR
		if(_m_data.ContainsKey(abName))
			return;
		_m_data.Add(abName,abName);
		#endif
	}

	static public MgrRecordMainRes instance{ get; private set; }
}
