using UnityEngine;
using System.Collections;
using System.IO;

namespace Kernel
{
	/// <summary>
	/// 类名 : 版本配置
	/// 作者 : Canyon / 龚阳辉
	/// 日期 : 2017-12-07 10:35
	/// 功能 : 注意点version版本地址必须固定,避免version地址出差错(更新下去后不能再次访问)
	/// version地址不给配置，写死(每个服务器地址都写死)
	/// </summary>
	public class CfgVersion  {

		static public readonly string m_defFileName = "version.txt";

		static string URL_HEAD = "http://";

		// 资源版本号(yyMMddHHmmss)
		public string m_resVerCode = "";

		// 上一次资源版本号(yyMMddHHmmss)
		public string m_lastResVerCode = "";

		// 游戏版本号
		public string m_gameVerCode = "";

		// git,或者svn版本号
		public string m_svnVerCode = "";

		// 平台标识
		public string m_platformType = "";

		// 基础语言类型
		public string m_language = "";

		// 版本地址
		public string m_urlVersion = "";

		// 文件列表地址
		public string m_urlFilelist = "";

		// 下载资源的地址
		public string m_urlRes = "";

		// 文件路径
		string m_filePath = "";

		public string m_content{ get; private set; }

		const string m_kResVerCode = "resVersion";
		const string m_kLastResVerCode = "lastResVersion";
		const string m_kGameVerCode = "version";
		const string m_kSvnVerCode = "svnVersion";
		const string m_kPlatformType = "platform";
		const string m_kLanguage = "language";
		const string m_kUrlVersion = "url";
		const string m_kUrlFilelist = "url_fl";
		const string m_kUrlRes = "downurl";

		public string urlPath4Ver{
			get{
				return string.Format (_FmtUrlPath (m_urlVersion), m_defFileName);
			}
		}

		public string urlPath4FileList{
			get{
				return string.Format (_FmtUrlPath (m_urlFilelist),CfgFileList.m_defFileName);
			}
		}

		public CfgVersion(){
			this.RefreshResVerCode ();
			m_gameVerCode = "1.0.0";
			m_svnVerCode = "0";
			m_lastResVerCode = "0";

			m_platformType = "CN";
			m_language = "CN";
			m_urlVersion = URL_HEAD;
			m_urlRes = URL_HEAD;
			m_urlFilelist = URL_HEAD;
		}

		public void Load(string fn){
			this.m_filePath = Kernel.GameFile.GetFilePath (fn);
			Init (Kernel.GameFile.GetText (fn));
		}

		public void Init(string content){
			if(string.IsNullOrEmpty(content)){
				return;
			}

			this.m_content = content;

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
				case m_kPlatformType:
					m_platformType = _arrs [1];
					break;
				case m_kLanguage:
					m_language = _arrs [1];
					break;
				case m_kUrlVersion:
					m_urlVersion = _arrs [1];
					break;
				case m_kUrlFilelist:
					m_urlFilelist = _arrs [1];
					break;
				case m_kUrlRes:
					m_urlRes = _arrs [1];
					break;
				default:
					break;
				}
			}
		}

		public void Save(){
			try {
				if (string.IsNullOrEmpty (this.m_filePath)) {
					this.m_filePath = Kernel.GameFile.GetFilePath (m_defFileName);
				}

				GameFile.CreateFolder (this.m_filePath);

				using (FileStream stream = new FileStream (this.m_filePath, FileMode.OpenOrCreate)) {
					using (StreamWriter writer = new StreamWriter (stream)) {
						string _fmt = "{0}={1}";
						writer.WriteLine (string.Format (_fmt, m_kResVerCode, m_resVerCode));
						writer.WriteLine (string.Format (_fmt, m_kLastResVerCode, m_lastResVerCode));
						writer.WriteLine (string.Format (_fmt, m_kGameVerCode, m_gameVerCode));
						writer.WriteLine (string.Format (_fmt, m_kSvnVerCode, m_svnVerCode));
						writer.WriteLine (string.Format (_fmt, m_kPlatformType, m_platformType));
						writer.WriteLine (string.Format (_fmt, m_kLanguage, m_language));
						writer.WriteLine (string.Format (_fmt, m_kUrlVersion, m_urlVersion));
						writer.WriteLine (string.Format (_fmt, m_kUrlFilelist, m_urlFilelist));
						writer.WriteLine (string.Format (_fmt, m_kUrlRes, m_urlRes));
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

		public bool IsUpdate(bool isCheckResUrl = false){
			if (string.IsNullOrEmpty (m_resVerCode))
				return false;

			if (string.IsNullOrEmpty (m_urlVersion) || URL_HEAD.Equals(m_urlVersion) || m_urlVersion.IndexOf(URL_HEAD) != 0)
				return false;

			if (string.IsNullOrEmpty (m_urlFilelist) || URL_HEAD.Equals(m_urlFilelist) || m_urlFilelist.IndexOf(URL_HEAD) != 0)
				return false;
			
			if (isCheckResUrl) {
				// 外网要检测，本地不需要检测
				if (string.IsNullOrEmpty (m_urlRes) || URL_HEAD.Equals (m_urlRes) || m_urlRes.IndexOf(URL_HEAD) != 0)
					return false;
			}

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

		string _FmtUrlPath(string url){
			int _index = url.LastIndexOf("/");
			if (_index == url.Length - 1) {
				return string.Concat (url, "{0}");
			}
			return string.Concat (url, "/{0}");
		}

		public void CloneFromOther(CfgVersion other){
			this.m_resVerCode = other.m_resVerCode;
			this.m_gameVerCode = other.m_gameVerCode;
			this.m_svnVerCode = other.m_svnVerCode;
			this.m_platformType = other.m_platformType;
			this.m_language = other.m_language;
			this.m_urlVersion = other.m_urlVersion;
			this.m_urlFilelist = other.m_urlFilelist;
			this.m_urlRes = other.m_urlRes;
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
