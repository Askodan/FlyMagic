﻿using UnityEngine;
using System.Collections;
//jeszcze do zrobienia
//	1. omijanie kolizji i potencjalnych kolizj
//		- raycasty na końcach skrzydeł i w torsie do przodu
//		- byc mnoze pierwsze wystarczy
//		- lot określoną wysokość nad przeszkodami
//		- być może raycaster kilka metrów przed smokiem w dół

public class DragonMovement : MonoBehaviour {
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
	public bool useWaypoints;
	//na razie zakładam, że smok nie umie lądować, choć fajnie by było, gdyby czasem lądował...
	[Tooltip("Stan smoka.")]
	public DragonFlightState state;
	// tak więc będzie chwilowo latać od punktu do punktu
	[Tooltip("Lista punktów.")]
	public GameObject[] wayPoints;
	int target_point;//prywatne
	bool stopAtTarget;
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

	public Transform target;
	[Tooltip("Odległość na jakiej co najwyżej musi znaleźć się smok, by zaliczyć mu dotarcie do celu.")]
	public float dist2pass;
	[Tooltip("Odległość na jakiej co najwyżej musi znaleźć się smok, by zaczął łaskawie hamować, by zawisnąć w powietrzu.")]
	public float dist2break;
	Vector3 target_pos;//prywatne
	Quaternion target_rot;//prywatne

	float distHor;//prywatne
	float distVer;//prywatne
	float angleHor;//prywatne
	float angleVer;//prywatne
	float distHorGlob;//prywatne
	float distVerGlob;//prywatne
	float angleHorGlob;//prywatne
	float angleVerGlob;//prywatne

	float calculatedSpeedChange;//prywatne
	float calculatedDownSpeed;//prywatne
	float flapPower;
	Animator anim;

	void Start () {
		anim = GetComponent<Animator> ();
	}
	bool atTarget;
	bool inNotInterruptableState = false;
	void Update () {
		//zakoncz update'a, jeżeli jest w stanie, którego nie wolno przerwać
		if (inNotInterruptableState) {
			UpdateDragonSteering ();
			return;
		}
		
		FindTarget ();
		if (distHor + Mathf.Abs(distVer) < dist2pass) {
			atTarget = true;
		} else {
			atTarget = false;
		}
		if (atTarget && useWaypoints) {
			target_point++;
			target_point = target_point % wayPoints.Length;
			target = wayPoints [target_point].transform;
		} else {
			AnalyzeState ();
			UpdateDragonSteering ();
		}
	}
	//używa predkości do przesunięcia smoka w grze
	void UpdateDragonSteering (){
		if (state != DragonFlightState.turningBack) {
			if (state == DragonFlightState.hovering) {
				transform.rotation = Quaternion.Slerp (transform.rotation, Quaternion.Euler (0f, transform.rotation.y, 0f), Time.deltaTime);
			} else {
				Quaternion temperTemp = Quaternion.Slerp (transform.rotation, target_rot, actualAngleSpeed * Time.deltaTime);
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
		stopAtTarget = !useWaypoints;
		float speedCoef = actualGlideSpeed / terminalSpeedVertical;
		actualAngleSpeed = Mathf.Clamp (targetAngularSpeed, 0, maxAngleSpeed) * Mathf.Clamp01(1.5f-speedCoef);
		DragonFlightState before = state;
		if ((Mathf.Abs(angleHor) > maxAngleHor && distHor > dist2break)) {
			StartCoroutine (TurnBack ());
			state = DragonFlightState.turningBack;
		} else {
			if ((distHor < dist2break*Mathf.Clamp(speedCoef*10f, 1, Mathf.Infinity) && stopAtTarget) || state == DragonFlightState.breaking) {
				//if(Mathf.Abs(distVer)<dist2break){
				if (Mathf.Abs(distVer+distHor)<dist2pass){//actualGlideSpeed < almostZeroSpeed) {
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
					if (Mathf.Abs (angleHor) > Angle2Animate) {
						if (angleHor < 0) {
							state = DragonFlightState.turningLeft;	
						} else {
							state = DragonFlightState.turningRight;
						}
					} else {
						if (Mathf.Abs (angleVerGlob) > Angle2Animate) {
							if (distVer > 0) {
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
					if (Mathf.Abs(angleHor) > Angle2Animate) {
						if (angleHor < 0) {
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
	//patrzy gdzie znajduje się cel... może warto wrzucić to do korutyny?
	Vector3 FindTarget(){
		target_pos = transform.InverseTransformPoint(target.position);
		target_rot = Quaternion.LookRotation (target.position - transform.position);
		CalculateBasicTargetInfo (out angleHor, out angleVer, out distHor, out distVer, Vector3.zero, target_pos);
		CalculateBasicTargetInfo (out angleHorGlob, out angleVerGlob, out distHorGlob, out distVerGlob, transform.position, target.position);
		CalculateNeededSpeed ();
		return target_pos;
	}
	//obliczenie kilku parametrów, które wskażą drogę do celu
	// prawdopodobnie przerobie bo generuje za duzo informacji
	static void CalculateBasicTargetInfo (out float angleHor_, out float angleVer_, out float distHor_, out float distVer_, Vector3 pos, Vector3 tar_pos){
		distVer_ = tar_pos.y-pos.y;
		Vector3 target_pos_temp = new Vector3(tar_pos.x-pos.x, 0f, tar_pos.z-pos.z);
		distHor_ = target_pos_temp.magnitude;
		angleHor_ = Mathf.Atan2 (tar_pos.x-pos.x, tar_pos.z-pos.z);
		angleVer_ = Mathf.Atan2 (tar_pos.y-pos.y, distHor_);
		angleHor_ *= Mathf.Rad2Deg;
		angleVer_ *= Mathf.Rad2Deg;
	}
	void CalculateNeededSpeed (){
		targetAngularSpeed = Mathf.Abs(angleHor)+Mathf.Abs(angleVer);
		float panicAddition = angleVerGlob<0 ? 0 :  angleVerGlob * panicCoef;
		if (state!=DragonFlightState.breaking)
			targetGlideSpeed = Mathf.Clamp (distHor * 0.2f, 0f, maxGlideSpeed) + panicAddition;
		else
			targetGlideSpeed = Mathf.Clamp ((distHor + distVer) * 0.2f  + panicAddition, 0f, maxGlideSpeed);
	}

	//dodaje prędkość postępową o dmachania skrzydłami za pomocą eventu w animacji
	public void WingFlaped (){
		if (state != DragonFlightState.hovering && actualGlideSpeed < maxGlideSpeed)
			actualGlideSpeed += flapSpeed * flapPower;
	}
	//zawracanie jako korutyna - like a BOSS(i chuj, ze chujowa korutyna i istnieją lepsze i tak się rzadko odpala i na krótko)
	IEnumerator TurnBack(){
		inNotInterruptableState = true;
		Quaternion targetRotation = Quaternion.LookRotation (target.position - transform.position);
		Quaternion firstRotation = transform.rotation;
		for (float i = 0; i < TimeOfTurningBack; i+=Time.deltaTime) {
			float progress = i / TimeOfTurningBack;
			transform.rotation = Quaternion.Slerp(firstRotation, targetRotation, progress);
			yield return null;
		}
		transform.rotation = targetRotation;
		inNotInterruptableState = false;
	}
}
