using UnityEngine;
using System.Collections;
using System.IO;

namespace Kernel
{
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
			WriteFile = 3,

			Finished = 10,

			Error_Url = 11,
			Error_Net = 12,
			Error_Write = 13,
		}

		State _m_state = State.None;
		public State m_state{ get { return _m_state; } private set { _m_state = value; } }

		public bool isError{
			get{
				return m_state == State.Error_Url || m_state == State.Error_Net || m_state == State.Error_Write;
			}
		}

		public bool isFinished{
			get{
				return m_state == State.Finished;
			}
		}

		// 错误信息
		public System.Exception m_error{ get; private set;}

		// 下载地址
		public string m_url = "";

		string m_realUrl = "";
		WWW m_www = null;

		// 限定失败后下载次数
		int m_numLimitTry = 3;

		// 当前下载次数
		int m_numCountTry = 0;

		bool m_isRunning = true;

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
				if (string.IsNullOrEmpty (this.m_url)) {
					m_state = State.Error_Url;
					_NewError(string.Format("Url Error : url is null"));
					return;
				}

				int _index = m_url.LastIndexOf("/");
				string _fmt = "{0}/{1}?time={2}";
				if (_index == m_url.Length - 1) {
					_fmt = "{0}{1}?time={2}";
				}

				m_realUrl = string.Format (_fmt, this.m_url,this.m_filePath,System.DateTime.Now.Ticks);
				m_www = new WWW(m_realUrl);

				m_state = State.DownLoad;
			}
		}

		void _ST_DownLoad(){
			if (m_www == null) {
				if(m_state == State.DownLoad)
					m_state = State.Init;
				
				return;
			}
				
			if (m_www.isDone) {
				if (string.IsNullOrEmpty (m_www.error)) { 
					m_state = State.WriteFile;
					_WriteFile ();
				} else {
					if (m_numLimitTry > m_numCountTry) {
						m_numCountTry++;
					}else{
						m_state = State.Error_Net;
						_NewError (string.Format ("Down Load Error : url = [{0}] , Error = [{1}]", m_realUrl, m_www.error));
					}
				}
				m_www.Dispose ();
				m_www = null;
			}
		}

		void _NewError(string errorMsg){
			m_error = new System.Exception (errorMsg);
		}

		void _WriteFile(){
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
				m_error = ex;
			}
		}

		void Reset ()
		{
			if (m_www != null) {
				m_www.Dispose ();
				m_www = null;
			}
			m_state = State.None;
		}

		public void ReDown(){
			Reset ();
			m_state = State.Init;
		}

		static public DownLoadFile ParseBy(ResInfo info){
			DownLoadFile ret = null;

			if(info != null){
				if (info is DownLoadFile) {
					ret = (DownLoadFile)info;
				} else {
					ret = new DownLoadFile ();
					ret.Init (info.ToString ());
				}

				ret.ReDown ();
			}
			return ret;
		}
	}
}
