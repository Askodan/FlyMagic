using UnityEngine;
using System.Collections;

public class SimpleSmooth : MonoBehaviour {
	public float speed;
	public float rotSpeed;
	public Transform target;
	
	// Update is called once per frame
	void LateUpdate () {
		transform.position = Vector3.Slerp (transform.position, target.position, speed * Time.deltaTime);
		transform.rotation = Quaternion.Slerp (transform.rotation, target.rotation, rotSpeed * Time.deltaTime);
	}
}
