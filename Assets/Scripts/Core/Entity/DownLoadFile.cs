using UnityEngine;
using System.Collections;
using System.IO;

namespace Kernel
{
	/// <summary>
	/// 类名 : 资源对象枚举
	/// 作者 : Canyon / 龚阳辉
	/// 日期 : 2018-01-30 14:35
	/// 功能 : 
	/// </summary>
	public enum AssetType{
		None = 0,
		Text = 1,
		Bytes = 2,
		Texture = 3,
		AssetBundle = 4
	}

	/// <summary>
	/// 类名 : 资源下载对象
	/// 作者 : Canyon / 龚阳辉
	/// 日期 : 2017-12-07 14:55
	/// 功能 : 
	/// </summary>
	public class DownLoadFile : ResInfo {

		public enum State{
			None = 0,
			Init = 1,
			DownLoad = 2,
			WaitCommand = 3,

			Finished = 10,

			Error_Url = 11,
			Error_Net = 12,
			Error_Write = 13,

			Error_ExcuteCall = 500,
			Error_NotMatchCode = 998,
			Error_TimeOut = 999,
		}

		State _m_state = State.None;
		public State m_state{ get { return _m_state; } private set { _m_state = value; } }

		public bool isError{
			get{
				return m_state == State.Error_Url || m_state == State.Error_Net || 
					m_state == State.Error_Write || m_state == State.Error_NotMatchCode || 
					m_state == State.Error_TimeOut || m_state == State.Error_ExcuteCall;
			}
		}

		public bool isFinished{ get{ return m_state == State.Finished; } }

		// 错误信息
		public System.Exception m_error{ get; private set;}

		// 下载地址
		private string _m_url = "";
		private string[] _arrsUrls = null;
		private int _indexUrl = -1;
		public string m_url{
			get{ return _m_url; }
			set{
				if (string.IsNullOrEmpty(value)) {
					_arrsUrls = null;
				} else if (!value.Equals (_m_url)) {
					_arrsUrls = value.Split (";".ToCharArray(),System.StringSplitOptions.RemoveEmptyEntries);
					_indexUrl = -1;
				}
				_m_url = value;
			}
		}

		string m_realUrl = "";
		WWW m_www = null;

		// 限定失败后下载次数
		int m_numLimitTry = 3;
		// 当前下载次数
		int m_numCountTry = 0;
		// 下载时候出错的描述
		string m_strError = "";

		bool m_isRunning = true;

		// 默认下载超时时间
		float m_wwwProgress = 0; // 下载进度
		float m_timeout = 15;
		float m_lasttime = 0;
		float m_curtime = 0;
		public bool m_isPrintError = false;

		// 默认资源类型为:文本内容
		AssetType m_assetType = AssetType.Text;
		System.Action<object> m_callSuccess = null;

		public DownLoadFile():base(){
		}

		// 初始filelist数据
		public DownLoadFile(string row):base(row){
		}

		#region == 初始化下载资源 ==
		public DownLoadFile ReInit(string url,string proj,string fn,System.Action<object> callSuccess,AssetType aType = AssetType.Text){
			m_url = url;
			m_resPackage = proj;
			m_resName = fn;
			m_assetType = aType;
			m_callSuccess = callSuccess;
			return this;
		}

		public DownLoadFile ReInit(string url,string fn,System.Action<object> callSuccess,AssetType aType = AssetType.Text){
			return ReInit (url, "", fn, callSuccess, aType);
		}

		public DownLoadFile ReInit(string url,System.Action<object> callSuccess,AssetType aType = AssetType.Text){
			return ReInit (url, "", callSuccess, aType);
		}
		#endregion

		string _ReUrlPath(string url,string fn = null){
			if(string.IsNullOrEmpty(fn))
				return CfgVersion.ReUrlTime(url);
			return CfgVersion.ReUrlTime (url,fn);
		}

		string _GetUrl(string[] arrs,string defUrl,ref int index){
			return CfgVersion.GetUrl(arrs,defUrl,ref index);
		}

		public void OnUpdate(){
			if (!m_isRunning)
				return;

			switch (m_state) {
			case State.Init:
				_ST_Init ();
				break;
			case State.DownLoad:
				_ST_DownLoad ();
				break;
			default:
				break;
			}
		}

		void _ST_Init(){
			if (isError)
				return;
			
			m_isRunning = true;

			if (m_www == null) {
				if (string.IsNullOrEmpty (m_url)) {
					m_state = State.Error_Url;
					_NewError(string.Format("Url Error : url is null"));
					return;
				}

				string _url = _GetUrl (_arrsUrls, m_url, ref _indexUrl);
				m_realUrl = _ReUrlPath (_url,m_filePath);
				
				m_www = new WWW(m_realUrl);
				m_state = State.DownLoad;

				m_curtime = 0;
				m_wwwProgress = 0;
				m_lasttime = 0;
			}
		}

