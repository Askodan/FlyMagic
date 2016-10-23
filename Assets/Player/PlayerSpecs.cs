using UnityEngine;
using System.Collections;
[RequireComponent(typeof(PlayerSteering))]

public class PlayerSpecs : MonoBehaviour {
	public VehicleTypeDefiner mainVehicle;
	[HideInInspector] public VehicleTypeDefiner actualVehicle;
	[HideInInspector] public WeaponManager weaponManager;
	new Transform camera;
	PlayerSteering playerSteering;
	void Start(){
		playerSteering = GetComponent<PlayerSteering> ();
		camera = GameManager.Instance.camera.transform;
		IntoVehicle (mainVehicle);
	}

	public void IntoVehicle(VehicleTypeDefiner newVehicle){
		ParentAndZero (newVehicle.transform, transform);
		UnparentRigidbodies UNPR = newVehicle.GetComponent<UnparentRigidbodies> ();
		Transform[] manytransforms;
		if (UNPR != null) {
			manytransforms = UNPR.originalChildren;
		} else {
			manytransforms = newVehicle.GetComponentsInChildren<Transform> ();
		}
		Transform cameraPoint = null;
		Transform Driver = null;
		for (int i = 0; i < manytransforms.Length; i++) {
			if (manytransforms [i].name == "CameraPoint") {
				cameraPoint = manytransforms [i];
			}
			if (manytransforms [i].name == "Driver") {
				Driver = manytransforms [i];
			}
		}
		if (cameraPoint == null) {
			Debug.LogError ("There's no \"CameraPoint\" in " + newVehicle.name);
			return;
		}
		weaponManager = newVehicle.GetComponent<WeaponManager> ();
		if (weaponManager == null) {
			Debug.Log ("There's no \"WeaponManager\" in " + newVehicle.name);
		}
		camera = GameManager.Instance.camera.transform;

		switch (newVehicle.vehicleType) {
		case VehicleType.drone:
			ParentAndZero (cameraPoint, camera);
			playerSteering.steeringDrone = newVehicle.GetComponent<SteeringDrone> ();
			playerSteering.steeringDrone.enabled = true;
			break;
		case VehicleType.tractor:
			SimpleCamera SC = camera.GetComponent<SimpleCamera> ();
			SC.enabled = true;
			SC.target = cameraPoint;

			if (Driver) {
				mainVehicle.GetComponent<Rigidbody> ().isKinematic = true;
				ParentAndZero (Driver, mainVehicle.transform);
			}
			foreach (Shooter weapon in weaponManager.weapons_Shooter) {
				weapon.aimer.GetComponent<PlayerAim> ().enabled = true;
			}

			playerSteering.tractorScriptEasy = newVehicle.GetComponent<tractorscripteasy> ();
			playerSteering.tractorScriptEasy.enabled = true;
			break;
		case VehicleType.robot:
			SimpleSmooth SS = camera.GetComponent<SimpleSmooth> ();
			SS.enabled = true;
			SS.target = cameraPoint;

			ParentAndZero (camera, mainVehicle.transform);
			mainVehicle.GetComponent<Rigidbody> ().isKinematic = true;

			playerSteering.robotWheelsSteering = newVehicle.GetComponent<RobotLegsWheelsSteering> ();
			playerSteering.robotWheelsSteering.enabled = true;

			playerSteering.robotGrabber = newVehicle.GetComponent<RobotGrabberPlayer> ();
			playerSteering.robotGrabber.enabled = true;
			break;
		}
		actualVehicle = newVehicle;
	}

	public void OutofVehicle(){
		switch (actualVehicle.vehicleType) {
		case VehicleType.drone:
			camera.transform.parent = null;
			actualVehicle.GetComponent<SteeringDrone> ().enabled = false;
			break;
		case VehicleType.tractor:
			SimpleCamera SC = camera.GetComponent<SimpleCamera> ();
			SC.enabled = false;

			mainVehicle.transform.parent = null;
			mainVehicle.GetComponent<Rigidbody> ().isKinematic = false;


			foreach (Shooter weapon in weaponManager.weapons_Shooter) {
				weapon.aimer.GetComponent<PlayerAim> ().enabled = false;
			}

			actualVehicle.GetComponent<tractorscripteasy> ().enabled = false;
			break;
		case VehicleType.robot:
			SimpleSmooth SS = camera.GetComponent<SimpleSmooth> ();
			SS.enabled = false;

			mainVehicle.transform.parent = null;
			mainVehicle.GetComponent<Rigidbody> ().isKinematic = false;
			actualVehicle.GetComponent<RobotLegsWheelsSteering> ().enabled = false;
			actualVehicle.GetComponent<RobotGrabberPlayer> ().enabled = false;
			break;
		}
	}

	static public void ParentAndZero(Transform newParent, Transform newChild){
		newChild.parent = newParent;
		newChild.localPosition = Vector3.zero;
		newChild.localRotation = Quaternion.identity;
	}
}
