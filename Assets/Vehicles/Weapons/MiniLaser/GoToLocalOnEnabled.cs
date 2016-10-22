using UnityEngine;
using System.Collections;

public class GoToLocalOnEnabled : MonoBehaviour {
	[Tooltip("On Enabled goes to this local position")]
	public Vector3 position;
	private Vector3 startPosition;
	void Awake(){
		startPosition = transform.localPosition;
	}
	void OnEnable () {
		transform.localPosition = position;
	}
	void OnDisable(){
		transform.localPosition = startPosition;
	}
}
