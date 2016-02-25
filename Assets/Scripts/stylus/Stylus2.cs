using UnityEngine;
using System.Collections;

//Stylus2: Simple example with a raycast hover function, that highlights objects pointed to,
//			no matter how far away. 

public class Stylus2 : MonoBehaviour
{
	
	private GameObject contactPoint;
	private class LastHoveredGO {
		public GameObject GO;
		public Color originalColor;
	};
	private LastHoveredGO lastHoveredGO;
	
	private Quaternion initialRotation = new Quaternion();
	private Vector3 initialPosition = new Vector3();
	private ZSCore _zsCore;
	private ZSCore.TrackerTargetType _targetType = ZSCore.TrackerTargetType.Primary;

	protected void Start ()
	{
		_zsCore = GameObject.Find ("ZSCore").GetComponent<ZSCore> ();
		_zsCore.Updated += new ZSCore.CoreEventHandler (OnCoreUpdated);
		initialRotation = transform.rotation;
		initialPosition = transform.position;

		contactPoint = GameObject.Find("ContactPoint");
		contactPoint.SetActive(false);
		lastHoveredGO = null;
	}
	
	public void Update(){
		RaycastHit hit;
		Debug.DrawRay(transform.position,transform.forward);
		if (Physics.Raycast (transform.position,
		                     transform.forward,
		                     out hit)){
			contactPoint.SetActive(true);
			contactPoint.transform.position = hit.point;
			if (lastHoveredGO != null){
				lastHoveredGO.GO.GetComponent<Renderer>().material.color = lastHoveredGO.originalColor;
			}
			else {
				lastHoveredGO = new LastHoveredGO();
			}
			lastHoveredGO.GO = hit.collider.gameObject;
			lastHoveredGO.originalColor = hit.collider.gameObject.GetComponent<Renderer>().material.color;
			hit.collider.gameObject.GetComponent<Renderer>().material.color = Color.magenta;
		}
		else{
			contactPoint.SetActive(false);
			if (lastHoveredGO != null){
				lastHoveredGO.GO.GetComponent<Renderer>().material.color = lastHoveredGO.originalColor;
				lastHoveredGO = null;
			}
		}
		
	}

	/// called by ZSCore after each input update.
	private void OnCoreUpdated (ZSCore sender)
	{
		UpdateStylusPose ();
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
}

