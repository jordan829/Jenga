using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//Stylus5: Demonstrates a snap-like characteristic where items close to the stylus tip turn black.
//			The collision mesh of the tip has been extended to x6. 
//			When user presses button, black object turns white. 

public class Stylus5 : MonoBehaviour
{
	
	private Quaternion initialRotation = new Quaternion();
	private Vector3 initialPosition = new Vector3();
	private ZSCore _zsCore;
	private ZSCore.TrackerTargetType _targetType = ZSCore.TrackerTargetType.Primary;
	
	public List<GameObject> collidingWith; 
	public GameObject interactingWith;
	
	protected void Start ()
	{
		_zsCore = GameObject.Find ("ZSCore").GetComponent<ZSCore> ();
		_zsCore.Updated += new ZSCore.CoreEventHandler (OnCoreUpdated);
		initialRotation = transform.rotation;
		initialPosition = transform.position;

		collidingWith = new List<GameObject>();
	}
	
	/// called by ZSCore after each input update.
	private void OnCoreUpdated (ZSCore sender)
	{
		UpdateStylusPose ();
		if (collidingWith.Count > 0){
			if (_zsCore.IsTrackerTargetButtonPressed(ZSCore.TrackerTargetType.Primary, 0)){
				for (int i = 0; i < collidingWith.Count; i++) {
					collidingWith [i].GetComponent<Renderer> ().material.color = Color.black;
				}

				if (collidingWith.Count == 1) {
					if (interactingWith == null && !GameState.replayOn) {
						interactingWith = collidingWith [0];
					}
				}

			}
			else {
				for (int i = 0; i < collidingWith.Count; i++) {
					collidingWith [i].GetComponent<Renderer>().material.color = new Color(0.8f, 0.8f, 0.8f);
					collidingWith [i].GetComponent<Interact> ().interact = false;
					interactingWith = null;
				}
			}
		}

		if (_zsCore.IsTrackerTargetButtonPressed (ZSCore.TrackerTargetType.Primary, 0) && interactingWith != null) {
			interactingWith.GetComponent<Interact> ().interact = true;
			interactingWith.GetComponent<Interact> ().penPos = transform.position;
			interactingWith.GetComponent<Interact> ().penRot = transform.rotation;
		} else if (interactingWith != null) {
			interactingWith.GetComponent<Interact> ().interact = false;
			if(interactingWith.layer == 8) {
				interactingWith.GetComponent<Rigidbody>().useGravity = true;   /// can't use with widgets
				interactingWith.transform.SetParent (interactingWith.GetComponent<BlockInteraction> ().lastParent); //// can't use with widgets
			}
			interactingWith = null;
		} else if (!_zsCore.IsTrackerTargetButtonPressed (ZSCore.TrackerTargetType.Primary, 0)) {
			// loop through pen's children and reset them
		}

	}		
	private void UpdateStylusPose ()
	{
		Matrix4x4 pose = _zsCore.GetTrackerTargetWorldPose (_targetType);
		transform.position = new Vector3 (pose.m03 + initialPosition.x,
		                                  pose.m13 + initialPosition.y,
		                                  pose.m23 + initialPosition.z);
		transform.rotation = Quaternion.LookRotation(pose.GetColumn(2), pose.GetColumn(1))
			* initialRotation;
	}

	public bool checkInteract() {
		return (interactingWith == null) ? false : true;
	}
}

