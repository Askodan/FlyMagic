using UnityEngine;
using System.Collections;

public class tractorscripteasy : MonoBehaviour {
	public Rigidbody[] kolat;
	public Rigidbody[] kolap;
	public Rigidbody os;
	public float power;
	public float curve;
	// Use this for initialization
	JointMotor[] motors;
	void Start () {
		//motors = new JointMotor[kolap.Length];
		//for(int i = 0;i<kolap.Length; i++)
		//	motors[i] = kolap[i].GetComponent<HingeJoint>().motor;
	}
	
	// Update is called once per frame
	void Update () {
		//if(Input.GetAxis("Vertical")>0){
			for (int i = 0; i < kolat.Length; i++) {
				kolat [i].AddRelativeTorque (power * Input.GetAxis("Vertical"), 0f, 0f);
			}
		//}
		for(int i = 0;i<kolap.Length; i++){
			kolap [i].MoveRotation(os.rotation*Quaternion.Euler(0f, Input.GetAxis ("Horizontal") * curve, 0f));

			//kolap[i].transform.localRotation = Quaternion.Euler(0f, Input.GetAxis ("Horizontal") * curve, 0f);
			//motors[i].targetVelocity = Input.GetAxis ("Horizontal") * curve;
		}
	}
}
