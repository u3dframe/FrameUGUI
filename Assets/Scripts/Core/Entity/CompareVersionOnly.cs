using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;

namespace Kernel
{
	/// <summary>
	/// 类名 : 对比版本(针对一个version对象)
	/// 作者 : Canyon / 龚阳辉
	/// 日期 : 2017-12-16 17:05
	/// 功能 : 
	/// </summary>
	public class CompareVersionOnly : CompareVersion  {
		int startIndex = -1,endIndex = -1;
		List<string> needDownFls = null;
		string strUrl = "";

		public CompareVersionOnly():base(){
			m_cfgOld = new CfgVersionOnly();
			m_cfgNew = new CfgVersionOnly();
		}

		protected override void _OnST_DownFileList ()
		{
			if (startIndex == -1) {
				needDownFls = ((CfgVersionOnly)m_cfgNew).m_lUrlFls;
				startIndex = ((CfgVersionOnly)m_cfgOld).m_iIndInFls;
				endIndex = needDownFls.Count - 1;
			}

			if (endIndex < 0 || startIndex < 0 || startIndex > endIndex) {
				if (m_lComFiles.Count <= 0) {
					m_state = State.Finished;
				} else {
					m_state = State.CompareFileList;
				}
				return;
			}

			startIndex++;
			strUrl = needDownFls [startIndex];

			if (m_www == null) {
				m_www = new WWW (m_cfgNew.GetUrlFilelist(strUrl));
			}

			if (m_www.isDone) {
				if (string.IsNullOrEmpty (m_www.error)) { 
					if (m_lComFiles.Count > 0) {
						m_last = m_lComFiles [m_lComFiles.Count - 1];
					}
					m_curr = new CompareFiles ();
					if (m_last != null) {
						m_curr.Init(m_last.m_cfgOld,m_www.text, m_cfgNew.m_urlRes);
					} else {
						m_curr.Init (m_www.text, m_cfgNew.m_urlRes);
					}

					m_lComFiles.Add (m_curr);

					m_last = null;
					m_curr = null;

					m_cfgOld.CloneFromOther (m_cfgNew);
				} else {
					m_state = State.Error_DownFileList;
					_LogError(string.Format ("Down FileList Error : url = [{0}] , Error = [{1}]", m_cfgNew.urlPath4FileList, m_www.error));
				}
				m_www.Dispose ();
				m_www = null;
			}
		}

		protected override void _OnClear ()
		{
			base._OnClear ();

			startIndex = -1;
			endIndex = -1;
			needDownFls = null;
			strUrl = "";
		}

		static CompareVersionOnly _instance;
		static public new CompareVersionOnly instance{
			get{ 
				if (_instance == null) {
					_instance = new CompareVersionOnly ();
				}
				return _instance;
			}
		}
	}
}
