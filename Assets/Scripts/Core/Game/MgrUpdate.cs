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

		Error_UnZip_Init = 11,// 首次解压文件错误(只执行一次)
		Error_Net = 12,
		Error_Ver = 13,
		Error_FileList = 14,
		Error_DownFiles = 15,
	}

	State m_preState = State.None;
	State m_state = State.None;

	bool m_isRunning = false;
	bool m_isInit = false;

	long downCurr = 0,downSize = 0;

	Dictionary<string,string> m_dicLanguage = new Dictionary<string, string>();

	// Use this for initialization
	void Awake () {
		_Init ();
	}
	
	// Update is called once per frame
	void Update () {
		if (!m_isRunning)
			return;
		
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
	}

	void _Init(){
		if (m_isInit)
			return;
		m_isInit = true;

		_LoadLanguage ();

		_LoadUpdateUI ();

		m_state = State.Init;

		m_isRunning = true;
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

	void _LoadUpdateUI(){
	}

	void _ST_Init(){
		if (m_isInit) {
			m_state = State.UnZipResource;
			return;
		}

		_Init ();
	}

	void _ST_UnZipResource(){
		m_state = State.CheckNet;
	}

	void _ST_CheckNet(){
		bool isNotNet = Application.internetReachability == NetworkReachability.NotReachable;
		if (isNotNet) {
			m_state = State.Error_Net;
			return;
		}

		m_state = State.CheckVersion;
	}

	void _ST_CheckAndDown(){
		Kernel.CompareVersion _comVer = Kernel.CompareVersion.instance;

		if (_comVer.m_isInit) {
			_comVer.OnUpdate ();

			if (_comVer.isFinished) {
				m_state = State.Completed;
			} else if (_comVer.isPreDown) {
				if (!_comVer.m_isDoDownFiles) {
					_ReState (true);

					// 询问是否开始下载?
					_comVer.m_isDoDownFiles = true;
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
		this.downCurr = curr;
		this.downSize = target;
	}

	void _ST_Completed(){
		m_isRunning = false;
		// 进入游戏
	}

	void _ReState(bool isWaitCommand = false){
		if (isWaitCommand) {
			m_preState = m_state;
			m_state = State.WaitCommand;
			return;
		}

		m_state = m_preState;
		m_preState = State.None;
	}

	void OnUpdateUI(){
		if (m_state == State.WaitCommand)
			return;
		
		switch (m_state) {
		case State.Init:
			break;
		case State.UnZipResource:
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
			Debug.Log (downCurr + " == " + downSize);
			break;
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
		default:
			break;
		}
	}
}
