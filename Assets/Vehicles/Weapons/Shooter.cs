using UnityEngine;
using System.Collections;
public abstract class Shooter: MonoBehaviour{
	public float Power;
	public float Range;

	public bool shoot;
	public Transform FrontSight;
	public Transform RearSight;
	public Transform aimer;
	public float loadTime;
	public float shootTime;
	public GameObject projectilePrefab;
	protected SkinnedMeshRenderer BlendShape;

	//public LayerMask mask;
	abstract public void Shoot (GameObject projectile, Vector3 projectileSpawnPoint, float force);
	abstract public void Aim (Vector3 target_pos);
}
