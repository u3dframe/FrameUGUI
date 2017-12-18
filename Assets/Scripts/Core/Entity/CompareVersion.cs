using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;

namespace Kernel
{
	/// <summary>
	/// 类名 : 对比版本
	/// 作者 : Canyon / 龚阳辉
	/// 日期 : 2017-12-08 14:35
	/// 功能 : 
	/// </summary>
	public class CompareVersion  {

		public enum State{
			None = 0,
			Init = 1,
			DownVersion = 2,
			CheckVersion = 3,
			DownFileList = 4,
			CompareFileList = 5,
			PreDownFiles = 6,
			DownFiles = 7,
			SaveVersionAndFileList = 8,

			Finished = 10,

			// 下载错误(与DownloadFile 错误一致）
			Error_Url = 11,
			Error_Net = 12,
			Error_Write = 13,

			Error_Ver = 21,
			Error_VerUrl = 22,
			Error_DownVer = 23,
			Error_DownFileList = 24,
		}

		bool m_isRunning = true;

		// 是否可以执行下文件下载
		public bool m_isDoDownFiles = false;

		State _m_state = State.None;
		public State m_state{ get{ return _m_state; } protected set{ _m_state = value; } }

		public bool isFinished{ get{ return m_state == State.Finished; } }

		public bool isPreDown{ get{ return m_state == State.PreDownFiles; } }

		protected CfgVersion m_cfgOld,m_cfgNew;

		protected List<CompareFiles> m_lComFiles = new List<CompareFiles> ();
		protected CompareFiles m_last = null,m_curr = null;

		protected WWW m_www = null;
		protected int m_numLimitTry = 3,m_numCountTry = 0;

		long m_nSizeAll = 0;
		long m_nSizeCurr = 0;

		// 更新状态(参数:状态)
		System.Action<int> m_callUpdate;

		// 下载资源的状态更新
		System.Action<long,long> m_callDownfile;

		public bool m_isInit { get; private set; }

		public CompareVersion(){
			m_cfgOld = new CfgVersion();
			m_cfgNew = new CfgVersion();
		}

		public void Init(System.Action<int> callUpdate,System.Action<long,long> callDownfile){
			if (m_isInit)
				return;
			m_isInit = true;

			Clear ();

			this.m_callUpdate = callUpdate;
			this.m_callDownfile = callDownfile;

			m_state = State.Init;
		}

		public void ReInit(System.Action<int> callUpdate,System.Action<long,long> callDownfile){
			m_isInit = false;
			m_state = State.None;
			Init (callUpdate, callDownfile);
		}

		public void OnUpdate(){
			if (!m_isRunning)
				return;

			switch (m_state) {
			case State.Init:
				_ST_Init ();
				break;
			case State.DownVersion:
				_ST_DownVersion ();
				break;
			case State.CheckVersion:
				_ST_CheckVersion ();
				break;
			case State.DownFileList:
				_ST_DownFileList ();
				break;
			case State.CompareFileList:
				_ST_CompareFileList ();
				break;
			case State.PreDownFiles:
				_ST_PreDownFiles ();
				break;
			case State.DownFiles:
				_ST_DownFiles ();
				break;
			case State.SaveVersionAndFileList:
				_ST_SaveVersionAndFileList ();
				break;
			case State.Finished:
				_ST_Finished ();
				break;
			default:
				break;
			}

			_ExcCallUpdate ();
		}

		void _ST_Init(){
			m_cfgOld.LoadDefault ();
			m_state = State.DownVersion;
		}

		void _ST_DownVersion(){
			if (!m_cfgOld.IsUpdate ()) {
				m_state = State.Error_VerUrl;
				_LogError(string.Format("Version Error : file has some error,file = [{0}]",m_cfgOld.m_content));
				return;
			}

			if (m_www == null) {
				m_www = new WWW (m_cfgOld.urlPath4Ver);
			}

			if (m_www.isDone) {
				if (string.IsNullOrEmpty (m_www.error)) { 
					m_state = State.CheckVersion;
					m_cfgNew.Init (m_www.text);
					m_numCountTry = 0;
				} else {
					if (m_numLimitTry > m_numCountTry) {
						m_numCountTry++;
					} else {
						m_state = State.Error_DownVer;
						_LogError (string.Format ("Down Version Error : url = [{0}] , Error = [{1}]", m_cfgOld.urlPath4Ver, m_www.error));
					}
				}
				m_www.Dispose ();
				m_www = null;
			}
		}

		void _ST_CheckVersion(){
			if (m_cfgOld.IsUpdate4Other (m_cfgNew)) {
				m_state = State.DownFileList;
			} else {
				if (m_lComFiles.Count <= 0) {
					m_state = State.Finished;
				} else {
					m_state = State.CompareFileList;
				}
			}
		}

		void _ST_DownFileList(){
			if (!m_cfgNew.IsUpdate (true)) {
				m_state = State.Error_Ver;
				_LogError(string.Format("url Error : res or ver url has error,file = [{0}]",m_cfgNew.m_content));
				return;
			}

			_OnST_DownFileList ();
		}

