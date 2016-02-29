using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class WidgetInteraction : MonoBehaviour {

	bool startAction;

	// Use this for initialization
	void Start () {
		startAction = false;
	}
	
	// Update is called once per frame
	void Update () {
		if (startAction != GetComponent<Interact> ().interact) {
			startAction = GetComponent<Interact> ().interact;

			if (startAction) {
				// do the actions here (switch case is fine)
				switch(tag) {
					case "PlayerToggle":
						Numbers.numPlayers = (Numbers.numPlayers == 1) ? 2 : 1;
						break;
					case "UpLayerNum":
						Numbers.numLayers = (Numbers.numLayers >= 50) ? 50 : Numbers.numLayers + 1;
						break;
					case "DownLayerNum":
						Numbers.numLayers = (Numbers.numLayers <= 2) ? 2 : Numbers.numLayers - 1;
						break;
					case "ExitToWindows":
						if (SceneManager.GetSceneByName ("UIScene").isLoaded) {
							Application.Quit ();
						}
						break;
					case "StartGame":
						SceneManager.LoadScene ("Scene1");
						break;

					///////////////////////////////////////////////////////////////
					case "Reset":
						TowerBuild.restartTower();
						break;
					case "MainMenu":
						SceneManager.LoadScene ("UIScene");
						break;
					case "Undo":
						GameState.undo();
						break;
					case "Redo":
						GameState.redo ();
						break;
					case "Rotate":
						Transform cam = GameObject.FindGameObjectWithTag ("MainCamera").transform;
						cam.position = new Vector3 (cam.position.x, cam.position.y, -1 * cam.position.z);
						cam.Rotate (0, 180.0f, 0);
						break;
					case "MoveUp":
						Transform camera = GameObject.FindGameObjectWithTag ("MainCamera").transform;
						float upby = (camera.position.y >= 1.2) ? 0.0f : 0.05f;
						camera.position = new Vector3 (camera.position.x, camera.position.y + upby, camera.position.z);
						break;
					case "MoveDown":
						Transform camer = GameObject.FindGameObjectWithTag ("MainCamera").transform;
						float downby = (camer.position.y <= 0.25) ? 0.0f : -0.05f;
						camer.position = new Vector3 (camer.position.x, camer.position.y + downby, camer.position.z);
						break;
					default:
						//do nothing
						break;
				}
				
				// reset startAction and interact for the widget
			}
		}
	}

	void OnTriggerEnter(Collider other){
		GameObject g = other.gameObject;
		if (g.CompareTag("PenTip")) {

		}
	}

	void OnTriggerStay(Collider other){
		GameObject g = other.gameObject;
		if (g.CompareTag("PenTip")) {
			
		}
	}

	void OnTriggerExit(Collider other){
		GameObject g = other.gameObject;
		if (g.CompareTag("PenTip")) {

		}
	}
}
