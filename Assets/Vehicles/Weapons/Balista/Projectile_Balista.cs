using UnityEngine;
using System.Collections;

public class Projectile_Balista : MonoBehaviour {
	public bool debug;
	public LayerMask mask;
	public float length;
	public float speed;
	public float maxLifeTime;
	// Update is called once per frame
	void Update () {
		if (debug) {
			Debug.DrawRay (transform.position, transform.forward*length, Color.red);
		}
		RaycastHit hit;
		if (Physics.Raycast (transform.position, transform.forward, out hit, length, mask)) {
			Debug.Log("Trafiono z balisty "+ hit.collider.name);
			ObjectPool.Instance.PoolObject (gameObject);
		}
		Move ();
	}
	void Move(){
		transform.position += transform.forward * speed* Time.deltaTime;
	}
	void OnEnable(){
		StartCoroutine (DieOnTime());
	}
	IEnumerator DieOnTime(){
		yield return new WaitForSeconds (maxLifeTime);
		ObjectPool.Instance.PoolObject (gameObject);
	}
}