		protected virtual void _OnST_DownFileList(){
			if (m_www == null) {
				m_www = new WWW (m_cfgNew.urlPath4FileList);
			}

			if (m_www.isDone) {
				if (string.IsNullOrEmpty (m_www.error)) { 
					if (m_lComFiles.Count > 0) {
						m_last = m_lComFiles [m_lComFiles.Count - 1];
					}
					m_curr = new CompareFiles ();
					if (m_last != null) {
						// m_curr.Init(m_last.m_newContent,m_www.text, m_cfgNew.m_urlRes);
						m_curr.Init(m_last.m_cfgOld,m_www.text, m_cfgNew.m_urlRes);
					} else {
						m_curr.Init (m_www.text, m_cfgNew.m_urlRes);
					}

					m_lComFiles.Add (m_curr);

					m_last = null;
					m_curr = null;

					m_cfgOld.CloneFromOther (m_cfgNew);

					// 再次进行数据下载
					m_state = State.DownVersion;
					m_numCountTry = 0;
				} else {
					if (m_numLimitTry > m_numCountTry) {
						m_numCountTry++;
					} else {
						m_state = State.Error_DownFileList;
						_LogError (string.Format ("Down FileList Error : url = [{0}] , Error = [{1}]", m_cfgNew.urlPath4FileList, m_www.error));
					}
				}
				m_www.Dispose ();
				m_www = null;
			}
		}

		void _ST_CompareFileList(){
			if (m_lComFiles.Count <= 0) {
				m_state = State.Finished;
				return;
			}

			m_curr = m_lComFiles [0];
			m_curr = m_curr.MergeGetMax (m_lComFiles);

			m_nSizeAll = m_curr.GetDownSize ();
			m_nSizeCurr = 0;

			_ExcCBDownFile ();

			m_state = State.PreDownFiles;
			m_isDoDownFiles = m_nSizeAll <= 0;
		}

		void _ST_PreDownFiles(){
			if (!m_isDoDownFiles) {
				return;
			}
			m_isDoDownFiles = false;

			m_state = State.DownFiles;

			if (m_curr != null)
				m_curr.m_callDownFile = _CallDownFile;
		}

		void _ST_DownFiles(){
			if (m_curr == null) {
				m_state = State.Finished;
				return;
			}

			m_curr.OnUpdate ();

			if (m_curr.isError) {
				m_state = (State)((int)m_curr.m_state);
			} else if (m_curr.isFinished) {
				m_state = State.SaveVersionAndFileList;
			}
		}

		void _CallDownFile(DownLoadFile obj){
			if (obj.isFinished) {
				m_nSizeCurr += obj.m_size;
			}
			_ExcCBDownFile ();
		}

		void _ST_SaveVersionAndFileList(){
			Save ();
			m_curr.Save ();
			m_state = State.Finished;
		}

		void _ST_Finished(){
			m_isRunning = false;
			Clear ();
		}

		protected void _LogError(string msg){
			UnityEngine.Debug.LogError (msg);
		}
			
		void _ExcCallUpdate(){
			int nState = 4;
			switch (m_state) {
			case State.DownVersion:
			case State.CheckVersion:
				nState = 4;
				break;
			case State.DownFileList:
				nState = 5;
				break;
			case State.CompareFileList:
			case State.PreDownFiles:
				nState = 6;
				break;
			case State.DownFiles:
				if (m_curr.m_state == CompareFiles.State.DownFiles) {
					nState = 8;
				} else {
					nState = 7;
				}
				break;
			case State.Error_Net:
			case State.Error_Url:
			case State.Error_Write:
				nState = 15;
				break;
			case State.Error_Ver:
			case State.Error_VerUrl:
			case State.Error_DownVer:
				nState = 13;
				break;
			case State.Error_DownFileList:
				nState = 14;
				break;
			default:
				break;
			}

			if (this.m_callUpdate != null) {
				this.m_callUpdate (nState);
			}
		}

		void _ExcCBDownFile(){
			if (this.m_callDownfile != null) {
				this.m_callDownfile (m_nSizeCurr, m_nSizeAll);
			}
		}

		public void Save(){
			this.m_cfgNew.SaveDefault ();
		}

		public void ReDownFile(){
			if (m_curr == null)
				return;
			
			this.m_curr.ReDownFile ();

			m_state = State.DownFiles;
		}

		public void Clear(){
			m_lComFiles.Clear ();
			m_last = null;
			m_curr = null;

			m_nSizeAll = 0;
			m_nSizeCurr = 0;

			m_callUpdate = null;
			m_callDownfile = null;

			m_numCountTry = 0;

			this._OnClear ();
		}

		protected virtual void _OnClear(){
		}

		static CompareVersion _instance;
		static public CompareVersion instance{
			get{ 
				if (_instance == null) {
					_instance = new CompareVersion ();
				}
				return _instance;
			}
		}
	}
}
