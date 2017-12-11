using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;

namespace Kernel
{
	/// <summary>
	/// 类名 : 文件列表配置
	/// 作者 : Canyon / 龚阳辉
	/// 日期 : 2017-12-08 14:45
	/// 功能 : 
	/// </summary>
	public class CfgFileList  {

		static public readonly string m_defFileName = "filelist.txt";

		Dictionary<string,ResInfo> _m_dicFiles = new Dictionary<string, ResInfo> ();

		List<ResInfo> m_lFiles = new List<ResInfo> ();

		public Dictionary<string,ResInfo> m_dicFiles {
			get{
				return _m_dicFiles;
			}
		}

		// 文件路径
		string m_filePath = "";

		public string m_content{ get; private set; }

		public CfgFileList(){
		}

		public void Load(string fn){
			this.m_filePath = Kernel.GameFile.GetFilePath (fn);
			Init (Kernel.GameFile.GetText (fn));
		}

		public void Init(string content){
			if(string.IsNullOrEmpty(content)){
				return;
			}

			Clear ();

			this.m_content = content;

			string[] _vals = content.Split ("\r\n\t".ToCharArray (), System.StringSplitOptions.RemoveEmptyEntries);
			for (int i = 0; i < _vals.Length; i++) {
				Add (new ResInfo (_vals[i]));
			}
		}

		public void Add(ResInfo info){
			if (info.m_size == 0)
				return;

			if (_m_dicFiles.ContainsKey (info.m_resName)) {
				Debug.Log (string.Format ("Compare Has Same File , name = [{0}]",info.m_resName));
			} else {
				_m_dicFiles.Add (info.m_resName, info);
				m_lFiles.Add (info);
			}
		}

		public void Save(){
			try {
				if (string.IsNullOrEmpty (this.m_content) && m_lFiles.Count <= 0)
					return;

				if (string.IsNullOrEmpty (this.m_filePath)) {
					this.m_filePath = Kernel.GameFile.GetFilePath (m_defFileName);
				}

				GameFile.CreateFolder (this.m_filePath);

				using (FileStream stream = new FileStream (this.m_filePath, FileMode.OpenOrCreate)) {
					using (StreamWriter writer = new StreamWriter (stream)) {
						if(string.IsNullOrEmpty(this.m_content)){
							for (int i = 0; i < m_lFiles.Count; i++) {
								writer.WriteLine (m_lFiles [i].ToString ());
							}
						}else{
							writer.Write (this.m_content);
						}
					}
				}
			} catch{
			}
		}

		/// <summary>
		/// 加载默认的资源
		/// </summary>
		public void LoadDefault(){
			Load (m_defFileName);
		}

		/// <summary>
		/// 保存到默认路径
		/// </summary>
		public void SaveDefault(){
			this.m_filePath = "";
			Save ();
		}

		public void Clear(){
			this.m_content = "";

			_m_dicFiles.Clear ();
			m_lFiles.Clear ();
		}

		static CfgFileList _instance;
		/// <summary>
		/// 此单例在打包的时候用
		/// </summary>
		static public CfgFileList instance{
			get{ 
				if (_instance == null) {
					_instance = new CfgFileList ();
				}
				return _instance;
			}
		}
	}
}
