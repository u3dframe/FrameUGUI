using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class Demo : MonoBehaviour {

	// Use this for initialization
	void Start () {
		UGUIEventSystem.instance.Init (true);
		Invoke ("FuncInvoke",5);
	}
	
	// Update is called once per frame
	void FuncInvoke () {
		Debug.Log ("== FuncInvoke ==");
		GameObject.Destroy (UGUIEventSystem.instance.gameObject);

//		Scene s = SceneManager.GetActiveScene ();
//		if (s.name == "Launcher") {
//			SceneManager.LoadScene (1);
//		} else {
//			SceneManager.LoadScene (0);
//		}
	}
}
