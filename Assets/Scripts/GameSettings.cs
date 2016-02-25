using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameSettings : MonoBehaviour {



	// Use this for initialization
	void Start () {
		//DontDestroyOnLoad (transform.gameObject);
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown ("+"))
			incLayer ();
		if (Input.GetKeyDown ("-"))
			decLayer ();
		if (Input.GetKeyDown ("p"))
			toggleNumPlayers ();
		if (Input.GetKeyDown ("space")) {
			if (SceneManager.GetSceneByName ("UIScene").isLoaded)
				SceneManager.LoadScene ("Scene1");
		}
		if (Input.GetKeyDown ("escape")) {
			if (SceneManager.GetSceneByName ("UIScene").isLoaded)
				Application.Quit ();
		}
	}

	void incLayer() {
		if (SceneManager.GetSceneByName ("UIScene").isLoaded) {
			if (Numbers.numLayers < 50)
				Numbers.numLayers++;
		}
	}

	void decLayer() {
		if (SceneManager.GetSceneByName ("UIScene").isLoaded) {
			if (Numbers.numLayers > 2)
				Numbers.numLayers--;
		}
	}

	void toggleNumPlayers() {
		if (SceneManager.GetSceneByName ("UIScene").isLoaded) {
			if (Numbers.numPlayers == 1)
				Numbers.numPlayers = 2;
			else
				Numbers.numPlayers = 1;
		}
	}
}