		void _ST_DownLoad(){
			if (m_www == null) {
				if(m_state == State.DownLoad)
					m_state = State.Init;
				
				return;
			}
				
			if (m_www.isDone) {
				bool _isSuccess = string.IsNullOrEmpty (m_www.error);
				if (_isSuccess) { 
					m_strError = "";
					bool _isValid = true;
					if (!string.IsNullOrEmpty (m_compareCode)) {
						string _code = ALG.CRC32Class.GetCRC32 (m_www.bytes);
						_isValid = m_compareCode.Equals (_code);
						if (!_isValid) {
							m_strError = "CRC cannot match";
							m_state = State.Error_NotMatchCode;
						}
					}

					if (_isValid) {
						if (!_ExcuteCallSuccess ())
							_WriteFile ();
					}
				} else {
					m_state = State.Error_Net;
					m_strError = m_www.error;
				}
				_DisposeWWW ();
			} else {
				if (m_www.progress == m_wwwProgress) {
					if (m_lasttime > 0) {
						m_curtime += Time.realtimeSinceStartup - m_lasttime;
					}
					m_lasttime = Time.realtimeSinceStartup;
				} else {
					m_curtime = 0;
					m_lasttime = 0;
					m_wwwProgress = m_www.progress;
				}
				
				if (m_timeout <= m_curtime) {
					_DisposeWWW ();
					m_strError = "time out";
					m_state = State.Error_TimeOut;
				}
			}

			if(isError){
				if (m_isPrintError) {
					Debug.LogError (string.Format ("== Down Load Error : url = [{0}] , Error = [{1}]", m_realUrl, m_strError));
				}

				if (m_numLimitTry > m_numCountTry) {
					m_numCountTry++;
					m_state = State.Init;
				} else {
					int _nState = (int)m_state;
					if (_nState >= 500)
						m_state = State.Error_Net;
					
					_NewError (string.Format ("== Down Load Error : url = [{0}] , Error = [{1}]", m_realUrl, m_strError));
				}
			}
		}

		void _NewError(string errorMsg){
			m_error = new System.Exception (errorMsg);
		}

		void _WriteFile(){
			m_state = State.WaitCommand;
			try {
				string _fp = GameFile.GetFilePath(this.m_resName);

				GameFile.CreateFolder(_fp);

				if(File.Exists(_fp)){
					File.Delete(_fp);
				}

				using(FileStream stream = new FileStream(_fp,FileMode.OpenOrCreate)){
					using(BinaryWriter writer = new BinaryWriter(stream)){
						writer.Write(m_www.bytes);
					}
				}

				m_state = State.Finished;
			} catch (System.Exception ex) {
				m_state = State.Error_Write;
				m_strError = ex.Message;
			}
		}

		bool _ExcuteCallSuccess(){
			if (m_callSuccess != null) {
				m_state = State.WaitCommand;
				try {
					object obj = null;
					switch (m_assetType) {
					case AssetType.Text:
						obj = m_www.text;
						break;
					case AssetType.Bytes:
						obj = m_www.bytes;
						break;
					case AssetType.Texture:
						obj = m_www.texture;
						break;
					case AssetType.AssetBundle:
						obj = m_www.assetBundle;
						break;
					default:
						break;
					}
					m_callSuccess(obj);

					m_state = State.Finished;
				} catch (System.Exception ex) {
					m_state = State.Error_ExcuteCall;
					m_strError = ex.Message;
				}
				return true;
			}
			return false;
		}

		void _DisposeWWW(){
			if (m_www != null) {
				m_www.Dispose ();
				m_www = null;
			}
		}

		void Reset ()
		{
			m_state = State.None;
			m_numCountTry = 0;
			_DisposeWWW ();
		}

		public void ReDown(){
			Reset ();
			this.m_state = State.Init;
		}

		public void Dispose(){
			_DisposeWWW ();
			m_isRunning = false;
			m_callSuccess = null;
			_arrsUrls = null;
		}

		static public DownLoadFile ParseBy(ResInfo info,string url,string proj){
			DownLoadFile ret = null;

			if(info != null){
				if (info is DownLoadFile) {
					ret = (DownLoadFile)info;
				} else {
					ret = new DownLoadFile ();
					ret.CloneFromOther (info);
				}

				ret.m_url = url;
				ret.m_resPackage = proj;
				ret.ReDown ();
			}
			return ret;
		}

		static public DownLoadFile ParseBy(ResInfo info){
			return ParseBy(info,"","");
		}
	}
}
