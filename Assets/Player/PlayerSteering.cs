using UnityEngine;
using System.Collections;

public class PlayerSteering : MonoBehaviour {
	public LayerMask mask;
	PlayerSpecs specs;
	[HideInInspector] public SteeringDrone steeringDrone;

	[HideInInspector] public RobotLegsWheelsSteering robotWheelsSteering;
	[HideInInspector] public RobotGrabberPlayer robotGrabber;

	[HideInInspector] public tractorscripteasy tractorScriptEasy;
	void Awake(){
		specs = GetComponent<PlayerSpecs> ();
	}
		
	void Update () {
		if (specs.actualVehicle != specs.mainVehicle) {
			if(Input.GetButtonDown("ChangeVehicle")){
				specs.OutofVehicle ();
				specs.IntoVehicle (specs.mainVehicle);
			}
		} else {
			RaycastHit hit;
			if (Physics.Raycast (transform.position, transform.forward, out hit, 10f, mask)) {

				if (Input.GetButtonDown ("ChangeVehicle")) {
					specs.OutofVehicle ();
					specs.IntoVehicle (hit.collider.gameObject.transform.parent.GetComponent<VehicleTypeDefiner> ());
				}
			}
		}
		if (specs.actualVehicle != null) {
			switch (specs.actualVehicle.vehicleType) {
			case VehicleType.drone:
				steeringDrone.Steer (Input.GetAxis ("Thrust"), Input.GetAxis ("Pitch"), Input.GetAxis ("Roll"), Input.GetAxis ("Yaw"), Input.GetAxis ("PrototypeTurbo"),
					Input.GetButtonDown ("Lights"), Input.GetButtonDown ("Turn off motors"), Input.GetButtonDown ("Stabilize"), Input.GetButtonDown ("Keep altitude"), Input.GetButtonDown ("Self leveling"));
				break;
			case VehicleType.tractor:
				tractorScriptEasy.Steer (Input.GetButtonDown ("Break"), Input.GetButtonUp ("Break"), Input.GetAxis ("Vertical"), Input.GetAxis ("Horizontal"));
				break;
			case VehicleType.robot:
				robotWheelsSteering.Steer (Input.GetAxis ("Vertical"), Input.GetAxis ("Horizontal"), Input.GetAxis ("Spread(robot)"), Input.GetAxis ("Height(robot)"), Input.GetButtonDown ("Turning in place"));
				robotGrabber.Steer (Input.GetButtonUp("Item outside"), Input.GetButtonUp ("Item inside"), Input.GetButton("Item outside"), Input.GetButton("Item inside"), Input.GetButtonDown ("Next Item"), Input.GetButtonDown ("Previous Item"), Input.GetButtonDown ("Hands Up"));
				break;
			}
		}
		if (specs.weaponManager) {
			foreach(Shooter weapon in specs.weaponManager.weapons_Shooter){
				weapon.shoot = Input.GetButton ("Fire");
			}
		}
	}
}
