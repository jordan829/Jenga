using UnityEngine;
using System.Collections;

public class Walls : MonoBehaviour {

	public bool activeWalls;
	public Transform walls;

	// Use this for initialization
	void Start () {
		activeWalls = true;
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (TowerBuild.setUpDone)
        {
            activeWalls = false;
        }
        else
        {
            activeWalls = true;
        }
        walls.gameObject.SetActive (activeWalls);
	}
}
