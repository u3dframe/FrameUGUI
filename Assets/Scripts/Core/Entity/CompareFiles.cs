using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;

namespace Kernel
{
	/// <summary>
	/// 类名 : 对比文件
	/// 作者 : Canyon / 龚阳辉
	/// 日期 : 2017-12-07 10:35
	/// 功能 : 
	/// </summary>
	public class CompareFiles
	{

		public enum State
		{
			None = 0,
			Init = 1,
			DeleteFiles = 2,
			PreDownFiles = 3,
			DownFiles = 4,

			Finished = 10,

			Error_Url = 11,
			Error_Net = 12,
			Error_Write = 13,
		}

		static int _Code = 0;

		static SortCompareFiles m_sort = new SortCompareFiles ();

		public int m_code{ get; private set; }

		// 更新地址
		public string m_newUrl{ get; private set; }

		public string m_newContent{ get; private set; }

		CfgFileList _m_cfgOld = new CfgFileList (true);
		CfgFileList _m_cfgNew = new CfgFileList (true);

		public CfgFileList m_cfgOld{ get { return _m_cfgOld; } }
		public CfgFileList m_cfgNew{ get { return _m_cfgNew; } }

		// 删除的
		Dictionary<string,ResInfo> m_deletes = new Dictionary<string, ResInfo> ();
		Dictionary<string,DownLoadFile> _m_updates = new Dictionary<string, DownLoadFile> ();
		public Dictionary<string,DownLoadFile> m_updates{ get { return _m_updates; } }
		List<DownLoadFile> m_lNeed = new List<DownLoadFile> ();

		// 限定正在下载的个数
		int m_nLimitDowning = 10;
		List<DownLoadFile> m_lDowning = new List<DownLoadFile> ();
		List<DownLoadFile> m_lDownError = new List<DownLoadFile> ();

		List<string> m_lKeys = new List<string> ();
		List<DownLoadFile> m_lRemoveDownings = new List<DownLoadFile> ();
		string m_key = "";
		DownLoadFile m_dwf;

		bool m_isRunning = true;

		State _m_state = State.None;

		public State m_state{ get { return _m_state; } private set { _m_state = value; } }

		public bool isError{
			get{
				return m_state == State.Error_Url || m_state == State.Error_Net || m_state == State.Error_Write;
			}
		}

		public bool isFinished{ get{ return m_state == State.Finished; } }

		// 下载文件的状态通知 参数:值(当前文件对象)
		public System.Action<DownLoadFile> m_callDownFile;

		public CompareFiles ()
		{
			m_code = (++_Code);
		}

		void Clear ()
		{
			m_cfgOld.Clear ();
			m_cfgNew.Clear ();

			m_deletes.Clear ();
			m_updates.Clear ();

			m_lKeys.Clear ();

			m_lNeed.Clear ();
			m_lDowning.Clear ();
			m_lDownError.Clear ();
			m_lRemoveDownings.Clear ();
		}

		public void Init (string newFiles, string newUrl)
		{
			m_cfgOld.LoadDefault ();

			Init (m_cfgOld.m_content, newFiles, newUrl);
		}

		public void Init (string oldFiles, string newFiles, string newUrl)
		{
			Clear ();

			this.m_newUrl = newUrl;
			this.m_newContent = newFiles;

			this.m_cfgOld.Init (oldFiles);
			this.m_cfgNew.Init (newFiles);

			this.DoCompare ();
		}

		public void Init(CfgFileList oldFiles, string newFiles, string newUrl){
			Clear ();

			this.m_newUrl = newUrl;
			this.m_newContent = newFiles;

			this.m_cfgOld.CloneFromOther(oldFiles);
			this.m_cfgNew.Init (newFiles);

			this.DoCompare ();
		}

		public void Init(CfgFileList oldFiles, CfgFileList newFiles, string newUrl){
			Clear ();

			this.m_newUrl = newUrl;
			this.m_newContent = newFiles.m_content;

			this.m_cfgOld.CloneFromOther(oldFiles);
			this.m_cfgNew.CloneFromOther (newFiles);

			this.DoCompare ();
		}

