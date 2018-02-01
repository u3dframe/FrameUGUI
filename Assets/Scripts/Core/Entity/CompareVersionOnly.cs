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

		CfgVersionOnly cfgTmp = null;

		public CompareVersionOnly():base(){
			m_cfgOld = new CfgVersionOnly();
			m_cfgNew = new CfgVersionOnly();
		}

		protected override void _OnST_DownFileList ()
		{
			cfgTmp = ((CfgVersionOnly)m_cfgNew);
			if (cfgTmp.m_isCanWriteUrlFls) {
				if (needDownFls == null) {
					needDownFls = cfgTmp.m_lUrlFls;
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
			} else {
				strUrl = cfgTmp.m_urlFilelist;
			}

			_Hd_ST_DownFileList (strUrl,cfgTmp);
		}

		protected override void _Hd_ST_DownFileList_End ()
		{
			if (cfgTmp.m_isCanWriteUrlFls) {
				m_cfgOld.CloneFromOther (cfgTmp);
			} else {
				m_state = State.CompareFileList;
			}
		}

		protected override void _OnClear ()
		{
			base._OnClear ();

			startIndex = -1;
			endIndex = -1;
			needDownFls = null;
			strUrl = "";
			cfgTmp = null;
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
