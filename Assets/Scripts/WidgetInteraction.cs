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

			}
		}
	}
}
