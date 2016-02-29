using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameState : MonoBehaviour {

    public static int playerTurn;
	static int currentPlayer;
    public static bool gameOver;

	static List<Vector3> undoTowerPos;					
	static List<Quaternion> undoTowerRot;			

	static List<Vector3> tempPos = new List<Vector3>();					
	static List<Quaternion> tempRot = new List<Quaternion>();			

	static List<Vector3> redoTowerPos;					
	static List<Quaternion> redoTowerRot;			

	static List<List<Vector3>> lastTurnPositions;			// holds positions of all blocks in every frame for the last turn
	static List<List<Vector3>> currentTurnPositions;		// holds positions of all blocks in every frame for the current turn
	static List<List<Quaternion>> lastTurnQuaternions;		// holds rotations of all blocks in every frame for the last turn
	static List<List<Quaternion>> currentTurnQuaternions;  // holds rotations of all blocks in every frame for the current turn

	public static bool replayOn;
	static int replayFrameNum;
	public static bool nextTurn;
	public static int turnsTaken;
	static bool recentlyUndo;
	static bool recentlyRedo;

    // Use this for initialization
    void Start () {
        playerTurn = 1;
		currentPlayer = 1;
        gameOver = false;
		replayOn = false;
		nextTurn = false;
		replayFrameNum = 0;
		turnsTaken = 0;
		recentlyUndo = false;
		recentlyRedo = false;
	}
	
	// Update is called once per frame
	void Update () {

		if (TowerBuild.setUpDone) {
			// check if the player turn has changed
			if (currentPlayer != playerTurn || nextTurn) {
				currentPlayer = playerTurn;
				nextTurn = false;
				if (gameOver)
					turnsTaken--;
				turnsTaken++;
				recentlyUndo = false;
				recentlyRedo = false;

				// reset redo undo lists
				redoTowerPos = new List<Vector3>();
				redoTowerRot = new List<Quaternion> ();
				undoTowerPos = tempPos;
				undoTowerRot = tempRot;
				tempPos = new List<Vector3> ();
				tempRot = new List<Quaternion> ();

				// get the list of blocklayers
				List<Transform> tower = TowerBuild.blkLayers;

				// in the beginning of the turn, get all the positions
				//print("FILLING IN TEMP");
				for (int i = 0; i < tower.Count; i++) {
					// fill in the perFrame buffer
					for (int j = 0; j < 3; j++) {
						Transform currBlock;
						if (tower [i].childCount == 3 || j < tower [i].childCount) {
							currBlock = tower [i].GetChild (j);
						}
					// if one block is being held, retrieve that child
					else if (tower [i].childCount < 3) {
							currBlock = tower [i].GetChild (0);
							// retrieve the held block
							Stylus5 stylusGO = (Stylus5)FindObjectOfType (typeof(Stylus5));
							if (stylusGO.interactingWith != null) {
								Transform missing = stylusGO.interactingWith.transform;
								if (j == missing.GetComponent<BlockInteraction> ().missIndex) {
									currBlock = missing;
								}
							} 
						} else
							currBlock = tower [i].GetChild (0);

						Vector3 toAdd = new Vector3 (0f, 0.001f * i, 0f);
						tempPos.Add (currBlock.position + toAdd);
						tempRot.Add (currBlock.rotation);
					}
				}


			}

		}

		GameObject.FindGameObjectWithTag ("TurnsTaken").GetComponent<UnityEngine.UI.Text> ().text = "Turns taken: " + turnsTaken;


        if(!gameOver)
            GameObject.FindGameObjectWithTag("PlayerText").GetComponent<UnityEngine.UI.Text>().text = "Player " + playerTurn + "'s turn";

        else
			GameObject.FindGameObjectWithTag("PlayerText").GetComponent<UnityEngine.UI.Text>().text = "Player " + playerTurn + " has lost!";
    }
    
    public static void restart()
    {
        playerTurn = 1;
		currentPlayer = 1;
        gameOver = false;
		turnsTaken = 0;
		nextTurn = false;
		redoTowerPos = new List<Vector3>();
		redoTowerRot = new List<Quaternion> ();
		undoTowerPos = new List<Vector3>();
		undoTowerRot = new List<Quaternion> ();
		tempPos = new List<Vector3> ();
		tempRot = new List<Quaternion> ();
    }

	public static void undo() {
		if (TowerBuild.setUpDone && undoTowerPos.Count > 1) {
			// get the list of block layers
			List<Transform> tower = TowerBuild.blkLayers;

			// if we've already moved one step, we don't have to do anything
			if (!recentlyUndo) {
				// modify playerTurn, turnsTaken, currentPlayer, and recentlyUndo
				playerTurn = (playerTurn == 1 && Numbers.numPlayers == 2) ? 2 : 1;
				turnsTaken--;
				//if (gameOver)
				//	turnsTaken--;
				currentPlayer = playerTurn;
				recentlyRedo = false;
				recentlyUndo = true;
			}

			// we shouldn't be in game over after an undo
			gameOver = false;
			GroundCollide.gameOver = false;

			// fill in the redo section
			for (int i = 0; i < tower.Count; i++) {
				// fill in the perFrame buffer
				for (int j = 0; j < 3; j++) {
					Transform currBlock;
					if (tower [i].childCount == 3 || j < tower [i].childCount) {
						currBlock = tower [i].GetChild (j);
					}
					// if one block is being held, retrieve that child
					else if (tower [i].childCount < 3) {
						currBlock = tower [i].GetChild (0);
						// retrieve the held block
						Stylus5 stylusGO = (Stylus5)FindObjectOfType (typeof(Stylus5));
						if (stylusGO.interactingWith != null) {
							Transform missing = stylusGO.interactingWith.transform;
							if (j == missing.GetComponent<BlockInteraction> ().missIndex) {
								currBlock = missing;
							}
						} 
					} else
						currBlock = tower [i].GetChild (0);

					Vector3 toAdd = new Vector3 (0f, 0.001f * i, 0f);
					redoTowerPos.Add (currBlock.position + toAdd);
					redoTowerRot.Add (currBlock.rotation);
				}
			}

			// reset the whole tower to the last turn
			for (int i = 0; i < tower.Count; i++) {
				for (int j = 0; j < 3; j++) {
					Transform block = tower [i].GetChild (j);
					block.position = undoTowerPos [(i * 3) + j];
					block.rotation = undoTowerRot [(i * 3) + j];
				}
			}
			if (GroundCollide.lastRemovedBlock.GetComponent<BlockState> ().hitTheGround) {
				GroundCollide.lastRemovedBlock.GetComponent<BlockState> ().undoed = true;
			}
			GroundCollide.lastRemovedBlock.GetComponent<BlockState> ().hitTheGround = false;

		}
		else {
			print ("undo failed");
		}
	}

	public static void redo() {
		if (TowerBuild.setUpDone && redoTowerPos.Count > 1) {
			// get the list of block layers
			List<Transform> tower = TowerBuild.blkLayers;

			// if we've already moved one step, we don't have to do anything
			if (!recentlyRedo) {
				// modify playerTurn, turnsTaken, currentPlayer, and recentlyUndo
				playerTurn = (playerTurn == 1 && Numbers.numPlayers == 2) ? 2 : 1;
				turnsTaken++;
				currentPlayer = playerTurn;
				recentlyRedo = true;
				recentlyUndo = false;
			}

			// we shouldn't be in game over after an undo
			gameOver = false;
			GroundCollide.gameOver = false;

			// reset the whole tower to the other last turn
			for (int i = 0; i < tower.Count; i++) {
				for (int j = 0; j < 3; j++) {
					Transform block = tower [i].GetChild (j);
					block.position = redoTowerPos [(i * 3) + j];
					block.rotation = redoTowerRot [(i * 3) + j];
				}
			}
			if (GroundCollide.lastRemovedBlock.GetComponent<BlockState> ().undoed) {
				GroundCollide.lastRemovedBlock.GetComponent<BlockState> ().hitTheGround = true;
			}

		} 
		else {
			print ("redo failed");
		}
	}

}
