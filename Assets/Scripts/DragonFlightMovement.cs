using UnityEngine;
using System.Collections;
//jeszcze do zrobienia
//	1. omijanie kolizji i potencjalnych kolizj
//		- raycasty na końcach skrzydeł i w torsie do przodu
//		- byc mnoze pierwsze wystarczy
//		- lot określoną wysokość nad przeszkodami
//		- być może raycaster kilka metrów przed smokiem w dół

public class DragonFlightMovement : MonoBehaviour {
	public enum DragonFlightState{
		gliding = 0,//
		flapping = 1,//
		turningLeft = 2,//
		turningRight = 3,//
		flappingLeft = 4,//
		flappingRight = 5,//
		diving = 6, //
		touchingTheSky = 7, //
		hovering = 8,
		breaking = 9,
		turningBack = 10,
	}
	[Tooltip("Stan smoka.")]
	public DragonFlightState state;

	[Tooltip("Aby było weselej, podczas szybowania do przodu będzie cały czas opadać z prędkością downSpeed (jak komuś się nudzi, niech zmieni nazwę).")]
	public float downSpeed;
	[Tooltip("Ale skoro szybuje, to musi mieć jakąś standardową prędkość szybowania glideSpeed.")]
	public float maxGlideSpeed;
	[Tooltip("Ponieważ nie chce mi się myśleć cały czas będzie tracić prędkość z przyspieszeniem dumpAcc.")]
	public float dumpAcc;
	[Tooltip("Jak straci poniżej progu to odpala machanie skrzydłami.")]
	public float minSpeed;
	[Tooltip("Prędkość uznawana już za praktycznie zerową.")]
	public float almostZeroSpeed;
	[Tooltip("Każde machnięcie dodaje prędkości o flapSpeed.")]
	public float flapSpeed;
	//to wszystko wymaga, żeby miał aktualną prędkość szybowania
	float actualGlideSpeed;//prywatne
	//to by było na tyle, jeżeli chodzi o lot do przodu
	//teraz lot w górę i dół

	//pikowanie odpala się gdy musi lecieć bardziej pionowo w dół niż aktualny limit, prędkość graniczna pikowania jest różna
	[Tooltip("Maksymalna prędkość opadania w pozycji poziomej smoka z rozłożonymi skrzydłami.")]
	public float terminalSpeedHorizontal;
	[Tooltip("Maksymalna prędkość lotu smoka - czyli podczas pikowania.")]
	public float terminalSpeedVertical;
	[Tooltip("Przyspieszenie ziemskie, potrzebne dla smoka, który nie leci idealnie poziomo - czyli praktycznie cały czas")]
	public float g;
	[Tooltip("Jeżeli kąt będzie większy, to smok zawróci w miejscu.")]
	public float maxAngleHor;
	[Tooltip("Jeżeli kąt będzie większy, to smok dostanie animację skrętu.")]
	public float Angle2Animate;

	[Tooltip("Jak długo ma trwać zawracanie smoka w sekundach.")]
	public float TimeOfTurningBack;
	//pozostał opis skręcania i krążenia w celu polecenia wyżej mimo braku dość prędkości
	//skręcanie musi mieć jakąś maksymalną prędkość obrotową...
	[Tooltip("Maksymalna prędkość skrętu smoka")]
	public float maxAngleSpeed;
	float actualAngleSpeed;
	//dobrze by było dać smokowi możliwość wiszenia w powietrzuw punkcie celu... chwilowo nie mam do tego animacji, więc wyglądać będzie chujowo
	//ale logika gry przewiduje coś takiego... czasami
	// więc musi jakoś hamować
	[Tooltip("Opóźnienie hamowaniem.")]
	public float breakAcc;

	//wyliczy sobie wymarzone prędkość i potem jakoś do nich będzie starał się dążyć...
	float targetAngularSpeed;
	float targetGlideSpeed;
	//float targetPitch;

	float pitch;

	[Tooltip("Jak szybko i mocno smok bije skrzydłami, gdy ma podlecieć do góry")]
	public float panicCoef;

	DragonTargetSystem targetSystem;
	public bool stopAtTarget;

	[Tooltip("Odległość na jakiej smok uzna że zderza się czołowo z przeszkodą i musi wylądować.")]
	public float dist2hit = 10;

	[Tooltip("Odległość na jakiej co najwyżej musi znaleźć się smok, by zaczął łaskawie hamować, by zawisnąć w powietrzu.")]
	public float dist2break;