		public void DoCompare(){
			this._Compare ();
			m_state = State.Init;
		}

		bool _IsCanDown (ResInfo info)
		{
			string _fp = GameFile.GetFilePath (info.m_resName);
			if (File.Exists (_fp)) {
				using (FileStream stream = new FileStream (_fp, FileMode.Open)) {
					byte[] buffer = new byte[stream.Length];
					stream.Read (buffer, 0, buffer.Length);

					string _compareCode = ALG.CRC32Class.GetCRC32 (buffer);
					if (_compareCode.Equals (info.m_compareCode)) {
						return false;
					}
				}
			}
			return true;
		}

		void _Compare ()
		{
			if (m_cfgNew.m_dicFiles.Count <= 0)
				return;

			foreach (string _key in m_cfgOld.m_dicFiles.Keys) {
				if (m_cfgNew.m_dicFiles.ContainsKey (_key))
					continue;
				m_deletes.Add (_key, m_cfgOld.m_dicFiles [_key]);
			}

			foreach (string _key in m_cfgNew.m_dicFiles.Keys) {
				if (m_cfgOld.m_dicFiles.ContainsKey (_key)) {
					if (m_cfgNew.m_dicFiles [_key].IsSame (m_cfgOld.m_dicFiles [_key]))
						continue;
				}
				// 读取文件流，下载会很卡的
//				if (!_IsCanDown (m_cfgNew.m_dicFiles [_key]))
//					continue;

				m_dwf = DownLoadFile.ParseBy (m_cfgNew.m_dicFiles [_key]);
				m_dwf.m_url = this.m_newUrl;
				m_updates.Add (_key, m_dwf);
			}
		}

		// 合并
		public void Merge (CompareFiles other)
		{
			int _diff = other.m_code - this.m_code;
			if (_diff == 0)
				return;

			CompareFiles min = _diff > 0 ? this : other;
			CompareFiles max = _diff > 0 ? other : this;

			foreach (string _key in min.m_deletes.Keys) {
				if (max.m_deletes.ContainsKey (_key))
					continue;
				
				if (max.m_updates.ContainsKey (_key))
					continue;
				
				max.m_deletes.Add (_key, min.m_deletes [_key]);
			}

			foreach (string _key in min.m_updates.Keys) {
				if (max.m_updates.ContainsKey (_key))
					continue;

				if (max.m_deletes.ContainsKey (_key))
					continue;

				max.m_updates.Add (_key, min.m_updates [_key]);
			}

			m_lKeys.Clear ();

			foreach (string _key in max.m_updates.Keys) {
				if (max.m_deletes.ContainsKey (_key)) {
					m_lKeys.Add (_key);
				}
			}

			for (int i = 0; i < m_lKeys.Count; i++) {
				max.m_updates.Remove (m_lKeys [i]);
			}

			m_lKeys.Clear ();
			min.Clear ();
		}

		public CompareFiles MergeGetMax (List<CompareFiles> list)
		{
			if (list == null || list.Count <= 0)
				return this;
			
			if (!list.Contains (this)) {
				list.Add (this);
			}

			if (list.Count == 1)
				return list [0];

			list.Sort (m_sort);

			CompareFiles min = list [0];
			list.RemoveAt (0);

			CompareFiles max = list [0];
			list.RemoveAt (0);
			while (max.m_code != min.m_code) {
				min.Merge (max);
				min = max;

				if (list.Count > 0) {
					max = list [0];
					list.RemoveAt (0);
				}
			}
			return max;
		}

		public void OnUpdate ()
		{
			if (!m_isRunning)
				return;
			
			switch (m_state) {
			case State.Init:
				m_state = State.DeleteFiles;
				break;
			case State.DeleteFiles:
				_ST_DeleteFiles ();
				break;
			case State.PreDownFiles:
				_ST_PreDownFiles ();
				break;
			case State.DownFiles:
				_ST_DownFiles ();
				break;
			case State.Finished:
				m_isRunning = false;
				break;
			default:
				break;
			}
		}

