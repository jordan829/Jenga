using UnityEngine;
using System.Collections;


public class GroundCollide : MonoBehaviour {

    float timeStart;
    static bool startTimer;
    float distThreshold;
    static bool check2ndCollision;
    public static GameObject lastRemovedBlock;
    static bool gameOver;

	// Use this for initialization
	void Start () {
        startTimer = false;
        check2ndCollision = false;
        gameOver = false;
        distThreshold = Numbers.numLayers * 0.018f / 2.0f;
	}

    // Update is called once per frame
    void Update()
    {
        if (!startTimer)
        {
            timeStart = Time.time;
        }

        // next turn
        if (Time.time - timeStart > 3.0 && !gameOver)
        {
            check2ndCollision = false;
            startTimer = false;

            // if two players, change the prompter
            if(Numbers.numPlayers == 2)
            {
                GameState.playerTurn = (GameState.playerTurn == 1) ? 2 : 1;
            }
        }
        else if(gameOver)
        {
            GameState.gameOver = true;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // check if block (one) hits the ground
        GameObject g = collision.gameObject;
		if(g.layer == 8)
        {
			BlockState bState = g.GetComponent<BlockState> ();
			if (!bState.hitTheGround) {
				bState.hitTheGround = true;
				print (bState.hitTheGround);
				lastRemovedBlock = g;
				// g.SetActive(false);
				startTimer = true;
				// if yes...
				// TODO: check if player holding block

				// check if another block hits ground in next 5 seconds
				if (Time.time - timeStart < 5.0 && check2ndCollision) {
					// fail game
					gameOver = true;
				}
				check2ndCollision = true;

				// check if top layer fo tower falls below a threshold
				Transform topLayer = TowerBuild.blkLayers [TowerBuild.blkLayers.Count - 1];
				Transform topX, topY, topZ;
				if (topLayer.childCount == 3) {
					topX = topLayer.GetChild (0);
					topY = topLayer.GetChild (1);
					topZ = topLayer.GetChild (2);
				} 
				else {
					topX = topLayer.GetChild (0);
					topY = topLayer.GetChild (1);

					Stylus5 stylusGO = (Stylus5)FindObjectOfType(typeof(Stylus5));
					if (stylusGO.collidingWith.Count == 1) {
						topZ = stylusGO.collidingWith [0].transform;
					} else
						topZ = topLayer.GetChild (1);
				}
				if (topLayer.position.y - topX.position.y > distThreshold ||
				            topLayer.position.y - topY.position.y > distThreshold ||
				            topLayer.position.y - topZ.position.y > distThreshold) {
					// fail game
					gameOver = true;
				}
			}
        }


        // when turn is done, set startTimer to false
    }

    public static void restart()
    {
        startTimer = false;
        check2ndCollision = false;
        gameOver = false;
    }
}
