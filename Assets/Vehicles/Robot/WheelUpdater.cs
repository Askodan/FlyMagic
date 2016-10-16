using UnityEngine;
using System.Collections;

public class WheelUpdater : MonoBehaviour {
	//tester obliczania pozycji, zostanie przestawiony w docelową pozycję koła
	public Transform tester;

	//czy to kolo napedowe
	public bool motorOn;
	public float force;
	//czy jest prawe, czy lewe
	public bool right;
	//czy jest przedie, czy tylne
	public bool front;

	HingeJoint joint;
	Vector3 localPosition;

	//GameObject axis;
	HingeJoint axis_joint;
	//Rigidbody axis_rigidbody;
	void Awake(){
		//set up local position and joint
		float minus = 1;
		if (right) {
			minus = -1;
		}

		localPosition = Quaternion.Euler(0,0,minus*90)*transform.localPosition;
		Rigidbody wheelbody = GetComponent<Rigidbody> ();//gameObject.AddComponent<Rigidbody>();

		joint = GetComponent<HingeJoint> ();//joint = gameObject.AddComponent <HingeJoint>();

		if (motorOn) {
			joint.useMotor = true;
			JointMotor mot = joint.motor;
			mot.freeSpin = false;
			mot.force = force;
			joint.motor = mot;
		}
		wheelbody.maxAngularVelocity = Mathf.Infinity;

		foreach(HingeJoint newJoint in transform.parent.GetComponentsInChildren <HingeJoint>()){
			if (newJoint != joint)
				axis_joint = newJoint;
		}
		//axis_rigidbody = axis_joint.transform.GetComponent<Rigidbody> ();
		/*//set up axis to turning
		axis = new GameObject();
		axis.transform.position = transform.parent.position;
		axis.transform.SetParent (transform.GetComponentInParent<RobotLegsWheelsSteering>().transform);

		axis_rigidbody = axis.AddComponent<Rigidbody> ();
		axis_rigidbody.mass = 0.00001f;

		axis_joint = axis.AddComponent<HingeJoint> ();
		axis_joint.autoConfigureConnectedAnchor = true;
		axis_joint.connectedBody = axis.transform.parent.GetComponent<Rigidbody>();
		axis_joint.axis = Vector3.up;
		axis_joint.useLimits = true;

		joint.autoConfigureConnectedAnchor = false;
		joint.connectedBody = axis_rigidbody;
		joint.connectedAnchor = localPosition;
		joint.axis = Vector3.right;
		joint.useLimits = true;*/
	}
	//implementacja na zwykłym koliderze
	public void  UpdateWheel(float motor, float steer){
		if (motorOn) {
			JointMotor mot = joint.motor;
			mot.targetVelocity = motor;
			joint.motor = mot;
		}
		if (tester) {
			tester.localPosition = localPosition + joint.connectedBody.transform.InverseTransformPoint(transform.parent.position);		
		}
		axis_joint.connectedAnchor = axis_joint.connectedBody.transform.InverseTransformPoint(transform.parent.position);
		//float minus = -1f;
		//if (front)
		//	minus = 1f;

		JointLimits lim = axis_joint.limits;
		lim.min = -steer;
		lim.max = steer;
		axis_joint.limits = lim;
		JointMotor mot_axis = axis_joint.motor;
		mot_axis.targetVelocity = steer;
		axis_joint.motor = mot_axis;
	}

	/* implementacja na wheelcolliderach
	WheelCollider wheelCollider;
	public float wheelSteerSpeed=6f;
	// Use this for initialization
	void Start () {
		wheelCollider = GetComponent<WheelCollider> ();
	}
	public void  UpdateWheel(float motor, float steer){
		wheelCollider.motorTorque = motor;
		wheelCollider.steerAngle = steer;
		Vector3 pos;
		Quaternion rot;
		wheelCollider.GetWorldPose (out pos, out rot);
		wheel.position = pos;
		wheel.rotation = rot;
	}
	public void UpdateWheel(float motor, float steer, float breaking, Transform axis, Transform arm, float wheelLevel, bool left){
		wheelCollider.motorTorque = motor;
		wheelCollider.steerAngle = Mathf.Lerp(wheelCollider.steerAngle, steer, Time.deltaTime*wheelSteerSpeed);
		wheelCollider.brakeTorque = breaking;
		Vector3 pos;
		Quaternion rot;
		wheelCollider.GetWorldPose (out pos, out rot);
		float num = (arm.parent.InverseTransformVector(arm.position - pos)).z - wheelLevel;
		num /= wheelCollider.radius;
		float alfa = Mathf.Rad2Deg * Mathf.Asin ( Mathf.Clamp(num,-1f,1f));
		//sssalfa =-alfa; 
		if (!left) {
			arm.localRotation = Quaternion.Euler (-alfa,0f,0f);
			axis.localRotation = Quaternion.Euler (alfa, 0f, 0f);
		} else {
			arm.localRotation = Quaternion.Euler (-alfa,0f,180f);
			axis.localRotation = Quaternion.Euler (-alfa, 0f, 180f);
		}
		//arm.localRotation = Quaternion.Euler (alfa,0f,0f);

		wheel.position = pos;//+Vector3.right*Mathf.Cos(alfa*Mathf.Deg2Rad);
		wheel.rotation = rot;
	}*/
}
