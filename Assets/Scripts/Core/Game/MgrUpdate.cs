using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 类名 : monobehaviour 基础脚本对象
/// 作者 : Canyon / 龚阳辉
/// 日期 : 2017-12-05 14:15
/// 功能 : 
/// </summary>
class MgrUpdate : MonoBehaviour {

	enum State{
		None = 0,
		Init = 1,

		UnZipResource = 2,
		CheckNet = 3,
		CheckVersion = 4,
		CheckFileList = 5,
		CompareFileList = 6,
		DeleteOldFiles = 7,
		DownFiles = 8,
		WaitCommand = 9,
		Completed = 10,

		NeedDownApkIpa = 99,

		Error_UnZip_Init = 11,// 首次解压文件错误(只执行一次)
		Error_Net = 12,
		Error_Ver = 13,
		Error_FileList = 14,
		Error_DownFiles = 15,
	}

	State m_preState = State.None;
	State m_state = State.None;

	bool isError{
		get{
			return m_state == State.Error_UnZip_Init || m_state == State.Error_Net || m_state == State.Error_Ver 
				|| m_state == State.Error_FileList || m_state == State.Error_DownFiles || m_state == State.NeedDownApkIpa;
		}
	}

	bool m_isRunning = false;
	bool m_isInit = false;

	long nCurr = 0,nSize = 0,nPercent = 0;

	long nLimitSize = 1024 * 1024 * 5;

	Dictionary<string,string> m_dicLanguage = new Dictionary<string, string>();

	bool m_isValidVersion = true;
	bool m_isUnZip = true;

	List<AssetBundle> m_uiAbs = new List<AssetBundle> ();

	float m_limitUpTime = 0.05f,m_currTime = 0;

	Kernel.CompareVersion _comVer{ get { return Kernel.CompareVersion.instance; } }

	// Use this for initialization
	public void Init (bool isValidVer = true,bool isUnZip = true) {
		this.m_isValidVersion = isValidVer;
		this.m_isUnZip = isUnZip;
		_Init ();
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		if (!m_isRunning)
			return;

		if (m_limitUpTime > 0.02f) {
			m_currTime += Time.fixedDeltaTime;
			if (m_currTime < m_limitUpTime)
				return;
			m_currTime -= m_limitUpTime;
		}

		switch (m_state) {
		case State.Init:
			_ST_Init ();
			break;
		case State.UnZipResource:
			_ST_UnZipResource ();
			break;
		case State.CheckNet:
			_ST_CheckNet ();
			break;
		case State.CheckVersion:
		case State.CheckFileList:
		case State.CompareFileList:
		case State.DeleteOldFiles:
		case State.DownFiles:
			_ST_CheckAndDown ();
			break;
		case State.Completed:
			_ST_Completed ();
			break;
		default:
			break;
		}

		OnUpdateUI ();

		OnUpdateUI4Error ();
	}

	void _Init(){
		if (m_isInit)
			return;
		m_isInit = true;

		_LoadLanguage ();

		_LoadUpdateUI ();

		m_state = State.Init;

		m_isRunning = true;

		EU_Bridge.Init ((strJson) => {
			Kernel.CfgPackage.instance.Init(strJson);
		});
		
		EU_Bridge.Send ("{\"cmd\":\"getPackageInfo\",\"filename\":\"cfg_z1.json\"}");
	}

	void _LoadLanguage(){
		string _filename = "updatestring.txt";
		string _val = Kernel.GameFile.GetText (_filename);

		if (string.IsNullOrEmpty (_val))
			return;

		string[] _vals = _val.Split ("\r\n\t".ToCharArray (), System.StringSplitOptions.RemoveEmptyEntries);
		string[] _arrs;
		for (int i = 0; i < _vals.Length; i++) {
			_arrs = _vals[i].Split ("|".ToCharArray (), System.StringSplitOptions.RemoveEmptyEntries);
			if (_arrs.Length < 2)
				continue;
			
			if (!m_dicLanguage.ContainsKey (_arrs [0])) {
				m_dicLanguage.Add (_arrs [0], _arrs [1]);
			}
		}
	}

	string _GetLanguage(string key){
		if (m_dicLanguage.ContainsKey (key))
			return m_dicLanguage [key];
		return key;
	}

