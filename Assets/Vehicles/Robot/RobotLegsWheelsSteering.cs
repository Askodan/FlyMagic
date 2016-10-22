using UnityEngine;
using System.Collections;

public class RobotLegsWheelsSteering : MonoBehaviour {
	public Transform[] wheels;
	public bool[] steeringWheel;
	Transform[] axes;
	Transform[] holders3;
	Transform[] arms2;
	Transform[] holders2;
	Transform[] arms1;
	Transform[] holders1;
	Transform Base;
	//public float H;//wysokośc bazy nad osiami kół
	//public float D;//rozstaw kół
	float l=2.1f;//długość ramion - 2.1 lub 0.5, jeżeli chcemy normalizować wysokość i rozstaw do <-1,1>

	public float armsSpeed;//predkosc ramion
	public float maxTorque;
	float speed;//prędkość obrotowa kół
	public float maxSteer;
	float steering;//kąt skrętu przedmich kół
	public Vector2 highLimes;
	public float high;//wysokośc nad ziemią bazy - najlepiej od 0f do 4.2f
	public Vector2 spreadLimes;
	public float spread;//to będzie łądny współczynnik rozstawu nóg
	private float l2;
	float[] wheelsHigh;
	float[] wheelsSpread;
	WheelUpdater[] wheelsActuators;

	void Start () {
		l2 = 4f * l * l;
		wheelsHigh = new float[wheels.Length];
		wheelsSpread = new float[wheels.Length];

		wheelsActuators = GetComponentsInChildren<WheelUpdater> ();
		for (int i = 0; i < wheelsActuators.Length; i++) {
			//wheelsActuators [i].wheel = wheels [i];
			wheelsActuators [i].transform.position = wheels [i].transform.position;
		}

		axes = new Transform[wheels.Length];
		holders3 = new Transform[wheels.Length];
		arms2 = new Transform[wheels.Length];
		holders2 = new Transform[wheels.Length];
		arms1 = new Transform[wheels.Length];
		holders1 = new Transform[wheels.Length];
		for (int i = 0; i < wheels.Length; i++) {
			axes [i] = wheels [i].parent;
			holders3 [i] = axes [i].parent;
			arms2 [i] = holders3 [i].parent;
			holders2 [i] = arms2 [i].parent;
			arms1 [i] = holders2 [i].parent;
			holders1 [i] = arms1 [i].parent;

			//firstposition
			wheelsHigh [i] = high;
			wheelsSpread [i] = spread;
			SetAngles (CalculateAngles (spread, high), 0f, i);
		}
		if (holders1.Length > 0)
			Base = holders1 [0].parent;
	}

	// y koła powinien byc prostopadły do normalnej powierzchni na której ono stoi
	//baza powinna się utrzymywać na stałej, zadanej wysokości nad ziemią i przeszkodami
	//mało tego baza ma być raczej poziomo
	public void Steer(float axis_Vertical, float axis_Horizontal, float axis_SpreadRobot, float axis_HeightRobot){
		speed = axis_Vertical*maxTorque;
		steering = axis_Horizontal*maxSteer;

		spread += axis_SpreadRobot* Time.deltaTime;
		high += axis_HeightRobot * Time.deltaTime;
	}
	void FixedUpdate () {
		spread = Mathf.Clamp (spread, spreadLimes.x, spreadLimes.y);	
		high = Mathf.Clamp (high, highLimes.x, highLimes.y);	
		for (int i = 0; i < wheels.Length; i++) {
			if (steeringWheel [i]) {
				if (!wheelsActuators [i].front) {
					wheelsActuators [i].UpdateWheel (speed, -steering);
				} else {
					wheelsActuators [i].UpdateWheel (speed, steering);
				}
			}else {
				wheelsActuators [i].UpdateWheel (speed, 0);
			}
		}
		KeepBase ();
	
	}
	Vector2 CalculateAngles(float spacing, float lowering){
		if (spacing * spacing + lowering * lowering < 4*l * l) {
			float a, b, c = spacing * spacing + lowering * lowering, p = Mathf.Sqrt (-(c) * (c - 4 * l * l));
			a = 2f * Mathf.Atan ((-2f * lowering * l + p) / (c + 2f * spacing * l));
			b = -2f * Mathf.Atan (p / c);
		
			if (!float.IsNaN (a)) {
				return new Vector2 (a, b);
			} else {
				return new Vector2 (float.NaN, 0);
			}
		} else {
			return new Vector2 (float.NaN, 0);
		}
	}
	void SetAngles(Vector2 angles, float steerAngle, int wheel){
		if (!float.IsNaN(angles.x)) {
			arms1 [wheel].localRotation = Quaternion.Euler (angles.x * Mathf.Rad2Deg, 0f, 0f);
			arms2 [wheel].localRotation = Quaternion.Euler (angles.y * Mathf.Rad2Deg, 0f, 0f);
			axes [wheel].localRotation = Quaternion.Euler (-(angles.x + angles.y) * Mathf.Rad2Deg, 0f, 0f);
			if (steeringWheel [wheel]) {
				if (wheelsActuators [wheel].front) {
					holders3 [wheel].localRotation = Quaternion.Euler (0f, -steerAngle, 0f);
				} else {
					holders3 [wheel].localRotation = Quaternion.Euler (0f, steerAngle, 0f);
				}
			}
		}
	}
	void KeepBase(){
		RaycastHit baseHit;
		if (Physics.Raycast (Base.position, Vector3.down, out baseHit, 3f * l)) {
			for (int i = 0; i < wheels.Length; i++) {
				RaycastHit hit;
				if (Physics.Raycast (wheels [i].position, Vector3.down, out hit, 2f)) {
					float dif = baseHit.point.y - hit.point.y;
					wheelsHigh [i] = Mathf.Lerp(wheelsHigh[i],high + dif, armsSpeed*Time.deltaTime);
				}
				if (wheelsSpread [i] * wheelsSpread [i] + wheelsHigh [i] * wheelsHigh [i] > l2) {
					wheelsSpread [i] = Mathf.Lerp (wheelsSpread [i], 0f, armsSpeed * Time.deltaTime);
				} else {
					wheelsSpread [i] = Mathf.Lerp (wheelsSpread [i], spread, armsSpeed * Time.deltaTime);
				}

				SetAngles (CalculateAngles (wheelsSpread[i], wheelsHigh [i]), steering, i);
			}
		}
	}
}