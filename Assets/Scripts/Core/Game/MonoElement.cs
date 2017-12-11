using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

/// <summary>
/// 类名 : mono 对象
/// 作者 : Canyon / 龚阳辉
/// 日期 : 2017-12-05 14:25
/// 功能 : 
/// </summary>
public class MonoElement : MonoBasic {
	/// <summary>
	/// 操作的对象
	/// </summary>
	[SerializeField]
	private GameObject[] m_gobjs;

	/// <summary>
	/// key = name;
	/// val = gobj;
	/// </summary>
	Dictionary<string,GameObject> m_dicName2Gobj = new Dictionary<string,GameObject>();

	/// <summary>
	/// key = Relative name (相对应自身对象);
	/// val = gobj;
	/// </summary>
	Dictionary<string,GameObject> m_dicRName2Gobj = new Dictionary<string,GameObject>();

	protected override void _OnInit ()
	{
		base._OnInit ();
		GameObject tmp = null;
		string _tmpName = "";
		for(int i = 0; i < m_gobjs.Length;i++)
		{
			tmp = m_gobjs[i];
			if(tmp){
				_tmpName = tmp.name;
				if(!m_dicName2Gobj.ContainsKey(_tmpName)){
					m_dicName2Gobj.Add(_tmpName,tmp);
				}else{
					Debug.LogError(string.Format("the same name = [{0}] in gameObject.name = [{1}]",_tmpName,gameObject.name));
				}

				_tmpName = GetRelativeName(tmp);
				if(!m_dicRName2Gobj.ContainsKey(_tmpName)){
					m_dicRName2Gobj.Add(_tmpName,tmp);
				}else{
					Debug.LogError(string.Format("the same name = [{0}] in gameObject.name = [{1}]",_tmpName,gameObject.name));
				}
			}
		}
	}

	/// <summary>
	/// 取得自身对象下面的对象的相对路径name
	/// </summary>
	void _GetRelativeName(Transform trsf,ref string refName)
	{
		if(!trsf){
			return;
		}

		if(trsf == m_trsf){
			if(string.IsNullOrEmpty(refName))
			{
				refName = "/";
			}
			return;
		}

		if(string.IsNullOrEmpty(refName))
		{
			refName = trsf.name;
		} else {
			refName = trsf.name + "/" + refName;
		}

		_GetRelativeName(trsf.parent,ref refName);
	}

	/// <summary>
	/// 取得自身对象下面的对象的相对路径name
	/// </summary>
	string GetRelativeName(GameObject gobj)
	{
		string ret = "";
		_GetRelativeName(gobj.transform,ref ret);
		return ret;
	}

	/// <summary>
	/// 取得可操作的对象
	/// </summary>
	public GameObject GetGobjElement(string elName){
		if(string.IsNullOrEmpty(elName))
			return null;

		if("/".Equals(elName))
		{
			return gameObject;
		}

		if(m_dicRName2Gobj.ContainsKey(elName)){
			return m_dicRName2Gobj[elName];
		}

		if(m_dicName2Gobj.ContainsKey(elName)){
			return m_dicName2Gobj[elName];
		}
		return null;
	}

	/// <summary>
	/// 是否包含元素
	/// </summary>
	public bool IsHasGobj(string elName)
	{
		GameObject gobj = GetGobjElement(elName);
		return !!gobj;
	}

	/// <summary>
	/// 取得子对象的组件
	/// </summary>
	public T FindComponent<T>(string elName) where T : Component
	{
		GameObject gobj = GetGobjElement(elName);

		if(gobj == null){
			return null;
		}
		return gobj.GetComponent<T>();
	}

	/// <summary>
	/// 取得子对象的组件
	/// </summary>
	public Component FindComponent(string elName,string comType)
	{
		GameObject gobj = GetGobjElement(elName);

		if(gobj == null){
			return null;
		}
		return gobj.GetComponent(comType);
	}

	/// <summary>
	/// 取得子对象的组件
	/// </summary>
	public Component FindComponent(string elName,Type comType)
	{
		GameObject gobj = GetGobjElement(elName);

		if(gobj == null){
			return null;
		}
		return gobj.GetComponent(comType);
	}

	/// <summary>
	/// 设置元素显隐
	/// </summary>
	public void SetActive4Child(string elName,bool isActive)
	{
		GameObject gobj = GetGobjElement(elName);
		if(gobj){
			gobj.SetActive(isActive);
		}
	}

	/// <summary>
	/// 取得自身组件 - C:CShape,C:Class
	/// </summary>
	static public new MonoElement GetCC(GameObject gobj){
		return Get<MonoElement> (gobj);
	}
}
