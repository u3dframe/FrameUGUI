using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// 类名 : 游戏入口
/// 作者 : Canyon / 龚阳辉
/// 日期 : 2017-12-05 16:15
/// 功能 : 
/// </summary>
public class Launcher : MonoBehaviour {
	void Start () {
		Kernel.CfgVersion.instance.SaveDefault ();

		int test = 10;
		Debug.Log("=1=" + Convert.ToString (test, 16).ToUpper ());
		Debug.Log ("=2=" + string.Format ("{0:X000}", test).ToUpper ());
	}
}
