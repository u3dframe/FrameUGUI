using UnityEngine;
using System.Collections;

/// <summary>
/// 类名 : monobehaviour 基础脚本对象
/// 作者 : Canyon / 龚阳辉
/// 日期 : 2017-12-05 14:15
/// 功能 : 
/// </summary>
public class MonoBasic : MonoBehaviour {

	/// <summary>
	/// 初始化回调 (后操作)
	/// </summary>
	public System.Action m_callInit;

	/// <summary>
	/// 隐藏回调 (前操作)
	/// </summary>
	public System.Action m_callHide;

	/// <summary>
	/// 显示回调 (后操作,virtual执行之后)
	/// </summary>
	public System.Action m_callShow;

	/// <summary>
	/// 销毁回调 (前操作)
	/// </summary>
	public System.Action m_callDestroy;

	// 初始化
	protected bool m_isInited = false;

	// 设置显隐
	bool _m_isVisible = false;
	public bool m_isVisible{
		get{
			return _m_isVisible;
		}
		set{
			if (_m_isVisible != value) {
				_m_isVisible = value;
				if (m_gobj) {
					m_gobj.SetActive (_m_isVisible);
				}
			}
		}
	}

	// 自身对象
	Transform _m_trsf;
	public Transform m_trsf
	{
		get{
			if(_m_trsf == null){
				_m_trsf = transform;
			}
			return _m_trsf;
		}
	}

	GameObject _m_gobj;
	public GameObject m_gobj{
		get{
			if(_m_gobj == null){
				_m_gobj = gameObject;
			}
			return _m_gobj;
		}
	}

	void Awake () {
		Init ();
	}

	void OnEnable () {
		_m_isVisible = true;

		Init ();

		_OnShow ();

		if (m_callShow != null) {
			m_callShow ();
		}
	}

	void OnDisable () {
		_m_isVisible = false;

		if (m_callHide != null) {
			m_callHide ();
		}

		_OnHide ();
	}

	void OnDestroy(){
		if (m_callDestroy != null) {
			m_callDestroy ();
		}
		_OnDestroy ();
	}

	void Init(){
		if (m_isInited)
			return;
		m_isInited = true;

		_OnInit ();

		if (m_callInit != null) {
			m_callInit ();
		}
	}

	protected virtual void _OnInit(){
	}

	protected virtual void _OnShow(){
	}

	protected virtual void _OnHide(){
	}

	protected virtual void _OnDestroy(){
	}

	/// <summary>
	/// 取得自身组件 - C:CShape,C:Class
	/// </summary>
	static public MonoBasic GetCC(GameObject gobj){
		return Get<MonoBasic> (gobj);
	}

	#region === mono tools ===

	/// <summary>
	/// 找到组件
	/// </summary>
	static public T Find<T>(GameObject gobj) where T : Component
	{
		return gobj.GetComponent<T> ();
	}

	static public Component Find(GameObject gobj,System.Type comType)
	{
		return gobj.GetComponent(comType);
	}

	static public Component Find(GameObject gobj,string comType)
	{
		return gobj.GetComponent(comType);
	}

	/// <summary>
	/// 取得组件
	/// </summary>
	static public T Get<T>(GameObject gobj) where T : Component
	{
		T ret = null;
		if (gobj) {
			ret = gobj.GetComponent<T> ();
			if (!ret)
				ret = gobj.AddComponent<T> ();
		}
		return ret;
	}

	static public Component Get(GameObject gobj,System.Type comType)
	{
		Component ret = null;
		if (gobj) {
			ret = gobj.GetComponent(comType);
			if (!ret)
				ret = gobj.AddComponent(comType);
		}
		return ret;
	}

	static public Component Get(GameObject gobj,string comType)
	{
		return Get (gobj,System.Type.GetType (comType, true, true));
	}

	/// <summary>
	/// 设置父节点
	/// </summary>
	static public void SetParent(Transform trsf,Transform trsfParent,bool isLocal){
		bool isWorldPosStays = !isLocal;
		trsf.SetParent (trsfParent, isWorldPosStays);
		if (isLocal) {
			trsf.localPosition = Vector3.zero;
			trsf.localEulerAngles = Vector3.zero;
			trsf.localScale = Vector3.one;
		}
	}

	static public void SetParent (GameObject gobj, GameObject gobjParent, bool isLocal)
	{
		Transform trsf = gobj.transform;
		Transform trsfParent = null;
		if (gobjParent != null) {
			trsfParent = gobjParent.transform;
		}
		SetParent (trsf, trsfParent, isLocal);
	}

	// 为什么这么写？方便lua编译
	static public void SetParent(Transform trsf,Transform trsfParent){
		SetParent (trsf, trsfParent, true);
	}

	static public void SetParent (GameObject gobj, GameObject gobjParent){
		SetParent (gobj, gobjParent, true);
	}
	#endregion
}
