using UnityEngine;
using System.Collections;

public class DisplayNumLayers : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		GetComponent<TextMesh> ().text = "Number of Layers: " + Numbers.numLayers;
	}
}
