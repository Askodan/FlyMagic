using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class Sensor : MonoBehaviour {
	//public GameObject[] sensed;
	public List<Transform> sensed;
	public List<Transform> puttingPoints;
	void Start(){
		sensed = new List<Transform> ();
		puttingPoints = new List<Transform> ();
	}
	void OnTriggerEnter(Collider col){
		if (col.attachedRigidbody) {
			if (col.attachedRigidbody.tag == "Pickable") {
				sensed.Add (col.attachedRigidbody.gameObject.transform);
				col.attachedRigidbody.gameObject.GetComponent<ManageChoosen> ().SetState (SelectableObjectStates.Available);
			}else
			if (col.tag == "Puttable") {
				puttingPoints.Add (col.gameObject.transform);
				col.gameObject.GetComponent<ManageChoosen> ().SetState (SelectableObjectStates.Available);
			}
		}
	}
	void OnTriggerExit(Collider col){
		if (col.attachedRigidbody) {
			if (col.attachedRigidbody.tag == "Pickable") {
				RemoveFromSensed (col.attachedRigidbody.gameObject.transform);
			}else
			if (col.tag == "Puttable") {
				puttingPoints.Remove (col.gameObject.transform);
				col.gameObject.GetComponent<ManageChoosen> ().SetState (SelectableObjectStates.NotAvailable);
			}
		}
	}
	public void RemoveFromSensed(Transform item){
		sensed.Remove(item);
		item.GetComponent<ManageChoosen> ().SetState (SelectableObjectStates.NotAvailable);
	}
}
