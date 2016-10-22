using UnityEngine;
using System.Collections;

public class Projectile_ForwardAndParabole : MonoBehaviour {
	public LayerMask mask;
	public float length;
	public float speed;
	public float maxLifeTime;
	public float fallSpeed;
	// Update is called once per frame
	void Update () {
		Debug.DrawRay (transform.position, length * transform.forward);
		RaycastHit hit;
		if (Physics.Raycast (transform.position, transform.forward, out hit, length, mask)) {
			Debug.Log("Trafiono "+ hit.collider.name);
			ObjectPool.Instance.PoolObject (gameObject);
		}
		Move ();
	}
	void Move(){
		transform.position += transform.forward * speed* Time.deltaTime;

		float angle = Vector3.Angle (Vector3.up, transform.forward);
		transform.RotateAround (transform.position, Vector3.Cross (transform.forward, Vector3.down), Mathf.Sin (angle*Mathf.Deg2Rad)*  Time.deltaTime * fallSpeed);
	}
	void OnEnable(){
		StartCoroutine (DieOnTime());
	}
	IEnumerator DieOnTime(){
		yield return new WaitForSeconds (maxLifeTime);
		ObjectPool.Instance.PoolObject (gameObject);
	}
}
