using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class RobotGrabberPlayer : MonoBehaviour {
	//public Transform aimTarget;
	public float time2move;
	public Transform grasper;
	public Transform graspPoint;
	public int pose;
	public Sensor pickableSensor;
	//Transform[] fingers;
	Transform holder3;
	Transform arm2;
	Transform holder2;
	Transform arm1;
	Transform holder1;
	Transform Base;
	Transform Smth;//co chwytak trzyma
	public int maxItems;
	List<Transform> itemsList;//rzeczy na pace trochę sensowniej
	Vector3[] poses;
	bool waitForPose;
	bool busy;
	private int choosenItemInside;
	private int choosenItemOutside;
	private int choosenPuttingPoint;
	void Awake () {
		itemsList = new List<Transform> ();
		//items = new Transform[maxItems];
		poses = new Vector3[5];
		poses [0] = new Vector3 (0f, 3.0f, 0.0f);//midair pose
		poses [1] = new Vector3 (0f, 0.2f, -0.8f);//putting pose to temp
		poses [2] = new Vector3 (0f, 0.5f, 0.6f);//resting pose
		poses [3] = new Vector3 (0f, 0f, 0f);//ik pose
		poses [4] = new Vector3 (0f, 1.3f, 1.6f);//putting pose

		holder3 = grasper.parent;
		arm2 = holder3.parent;
		holder2 = arm2.parent;
		arm1 = holder2.parent;
		holder1 = arm1.parent;
		Base = holder1.parent;

		arm1.localRotation = Quaternion.Euler (poses[2].x,0f,0f);
		arm2.localRotation = Quaternion.Euler (poses[2].y,0f,0f);
		grasper.localRotation = Quaternion.Euler (poses[2].z,0f,270f);
		l_2 = l*l;
		k_2 = k*k;
		StartCoroutine (move2Pose (2));
	}
	void SetCollidersInItem(Transform item, bool active){
		Rigidbody newbody = item.GetComponent<Rigidbody> ();
		if (newbody != null) {
			newbody.isKinematic = !active;

			Collider[] cols = item.GetComponentsInChildren<Collider> ();
			for (int i = 0; i < cols.Length; i++) {
				cols [i].enabled = active;
			}
		} else {
			Collider col = item.GetComponent<Collider> ();
			if (col) {
				col.enabled = active;
			}
		}
	}
	public IEnumerator take(Transform newItem, bool outside){
		if (!busy) {
			busy = true;
			if (outside) {	
				Pose3FromWorldPosition (newItem.position);

				StartCoroutine (move2Pose (3));
				while (waitForPose) {
					yield return null;
				}
				SetCollidersInItem (newItem, false);
				newItem.GetComponent<ManageChoosen> ().SetState (SelectableObjectStates.NotAvailable);
				pickableSensor.RemoveFromSensed (newItem);
			} else {
				StartCoroutine (move2Pose (1));
				while (waitForPose) {
					yield return null;
				}
				newItem.GetComponent<ManageChoosen> ().SetState (SelectableObjectStates.NotAvailable);
				itemsList.Remove (newItem);
			}
			newItem.parent = graspPoint;
			newItem.localPosition = Vector3.zero;

			Smth = newItem;
			StartCoroutine (move2Pose (2));
			while (waitForPose) {
				yield return null;
			}
			busy = false;
		}
	}

	void Pose3FromWorldPosition (Vector3 position) {
		Vector3 temp = Quaternion.Inverse (Base.parent.rotation) * (position - arm1.transform.position);
		poses [3] = temp;
	}
	int checkIndex(int minIndex, int maxIndex, int index){
		if (index >= minIndex && index < maxIndex) {
			return index;
		} else {
			if (index >= minIndex) {
				return 0;
			} else {
				return maxIndex - 1;
			}
		}
	}
	public IEnumerator put(Transform putItem, bool outside){
		if (!busy) {
			busy = true;
			if (outside) {
				if (pickableSensor.puttingPoints.Count > 0) {
					choosenPuttingPoint = checkIndex (0, pickableSensor.puttingPoints.Count, choosenPuttingPoint);

					Pose3FromWorldPosition (pickableSensor.puttingPoints [choosenPuttingPoint].transform.position);

					StartCoroutine (move2Pose (3));
					while (waitForPose) {
						yield return null;
					}
					putItem.transform.position = pickableSensor.puttingPoints [choosenPuttingPoint].transform.position;

				} else {
					StartCoroutine (move2Pose (4));
					while (waitForPose) {
						yield return null;
					}
				}

				SetCollidersInItem (putItem, true);
				putItem.parent = null;
			} else {
				StartCoroutine (move2Pose (1));
				while (waitForPose) {
					yield return null;
				}
				putItem.SetParent (Base);

				itemsList.Add (putItem);
			}
			StartCoroutine (move2Pose (2));
			while (waitForPose) {
				yield return null;
			}

			Smth = null;

			busy = false;
		}
	}
	IEnumerator move2Pose(int i){
		waitForPose = true;
		if (time2move == 0) {
			time2move = 1f;
		}
		Vector3 angles = CalculateAngles (poses[i].x, poses[i].y, poses[i].z);

		Quaternion arm1Rot = arm1.localRotation;
		Quaternion arm2Rot = arm2.localRotation;
		Quaternion grasperRot = grasper.localRotation;
		Quaternion holder1Rot = holder1.localRotation;

		Quaternion arm1RotTar = Quaternion.Euler (angles.x, 0f, 0f);
		Quaternion arm2RotTar = Quaternion.Euler (angles.y, 0f, 0f);
		Quaternion grasperRotTar = Quaternion.Euler (360f, 0f, 270f);
		Quaternion holder1RotTar = Quaternion.Euler (angles.z, 90f, 90f);

		for(float j = 0f;j<time2move;j+=Time.deltaTime){
			float progress = j / time2move;
			arm1.localRotation = Quaternion.Slerp(arm1Rot, arm1RotTar, progress);
			arm2.localRotation = Quaternion.Slerp(arm2Rot, arm2RotTar, progress);
			grasper.localRotation = Quaternion.Slerp(grasperRot, grasperRotTar, progress);
			holder1.localRotation = Quaternion.Slerp(holder1Rot, holder1RotTar, progress);
			yield return null;
		}
		arm1.localRotation = arm1RotTar;
		arm2.localRotation = arm2RotTar;
		grasper.localRotation = grasperRotTar;
		holder1.localRotation = holder1RotTar;

		waitForPose = false;
	}
	public void Steer(bool butUp_Outside, bool butUp_Inside, bool but_Outside, bool but_Inside, bool butDown_Next, bool butDown_Previous, bool butDown_HandsUp){
		if (butUp_Outside){
			if (Smth) {	
				StartCoroutine (put (Smth, true));
			} else {
				if (pickableSensor.sensed.Count > 0) {
					choosenItemOutside = checkIndex (0, pickableSensor.sensed.Count, choosenItemOutside);
					StartCoroutine (take (pickableSensor.sensed [choosenItemOutside], true));
				}
			}
		}
		if (butUp_Inside) {
			if (Smth) {	
				if (itemsList.Count == maxItems) {
					print ("maksimum rzeczow osiągnięte");
				} else {
					StartCoroutine (put (Smth, false));
				}
			} else {
				if (itemsList.Count > 0) {
					choosenItemInside = checkIndex (0, itemsList.Count, choosenItemInside);
					StartCoroutine (take (itemsList [0], false));
				}
			}
		}
		if (butDown_Next) {
			if (Smth&&pickableSensor.puttingPoints.Count>0) {
				pickableSensor.puttingPoints [choosenPuttingPoint].GetComponent<ManageChoosen> ().SetState (SelectableObjectStates.Available);
				choosenPuttingPoint++;
				choosenPuttingPoint = checkIndex (0, pickableSensor.puttingPoints.Count, choosenPuttingPoint);
				pickableSensor.puttingPoints [choosenPuttingPoint].GetComponent<ManageChoosen> ().SetState (SelectableObjectStates.Choosen);
			} else {
				if(but_Outside&&pickableSensor.sensed.Count>0){
					pickableSensor.sensed [choosenItemOutside].GetComponent<ManageChoosen> ().SetState (SelectableObjectStates.Available);
					choosenItemOutside++;
					choosenItemOutside = checkIndex (0, pickableSensor.sensed.Count, choosenItemOutside);
					pickableSensor.sensed [choosenItemOutside].GetComponent<ManageChoosen> ().SetState (SelectableObjectStates.Choosen);
				}else
				if(but_Inside&&itemsList.Count>0){
					itemsList[choosenItemInside].GetComponent<ManageChoosen> ().SetState (SelectableObjectStates.NotAvailable);
					choosenItemInside++;
					choosenItemInside = checkIndex (0, itemsList.Count, choosenItemInside);
					itemsList[choosenItemInside].GetComponent<ManageChoosen> ().SetState (SelectableObjectStates.Choosen);
				}
			}
		}
		if (butDown_Previous) {
			if (Smth&&pickableSensor.puttingPoints.Count>0) {
				pickableSensor.puttingPoints [choosenPuttingPoint].GetComponent<ManageChoosen> ().SetState (SelectableObjectStates.Available);
				choosenPuttingPoint--;
				choosenPuttingPoint = checkIndex (0, pickableSensor.puttingPoints.Count, choosenPuttingPoint);
				pickableSensor.puttingPoints [choosenPuttingPoint].GetComponent<ManageChoosen> ().SetState (SelectableObjectStates.Choosen);
			} else {
				if(but_Outside&&pickableSensor.sensed.Count>0){
					pickableSensor.sensed [choosenItemOutside].GetComponent<ManageChoosen> ().SetState (SelectableObjectStates.Available);
					choosenItemOutside--;
					choosenItemOutside = checkIndex (0, pickableSensor.sensed.Count, choosenItemOutside);
					pickableSensor.sensed [choosenItemOutside].GetComponent<ManageChoosen> ().SetState (SelectableObjectStates.Choosen);
				}else
				if(but_Inside&&itemsList.Count>0){
					itemsList[choosenItemInside].GetComponent<ManageChoosen> ().SetState (SelectableObjectStates.NotAvailable);
					choosenItemInside--;
					choosenItemInside = checkIndex (0, itemsList.Count, choosenItemInside);
					itemsList[choosenItemInside].GetComponent<ManageChoosen> ().SetState (SelectableObjectStates.Choosen);
				}
			}
		}
		if (butDown_HandsUp) {
			StartCoroutine (move2Pose (0));
		}
	}
	void Update () {

		//IK setup
		/*
		if (pickableSensor.sensed.Count > 0) {
			Vector3 temp = Quaternion.Inverse (Base.parent.rotation) * (pickableSensor.sensed [0].position - arm1.transform.position);
			//Vector3 temp = poses[2];
			Vector3 angles = CalculateAngles (temp.x, temp.y, temp.z);
			if (angles.x == 1000f) {
				print ("cos sie zepsuło przy liczeniu IK");
			} else {
				arm1.localRotation = Quaternion.Euler (angles.x, 0f, 0f);
				arm2.localRotation = Quaternion.Euler (angles.y, 0f, 0f);
				grasper.localRotation = Quaternion.Euler (360, 0f, 270f);
				holder1.localRotation = Quaternion.Euler (angles.z, 90f, 90f);
			}
		}*/
	}
	float l = 1.05f;//do zmiany przy zmianie skali
	float k = 1.3f;//drugi czlon do zabawy
	float l_2;
	float k_2;
	Vector3 CalculateAngles(float vert, float hory, float spat){
		
		float a, b, c;
		c = Mathf.Atan2 (spat, vert);
		vert = new Vector2(vert, spat).magnitude;

		float d_2 = vert * vert, h_2 = hory * hory, lk_2 =(l_2 + k_2);
		if ( lk_2 < d_2+h_2+spat*spat) {
			float alfa_2 =lk_2/(d_2 + h_2+spat*spat)-0.005f;
			float alfa = Mathf.Sqrt (alfa_2);
			d_2 *= alfa_2;
			h_2 *= alfa_2;
			vert *= alfa;
			hory *= alfa;
			//print ((d_2 + h_2).ToString () + " " + lk_2); 
		}
		float sqrSum=d_2+h_2-k_2, p = Mathf.Sqrt(-d_2*d_2-h_2*h_2 -2*d_2*h_2 +2*d_2*k_2+2*d_2*l_2 +2*h_2*k_2+2*h_2*l_2 -k_2*k_2+2*k_2*l_2-l_2*l_2);

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