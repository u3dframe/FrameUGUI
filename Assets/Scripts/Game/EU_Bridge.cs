using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 类名 : U3D 与 平台 通讯
/// 作者 : Canyon / 龚阳辉
/// 日期 : 2016-05-22 10:15
/// 功能 : 
/// </summary>
public static class EU_Bridge {
	
	public delegate void CallBackBridge (string data);

	static public void Init(CallBackBridge onResult) {
#if UNITY_ANDROID
		EUO_JavaBridge.CallBackBridge _call = null;
		if (onResult != null) {
			_call = data => onResult (data);
		}
		EUO_JavaBridge.instance.Init (_call);
#endif
	}
	
	static public void Send(string param){
#if UNITY_ANDROID
		EUO_JavaBridge.instance.SendToJava (param);
#endif
	}
}