	float calculatedSpeedChange;//prywatne
	float calculatedDownSpeed;//prywatne
	float flapPower;
	Animator anim;
	DragonGroundedMovement dgm;
	void Awake () {
		anim = GetComponent<Animator> ();
		targetSystem=GetComponent<DragonTargetSystem> ();
		dgm = GetComponent<DragonGroundedMovement> ();
	}

	bool inNotInterruptableState = false;
	void Update () {
		//zakoncz update'a, jeżeli jest w stanie, którego nie wolno przerwać
		if (inNotInterruptableState) {
			UpdateDragonSteering ();
			return;
		}
		if (!waiting2Check) {
			StartCoroutine (CheckTarget ());
		}
		CalculateNeededSpeed ();

		AnalyzeState ();
		UpdateDragonSteering ();

	}
	//używa predkości do przesunięcia smoka w grze
	void UpdateDragonSteering (){
		if (state != DragonFlightState.turningBack) {
			if (state == DragonFlightState.hovering) {
				transform.rotation = Quaternion.Slerp (transform.rotation, Quaternion.Euler (0f, transform.rotation.y, 0f), Time.deltaTime);
			} else {
				Quaternion temperTemp = Quaternion.Slerp (transform.rotation, targetSystem.target_rot, actualAngleSpeed * Time.deltaTime);
				transform.rotation = temperTemp;
			}
		}
		transform.position += transform.forward * Time.deltaTime * CalculateActualSpeed ();
		transform.position -= Vector3.up * Time.deltaTime * CalculateDownSpeed ();
	}
	//aktualizuję prędkość postępową zgodnie z oporem powietrza(dumpSpeed) oraz grawitacją(sin pochylenia)
	float CalculateActualSpeed(){
		pitch = transform.localRotation.eulerAngles.x;
		if (pitch > 180f) {
			pitch -= 360f;
		}
		calculatedSpeedChange = 0;
		calculatedSpeedChange -= dumpAcc * actualGlideSpeed / maxGlideSpeed * Time.deltaTime;
		calculatedSpeedChange += Mathf.Sin (pitch * Mathf.Deg2Rad) * g * Time.deltaTime;

		if ((state == DragonFlightState.breaking || state == DragonFlightState.hovering) && stopAtTarget) {
			actualGlideSpeed = Mathf.Lerp(actualGlideSpeed, targetGlideSpeed, breakAcc*Time.deltaTime);
		} else {
			actualGlideSpeed = Mathf.Clamp (actualGlideSpeed + calculatedSpeedChange, 0, terminalSpeedVertical);
		}
		return actualGlideSpeed;
	}
	//oblicza prędkość spadku - smok cały czas trochę opada
	float CalculateDownSpeed(){
		if (actualGlideSpeed < minSpeed) {
			calculatedDownSpeed = Mathf.Lerp (terminalSpeedHorizontal, downSpeed, actualGlideSpeed/minSpeed); 
		} else {
			calculatedDownSpeed = downSpeed;
		}
		if (state == DragonFlightState.hovering) {
			calculatedDownSpeed = 0;
		}
		return calculatedDownSpeed;
	}
	//analizuje różne parametry i dopiera odpowiedni stan dla smoka
	//na koniec przekazuje go animatorowi
	// to wszystko trzeba przerobić, bo zrobiło się mnóstwo stanów
	void AnalyzeState(){
		stopAtTarget = !targetSystem.useWaypoints;
		float speedCoef = actualGlideSpeed / terminalSpeedVertical;
		actualAngleSpeed = Mathf.Clamp (targetAngularSpeed, 0, maxAngleSpeed) * Mathf.Clamp01(1.5f-speedCoef);
		DragonFlightState before = state;
		if ((Mathf.Abs(targetSystem.angleHor) > maxAngleHor && targetSystem.distHor > dist2break)) {
			StartCoroutine (TurnBack ());
			state = DragonFlightState.turningBack;
		} else {
			if ((targetSystem.distHor < dist2break*Mathf.Clamp(speedCoef*10f, 1, Mathf.Infinity) && stopAtTarget) || state == DragonFlightState.breaking) {
				//if(Mathf.Abs(distVer)<dist2break){
				if (Mathf.Abs(targetSystem.distVer+targetSystem.distHor)<targetSystem.dist2pass){//actualGlideSpeed < almostZeroSpeed) {
					state = DragonFlightState.hovering;
					targetGlideSpeed = 0;
				} else {
					if(actualGlideSpeed > targetGlideSpeed)
						state = DragonFlightState.breaking;
					//targetGlideSpeed = 0f;
				}
				/*}else{
					if (distVer < 0) {
						state = DragonFlightState.diving;
					} else {
						state = DragonFlightState.touchingTheSky;
						if (actualGlideSpeed < targetGlideSpeed) {
							state = DragonFlightState.flapping;
							flapPower = Mathf.Lerp (3, 1, actualGlideSpeed / targetGlideSpeed);
							anim.SetFloat("flapper", flapPower);
						}
					}
				}*/
			} else {
				if (actualGlideSpeed > targetGlideSpeed) {
					state = DragonFlightState.gliding;
					if (Mathf.Abs (targetSystem.angleHor) > Angle2Animate) {
						if (targetSystem.angleHor < 0) {
							state = DragonFlightState.turningLeft;	
						} else {
							state = DragonFlightState.turningRight;
						}
					} else {
						if (Mathf.Abs (targetSystem.angleVerGlob) > Angle2Animate) {
							if (targetSystem.distVer > 0) {
								state = DragonFlightState.diving;
							} else {
								state = DragonFlightState.touchingTheSky;
							}
						}
					}
				} else {
					state = DragonFlightState.flapping;
					flapPower = Mathf.Lerp (3, 1, actualGlideSpeed / targetGlideSpeed);
					anim.SetFloat("flapper", flapPower);
					if (Mathf.Abs(targetSystem.angleHor) > Angle2Animate) {
						if (targetSystem.angleHor < 0) {
							state = DragonFlightState.flappingLeft;
						} else {
							state = DragonFlightState.flappingRight;
						}
					}
				}
			}
		}
		if(state!=before)
			anim.SetInteger ("State", (int)state);
	}

