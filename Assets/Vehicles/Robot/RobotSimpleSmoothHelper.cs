using UnityEngine;
using System.Collections;

public class RobotSimpleSmoothHelper : MonoBehaviour {
	public float distz;
	public float height;

	void Update () {
		transform.position = transform.parent.position - transform.forward * distz + Vector3.up * height;
	}
}
