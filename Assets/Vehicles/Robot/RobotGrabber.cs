using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class RobotGrabber : MonoBehaviour {
	public Transform tempTarget;
	public bool manual;
	public float time2move;
	public Transform grasper;
	public int pose;
	public Sensor pickableSensor;
	//Transform[] fingers;
	Transform holder3;
	Transform arm2;
	Transform holder2;
	Transform arm1;
	Transform holder1;
	Transform Base;
	public bool hasSmth;//czy chwytak coś trzyma
	public int maxItems;
	public int numItems;
	//public Transform[] items;//rzeczy na pace
	public List<Transform> itemsList;//rzeczy na pace trochę sensowniej
	Vector3[] poses;
	float holderPos;
	float prevHolderPos;
	//private bool inPose;
	private bool waitForPose;
	public bool taking;
	private int actualPose;
	void Start () {
		//items = new Transform[maxItems];
		poses = new Vector3[5];
		poses [0] = new Vector3 (280f, 290f, 330f);//taking pose
		poses [1] = new Vector3 (10f, 120f, 40f);//putting pose
		poses [2] = new Vector3 (30f, 220f, 300f);//resting pose
		poses [3] = new Vector3 (0f , 0f, 0f);//ik taking pose
		poses [4] = Vector3.zero; // ik putting pose
		//fingers = grasper.GetComponentsInChildren<Transform> ();
		holder3 = grasper.parent;
		arm2 = holder3.parent;
		holder2 = arm2.parent;
		arm1 = holder2.parent;
		holder1 = arm1.parent;
		Base = holder1.parent;

		arm1.localRotation = Quaternion.Euler (poses[2].x,0f,0f);
		arm2.localRotation = Quaternion.Euler (poses[2].y,0f,0f);
		grasper.localRotation = Quaternion.Euler (poses[2].z,0f,270f);
		actualPose = 2; 
		pose = 2;
		//inPose = true;
		l_2 = l*l;
		k_2 = k*k;
	}
	public IEnumerator take(GameObject newItem){
		if (!taking) {
			taking = true;
			if (numItems == maxItems) {
				print ("maksimum rzeczow osiągnięte");
			} else {
				pose = 3;
				Vector3 temp = Quaternion.Inverse (Base.parent.rotation) * (newItem.transform.position - arm1.transform.position);
				Vector3 angles = CalculateAngles (temp.x, temp.y, temp.z);
				if (angles.x == 1000f) {
					print ("cos sie zepsuło przy liczeniu IK");
				} else {
					poses [3] = new Vector3 (angles.x, angles.y, 360f);
					holderPos = angles.z;
				}
				waitForPose = true;
				StartCoroutine (move2Pose (pose));
				while (waitForPose) {
					yield return null;
				}
				pickableSensor.RemoveFromSensed (newItem.transform);
				newItem.transform.parent = grasper;
				newItem.GetComponent<Rigidbody> ().isKinematic = true;
				Collider col = newItem.GetComponent<Collider> ();
				if (col) {
					col.enabled = false;
				}
				Collider[] cols = newItem.GetComponent<Rigidbody> ().GetComponentsInChildren<Collider> ();
				for (int i = 0; i < cols.Length; i++) {
					cols [i].enabled = false;
				}
				hasSmth = true;

				pose = 1;
				holderPos = 0f;

				waitForPose = true;
				StartCoroutine (move2Pose (pose));
				while (waitForPose) {
					yield return null;
				}

				newItem.transform.SetParent (Base);
				numItems++;
				hasSmth = false;
				itemsList.Add (newItem.transform);
				//for (int i = 0; i < maxItems; i++) {
				//	if (items [i] == null) {
				//		items [i] = newItem.transform;
				//		break;
				//	}
				//}

				pose = 2;
				waitForPose = true;
				StartCoroutine (move2Pose (pose));
				while (waitForPose) {
					yield return null;
				}
			}
			taking = false;
		}
	}
	public IEnumerator put(GameObject putItem){
		if (!taking) {
			taking = true;
			if (!itemsList.Contains (putItem.transform)) {
				print ("nie ma takiej rzeczy");
			} else {
				if (pickableSensor.sensed == null) {
					print ("nic tu nie ma");
				} else {
					pose = 1;

					waitForPose = true;
					StartCoroutine (move2Pose (pose));
					while (waitForPose) {
						yield return null;
					}
					putItem.transform.parent = grasper;

					hasSmth = true;

					pose = 4;
					Vector3 point2put = pickableSensor.transform.position;
					RaycastHit hit;
					if (Physics.Raycast (pickableSensor.transform.position + Vector3.up * 4f, Vector3.down, out hit, 10f)) {
						point2put = hit.point + Vector3.up * 0.5f; 
					}
					Vector3 temp = Quaternion.Inverse (Base.parent.rotation) * (point2put - arm1.transform.position);
					Vector3 angles = CalculateAngles (temp.x, temp.y, temp.z);
					if (angles.x == 1000f) {
						print ("cos sie zjeblo przy liczeniu IK");
					} else {
						poses [4] = new Vector3 (angles.x, angles.y, 360f);
						holderPos = angles.z;
					}
					waitForPose = true;
					StartCoroutine (move2Pose (pose));
					while (waitForPose) {
						yield return null;
					}

					putItem.transform.SetParent (null);
					numItems--;
					itemsList.Remove (putItem.transform);
					hasSmth = false;
					//for (int i = 0; i < maxItems; i++) {
					//	if (items [i] == putItem.transform) {
					//		items [i] = null;
					//		break;
					//	}
					//}
					putItem.GetComponent<Rigidbody> ().isKinematic = false;
					Collider col = putItem.GetComponent<Collider> ();
					if (col) {
						col.enabled = true;
					}
					Collider[] cols = putItem.GetComponent<Rigidbody> ().GetComponentsInChildren<Collider> ();
					for (int i = 0; i < cols.Length; i++) {
						cols [i].enabled = true;
					}

					pose = 2;
					waitForPose = true;
					StartCoroutine (move2Pose (pose));
					while (waitForPose) {
						yield return null;
					}
				}
			}
			taking = false;
		}
	}
	IEnumerator move2Pose(int i){
		if (time2move == 0) {
			time2move = 0.1f;
		}
		//inPose = false;
		Vector3 steps = poses [i] - poses [actualPose];
		float holderSteps = holderPos - prevHolderPos;
		steps = new Vector3 (WhichWay2NotHit(steps.x, poses[actualPose].x, 180f), WhichWay2NotHit(steps.y, poses[actualPose].y, 180f),WhichWay2NotHit(steps.z, poses[actualPose].z, 180f));
		//print (steps);
		for(float j= 0f;j<time2move;j+=Time.deltaTime){
			arm1.localRotation = Quaternion.Euler (poses[actualPose].x+steps.x*j/time2move,0f,0f);
			arm2.localRotation = Quaternion.Euler (poses[actualPose].y+steps.y*j/time2move,0f,0f);
			grasper.localRotation = Quaternion.Euler (poses[actualPose].z+steps.z*j/time2move,0f,270f);
			holder1.localRotation = Quaternion.Euler (prevHolderPos+holderSteps*j/time2move, 90f, 90f);
			yield return null;
		}
		arm1.localRotation = Quaternion.Euler (poses[i].x,0f,0f);
		arm2.localRotation = Quaternion.Euler (poses[i].y,0f,0f);
		grasper.localRotation = Quaternion.Euler (poses[i].z,0f,270f);
		holder1.localRotation = Quaternion.Euler (holderPos, 90f, 90f);
		prevHolderPos = holderPos;
		actualPose = i;
		//inPose =true;
		waitForPose = false;
	}
	float WhichWay2NotHit(float angle2move, float angleActual, float angleForbidden){
		//to normalizuje kąty do <-180, 180>
		/*while (Mathf.Abs(angle2move) >= 180f) {
			angle2move = angle2move - Mathf.Sign(angle2move)*360f;
		}
		while (Mathf.Abs(angleActual) >= 180f) {
			angleActual = angleActual - Mathf.Sign(angleActual)*360f;
		}*/
		//to normalizuje do <0, 360>
		angle2move = Mathf.Repeat(angle2move, 360f);
		angleActual = Mathf.Repeat (angleActual, 360f);
		float angleAfter = angleActual + angle2move;

		bool Over = angleActual > angleForbidden;
		bool stillOver = angleAfter > angleForbidden&&angleAfter < angleForbidden+360f;
		//print (angleActual + " " + angle2move);
		if (Over != stillOver) {
			if (Over&&stillOver) {
				angle2move += 360f;
			} else {
				angle2move -= 360f;
			}
			//print (angle2move);
		}
		return angle2move;
	}
	void Update () {
		if (manual){
			if (Input.GetKeyDown (KeyCode.Space) && !taking) {
				if(pickableSensor.sensed.Count>0)
					StartCoroutine (take (pickableSensor.sensed[0].gameObject));
				else
				if(itemsList.Count>0){
					StartCoroutine (put (itemsList[0].gameObject));
				}else{
					print ("nic tu nie ma");
				}
			}
		}
	}
	float l = 2.1f;//do zmiany przy zmianie skali
	float k = 3.4f;//drugi czlon do zabawy
	float l_2;
	float k_2;
	Vector3 CalculateAngles(float vert, float hory, float spat){
		float a, b, c;
		c = Mathf.Atan2 (spat, vert);
		vert = new Vector2(vert, spat).magnitude;

		//only l
		//float sqrSum=vert*vert+hory*hory, p = Mathf.Sqrt(-(sqrSum)*(sqrSum-4*l*l));
		//a = 2f*Mathf.Atan((-2f*vert*l+p)/(sqrSum+2f*hory*l));
		//b = -2f*Mathf.Atan(p/sqrSum);

		//l and k
		float d_2 = vert*vert, h_2 = hory*hory, sqrSum=d_2+h_2-k_2, p = Mathf.Sqrt(-d_2*d_2-h_2*h_2 -2*d_2*h_2 +2*d_2*k_2+2*d_2*l_2 +2*h_2*k_2+2*h_2*l_2 -k_2*k_2+2*k_2*l_2-l_2*l_2);

		a = 2f*Mathf.Atan((p-2f*vert*l)/(sqrSum + 2f*hory*l+l_2));

		b = -2f*Mathf.Atan(p/(sqrSum + 2*k*l-l_2));
		if (!float.IsNaN(a)) {
			return new Vector3 (a * Mathf.Rad2Deg+360f, b * Mathf.Rad2Deg+360f, c * Mathf.Rad2Deg-90f);
		} else {
			return Vector3.one * 1000f;
		}
	}
	void SetAngles(Vector3 angles){
		if (angles.x < 1000f) {
			arm1.localRotation = Quaternion.Euler (angles.x, 0f, 0f);
			arm2.localRotation = Quaternion.Euler (angles.y, 0f, 0f);
			holder1.localRotation = Quaternion.Euler (angles.z, 90f, 90f);
			grasper.localRotation = Quaternion.Euler (0f, 0f, -90f);
		}
	}
}
