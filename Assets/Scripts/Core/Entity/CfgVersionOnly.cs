using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using LitJson;

namespace Kernel
{
	/// <summary>
	/// 类名 : 版本配置
	/// 作者 : Canyon / 龚阳辉
	/// 日期 : 2017-12-16 12:35
	/// 功能 : 只有一个version
	/// </summary>
	public class CfgVersionOnly : CfgVersion  {
		// 地址
		public List<string> m_lUrlFls = new List<string>();
		const string m_kUrlFls = "url_fls";

		public CfgVersionOnly() : base(){
		}

		protected override void _OnInit (string content)
		{
			this.m_lUrlFls.Clear();

			JsonData _jsonData = JsonMapper.ToObject (content);
			if (_jsonData == null)
				return;
			
			this.m_resVerCode = _ToStr(_jsonData[m_kResVerCode]);
			this.m_lastResVerCode = _ToStr(_jsonData[m_kLastResVerCode]);
			this.m_gameVerCode = _ToStr(_jsonData[m_kGameVerCode]);
			this.m_svnVerCode = _ToStr(_jsonData[m_kSvnVerCode]);
			this.m_platformType = _ToStr(_jsonData[m_kPlatformType]);
			this.m_language = _ToStr(_jsonData[m_kLanguage]);
			this.m_urlFilelist = _ToStr(_jsonData[m_kUrlFilelist]);
			_ToList (_jsonData[m_kUrlFls]);
		}

		string _ToStr(JsonData jsonData){
			if (jsonData != null)
				return jsonData.ToString ();
			return "";
		}

		void _ToList(JsonData jsonData){
			this.m_lUrlFls.Clear();

			if (jsonData == null || !jsonData.IsArray)
				return;
			
			for (int i = 0; i < jsonData.Count; i++) {
				this.m_lUrlFls.Add (_ToStr(jsonData[i]));
			}
		}

		public override bool IsUpdate (bool isCheckResUrl = false)
		{
			if (string.IsNullOrEmpty (m_resVerCode))
				return false;

			if (string.IsNullOrEmpty (m_urlFilelist) || URL_HEAD.Equals(m_urlFilelist) || m_urlFilelist.IndexOf(URL_HEAD) != 0)
				return false;

			return true;
		}

		public override void CloneFromOther (CfgVersion other)
		{
			base.CloneFromOther (other);

			if (other is CfgVersionOnly) {
				CfgVersionOnly tmp = (CfgVersionOnly)other;
				this.m_lUrlFls.AddRange (tmp.m_lUrlFls);
			}
		}

		string _ToJsonStr4Lists(){
			JsonData _jsonData = new JsonData ();
			_jsonData.SetJsonType (JsonType.Array);
			for (int i = 0; i < this.m_lUrlFls.Count; i++) {
				_jsonData.Add (this.m_lUrlFls [i]);
			}
			return _jsonData.ToJson ();
		}

		protected override void _OnSave ()
		{
			JsonData _jsonData = new JsonData ();
			_jsonData.SetJsonType (JsonType.Object);
			_jsonData[m_kResVerCode] = this.m_resVerCode;
			_jsonData[m_kLastResVerCode] = this.m_lastResVerCode;
			_jsonData[m_kGameVerCode] = this.m_gameVerCode;
			_jsonData[m_kSvnVerCode] = this.m_svnVerCode;
			_jsonData[m_kPlatformType] = this.m_platformType;
			_jsonData[m_kLanguage] = this.m_language;
			_jsonData[m_kUrlFilelist] = this.m_urlFilelist;

			#if UNITY_EDITOR
			if (!this.m_lUrlFls.Contains (this.m_urlFilelist)) {
				this.m_lUrlFls.Add (this.m_urlFilelist);
			}

			_jsonData [m_kUrlFls] = _ToJsonStr4Lists();
			#endif

			File.WriteAllText (this.m_filePath, _jsonData.ToJson ());
		}

		static CfgVersionOnly _instance;
		static new public CfgVersionOnly instance{
			get{ 
				if (_instance == null) {
					_instance = new CfgVersionOnly ();
				}
				return _instance;
			}
		}
	}
}

