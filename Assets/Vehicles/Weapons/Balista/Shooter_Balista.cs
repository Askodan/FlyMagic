using UnityEngine;
using System.Collections;

public class Shooter_Balista: Shooter{
	//hierarchia:
	//-Rotator
	//--Rotator
	//---RearSight
                                                	//---FrontSight
	public float rotSpeed;
	public float AngleLimes;
	float TanAngleLimes;

	public Transform VerticalRotator;
	public Transform HorizontalRotator;
	public GameObject projectileAnim;//projectile in Ballista used solely for animation
	public Transform Hoist;
	bool canShoot = false;
	void Start(){
		BlendShape = GetComponentInChildren<SkinnedMeshRenderer> ();
		StartCoroutine (getReady ());
		TanAngleLimes = Mathf.Tan (Mathf.Deg2Rad * AngleLimes);
	}
	void Update(){
		if(shoot){
			Shoot (projectilePrefab, FrontSight.position, Power);
		}
		Aim (aimer.position);
	}
	override public void Shoot (GameObject projectile, Vector3 projectileSpawnPoint, float force){
		if (canShoot) {
			StartCoroutine (animShoot ());
			projectile = ObjectPool.Instance.GetObjectForType (projectile.name, false);
			projectile.transform.position = projectileSpawnPoint;
			projectile.transform.rotation = Quaternion.LookRotation (FrontSight.position - RearSight.position, FrontSight.up);
			projectile.GetComponent<Projectile_ForwardAndParabole> ().speed = force;
		}
	}

	override public void Aim (Vector3 target_pos){
		Vector3 target = Vector3.ProjectOnPlane (target_pos - HorizontalRotator.position, HorizontalRotator.parent.up);
		HorizontalRotator.transform.rotation = Quaternion.Lerp(HorizontalRotator.transform.rotation, Quaternion.LookRotation (target, HorizontalRotator.parent.up), rotSpeed*Time.deltaTime);

		Vector3 target_ver = target_pos - VerticalRotator.position;
		Vector3 target_ver_tr = VerticalRotator.parent.InverseTransformDirection (target_ver);
		target_ver_tr.x = 0;
		if ( Mathf.Abs(target_ver_tr.y/target_ver_tr.z)> TanAngleLimes) {
			target_ver_tr.y = TanAngleLimes * target_ver_tr.z*Mathf.Sign(target_ver_tr.y);
		}

		Quaternion newRot = Quaternion.LookRotation(VerticalRotator.parent.TransformDirection (target_ver_tr), VerticalRotator.parent.up);
		VerticalRotator.transform.rotation = Quaternion.Lerp(VerticalRotator.transform.rotation, newRot, rotSpeed*Time.deltaTime);
	}
	IEnumerator animShoot(){
		canShoot = false;
		float time = 0;
		while(time<shootTime){
			time += Time.deltaTime;
			BlendShape.SetBlendShapeWeight(0, 100f - time/shootTime*100f);
			projectileAnim.SetActive (false);
			yield return null;
		}
		BlendShape.SetBlendShapeWeight (0, 0f);
		StartCoroutine (getReady ());
	}
	IEnumerator getReady(){
		float time = 0;
		while(time<loadTime){
			time += Time.deltaTime;
			BlendShape.SetBlendShapeWeight(0, time/loadTime*100f);
			Hoist.Rotate (0f, 0f, 400f * Time.deltaTime, Space.Self);
			yield return null;
		}
		BlendShape.SetBlendShapeWeight (0, 100f);
		projectileAnim.SetActive (true);
		canShoot = true;
	}
}
