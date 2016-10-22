using UnityEngine;
using System.Collections;

public class TesterGoDown : MonoBehaviour {
	public float speed;

	void Update () {
		transform.Translate (Vector3.down * speed*Time.deltaTime);
	}
}
