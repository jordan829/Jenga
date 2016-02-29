using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class TowerBuild : MonoBehaviour {

	public Transform blockLayer;
	public static List<Transform> blkLayers;
	public static bool setUpDone;
	static float startTime;
	static int i;
    float timePerLayer;

	// Use this for initialization
	void Start () {
		startTime = Time.time;
		setUpDone = false;
		blkLayers = new List<Transform> ();
		i = 0;
        timePerLayer = 0.15f;
	}
	
	// Update is called once per frame
	void Update () {
		if (i < Numbers.numLayers) {
			if (Time.time - startTime >= timePerLayer) {
				Transform x = Instantiate (blockLayer, new Vector3 (0.0f, (0.018f * i) + 0.055f, 0.0f), Quaternion.Euler (0, i * 90, 0)) as Transform;
				x.SetParent (transform);
				x.gameObject.name = i.ToString ();
				blkLayers.Add (x);
				startTime = Time.time;
				i++;
			}
		} 
		else if(!setUpDone) {
			setUpDone = true;
			GameState.nextTurn = true;
			GameState.turnsTaken--;
		}


		if (setUpDone) {
			if (Input.GetKeyDown ("space"))
				exitToMenu ();

			if (Input.GetKeyDown ("r"))
				restartTower ();

            

		}
	}

	public static void restartTower() {
		for (int x = 0; x < blkLayers.Count; x++) {
			Destroy (blkLayers [x].gameObject);
		}
		blkLayers = new List<Transform> ();
		setUpDone = false;
		startTime = Time.time;
		i = 0;
        GameState.restart();
        GroundCollide.restart();
	}

	void exitToMenu() {
		//Destroy (GameObject.FindGameObjectWithTag ("GameSettings"));
		SceneManager.LoadScene ("UIScene");
	}

    void checkGameOver()
    {
        GameObject ground = GameObject.FindGameObjectWithTag("Ground") as GameObject;
        

    }
}
