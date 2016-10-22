using UnityEngine;
using System.Collections;

public class SteeringDroneAlternative : MonoBehaviour {

	//śmigła MUSZĄ nazywać się "Propeller", a środek masy "Center of mass"
	[HideInInspector]
	public Transform[] propellers;
	public GameObject SmudgePrefab;
	public DroneType type;
	//ustawienia elektroniki(wzmocnień)
	public float pitchMax;
	public PIDController pitchPID;//A = 1, P = 0.01, I = 0.001, Imax = 10, D = 0.01
	public float rollMax;
	public PIDController rollPID; //A = 1, P = 0.01, I = 0.001, Imax = 10, D = 0.01
	public float yawSpeed;
	public PIDController yawPID;  //A = 1, P = 0.02, I = 0,     Imax = 0, D = 0.02
	private float thrust;
	private float pitch;
	private float roll;
	private float yaw;

	public float forceFactor = 3.6f;
	[Tooltip ("To tylko dla prototypu")]
	public float wingsSpeed=36f;
	[Tooltip ("To tylko dla prototypu")]
	public float turboSpeed=50f;
	bool turboMode;
	private float maxVel = 18000f;

	private new Rigidbody rigidbody;
	private float zeroThrust;
	private SmudgeRotation[] smudgers;

	public bool keepAltitude{ get; set; }
	public bool motorsOn{ get; set; }

	[HideInInspector]public float[] velPropellers;
	[HideInInspector]public GameObject flashLight;
	private Vector3 motorsSpacings;
	private int number;
	private Coroutine coroutine;
	private Vector3 Rot;
	// Use this for initialization
	void Start () {
		switch (type) {
		case DroneType.quadrocopter:
			break;
		case DroneType.heksacopter:
			break;
		case DroneType.octacopter:
			break;
		case DroneType.prototype:
			number = 4;
			break;
		}
		//inicjalizacja tablic
		switch (type) {
		case DroneType.quadrocopter:
			propellers = new Transform[4];
			break;
		case DroneType.heksacopter:
			propellers = new Transform[6];
			break;
		case DroneType.octacopter:
			propellers = new Transform[8];
			break;
		case DroneType.prototype:
			propellers = new Transform[8];
			break;
		}
		velPropellers = new float[propellers.Length];
		//poszukiwanie syfu
		rigidbody = this.GetComponent<Rigidbody> ();
		rigidbody.centerOfMass = transform.Find ("Center of mass").localPosition;

		Transform[] children = transform.GetComponentsInChildren<Transform> ();
		int j = 0;
		for (int i = 0; i < children.Length; i++) {
			if (children [i].name == "Propeller") {
				propellers [j] = children [i];
				j++;
			} else {
				if (children [i].name == "FlashLight") {
					flashLight = children [i].gameObject;
					if (flashLight.activeSelf) {
						flashLight.SetActive (false);
					}
				}
			}
			switch (type) { 
			case DroneType.quadrocopter:
				if (j == 4)
					break;
				break;
			case DroneType.heksacopter:
				if (j == 6)
					break;
				break;
			case DroneType.octacopter:
				if (j == 8)
					break;
				break;
			case DroneType.prototype:
				if (j == 8)
					break;
				break;
			}
		}

		//obliczanie pierdół
		rigidbody.maxAngularVelocity = 100;

		switch (type) {
		case DroneType.quadrocopter:
			SetupQuadro ();
			motorsSpacings.x = Vector3.Distance (propellers [0].position, propellers [2].position);
			motorsSpacings.y = Vector3.Distance (propellers [0].position, propellers [1].position);
			motorsSpacings.z = Vector3.Distance (propellers [0].position, transform.position);
			zeroThrust = -Physics.gravity.y / propellers.Length * rigidbody.mass / forceFactor;

			break;
		case DroneType.heksacopter:
			zeroThrust = -Physics.gravity.y / propellers.Length * rigidbody.mass / forceFactor;

			break;
		case DroneType.octacopter:
			zeroThrust = -Physics.gravity.y / propellers.Length * rigidbody.mass / forceFactor;

			break;
		case DroneType.prototype:
			SetupPrototype ();
			motorsSpacings.z = Vector3.Distance (propellers [4].position, transform.position);
			zeroThrust = -Physics.gravity.y / 4f * rigidbody.mass / forceFactor;

			break;
		}
	}
	