		void _ST_DeleteFiles ()
		{
			if (m_deletes.Count <= 0) {
				m_state = State.PreDownFiles;
				return;
			}

			m_lKeys.Clear ();
			m_lKeys.AddRange (m_deletes.Keys);

			int lens = m_lKeys.Count;
			string key;
			ResInfo info;
			for (int i = 0; i < 10; i++) {
				if (lens > i) {
					key = m_lKeys [i];
					info = m_deletes [key];
					m_deletes.Remove (key);
					GameFile.DeleteFile (info.m_resName);
				}
			}
		}

		void _ST_PreDownFiles(){
			if (m_lNeed.Count <= 0) {
				m_state = State.DownFiles;
				return;
			}

			for (int i = 0; i < m_lNeed.Count; i++) {
				m_dwf = m_lNeed [i];
				if (m_updates.ContainsKey (m_dwf.m_resName))
					continue;
				m_updates.Add (m_dwf.m_resName,m_dwf);
			}

			m_lNeed.Clear ();
		}

		void _ST_DownFiles ()
		{
			if (m_updates.Count <= 0 && m_lDowning.Count <= 0) {
				if (m_state == State.DownFiles)
					m_state = State.Finished;
				return;
			}

			m_lKeys.Clear ();

			if (m_lDowning.Count < m_nLimitDowning && m_updates.Count > 0) {
				m_lKeys.AddRange (m_updates.Keys);

				int diff = m_nLimitDowning - m_lDowning.Count;
				while (diff > 0 && m_lKeys.Count > 0) {
					m_key = m_lKeys [0];
					m_lKeys.RemoveAt (0);

					m_dwf = m_updates [m_key];
					m_lDowning.Add (m_dwf);
					m_updates.Remove (m_key);

					diff--;
				}

				m_lKeys.Clear ();
			}

			for (int i = 0; i < m_lDowning.Count; i++) {
				m_lDowning [i].OnUpdate ();
			}

			for (int i = 0; i < m_lDowning.Count; i++) {
				m_dwf = m_lDowning [i];
				if (m_dwf.isError) {
					m_lRemoveDownings.Add (m_dwf);
					m_lDownError.Add (m_dwf);
					m_state = (State)((int)m_dwf.m_state);
					_ExcCBDownFile (m_dwf);
					_LogError (m_dwf.m_error.Message);
				} else if (m_dwf.isFinished) {
					m_lRemoveDownings.Add (m_dwf);
					_ExcCBDownFile (m_dwf);
				}
			}

			if (m_lRemoveDownings.Count > 0) {
				for (int i = 0; i < m_lRemoveDownings.Count; i++) {
					m_lDowning.Remove (m_lRemoveDownings [i]);
				}

				m_lRemoveDownings.Clear ();
			}
		}

		void _ExcCBDownFile (DownLoadFile obj)
		{
			if (this.m_callDownFile != null) {
				this.m_callDownFile (obj);
			}
		}

		void _LogError(string msg){
			UnityEngine.Debug.LogError (msg);
		}

		public long GetDownSize(){
			long sum = 0;
			foreach (var item in m_updates.Values) {
				sum += item.m_size;
			}
			return sum;
		}

		public void ReDownFile(){
			if (m_lDownError.Count > 0) {
				for (int i = 0; i < m_lDownError.Count; i++) {
					m_lDownError [i].ReDown ();
				}
				m_lNeed.AddRange (m_lDownError);

				m_lDownError.Clear ();
			}
			this.m_state = State.PreDownFiles;
		}

		public void Save ()
		{
			this.m_cfgNew.SaveDefault ();
		}
	}

	class SortCompareFiles : Comparer<CompareFiles>
	{
		#region implemented abstract members of Comparer

		public override int Compare (CompareFiles x, CompareFiles y)
		{
			return x.m_code.CompareTo (y.m_code);
		}

		#endregion
		
	}
}
