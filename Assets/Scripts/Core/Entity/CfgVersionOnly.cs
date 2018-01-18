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
	/// 功能 : 只有一个version,根据渠道宏来定地址
	/// </summary>
	public class CfgVersionOnly : CfgVersion  {
		// 地址
		public List<string> m_lUrlFls = new List<string>();
		const string m_kUrlFls = "url_fls";

		// 是否可以写入文件列表地址s
		public bool m_isCanWriteUrlFls{get;set;}

		// 当前下载filelist的地址在列表中的位置
		public int m_iIndInFls { get; private set; }

		public CfgVersionOnly() : base(){
			this.m_isCanWriteUrlFls = false;
			this.m_iIndInFls = 0;

			#if UNITY_EDITOR
			this.m_urlVersion = "http://192.168.30:8006/z1";
			#else
			this.m_urlVersion = "http://192.168.30:8006/z1";
			#endif

			// versiton地址从package对象里面获得
			if (!string.IsNullOrEmpty (CfgPackage.instance.m_urlVersion)) {
				this.m_urlVersion = CfgPackage.instance.m_urlVersion;
			}

			if (!string.IsNullOrEmpty (CfgPackage.instance.m_uprojVer)) {
				this.m_pkgVersion = CfgPackage.instance.m_uprojVer;
			}
		}

		protected override void _OnInit (string content)
		{
			this.m_lUrlFls.Clear();

			JsonData _jsonData = null;
			try {
				_jsonData = JsonMapper.ToObject (content);
			} catch{				
			}
			if (_jsonData == null)
				return;
			
			this.m_resVerCode = _ToStr(_jsonData,m_kResVerCode);
			this.m_gameVerCode = _ToStr(_jsonData,m_kGameVerCode);
			this.m_urlFilelist = _ToStr(_jsonData,m_kUrlFilelist);
			this.m_bigVerCode = _ToStr(_jsonData,m_kBigVerCode);
			this.m_urlNewApkIpa = _ToStr(_jsonData,m_kUrlNewApkIpa);
			this.m_urlSv = _ToStr(_jsonData,m_kUrlSV);
			this.m_pkgFilelist = _ToStr(_jsonData,m_kPkgFilelist);
			this.m_pkgFiles = _ToStr(_jsonData,m_kPkgFiles,"files");

			if(_jsonData.Keys.Contains(m_kUrlFls))
				_ToList (_jsonData[m_kUrlFls]);
		}

		string _ToStr(JsonData jsonData,string key,string def = null){
			if (jsonData != null) {
				if (jsonData.Keys.Contains (key))
					return jsonData [key].ToString ();
			}

			if (def != null)
				return def;
			
			return "";
		}

		void _ToList(JsonData jsonData){
			this.m_lUrlFls.Clear();

			if (jsonData == null || !jsonData.IsArray)
				return;

			string _url = "";
			for (int i = 0; i < jsonData.Count; i++) {
				_url = jsonData [i].ToString();
				if (this.m_lUrlFls.Contains (_url))
					continue;
				
				this.m_lUrlFls.Add (_url);
				if (_url.Equals (this.m_urlFilelist)) {
					this.m_iIndInFls = this.m_lUrlFls.Count - 1;
				}
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
			_jsonData[m_kGameVerCode] = this.m_gameVerCode;
			_jsonData[m_kBigVerCode] = this.m_bigVerCode;
			_jsonData[m_kUrlNewApkIpa] = this.m_urlNewApkIpa;
			_jsonData[m_kUrlFilelist] = this.m_urlFilelist;
			_jsonData[m_kUrlSV] = this.m_urlSv;

			if (this.m_isCanWriteUrlFls) {
				if (!this.m_lUrlFls.Contains (this.m_urlFilelist)) {
					this.m_lUrlFls.Add (this.m_urlFilelist);
				}

				_jsonData [m_kUrlFls] = _ToJsonStr4Lists ();
			}

			File.WriteAllText (this.m_filePath, _jsonData.ToJson());
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

