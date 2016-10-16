using UnityEngine;
using System.Collections;

public class WeaponManager : MonoBehaviour {
	Transform WeaponPoint;
	public GameObject[] weapons;
	public int choosenWeapon;
	GameObject activeWeapon;
	void Awake(){
		//WeaponPoint = transform.Find ("WeaponPoint");
		Transform[] manytransforms = GetComponentsInChildren<Transform> ();
		for (int i = 0; i < manytransforms.Length; i++) {
			if (manytransforms [i].name == "WeaponPoint") {
				WeaponPoint = manytransforms [i];
				break;
			}
		}
	}

	// Use this for initialization
	void Start () {
		activeWeapon = Instantiate (weapons [choosenWeapon], WeaponPoint) as GameObject;
		activeWeapon.transform.localPosition = Vector3.zero;
		activeWeapon.transform.localRotation = Quaternion.identity;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
