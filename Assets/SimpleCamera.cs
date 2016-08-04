using UnityEngine;
using System.Collections;

public class SimpleCamera : MonoBehaviour {
	public Transform target;
	[Tooltip("Odległość kamery od targetu.")]
	public float dist;
	[Tooltip("X - min, Y - maks, odległość od obiektu.")]
	public Vector2 distLimes;
	[Tooltip("X - prędkość w osi y, Y - prędkość w osi x.")]
	public Vector2 speeds;
	[Tooltip("X - min, Y - maks, kąt w osi X.")]
	public Vector2 angleLimes;
	Vector2 rotation;
	// Use this for initialization
	void Start () {
		rotation.x = transform.eulerAngles.y;
		rotation.y = transform.eulerAngles.x;
		distLimes = CheckXBiggerThanY (distLimes);
		angleLimes = CheckXBiggerThanY (angleLimes);
	}
	
	// Update is called once per frame
	void LateUpdate () {
		rotation.x += Input.GetAxis("Mouse X") * speeds.x * Time.deltaTime;
		rotation.y -= Input.GetAxis("Mouse Y") * speeds.y * Time.deltaTime;

		Quaternion rot = Quaternion.Euler(rotation.y, rotation.x, 0f);
		dist = Mathf.Clamp(dist - Input.GetAxis("Mouse ScrollWheel")*5, distLimes.x, distLimes.y);

		rotation.y = ClampAngle(rotation.y, angleLimes.x, angleLimes.y);
		transform.position = rot * new Vector3 (0f, 0f, -dist) + target.position;
		transform.rotation = rot;
	}

	static float ClampAngle (float angle, float min, float max) {
		if (angle < -360)
			angle += 360;
		if (angle > 360)
			angle -= 360;
		return Mathf.Clamp (angle, min, max);
	}

	static Vector2 CheckXBiggerThanY(Vector2 input){
		if (input.x > input.y) {
			float a = input.x;
			input.x = input.y;
			input.y = a;
		}
		return input;
	}
}
