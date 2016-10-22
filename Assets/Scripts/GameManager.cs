using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class GameManager : Singleton<GameManager> {
	public GameObject[] LooseVehicles;
	[HideInInspector]
	new public Camera camera;
	//[HideInInspector]
	public List<Transform> dragonsInDangerZone;
	void Awake(){
		dragonsInDangerZone = new List<Transform>();
		camera = Camera.main;
	}
	public void DragonFlewIntoDangerZone(Transform dragon){
		dragonsInDangerZone.Add (dragon);
	}
	public void DragonFlewOutOfDangerZone(Transform dragon){
		dragonsInDangerZone.Remove (dragon);
	}
	public Transform GetClosestDragonFromDangerZone(Transform closest2, out float dist){
		Transform closest = null;
		float sqrdist = Mathf.Infinity;
		foreach (Transform dragon in dragonsInDangerZone) {
			float dragonsqrdist = (dragon.position - closest2.position).sqrMagnitude;
			if ( dragonsqrdist < sqrdist) {
				closest = dragon;
				sqrdist = dragonsqrdist;
			}
		}
		dist = sqrdist;
		return closest;
	}
}
