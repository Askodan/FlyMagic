using UnityEngine;
using System.Collections;

public class Shooter_MiniLaser :Shooter {
	public float maxAimAngle;
	public GameObject LoadEffect;
	Vector3 aimDirection;
	// Use this for initialization
	void Awake () {
		BlendShape = GetComponentInChildren<SkinnedMeshRenderer> ();
		StartCoroutine (getReady ());
	}
	
	// Update is called once per frame
	void Update () {
		if (shoot) {
			Shoot (projectilePrefab, FrontSight.position, Power);
		}
	}
	override public void Aim (Vector3 target_pos){
		Vector3 newAim = (target_pos - FrontSight.position).normalized;
		if (Vector3.Angle (newAim, transform.forward) < maxAimAngle) {
			aimDirection = newAim;
		} else {
			aimDirection = FrontSight.forward;
		}
	}
	override public void Shoot (GameObject projectile, Vector3 projectileSpawnPoint, float force){
		if (canShoot) {
			StartCoroutine (animShoot ());
			Aim (transform.forward + transform.position);
			projectile = ObjectPool.Instance.GetObjectForType (projectile.name, false);
			projectile.transform.position = projectileSpawnPoint;
			projectile.transform.rotation = Quaternion.LookRotation (aimDirection, FrontSight.up);
			Projectile_ForwardAndParabole flyProjectile = projectile.GetComponent<Projectile_ForwardAndParabole> ();
			flyProjectile.speed = force;
			flyProjectile.maxLifeTime = Range / force;
			//to jest niezbędne, żeby automatyczne ograniczenie czasu życia działało od pierwszej iteracji
			flyProjectile.enabled = false;
			flyProjectile.enabled = true;
		}
	}
	bool canShoot;
	IEnumerator getReady(){
		canShoot = false;
		LoadEffect.SetActive (true);
		float time = 0;
		while(time<loadTime){
			time += Time.deltaTime;
			BlendShape.SetBlendShapeWeight(0, time/loadTime*100f);
			yield return null;
		}
		BlendShape.SetBlendShapeWeight (0, 100f);

		canShoot = true;
	}
	IEnumerator animShoot(){
		canShoot = false;
		LoadEffect.SetActive (false);
		float time = 0;
		while(time<shootTime){
			time += Time.deltaTime;
			BlendShape.SetBlendShapeWeight(0, 100f - time/shootTime*100f);
			yield return null;
		}
		BlendShape.SetBlendShapeWeight (0, 0f);
		StartCoroutine (getReady ());
	}
}
