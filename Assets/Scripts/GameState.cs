using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameState : MonoBehaviour {

    public static int playerTurn;
	static int currentPlayer;
    public static bool gameOver;

	static List<Vector3> perFramePosition;					// placeholder for block positions in each frame
	static List<Quaternion> perFrameQuaternion;			// placeholder for block rotations in each frame

	static List<List<Vector3>> lastTurnPositions;			// holds positions of all blocks in every frame for the last turn
	static List<List<Vector3>> currentTurnPositions;		// holds positions of all blocks in every frame for the current turn
	static List<List<Quaternion>> lastTurnQuaternions;		// holds rotations of all blocks in every frame for the last turn
	static List<List<Quaternion>> currentTurnQuaternions;  // holds rotations of all blocks in every frame for the current turn

	public static bool replayOn;
	static int replayFrameNum;

    // Use this for initialization
    void Start () {
        playerTurn = 1;
		currentPlayer = 1;
        gameOver = false;
		replayOn = false;
		replayFrameNum = 0;

		perFramePosition = new List<Vector3>();
		perFrameQuaternion = new List<Quaternion> ();

		lastTurnPositions = new List<List<Vector3>> ();
		currentTurnPositions = new List<List<Vector3>> ();
		lastTurnQuaternions = new List<List<Quaternion>> ();
		currentTurnQuaternions = new List<List<Quaternion>> ();
	}
	
	// Update is called once per frame
	void Update () {

		if (TowerBuild.setUpDone) {
			// check if the player turn has changed
			if (currentPlayer != playerTurn) {
				currentPlayer = playerTurn;
				// reset/save the list of lists
				lastTurnPositions = currentTurnPositions;
				lastTurnQuaternions = currentTurnQuaternions;
				currentTurnPositions = new List<List<Vector3>> ();
				currentTurnQuaternions = new List<List<Quaternion>> ();
			}

			List<Transform> tower = TowerBuild.blkLayers;

			for (int i = 0; i < tower.Count; i++) {
				// fill in the perFrame buffer
				for (int j = 0; j < tower[i].childCount; j++) {
					Transform currBlock = tower [i].GetChild (j);
					if (tower [i].childCount < 3) {
						// retrieve the held block
						Stylus5 stylusGO = (Stylus5)FindObjectOfType(typeof(Stylus5));
						if (stylusGO.collidingWith.Count == 1) {
							Transform missing = stylusGO.collidingWith [0].transform;
							if (j == missing.GetComponent<BlockInteraction> ().missIndex) {
								currBlock = missing;
							}
						}
					}

					perFramePosition.Add (currBlock.position);
					perFrameQuaternion.Add (currBlock.rotation);
				}
			}

			currentTurnPositions.Add (perFramePosition);
			currentTurnQuaternions.Add (perFrameQuaternion);

			replay ();
		}

        if(!gameOver)
            GameObject.FindGameObjectWithTag("PlayerText").GetComponent<UnityEngine.UI.Text>().text = "Player " + playerTurn + "'s turn";

        else
            GameObject.FindGameObjectWithTag("PlayerText").GetComponent<UnityEngine.UI.Text>().text = "YOOOOOOOOOOUUUUUUUUU LOOOOOOOOOSSSST";
    }
    
    public static void restart()
    {
        playerTurn = 1;
		currentPlayer = 1;
        gameOver = false;

		perFramePosition = new List<Vector3>();
		perFrameQuaternion = new List<Quaternion> ();

		lastTurnPositions = new List<List<Vector3>> ();
		currentTurnPositions = new List<List<Vector3>> ();
		lastTurnQuaternions = new List<List<Quaternion>> ();
		currentTurnQuaternions = new List<List<Quaternion>> ();
    }

	public static void undo() {
		if (TowerBuild.setUpDone) {
			List<Transform> tower = TowerBuild.blkLayers;
			playerTurn = (playerTurn == 1) ? 2 : 1;
			currentPlayer = playerTurn;
			gameOver = false;

			List<Vector3> firstFramePos = lastTurnPositions [0];
			List<Quaternion> firstFrameRot = lastTurnQuaternions [0];

			// reset the whole tower to the last turn
			for (int i = 0; i < tower.Count; i++) {
				for (int j = 0; j < 3; j++) {
					Transform block = tower [i].GetChild (j);
					block.position = firstFramePos [(i * 3) + j];
					block.rotation = firstFrameRot [(i * 3) + j];
				}
			}

			// reset the lists
			perFramePosition = new List<Vector3> ();
			perFrameQuaternion = new List<Quaternion> ();

			lastTurnPositions = new List<List<Vector3>> ();
			currentTurnPositions = new List<List<Vector3>> ();
			lastTurnQuaternions = new List<List<Quaternion>> ();
			currentTurnQuaternions = new List<List<Quaternion>> ();
		} 
		else {
			print ("undo failed");
		}
	}


	public static void replay() {
		// first order of business is to restrict any commands or interactions during a replay
		if (!replayOn && lastTurnPositions.Count != 0) {  //////////////////// TODO: Add a condition for a boolean set to true and false by the replay widget (immediately set to false after)
			replayOn = true;
			replayFrameNum = 0;
		}

		// next is to load the list of frames' data and then move it every frame (external counter)
		if (replayOn && lastTurnPositions.Count != 0) {
			List<Vector3> pos = lastTurnPositions [replayFrameNum];
			List<Quaternion> rot = lastTurnQuaternions [replayFrameNum];
			List<Transform> tower = TowerBuild.blkLayers;

			for (int i = 0; i < tower.Count; i++) {
				for (int j = 0; j < 3; j++) {
					Transform block = tower [i].GetChild (j);
					block.position = pos [(i * 3) + j];
					block.rotation = rot [(i * 3) + j];
				}
			}

			replayFrameNum++;
		}

		// last is to resume play after the tower has been returned to before the replay
		if (replayOn && replayFrameNum >= lastTurnPositions.Count && lastTurnPositions.Count != 0) {
			replayOn = false;

			// resume from latest currentTurn values
			List<Vector3> pos = currentTurnPositions [currentTurnPositions.Count - 1];
			List<Quaternion> rot = currentTurnQuaternions [currentTurnQuaternions.Count - 1];
			List<Transform> tower = TowerBuild.blkLayers;

			for (int i = 0; i < tower.Count; i++) {
				for (int j = 0; j < 3; j++) {
					Transform block = tower [i].GetChild (j);
					block.position = pos [(i * 3) + j];
					block.rotation = rot [(i * 3) + j];
				}
			}

		}
	}
}
