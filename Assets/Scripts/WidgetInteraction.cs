using UnityEngine;
using System.Collections;

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
					
						break;
					case "UpLayerNum":
					
						break;
					case "DownLayerNum":
					
						break;
					case "ExitToWindows":
					
						break;
					case "StartGame":
					
						break;
					case "Reset":
					
						break;
					case "MainMenu":
					
						break;
					case "Undo":
					
						break;
					case "Replay":
					
						break;
					default:
						//do nothing
				}
				
				// reset startAction and interact for the widget
			}
		}
	}
}
