using UnityEngine;
using System.Collections;

public class PlayerAim : MonoBehaviour {
	new Transform camera;
	Transform FrontSight;
	// Use this for initialization
	void Start () {
		camera = GameManager.Instance.camera.transform;
		FrontSight = transform.parent.GetComponent<Shooter> ().FrontSight;
	}
	
	// Update is called once per frame
	void Update () {
		transform.position = FrontSight.position + camera.forward;
	}
}
