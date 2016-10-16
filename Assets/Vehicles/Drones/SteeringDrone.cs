using UnityEngine;
using System.Collections;

public class SteeringDrone : MonoBehaviour
{
	//śmigła MUSZĄ nazywać się "Propeller", a środek masy "Center of mass"
	[HideInInspector]
	public Transform[] propellers;
	public GameObject SmudgePrefab;
	public DroneType type;
	//ustawienia elektroniki(wzmocnień)
	public float pitchFactor = 0.03f;
	public float rollFactor = 0.03f;
	public float yawFactor = 0.05f;
	public float stabVelFactor = 1f;
	public float stabFactor = 0.2f;
	//ustawienia fizyki
	public float forceFactor = 3.6f;
	[Tooltip ("To tylko dla prototypu")]
	public float wingsSpeed=36f;
	[Tooltip ("To tylko dla prototypu")]
	public float turboSpeed=50f;
	private float maxVel = 18000f;
	private float thrust;
	private float pitch;
	private float roll;
	private float yaw;

	private new Rigidbody rigidbody;
	private float zeroThrust;

	private float[] cor;
	private float[] pitchRollCor;
	[HideInInspector]
	public float[] velPropellers;

	public bool keepAltitude{ get; set; }

	public bool stabilize{ get; set; }

	public bool motorsOn{ get; set; }

	public bool selfLeveling{ get; set; }

	private SmudgeRotation[] smudgers;
	private int number;
	[HideInInspector]public GameObject flashLight;
	private Vector3 motorsSpacings;
//xaxis, zaxis, toOrigin
	private Coroutine coroutine;
	//[HideInInspector]
	//public GameManager gameManager;
	void Start(){
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
		selfLeveling = true;
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
		cor = new float[propellers.Length];
		pitchRollCor = new float[4];
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

		/*w razie czego test na odległości, jakby były nierówne
		pitchRollCor[0] =Vector3.Distance (propellers [0].position, transform.position);
		pitchRollCor[1] =Vector3.Distance (propellers [1].position, transform.position);
		pitchRollCor[2] =Vector3.Distance (propellers [2].position, transform.position);
		pitchRollCor[3] =Vector3.Distance (propellers [3].position, transform.position);
		*/
	}
	
	// Update is called once per frame
	void Update ()
	{
		//if(gameManager)
		//	if (gameManager.singlePlayer) {
				Steer ();
		//	}
		RotatePropellers (propellers, velPropellers, maxVel);
	}
	public void Steer(){
		if (motorsOn) {
			thrust = Input.GetAxis ("Thrust");
			pitch = Input.GetAxis ("Pitch") * pitchFactor;
			roll = Input.GetAxis ("Roll") * rollFactor;
			yaw = Input.GetAxis ("Yaw") * yawFactor;

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
		if (Input.GetButtonDown ("Stabilize")) {
			if (stabilize) {
				stabilize = false;
				for (int i = 0; i < propellers.Length; i++) {
					cor [i] = 0;
				}
			} else
				stabilize = true;
		}
		if (Input.GetButtonDown ("Keep altitude")) {
			if (keepAltitude)
				keepAltitude = false;
			else
				keepAltitude = true;
		}
		if (Input.GetButtonDown ("Self leveling")) {
			if(coroutine!=null)
				StopCoroutine (coroutine);
			coroutine = StartCoroutine (MoveWings ());
			if (selfLeveling) {
				selfLeveling = false;
				for (int i = 0; i < cor.Length; i++) {
					cor [i] = 0;
				}
			}else
				selfLeveling = true;
		}
	}
	void FixedUpdate ()
	{
		//if (gameManager)
		//	if (gameManager.singlePlayer) {
				ApplyPhysics ();
		//	}
	}
	public void ApplyPhysics(){
		if (motorsOn) {
			//compute stabilize corrections
			float averageAltitude = 0;
			float[] tempCor = new float[4];
			pitchRollCor = new float[4];
			if (selfLeveling) {
				for (int i = 0; i < propellers.Length-number; i++) {
					averageAltitude += propellers [i].position.y;
				}
				averageAltitude /= 4;
		
				for (int i = 0; i < propellers.Length-number ; i++) {
					tempCor [i] = (averageAltitude - propellers [i].position.y) / motorsSpacings.z;
				}
				//x+
				pitchRollCor [0] = (tempCor [0] + tempCor [1]) / 2f;
				//x-
				pitchRollCor [1] = (tempCor [2] + tempCor [3]) / 2f;
				//z+
				pitchRollCor [2] = (tempCor [0] + tempCor [2]) / 2f;
				//z-
				pitchRollCor [3] = (tempCor [1] + tempCor [3]) / 2f;
				pitch = Input.GetAxis ("Pitch");
				roll = Input.GetAxis ("Roll");
				cor [0] = pitchRollCor [0] * pitch + tempCor [0] * (1 - pitch);
				cor [1] = pitchRollCor [0] * pitch + tempCor [1] * (1 - pitch);
				cor [2] = pitchRollCor [1] * pitch + tempCor [2] * (1 - pitch);
				cor [3] = pitchRollCor [1] * pitch + tempCor [3] * (1 - pitch);
				cor [0] += pitchRollCor [2] * roll + tempCor [0] * (1 - roll);
				cor [1] += pitchRollCor [3] * roll + tempCor [1] * (1 - roll);
				cor [2] += pitchRollCor [2] * roll + tempCor [2] * (1 - roll);
				cor [3] += pitchRollCor [3] * roll + tempCor [3] * (1 - roll);
				for (int i = 0; i < propellers.Length; i++) {
					cor [i] /= 2;
				}
			}
			for (int i = 0; i < propellers.Length; i++) {
				Vector3 thrustVec;
				thrustVec = new Vector3 (0, 0, velPropellers [i] * forceFactor + cor [i] * stabFactor);
				
				rigidbody.AddForceAtPosition (propellers [i].rotation * thrustVec, propellers [i].position);
				//Debug.DrawLine (propellers [i].position, propellers [i].position + propellers [i].rotation * thrustVec);
			}
			if (stabilize) {
				rigidbody.AddTorque (rigidbody.angularVelocity * (-Time.deltaTime * stabVelFactor));
			}
			rigidbody.AddTorque (transform.rotation * new Vector3 (0, yaw, 0));
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
		if (!selfLeveling) {
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
}

public enum DroneType
{
	quadrocopter,
	heksacopter,
	octacopter,
	prototype
}