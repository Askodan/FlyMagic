using UnityEngine;
using System.Collections;

public class DragonTargetSystem : MonoBehaviour {

	public bool useWaypoints;
	public bool start_flying;
	[Tooltip("Lista punktów.")]
	public GameObject[] wayPoints;
	public LayerMask layerMask;
	public float dist2pass;

	public Transform target;
	[HideInInspector] public bool atTarget;
	[HideInInspector] public Vector3 target_pos;
	[HideInInspector] public Quaternion target_rot;

	[HideInInspector] public float distHor;//prywatne
	[HideInInspector] public float distVer;//prywatne
	[HideInInspector] public float angleHor;//prywatne
	[HideInInspector] public float angleVer;//prywatne
	[HideInInspector] public float distHorGlob;//prywatne
	[HideInInspector] public float distVerGlob;//prywatne
	[HideInInspector] public float angleHorGlob;//prywatne
	[HideInInspector] public float angleVerGlob;//prywatne
	public RuntimeAnimatorController Running;
	public RuntimeAnimatorController Flying;
	int target_point;//prywatne
	// Use this for initialization
	Animator anim;
	void Awake () {
		GetComponent<DragonGroundedMovement> ().enabled = !start_flying;
		GetComponent<DragonFlightMovement> ().enabled = start_flying;
		anim = GetComponent<Animator> ();
		anim.runtimeAnimatorController = Flying;
		FindTarget ();
	}
	
	// Update is called once per frame
	void Update () {
		//find target uzupełnia dane o celu względem smoka moze korutynka?
		FindTarget ();

		if (distHor + Mathf.Abs(distVer) < dist2pass) {
			atTarget = true;
		} else {
			atTarget = false;
		}

		if (atTarget && useWaypoints) {
			target_point++;
			target_point = target_point % wayPoints.Length;
			target = wayPoints [target_point].transform;
		} 

	}
	//patrzy gdzie znajduje się cel... może warto wrzucić to do korutyny?
	void FindTarget(){
		target_pos = transform.InverseTransformPoint(target.position);
		target_rot = Quaternion.LookRotation (target.position - transform.position);
		CalculateBasicTargetInfo (out angleHor, out angleVer, out distHor, out distVer, Vector3.zero, target_pos);
		CalculateBasicTargetInfo (out angleHorGlob, out angleVerGlob, out distHorGlob, out distVerGlob, transform.position, target.position);
	}
	//obliczenie kilku parametrów, które wskażą drogę do celu
	// prawdopodobnie przerobie bo generuje za duzo informacji
	static void CalculateBasicTargetInfo (out float angleHor_, out float angleVer_, out float distHor_, out float distVer_, Vector3 pos, Vector3 tar_pos){
		distVer_ = tar_pos.y-pos.y;
		Vector3 target_pos_temp = new Vector3(tar_pos.x-pos.x, 0f, tar_pos.z-pos.z);
		distHor_ = target_pos_temp.magnitude;
		angleHor_ = Mathf.Atan2 (tar_pos.x-pos.x, tar_pos.z-pos.z);
		angleVer_ = Mathf.Atan2 (tar_pos.y-pos.y, distHor_);
		angleHor_ *= Mathf.Rad2Deg;
		angleVer_ *= Mathf.Rad2Deg;
	}
}
