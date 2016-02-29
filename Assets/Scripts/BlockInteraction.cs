using UnityEngine;
using System.Collections;

public class BlockInteraction : MonoBehaviour {

	bool startAction;
	Vector3 positionDiff;
	Quaternion angleDiff;
	public Transform lastParent;
	public int missIndex;

	// Use this for initialization
	void Start () {
		startAction = false;
		missIndex = -1;
		lastParent = transform.parent;
	}
	
	// Update is called once per frame
	void Update () {
		if (startAction != GetComponent<Interact> ().interact) {
			startAction = GetComponent<Interact> ().interact;

			if (startAction) {
				//lastParent = transform.parent;
				/*for (int i = 0; i < lastParent.childCount; i++) {
					if (lastParent.GetChild (i).GetInstanceID () == transform.GetInstanceID ()) {
						missIndex = i;
					}
				}*/
				missIndex = transform.GetSiblingIndex();
				GetComponent<Rigidbody> ().useGravity = false;
				transform.SetParent (GameObject.FindGameObjectWithTag ("Pen").transform);
				GetComponent<Rigidbody> ().isKinematic = false;
			} 
			else {
				GetComponent<Rigidbody> ().useGravity = true;
				transform.SetParent (lastParent);
				transform.SetSiblingIndex(missIndex);
			}
		}


	}
}
