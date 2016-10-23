using UnityEngine;
using System.Collections;

public class UnparentRigidbodies : MonoBehaviour {
	[HideInInspector] public Transform[] originalChildren;
	// Use this for initialization
	void Awake(){
		originalChildren = GetComponentsInChildren<Transform> ();
	}
	void Start () {
		Rigidbody[] bodies = GetComponentsInChildren<Rigidbody> ();
		foreach (Rigidbody body in bodies) {
			if (body.transform != transform && !body.isKinematic) {
				body.transform.parent = transform.parent;
			}
		}
	}
}
