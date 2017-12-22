using UnityEngine;
using System.Collections;
using System.IO;

namespace Kernel
{
	/// <summary>
	/// 类名 : 版本配置
	/// 作者 : Canyon / 龚阳辉
	/// 日期 : 2017-12-07 10:35
	/// 功能 : 1.多个version管理自身更新地址,2.只有一个version(多版本地址)
	/// 默认是多版本，各管各的信息
	/// </summary>
	public class CfgVersion  {

		static public readonly string m_defFileName = "version.txt";

		protected static string URL_HEAD = "http://";

		// 资源版本号(yyMMddHHmmss)
		public string m_resVerCode = "";

		// 上一次资源版本号(yyMMddHHmmss)
		public string m_lastResVerCode = "";

		// 游戏版本号
		public string m_gameVerCode = "";

		// git,或者svn版本号
		public string m_svnVerCode = "";

		// 版本地址
		public string m_urlVersion = "";

		// 文件列表地址
		public string m_urlFilelist = "";

		// 大版本信息(判断整包更新)
		public string m_bigVerCode = "";

		// 下载apk,ipa文件地址
		public string m_urlNewApkIpa = "";

		// 服务器入口地址(登录服务器,或者取得服务器列表)
		public string m_urlSv = "";

		// 服务器列表
		public string urlSvlist{
			get{
				#if UNITY_EDITOR
				return "http://192.168.30:8006/z1/svlist.json";
				#elif
				return m_urlSv;
				#endif
			}
		}

		// 下载资源的地址
		public string m_urlRes{
			get{
				return GetUrlFiles (m_urlFilelist);
			}
		}

		// 文件路径
		protected string m_filePath = "";

		public string m_content{ get; private set; }

		protected const string m_kResVerCode = "resVersion";
		protected const string m_kLastResVerCode = "lastResVersion";
		protected const string m_kGameVerCode = "version";
		protected const string m_kSvnVerCode = "svnVersion";
		const string m_kUrlVersion = "url_ver";
		protected const string m_kUrlFilelist = "url_fl";

		protected const string m_kBigVerCode = "bigVersion";
		protected const string m_kUrlNewApkIpa = "url_newdown";
		protected const string m_kUrlSV = "url_sv";

		public string urlPath4Ver{
			get{
				return string.Concat (_FmtUrlPath (m_urlVersion), m_defFileName);
			}
		}

		public string urlPath4FileList{
			get{
				return GetUrlFilelist (m_urlFilelist);
			}
		}

		public CfgVersion(){
			this.RefreshResVerCode ();
			m_gameVerCode = "1.0.0";
			m_svnVerCode = "0";
			m_lastResVerCode = "0";

			m_urlVersion = URL_HEAD;
			m_urlFilelist = URL_HEAD;
			m_urlNewApkIpa = URL_HEAD;
			m_urlSv = URL_HEAD;
			this.RefreshBigVerCode ();
		}

		public void Load(string fn){
			this.m_filePath = GameFile.GetFilePath (fn);
			Init (GameFile.GetText (fn));
		}

		public void Init(string content){
			if(string.IsNullOrEmpty(content)){
				return;
			}

			this.m_content = content;
			_OnInit (this.m_content);
		}

		protected virtual void _OnInit(string content){
			string[] _vals = content.Split ("\r\n\t".ToCharArray (), System.StringSplitOptions.RemoveEmptyEntries);
			string[] _arrs;
			for (int i = 0; i < _vals.Length; i++) {
				_arrs = _vals[i].Split ("=".ToCharArray (), System.StringSplitOptions.RemoveEmptyEntries);
				if (_arrs.Length < 2)
					continue;
				switch (_arrs[0]) {
				case m_kResVerCode:
					m_resVerCode = _arrs [1];
					break;
				case m_kLastResVerCode:
					m_lastResVerCode = _arrs [1];
					break;
				case m_kGameVerCode:
					m_gameVerCode = _arrs [1];
					break;
				case m_kSvnVerCode:
					m_svnVerCode = _arrs [1];
					break;
				case m_kUrlVersion:
					m_urlVersion = _arrs [1];
					break;
				case m_kUrlFilelist:
					m_urlFilelist = _arrs [1];
					break;
				case m_kBigVerCode:
					m_bigVerCode = _arrs [1];
					break;
				case m_kUrlNewApkIpa:
					m_urlNewApkIpa = _arrs [1];
					break;
				case m_kUrlSV:
					m_urlSv = _arrs [1];
					break;
				default:
					break;
				}
			}
		}

		public void Save(){
			if (string.IsNullOrEmpty (this.m_filePath)) {
				this.m_filePath = GameFile.GetFilePath (m_defFileName);
			}
			
			GameFile.CreateFolder (this.m_filePath);
			_OnSave ();
		}

