using UnityEngine;
using System.Collections;

public class tractorscripteasy : MonoBehaviour {
	public Rigidbody[] kolat;
	public HingeJoint[] breaksInTrail;
	public Rigidbody[] kolap;
	public Rigidbody os;
	public float power;
	public float curve;

	HingeJoint[] joints;
	void Awake () {
		joints = new HingeJoint[kolat.Length];
		for (int i = 0; i < kolat.Length; i++) {
			joints[i] = kolat [i].GetComponent<HingeJoint> ();
		}
		getBreaksReady (joints);
		getBreaksReady (breaksInTrail);
	}
	void getBreaksReady(HingeJoint[] joint){
		for (int i = 0; i < joint.Length; i++) {
			JointMotor mot= joint [i].motor;
			mot.force = 10000 * power;
			mot.targetVelocity = 0;
			joint [i].motor = mot;
		}
	}


	bool breaking = false;
	public void Steer(bool butDown_Break, bool butUp_Break, float axis_Vertical, float axis_Horizontal){
		if (butDown_Break) {				
			breaking = true;
			for (int i = 0; i < kolat.Length; i++) {
				joints [i].useMotor = true;
			}
			for (int i = 0; i < breaksInTrail.Length; i++) {
				breaksInTrail [i].useMotor = true;
			}
		}
		if (butUp_Break) {
			breaking = false;
			for (int i = 0; i < kolat.Length; i++) {
				joints [i].useMotor = false;
			}
			for (int i = 0; i < breaksInTrail.Length; i++) {
				breaksInTrail [i].useMotor = false;
			}
		}
		if(!breaking){
			for (int i = 0; i < kolat.Length; i++) {
				joints [i].useMotor = false;
				kolat [i].AddRelativeTorque (power * axis_Vertical, 0f, 0f);
			}
		}

		for (int i = 0; i < kolap.Length; i++) {
			kolap [i].MoveRotation (os.rotation * Quaternion.Euler (0f, axis_Horizontal * curve, 0f));
		}
	}
}
