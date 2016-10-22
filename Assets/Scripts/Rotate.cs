using UnityEngine;
using System.Collections;

public class Rotate : MonoBehaviour {
	public Space space;
	public Vector3 speed;

	// Update is called once per frame
	void Update () {
		transform.Rotate (speed*Time.deltaTime, space);
	}
}
