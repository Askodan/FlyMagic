using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class WeaponManager : MonoBehaviour {
	public GameObject[] weapons;
	public int choosenWeapon;
	GameObject activeWeapon;
	[HideInInspector]
	public List<Shooter> weapons_Shooter;

	void Awake(){
		weapons_Shooter = new List<Shooter> ();
		Transform[] manytransforms = GetComponentsInChildren<Transform> ();
		bool none = true;
		for (int i = 0; i < manytransforms.Length; i++) {
			if (manytransforms [i].name == "WeaponPoint") {
				activeWeapon = Instantiate (weapons [choosenWeapon], manytransforms[i]) as GameObject;
				activeWeapon.transform.localPosition = Vector3.zero;
				activeWeapon.transform.localRotation = Quaternion.identity;
				none = false;
				//break;
				weapons_Shooter.Add(activeWeapon.GetComponent<Shooter>());
			}
		}
		if (none) {
			Debug.LogWarning ("There were no WeaponPoint, so not a weapon was prepared!");
		}
	}
}
