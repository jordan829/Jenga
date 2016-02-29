using UnityEngine;
using System.Collections;

public class Stylus5Tip : MonoBehaviour
{
	private Color saveColor;
	Stylus5 stylusGO;

	void Start(){
		stylusGO = (Stylus5)FindObjectOfType(typeof(Stylus5));
	}

	void OnCollisionEnter(Collision other){
		GameObject g = other.gameObject;
		if (g.layer == 8 || g.layer == 9) {
			stylusGO.collidingWith.Add (other.gameObject);
			saveColor = other.gameObject.GetComponent<Renderer> ().material.color;
			GetComponent<Renderer> ().material.color = Color.red;
		}
	}
	void OnCollisionStay(Collision other){
		GameObject g = other.gameObject;
		if (g.layer == 8|| g.layer == 9) {
			//Debug.Log ("colliding");

			//GetComponent<Renderer>().material.color = Color.red;
		}
	}
	void OnCollisionExit(Collision other){
		GameObject g = other.gameObject;
		if (g.layer == 8|| g.layer == 9) {
			for (int i = 0; i < stylusGO.collidingWith.Count; i++) {
				if (stylusGO.collidingWith [i].GetInstanceID () == other.gameObject.GetInstanceID ()) {
					stylusGO.collidingWith.RemoveAt (i);
					break;
				}
			}
			//stylusGO.collidingWith = null;
			if (stylusGO.collidingWith.Count == 0) {
				GetComponent<Renderer> ().material.color = Color.green;
			} else {
				/*string s = "";
				for (int i = 0; i < stylusGO.collidingWith.Count; i++) {
					s += stylusGO.collidingWith [i].name + ", ";
				}
				print (s);*/
			}
			other.gameObject.GetComponent<Renderer> ().material.color = saveColor;
		}
	}

	/*
	void OnTriggerEnter(Collider other){
		GameObject g = other.gameObject;
		if (g.layer == 8 || g.layer == 9) {
			stylusGO.collidingWith.Add (other.gameObject);
			saveColor = other.gameObject.GetComponent<Renderer> ().material.color;
			GetComponent<Renderer> ().material.color = Color.red;
		}
	}

	void OnTriggerStay(Collider other){
		GameObject g = other.gameObject;
		if (g.layer == 8|| g.layer == 9) {
			//Debug.Log ("colliding");

			//GetComponent<Renderer>().material.color = Color.red;
		}
	}

	void OnTriggerExit(Collider other){
		GameObject g = other.gameObject;
		if (g.layer == 8|| g.layer == 9) {
			for (int i = 0; i < stylusGO.collidingWith.Count; i++) {
				if (stylusGO.collidingWith [i].GetInstanceID () == other.gameObject.GetInstanceID ()) {
					stylusGO.collidingWith.RemoveAt (i);
					break;
				}
			}
			//stylusGO.collidingWith = null;
			if (stylusGO.collidingWith.Count == 0) {
				GetComponent<Renderer> ().material.color = Color.green;
			} else {
				for (int i = 0; i < stylusGO.collidingWith.Count; i++) {
					print (stylusGO.collidingWith [i].name);
				}
			}
			other.gameObject.GetComponent<Renderer> ().material.color = saveColor;
		}
	}*/
}

