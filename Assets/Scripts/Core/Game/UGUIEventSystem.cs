using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

[RequireComponent(typeof(EventSystem))]
public class UGUIEventSystem : MonoBehaviour {

	bool _isCheck = false;
	bool _isAppQuit = false;
	EventSystem _curr;

	// Use this for initialization
	void Start () {
		_curr = gameObject.GetComponent<EventSystem> ();
	}
	
	// Update is called once per frame
	void Update () {
		if (!_isCheck)
			return;

		if (EventSystem.current == null) {
			EventSystem.current = _curr;
		}
	}

	void OnApplicationQuit(){
		this._isAppQuit = true;
	}

	void OnDestroy(){
		if (this._isAppQuit)
			return;

		_instance = null;
		instance.Init (true);
	}

	public void Init(bool isCheck){
		this._isCheck = isCheck;
		this._isAppQuit = false;
	}

	static UGUIEventSystem _instance;
	static public UGUIEventSystem instance{
		get{
			if (_instance == null) {
				GameObject _gobj = new GameObject ("EventSystem");
				_instance = _gobj.AddComponent<UGUIEventSystem> ();
				GameObject.DontDestroyOnLoad (_gobj);
			}
			return _instance;
		}
	}
}
