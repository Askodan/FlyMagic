using UnityEngine;
using System.Collections;

public class Shooter_Balista: MonoBehaviour, Shooter{
	//hierarchia:
	//-Rotator
	//--Rotator
	//---RearSight
	//---FrontSight
	public float loadTime;
	public float shootTime;
	public Transform FrontSight;
	public Transform RearSight;

	public Transform VerticalRotator;
	public Transform HorizontalRotator;
	public GameObject projectilePrefab;
	public SkinnedMeshRenderer BalistaBlendShape;
	public GameObject projectileAnim;
	public Transform Hoist;
	bool canShoot = false;
	void Start(){
		StartCoroutine (getReady ());
	}
	void Update(){
		if (Input.GetMouseButtonDown (0)) {
			Shoot (projectilePrefab, FrontSight.position, 50);
			StartCoroutine(animShoot ());
		}
		Aim (GameManager.Instance.aimer.position);

	}
	public void Shoot (GameObject projectile, Vector3 projectileSpawnPoint, float force){
		if (canShoot) {
			projectile = ObjectPool.Instance.GetObjectForType(projectile.name, false);
			projectile.transform.position = projectileSpawnPoint;
			projectile.transform.rotation = Quaternion.LookRotation (FrontSight.position - RearSight.position, FrontSight.up);
			projectile.GetComponent<Projectile_Balista> ().speed = force;
		}
	}

	public void Aim (Vector3 target_pos){
		
		Vector3 target = Vector3.ProjectOnPlane (target_pos - HorizontalRotator.position, HorizontalRotator.parent.up);
		HorizontalRotator.transform.rotation = Quaternion.LookRotation (target, HorizontalRotator.parent.up);

		target = Vector3.ProjectOnPlane (target_pos - VerticalRotator.position, VerticalRotator.parent.right);
		VerticalRotator.transform.rotation = Quaternion.LookRotation (target, VerticalRotator.parent.up);
	}
	IEnumerator animShoot(){
		canShoot = false;
		float time = 0;
		while(time<shootTime){
			time += Time.deltaTime;
			BalistaBlendShape.SetBlendShapeWeight(0, 100f - time/shootTime*100f);
			projectileAnim.SetActive (false);
			yield return null;
		}
		BalistaBlendShape.SetBlendShapeWeight (0, 0f);
		StartCoroutine (getReady ());
	}
	IEnumerator getReady(){
		float time = 0;
		while(time<loadTime){
			time += Time.deltaTime;
			BalistaBlendShape.SetBlendShapeWeight(0, time/loadTime*100f);
			Hoist.Rotate (0f, 0f, -400f * Time.deltaTime, Space.Self);
			yield return null;
		}
		BalistaBlendShape.SetBlendShapeWeight (0, 100f);
		projectileAnim.SetActive (true);
		canShoot = true;
	}
}
