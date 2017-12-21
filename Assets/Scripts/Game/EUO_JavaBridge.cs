using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 类名 : U3D 与 Android 通讯桥
/// 作者 : Canyon / 龚阳辉
/// 日期 : 2016-05-22 10:15
/// 功能 : 实现com.sdkplugin.extend.PluginBasic.java，该类继承 AbsU3DListener
/// </summary>
public class EUO_JavaBridge : MonoBehaviour {
	// java 类名
	const string NM_JAVA_BRIDGE_CLASS = "com.sdkplugin.bridge.U3DBridge";
	
	// GameObject对象名
	const string NM_Gobj = "JavaBridge";
	
	// 回调方法名
	const string NM_ON_RESULT_FUNC = "OnResult";
	
	// java设置方法名
	const string NM_JAVA_METHOD_INIT = "initPars";

	// java消息接受方法
	const string NM_JAVA_METHOD_NOTIFY = "request";

	bool _isAppQuit = false;

#if UNITY_ANDROID	
	// java连接对象
	AndroidJavaClass jcBridge;
#endif

	public delegate void CallBackBridge (string data);
	CallBackBridge _callBack;

	void InitBridge(){
#if UNITY_ANDROID
		if( jcBridge != null ){
			return;
		}
		jcBridge = new AndroidJavaClass( NM_JAVA_BRIDGE_CLASS );
		jcBridge.CallStatic( NM_JAVA_METHOD_INIT,NM_Gobj ,NM_ON_RESULT_FUNC);
#endif
	}
	
	public void Init(CallBackBridge onResult) {
		this._callBack = onResult;
#if UNITY_ANDROID
		InitBridge();
#endif
	}
	
	public void SendToJava(string param){
#if UNITY_ANDROID
		if(jcBridge != null){
			jcBridge.CallStatic(NM_JAVA_METHOD_NOTIFY, param);
		} else {
			Debug.LogWarning("SendToJava: jcBridge is null.");
		}
#endif
	}
	
	void Awake() {
		name = NM_Gobj;
	}

	void OnResult(string data){
		if(_callBack != null){
			_callBack(data);
		} else {
			Debug.LogWarning("OnResult: _callBack is null");
		}
	}

	void OnApplicationQuit(){
		this._isAppQuit = true;
	}

	void OnDestroy(){
		if (this._isAppQuit) {
			return;
		}

		_instance = null;
		instance.Init (this._callBack);
	}

	void Clear(){
#if UNITY_ANDROID
		if(jcBridge != null){
			jcBridge.Dispose();
			jcBridge = null;
		}
#endif
	}

	static EUO_JavaBridge _instance;
	static public EUO_JavaBridge instance{
		get{
			if (_instance == null) {
				GameObject _gobj = new GameObject (NM_Gobj);
				_instance = _gobj.AddComponent<EUO_JavaBridge> ();
				GameObject.DontDestroyOnLoad (_gobj);
			}
			return _instance;
		}
	}
}