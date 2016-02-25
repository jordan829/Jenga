using UnityEngine;
using System.Collections;

public class Movement : MonoBehaviour {

    Transform heldObject;
    Vector3 initialPoint;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        /*if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit raycastInfo;
            if (Physics.Raycast(ray, out raycastInfo, 100))
            {
                heldObject = raycastInfo.transform;
                initialPoint = raycastInfo.point;
                heldObject.gameObject.GetComponent<Rigidbody>().AddForce(1, 0, 0);
            }
        }*/

        if (Input.GetMouseButton(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit raycastInfo;
            int layerMask = ~(1 << 0);
            if (Physics.Raycast(ray, out raycastInfo, 200, layerMask))
            {
                raycastInfo.transform.position = raycastInfo.point;
            }
        }
	}
}