	// Update is called once per frame
	void Update () {
		Steer ();
		RotatePropellers (propellers, velPropellers, maxVel);
	}
	public void Steer(){
		if (motorsOn) {
			thrust = Input.GetAxis ("Thrust");
			pitch = Input.GetAxis ("Pitch");
			roll = Input.GetAxis ("Roll");
			yaw = Input.GetAxis ("Yaw");

			if (keepAltitude) {
				thrust = Mathf.Clamp (thrust + zeroThrust, -1, 1);
			}
			for (int i = 0; i < propellers.Length-number; i++) {
				velPropellers [i] = thrust;
			}
			velPropellers [0] -= pitch;
			velPropellers [1] += pitch;
			velPropellers [2] -= pitch;
			velPropellers [3] += pitch;
			velPropellers [0] -= roll;
			velPropellers [1] -= roll;
			velPropellers [2] += roll;
			velPropellers [3] += roll;

			switch (type) {
			case DroneType.quadrocopter:

				break;
			case DroneType.heksacopter:
				break;
			case DroneType.octacopter:
				break;
			case DroneType.prototype:
				velPropellers [4] = -Input.GetAxis ("PrototypeTurbo")*turboSpeed;
				velPropellers [5] = Input.GetAxis ("PrototypeTurbo")*turboSpeed;
				velPropellers [6] = -Input.GetAxis ("PrototypeTurbo")*turboSpeed;
				velPropellers [7] = Input.GetAxis ("PrototypeTurbo")*turboSpeed;
				break;
			}

		}
		if (Input.GetButtonDown ("Flashlight")) {
			if (flashLight.activeSelf) {
				flashLight.SetActive (false);
			} else {
				flashLight.SetActive (true);
			}
		}
		if (Input.GetButtonDown ("Turn off motors")) {
			if (motorsOn) {
				motorsOn = false;
				for (int i = 0; i < propellers.Length; i++) {
					velPropellers [i] = 0f;
				}
			}
			else {
				motorsOn = true;
			}
		}
		if (Input.GetButtonDown ("Keep altitude")) {
			if (keepAltitude)
				keepAltitude = false;
			else
				keepAltitude = true;
		}
		if (Input.GetButtonDown ("TurboMode")&&type==DroneType.prototype) {
			if (coroutine != null)
				StopCoroutine (coroutine);
			coroutine = StartCoroutine (MoveWings ());
			turboMode = !turboMode;
		}
	}
	void FixedUpdate(){
		ApplyPhysics ();
	}
//	Vector3 podglad;
//	Vector3 regulacja;
	public void ApplyPhysics(){
		if (motorsOn) {
			for (int i = 0; i < propellers.Length; i++) {
				Vector3 thrustVec = new Vector3 (0, 0, thrust * forceFactor);

				rigidbody.AddForceAtPosition (propellers [i].rotation * thrustVec, propellers [i].position);
			}
			//pochylenie w przód
			float actualPitch = Vector3.Angle (transform.forward, Vector3.up) - 90f;
			float pitchChange = pitchPID.Regulate (pitchMax*pitch - actualPitch);
			//pochylenie w bok
			float actualRoll = Vector3.Angle (transform.right, Vector3.up) - 90f;
			float rollChange = rollPID.Regulate (actualRoll - rollMax*roll);
			//uwzględnij
			rigidbody.AddRelativeTorque (pitchChange, 0f, rollChange);
			//obrót
			float actualYaw = rigidbody.angularVelocity.y;
			float yawChange = yawPID.Regulate (yaw*yawSpeed - actualYaw);
			//uwzględnij
			rigidbody.AddTorque (Vector3.up*yawChange);

//			podglad = new Vector3 (actualPitch, actualYaw, actualRoll);
//			regulacja = new Vector3 (pitchChange, yawChange, rollChange);
		}
	}
	void SetupQuadro ()
	{
		//order x+z+, x+z-, x-z+, x-z-
		Transform[] screws = new Transform[propellers.Length];
		if (SmudgePrefab) {
			smudgers = new SmudgeRotation[propellers.Length];
		}
		for (int i = 0; i < propellers.Length; i++) {
			if (transform.InverseTransformPoint (propellers [i].position).x > 0) {
				if (transform.InverseTransformPoint (propellers [i].position).z > 0) {
					screws [0] = propellers [i];
					AddSmudger (screws [0], true, 0);
				} else {
					screws [1] = propellers [i];
					AddSmudger (screws [1], false, 1);
				}
			} else {
				if (transform.InverseTransformPoint (propellers [i].position).z > 0) {
					screws [2] = propellers [i];
					AddSmudger (screws [2], false, 2);
				} else {
					screws [3] = propellers [i];
					AddSmudger (screws [3], true, 3);
				}
			}
		}
		propellers = screws;
	}
	void AddSmudger(Transform prop, bool left, int i){
		if (SmudgePrefab) {
			GameObject smudger = Instantiate (SmudgePrefab);
			smudger.transform.SetParent (prop);
			smudger.transform.localPosition = Vector3.zero;
			if (left)
				smudger.transform.localRotation = Quaternion.Euler(0f, 180f, 80f);
			else
				smudger.transform.localRotation = Quaternion.identity;
			smudgers [i] = smudger.GetComponent<SmudgeRotation>();
		}
	}
	void SetupPrototype ()
	{
		//order (x+z+, x+z-, x-z+, x-z-), close +4 
		Transform[] screws = new Transform[propellers.Length];
		float[] distances = new float[propellers.Length], dists = new float[2];
		bool[] dist1 = new bool[propellers.Length];
		distances [0] = Vector3.Distance (propellers [0].position, transform.position);
		dist1 [0] = true;
		for (int i = 1; i < propellers.Length; i++) {
			float distance = Vector3.Distance (propellers [i].position, transform.position);
			distances [i] = distance;
			if (distance > distances [0] * 0.75f && distance < distances [0] * 1.5f) {
				dist1 [i] = dist1 [0];
				dists [0] += distance;
			} else {
				dist1 [i] = !dist1 [0];
				dists [1] += distance;
			}
		}
		bool close;
		if (dists [0] > dists [1]) {
			close = false;
		} else {
			close = true;
		}
		for (int i = 0; i < propellers.Length; i++) {
			int j = 0;
			if (!close) {
				if (dist1 [i]) {
					j += 4;
				}
			} else {
				if (!dist1 [i]) {
					j += 4;
				}
			}
			if (transform.InverseTransformPoint (propellers [i].position).x > 0) {
				if (transform.InverseTransformPoint (propellers [i].position).z > 0) {

					screws [j + 0] = propellers [i];
				} else {
					screws [j + 1] = propellers [i];
				}
			} else {
				if (transform.InverseTransformPoint (propellers [i].position).z > 0) {
					screws [j + 2] = propellers [i];
				} else {
					screws [j + 3] = propellers [i];
				}
			}
		}
		propellers = screws;
	}
	IEnumerator MoveWings(){
		if (turboMode) {
			while (propellers [0].parent.localRotation != Quaternion.Euler (new Vector3 (0f, 0f, 270f))) {
				for (int i = number; i < propellers.Length; i++) {

					if (i == 2+number || i == 0+number) {
						propellers [i].parent.localRotation = Quaternion.RotateTowards (propellers [i].parent.localRotation, Quaternion.Euler (new Vector3 (0f, 0f, 270f)), Time.deltaTime * wingsSpeed);
					} else {
						propellers [i].parent.localRotation = Quaternion.RotateTowards (propellers [i].parent.localRotation, Quaternion.Euler (new Vector3 (0f, 0f, 0f)), Time.deltaTime * wingsSpeed);
					}
				}
				yield return null;
			}
		} else {
			while (propellers [0].parent.localRotation != Quaternion.Euler (new Vector3 (0f, 0f, 135f))) {
				for (int i = number; i < propellers.Length; i++) {

					if (i == 2+number || i == 0+number) {
						propellers [i].parent.localRotation = Quaternion.RotateTowards (propellers [i].parent.localRotation, Quaternion.Euler (new Vector3 (0f, 0f, 135f)), Time.deltaTime * wingsSpeed);
					} else {
						propellers [i].parent.localRotation = Quaternion.RotateTowards (propellers [i].parent.localRotation, Quaternion.Euler (new Vector3 (0f, 0f, 315f)), Time.deltaTime * wingsSpeed);
					}
				}
				yield return null;
			}
		}
	}
	void RotatePropellers (Transform[] _propellers, float[] _velPropellers, float _maxVel)
	{
		for (int i = 0; i < _propellers.Length; i++) {
			if (i == 1 || i == 2) {
				_propellers [i].Rotate (0, 0, Time.deltaTime * (_velPropellers [i]) * _maxVel);
			} else {
				_propellers [i].Rotate (0, 0, -Time.deltaTime * (_velPropellers [i]) * _maxVel);
			}
			if(SmudgePrefab)
				smudgers [i].Smudge (_velPropellers [i]*_maxVel);
		}
	}
}