		protected virtual void _OnSave(){
			try {
				using (FileStream stream = new FileStream (this.m_filePath, FileMode.OpenOrCreate)) {
					using (StreamWriter writer = new StreamWriter (stream)) {
						string _fmt = "{0}={1}";
						writer.WriteLine (string.Format (_fmt, m_kResVerCode, m_resVerCode));
						writer.WriteLine (string.Format (_fmt, m_kGameVerCode, m_gameVerCode));
						writer.WriteLine (string.Format (_fmt, m_kUrlFilelist, m_urlFilelist));
						writer.WriteLine (string.Format (_fmt, m_kBigVerCode, m_bigVerCode));
						writer.WriteLine (string.Format (_fmt, m_kUrlNewApkIpa, m_urlNewApkIpa));
						writer.WriteLine (string.Format (_fmt, m_kUrlSV, m_urlSv));
						writer.WriteLine (string.Format (_fmt, m_kUrlVersion, m_urlVersion));

						// 可以从外部资源中(比如jar中,或者.mm文件)获取得到
						writer.WriteLine (string.Format (_fmt, m_kLastResVerCode, m_lastResVerCode));
						writer.WriteLine (string.Format (_fmt, m_kSvnVerCode, m_svnVerCode));
					}
				}
			} catch{
			}
		}

		/// <summary>
		/// 刷新资源版本号
		/// </summary>
		public void RefreshResVerCode(){
			this.m_resVerCode = System.DateTime.Now.ToString ("yyMMddHHmmss");
		}

		public void RefreshBigVerCode(){
			this.m_bigVerCode = System.DateTime.Now.ToString ("yyMMddHHmmss");
		}

		/// <summary>
		/// 加载默认的资源
		/// </summary>
		public void LoadDefault(){
			Load (m_defFileName);
		}

		/// <summary>
		/// 加载默认的资源 4 Editor
		/// </summary>
		public void LoadDefault4EDT(){
			LoadDefault ();
			m_lastResVerCode = m_resVerCode;
			RefreshResVerCode ();
		}

		/// <summary>
		/// 保存到默认路径
		/// </summary>
		public void SaveDefault(){
			this.m_filePath = "";
			Save ();
		}

		public bool IsNewDown(CfgVersion other){
			if (other == null)
				return false;
			
			if (string.IsNullOrEmpty (other.m_bigVerCode))
				return false;
			int v = other.m_bigVerCode.CompareTo (m_bigVerCode);
			return v > 0;
		}

		public virtual bool IsUpdate(bool isCheckResUrl = false){
			if (string.IsNullOrEmpty (m_resVerCode))
				return false;
			
			if (string.IsNullOrEmpty (m_urlVersion) || URL_HEAD.Equals(m_urlVersion) || m_urlVersion.IndexOf(URL_HEAD) != 0)
				return false;

			if (string.IsNullOrEmpty (m_urlFilelist) || URL_HEAD.Equals(m_urlFilelist) || m_urlFilelist.IndexOf(URL_HEAD) != 0)
				return false;

			return true;
		}

		public bool IsUpdate4Other(CfgVersion other){
			if (other == null)
				return false;
			
			if (string.IsNullOrEmpty (other.m_resVerCode))
				return false;
			// A.CompareTo(B) 比较A在B的前-1,后1,或相同0
			int v = other.m_resVerCode.CompareTo (this.m_resVerCode);
			if (v > 0 && other.IsUpdate(true)) {
				return true;
			}

			return false;
		}

		protected string _FmtUrlPath(string url){
			int _index = url.LastIndexOf("/");
			if (_index == url.Length - 1) {
				return url;
			}
			return string.Concat (url, "/");
		}

		public virtual void CloneFromOther(CfgVersion other){
			this.m_resVerCode = other.m_resVerCode;
			this.m_gameVerCode = other.m_gameVerCode;
			this.m_svnVerCode = other.m_svnVerCode;
			this.m_urlVersion = other.m_urlVersion;
			this.m_urlFilelist = other.m_urlFilelist;
			this.m_bigVerCode = other.m_bigVerCode;
			this.m_urlNewApkIpa = other.m_urlNewApkIpa;
			this.m_urlSv = other.m_urlSv;
			this.m_content = other.m_content;
		}

		public string GetUrlFilelist(string url){
			return string.Concat (_FmtUrlPath (url),CfgFileList.m_defFileName);
		}

		public string GetUrlFiles(string url){
			return string.Concat (_FmtUrlPath (url),"files");
		}

		static CfgVersion _instance;
		static public CfgVersion instance{
			get{ 
				if (_instance == null) {
					_instance = new CfgVersion ();
				}
				return _instance;
			}
		}
	}
}