	void _LoadUpdateUI(){
//		System.Action<AssetBundle> _call = (ab) => {
//			ab.LoadAllAssets();
//			m_uiAbs.Add(ab);
//		};

		// _LoadAB ("", _call); 图集
		// _LoadAB ("", _call); 字体
		// 加载UI
		// _LoadAB ("", (ab)=>{
			// GameObject gobj = ab.LoadAsset<GameObject>("name");
			// GameObject.Instantiate(gobj).GetComponent<T>();
		// });

		for (int i = 0; i < m_uiAbs.Count; i++) {
			m_uiAbs [i].Unload (false);
		}
		m_uiAbs.Clear ();
	}

	void _LoadAB(string fn,System.Action<AssetBundle> callFunc = null){
		string _fp = Kernel.GameFile.GetPath (fn);
		AssetBundle _ab = AssetBundle.LoadFromFile (_fp);
		if (_ab != null) {
			if (callFunc != null) {
				callFunc (_ab);
			}
		}
	}

	void _ST_Init(){
		if (m_isInit) {
			#if UNITY_EDITOR
			m_state = State.UnZipResource;
			#else
			if (Kernel.CfgPackage.instance.m_isInit) {
				EU_Bridge.Init (null);
				m_state = State.UnZipResource;
			}
			#endif
			return;
		}

		_Init ();
	}

	void _ST_UnZipResource(){
		if (!m_isUnZip) {
			m_state = State.CheckNet;
			return;
		}
		// zip 解压操作 ????
		m_state = State.CheckNet;
	}

	void _ST_CheckNet(){
		bool isNotNet = Application.internetReachability == NetworkReachability.NotReachable;
		if (isNotNet) {
			m_state = State.Error_Net;
			return;
		}
		if (m_isValidVersion) {
			m_state = State.CheckVersion;
		} else {
			m_state = State.Completed;
		}
	}

	void _ST_CheckAndDown(){
		if (_comVer.m_isInit) {
			_comVer.OnUpdate ();

			if (_comVer.isFinished) {
				m_state = State.Completed;
			} else if (_comVer.isPreDown) {
				if (!_comVer.m_isDoDownFiles) {
					if (nLimitSize < nSize) {
						_ReState (true);
						// 询问是否开始下载?
						_comVer.m_isDoDownFiles = true;
						_ReState ();
					} else {
						_comVer.m_isDoDownFiles = true;
					}
				}
			}
		} else {
			_comVer.Init (_CallVersionUpdate, _CallDownfiles);
		}
	}

	void _CallVersionUpdate(int state){
		m_state = (State)state;
	}

	void _CallDownfiles(long curr,long target){
		this.nCurr = curr;
		this.nSize = target;
	}

	void _ST_Completed(){
		m_isRunning = false;
		// 进入游戏
		// Kernel.CfgVersionOnly.instance.LoadDefault();
	}

	void _ReState(bool isWaitCommand = false){
		if (isWaitCommand) {
			m_preState = m_state;
			m_state = State.WaitCommand;
			return;
		}

		_ReState (m_preState);
	}

	void _ReState(State currState){
		m_state = currState;
		m_preState = State.None;
	}

	void OnUpdateUI(){
		if (m_state == State.WaitCommand)
			return;
		
		switch (m_state) {
		case State.Init:
			break;
		case State.UnZipResource:
			Debug.Log (nCurr + " == " + nSize + " = " + nPercent);
			break;
		case State.CheckNet:
			break;
		case State.CheckVersion:
			break;
		case State.CheckFileList:
			break;
		case State.CompareFileList:
			break;
		case State.DeleteOldFiles:
			break;
		case State.DownFiles:
			Debug.Log (nCurr + " == " + nSize + " = " + nPercent);
			break;
		case State.Completed:
			break;
		}
	}

	void OnUpdateUI4Error(){
		if (m_state == State.WaitCommand || !isError)
			return;

		State _tmpState = m_state;
		_ReState (true);

		switch (_tmpState) {
		case State.Error_UnZip_Init:
			break;
		case State.Error_Net:
			break;
		case State.Error_Ver:
			break;
		case State.Error_FileList:
			break;
		case State.Error_DownFiles:
			break;
		case State.NeedDownApkIpa:
			break;
		default:
			break;
		}
	}

	string _GetSizeMB(float size){
		float fKB = size / 1024f;
		float fMB = fKB / 1024f;
		if (fMB > 1.0f) {
			return string.Format ("{0:F}MB", fMB);
		}

		if (fKB > 1.0f) {
			return string.Format ("{0:F}KB", fKB);
		}

		return string.Format ("{0:F}B", size);
	}

	void _ExitQuit(){
		#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
		#else
		Application.Quit ();
		#endif
	}
}
