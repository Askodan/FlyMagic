using UnityEngine;
using System.Collections;

public class DragonSteering : MonoBehaviour {
	public Transform target;
	public float dist;
	DragonTargetSystem targetSystem;
	// Use this for initialization
	void Start () {
		targetSystem = GetComponent<DragonTargetSystem> ();
		targetSystem.target = target;
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetAxis ("Vertical") > 0) {
			target.position = (transform.position - GameManager.Instance.camera.transform.position) * dist + transform.position;
		}

	}
}