	void CalculateNeededSpeed (){
		targetAngularSpeed = Mathf.Abs(targetSystem.angleHor)+Mathf.Abs(targetSystem.angleVer);
		float panicAddition = targetSystem.angleVerGlob<0 ? 0 :  targetSystem.angleVerGlob * panicCoef;
		if (state!=DragonFlightState.breaking)
			targetGlideSpeed = Mathf.Clamp (targetSystem.distHor * 0.2f, 0f, maxGlideSpeed) + panicAddition;
		else
			targetGlideSpeed = Mathf.Clamp ((targetSystem.distHor + targetSystem.distVer) * 0.2f  + panicAddition, 0f, maxGlideSpeed);
	}

	//dodaje prędkość postępową o dmachania skrzydłami za pomocą eventu w animacji
	public void WingFlaped (){
		if (state != DragonFlightState.hovering && actualGlideSpeed < maxGlideSpeed)
			actualGlideSpeed += flapSpeed * flapPower;
	}
	//zawracanie jako korutyna - like a BOSS(i chuj, ze chujowa korutyna i istnieją lepsze i tak się rzadko odpala i na krótko)
	IEnumerator TurnBack(){
		inNotInterruptableState = true;
		Quaternion targetRotation = Quaternion.LookRotation (targetSystem.target.position - transform.position);
		Quaternion firstRotation = transform.rotation;
		for (float i = 0; i < TimeOfTurningBack; i+=Time.deltaTime) {
			float progress = i / TimeOfTurningBack;
			transform.rotation = Quaternion.Slerp(firstRotation, targetRotation, progress);
			yield return null;
		}
		transform.rotation = targetRotation;
		inNotInterruptableState = false;
	}

	void TakeDown(RaycastHit hit){
		//if (col.tag == "Terrain") {

		dgm.enabled = true;
		anim.runtimeAnimatorController = targetSystem.Running;
		dgm.UpdatePosition (hit);
			enabled = false;
		//}
	}
	bool waiting2Check;
	IEnumerator CheckTarget(){
		waiting2Check = true;
		RaycastHit hitF, hitD, hit=new RaycastHit();
		Physics.Raycast (new Ray (transform.position, targetSystem.target.position - transform.position), out hitF, dist2hit, targetSystem.layerMask);
		Physics.Raycast (new Ray (transform.position, Vector3.down), out hitD, dgm.height, targetSystem.layerMask);
		if (hitD.normal!=Vector3.zero) {
			hit = hitD;
			//print ("hit gleba" + name);
		}
		if (hitF.normal!=Vector3.zero) {
			hit = hitF;
			//print ("hit czolo" + name);
		}
		if(hit.normal==Vector3.zero)
		{
			yield return new WaitForSeconds (0.5f);
			waiting2Check = false;
		} else {
			waiting2Check = false;
			TakeDown (hit);
		}
	}
	public void getAKick(float kick){
		actualGlideSpeed = kick;
		state = (DragonFlightState)100;
	}
}
