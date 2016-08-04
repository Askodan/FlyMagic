using UnityEngine;
using System.Collections;

public class DragonSteering : MonoBehaviour {
	DragonMovement DM;
	public Transform target;
	public Transform camera;
	public float dist;
	// Use this for initialization
	void Start () {
		DM = GetComponent<DragonMovement> ();
		DM.target = target;
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetAxis ("Vertical") > 0) {
			target.position = (transform.position - camera.transform.position) * dist + transform.position;
		}

	}
}
