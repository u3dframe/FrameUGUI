using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class Demo : MonoBehaviour {

	// Use this for initialization
	void Start () {
		Invoke ("FuncInvoke", 1);
	}
	
	// Update is called once per frame
	void FuncInvoke () {
		Scene s = SceneManager.GetActiveScene ();
		if (s.name == "Launcher") {
			SceneManager.LoadScene (1);
		} else {
			SceneManager.LoadScene (0);
		}
	}
}
