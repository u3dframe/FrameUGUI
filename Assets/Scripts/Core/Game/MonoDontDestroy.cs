using UnityEngine;
using System.Collections;

/// <summary>
/// 类名 : 设置对象不销毁
/// 作者 : Canyon / 龚阳辉
/// 日期 : 2017-12-05 14:15
/// 功能 : 
/// </summary>
public class MonoDontDestroy : MonoBasic {
	protected override void _OnInit ()
	{
		base._OnInit ();
		GameObject.DontDestroyOnLoad (m_gobj);
	}
}
