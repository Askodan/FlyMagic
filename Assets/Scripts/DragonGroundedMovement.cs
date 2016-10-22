using UnityEngine;
using System.Collections;

public class DragonGroundedMovement : MonoBehaviour {

	public enum DragonGroundedState{
		run = 1,
		crawl = 2,

	}
	Animator anim;
	DragonTargetSystem targetSystem;
	public float crawlSpeed;
	public float runSpeed;
	public float height;
	//szybkości lerpów
	public float rotationSpeed;
	public float positionSpeed;
	DragonGroundedState state;
	float actualSpeed;
	void Awake () {
		anim = GetComponent<Animator> ();
		targetSystem=GetComponent<DragonTargetSystem> ();
	}

	// Update is called once per frame
	void Update () {
		RaycastHit hit;
		Physics.Raycast (new Ray(transform.position, -transform.up), out hit);
	
		if (!waiting2Check) {
			StartCoroutine (CheckTarget ());
		}
		UpdatePosition (hit);
	}
	public void UpdatePosition(RaycastHit hit){
		float angle = Mathf.Abs(Vector3.Angle(hit.normal, Vector3.up));
		AnalyzeState (angle);
		Vector3 newPos = hit.point + hit.normal * height + transform.forward * Time.deltaTime * actualSpeed;
		Quaternion newRot = Quaternion.LookRotation (Vector3.ProjectOnPlane (targetSystem.target.position - transform.position, hit.normal), hit.normal);
		transform.rotation = Quaternion.Lerp(transform.rotation, newRot, rotationSpeed*Time.deltaTime);
		transform.position = Vector3.Lerp(transform.position, newPos, positionSpeed*Time.deltaTime);

		//transform.position = newPos;



		//transform.LookAt (Vector3.ProjectOnPlane (targetSystem.target.position - transform.position, hit.normal)+transform.position, hit.normal);
		//transform.position=newPps;

	}
	void AnalyzeState(float angle){
		if(angle > 40){
			state = DragonGroundedState.crawl;
			actualSpeed = crawlSpeed;
		}else{
			state = DragonGroundedState.run;
			actualSpeed = runSpeed;
		}
		anim.SetInteger ("State", (int)state);
	}

	void TakeOff(){
		DragonFlightMovement dfm = GetComponent<DragonFlightMovement> ();
		dfm.enabled = true;
		dfm.getAKick (dfm.minSpeed);
		anim.runtimeAnimatorController = targetSystem.Flying;
		enabled = false;
	}
	bool waiting2Check;
	IEnumerator CheckTarget(){
		waiting2Check = true;
		yield return new WaitForSeconds (2f);
		if (Physics.Raycast (new Ray (transform.position, targetSystem.target.position - transform.position), 20f)) {
			//Debug.DrawLine (transform.position, targetSystem.target.position - transform.position);

			waiting2Check = false;
		} else {
			waiting2Check = false;
			TakeOff ();
		}
	}
}
