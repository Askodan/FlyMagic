using UnityEngine;
using System.Collections;
//jeszcze do zrobienia
//	1. omijanie kolizji i potencjalnych kolizj
//		- raycasty na końcach skrzydeł i w torsie do przodu
//		- byc mnoze pierwsze wystarczy
//		- lot określoną wysokość nad przeszkodami
//		- być może raycaster kilka metrów przed smokiem w dół
//	2. hamowanie i wiszenie w powietrzu
//		- stany już istnieją brak: animacji, obsługi stanów, części logiki
//	3. przebudowa analizy stanów
//		- stany są chujowe.
//		- muszą się przełączać z sensem i psuć nawzajem
//		- istnieje szansa ze logika rozmyta może tu pomóc
//  4. stan szybkich skrętów
//		- dodatkowy stan z 1 animacją chyba nie blendowaną
//		- możliwe, ze animacja od pikowania i lotu w górę będzie tu tez pasować
//		- w tym stanie skręty są 3 razy szybsze
//		- dotyczy tylko skrętów w bok, pochylenie jest już wystarczająco szybkie i jest ok

public class DragonMovement : MonoBehaviour {
	public enum DragonFlightState{
		gliding = 0,
		flapping = 1,
		turningLeft = 2,
		turningRight = 3,
		flappingLeft = 4,
		flappingRight = 5,
		diving = 6,
		touchingTheSky = 7,
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
	[Tooltip("Aby było weselej, podczas szybowania do przodu będzie cały czas opadać z prędkością downSpeed (jak komuś się nudzi, niech zmieni nazwę).")]
	public float downSpeed;
	[Tooltip("Ale skoro szybuje, to musi mieć jakąś standardową prędkość szybowania glideSpeed.")]
	public float maxGlideSpeed;
	[Tooltip("Ponieważ nie chce mi się myśleć cały czas będzie ją traciś z przyspieszeniem dumpAcc.")]
	public float dumpAcc;
	[Tooltip("Jak straci poniżej progu to odpala machanie skrzydłami.")]
	public float minSpeed;
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
	[Tooltip("Przyspieszenie ziemskie - albo jego odpowiednik dla pikujacego smoka")]
	public float g;
	//to powoli wydaje się być niepotrzebne
	public float maxAngleDown;
	//hmmm... czy istnieje maksymalne kąt lotu w górę? istnieje i zalezy od prędkości (jak szybko energia potencjalna pochłania kinetyczną)
	// teraz pytanie, czy smok musi takie rzeczy przewidywać... w zasadzie przeliczenie energii kinetycznej na potencjalną to banał
	// czyli gdyby smok nagle zaczął lecieć w górę to wie jak wysoko doleci...
	// tylko czy nasza oszukańcza symulacja to poprawnie uwzględni...
	// załóżmy zatem dla uproszczenia, że siła nośna skrzydeł w poziomej pozycji smoka przy prędkośći między glideSpeed, a minSpeed pokonuje grawitacje i smok opada ze stałą prędkośćią dumpSpeed
	// teraz: jeżeli zwiększymy kąt smokowania do 90 siła nośna przestaje praktycznie istnieć - istnieje tylko malejąca prędkość którą miał na początku
	// jeżeli starczy prędkości na dostanie się do punktu to ok, smok dolatuje, a potem macha skrzydłami...i dopóki nie osiągnie prędkości minSpeed do swojego przodu
	// spada z prędkością graniczną
	// powyżej tego kąta

	public float maxAngleUp;

	//pozostał opis skręcania i krążenia w celu polecenia wyżej mimo braku dość prędkości
	//skręcanie musi mieć jakąś maksymalną prędkość obrotową...
	[Tooltip("Maksymalna prędkość skrętu smoka")]
	public float maxAngleSpeed;
	[Tooltip("Prędkość pochyłu")]
	public float pitchSpeed;
	float actualAngleSpeed;
	//dobrze by było dać smokowi możliwość wiszenia w powietrzuw punkcie celu... chwilowo nie mam do tego animacji, więc wyglądać będzie chujowo
	//ale logika gry przewiduje coś takiego... czasami
	// więc musi jakoś hamować
	[Tooltip("Opóźnienie hamowaniem.")]
	public float breakAcc;

	//wyliczy sobie wymarzone prędkość i potem jakoś do nich będzie starał się dążyć...
	float targetAngularSpeed;
	float targetGlideSpeed;
	float targetPitch;

	float pitch;

	[Tooltip("Jak szybko i mocno smok bije skrzydłami, gdy ma podlecieć do góry")]
	public float panicCoef;

	public Transform target;
	[Tooltip("Odległość na jakiej co najwyżej musi znaleźć się smok, by zaliczyć mu dotarcie do celu.")]
	public float dist2pass;

	Vector3 target_pos;//prywatne

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
	// Update is called once per frame
	void Update () {
		
		FindTarget ();
		if (distHorGlob + Mathf.Abs(distVerGlob) < dist2pass) {
			atTarget = true;
		} else {
			atTarget = false;
		}
		if (atTarget && useWaypoints) {
			target_point++;
			target_point = target_point % wayPoints.Length;
			target = wayPoints [target_point].transform;
			//FindTarget ();
		} else {
			AnalyzeState ();
			UpdateDragonSteering ();
		}
	}
	//używa predkości do przesunięcia smoka w grze
	void UpdateDragonSteering (){
		transform.Rotate(0, -actualAngleSpeed*Time.deltaTime, 0, Space.World);
		//pitch updater
		Quaternion temperTemp = Quaternion.Slerp(transform.rotation, Quaternion.Euler(-targetPitch, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z), pitchSpeed*Time.deltaTime);
		transform.rotation = temperTemp;
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

		if (state == DragonFlightState.breaking) {
			calculatedDownSpeed -= breakAcc * Time.deltaTime;
		}

		actualGlideSpeed = Mathf.Clamp (actualGlideSpeed + calculatedSpeedChange, 0, terminalSpeedVertical);
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
		actualAngleSpeed = Mathf.Lerp(actualAngleSpeed, targetAngularSpeed, 10*Time.deltaTime);
		float coefficent = 2 - actualGlideSpeed / maxGlideSpeed;
		actualAngleSpeed = Mathf.Clamp (actualAngleSpeed, -maxAngleSpeed*coefficent, maxAngleSpeed*coefficent);
		DragonFlightState before = state;

		if (actualAngleSpeed > maxAngleSpeed / 2f) {
			if (actualAngleSpeed > 0) {
				state = DragonFlightState.turningLeft;
			} else {
				state = DragonFlightState.turningRight;
			}
		}

		/*if (-pitch > maxAngleDown) {
			state = DragonFlightState.diving;
		} else {
			if (pitch > maxAngleUp) {
				state = DragonFlightState.touchingTheSky;
			}
		}*/

		//if (targetGlideSpeed - actualGlideSpeed < 10) {
		//	state = DragonFlightState.breaking;
		//}
		if (actualGlideSpeed > targetGlideSpeed) {
			state = DragonFlightState.gliding;
		}
		switch (state) {
		case DragonFlightState.gliding:
			if (actualGlideSpeed < targetGlideSpeed) {
				state = DragonFlightState.flapping;
			}
			break;
		case DragonFlightState.flapping:
			if (Mathf.Abs (actualAngleSpeed) > maxAngleSpeed / 2f) {
				if (actualAngleSpeed > 0) {
					state = DragonFlightState.flappingLeft;
				} else {
					state = DragonFlightState.flappingRight;
				}
			}
			if (actualGlideSpeed > targetGlideSpeed) {
				state = DragonFlightState.gliding;
			}
			flapPower = Mathf.Lerp (3, 1, actualGlideSpeed / targetGlideSpeed - distVer * panicCoef);
			anim.SetFloat("flapper", flapPower);
			break;
		case DragonFlightState.touchingTheSky:
			break;
		case DragonFlightState.diving:
			break;
		case DragonFlightState.turningLeft:
			if (actualGlideSpeed < targetGlideSpeed) {
				state = DragonFlightState.flappingLeft;
			}
			if (Mathf.Abs(actualAngleSpeed) < maxAngleSpeed / 2f) {
				state = DragonFlightState.gliding;
			}
			break;
		case DragonFlightState.turningRight:
			if (actualGlideSpeed < targetGlideSpeed) {
				state = DragonFlightState.flappingRight;
			}
			if (Mathf.Abs(actualAngleSpeed) < maxAngleSpeed / 2f) {
				state = DragonFlightState.gliding;
			}
			break;
		case DragonFlightState.flappingLeft:
			if (actualAngleSpeed < maxAngleSpeed / 2f) {
				state = DragonFlightState.flapping;
			}
			if (Mathf.Abs(actualAngleSpeed) < maxAngleSpeed / 2f) {
				state = DragonFlightState.gliding;
			}
			break;
		case DragonFlightState.flappingRight:
			if (actualAngleSpeed < maxAngleSpeed / 2f) {
				state = DragonFlightState.flapping;
			}
			if (Mathf.Abs(actualAngleSpeed) < maxAngleSpeed / 2f) {
				state = DragonFlightState.gliding;
			}
			break;
		case DragonFlightState.hovering:
			break;
		case DragonFlightState.breaking:
			break;
		}

		if (!useWaypoints && atTarget) {
			state = DragonFlightState.hovering;
		}

		if(state!=before)
			anim.SetInteger ("State", (int)state);
	}
	//patrzy gdzie znajduje się cel... może warto wrzucić to do korutyny?
	Vector3 FindTarget(){
		target_pos = transform.InverseTransformPoint(target.position);
		CalculateBasicTargetInfo (out angleHor, out angleVer, out distHor, out distVer, Vector3.zero, target_pos);
		CalculateBasicTargetInfo (out angleHorGlob, out angleVerGlob, out distHorGlob, out distVerGlob, transform.position, target.position);
		CalculateNeededSpeed ();
		return target_pos;
	}
	//obliczenie kilku parametrów, które wskażą drogę do celu
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
		//targetAngularSpeed = -360f * actualGlideSpeed / Mathf.PI / distHor;
		targetAngularSpeed = -angleHor;

		targetGlideSpeed = maxGlideSpeed;
		if (angleVerGlob < maxAngleUp || actualGlideSpeed > Mathf.Sqrt (g * distVerGlob / 2)) {
			targetPitch = angleVerGlob;
		} else {
			targetPitch = maxAngleUp;
		}
	}
	//dodaje prędkość postępową o dmachania skrzydłami za pomocą eventu w animacji
	public void WingFlaped (){
		if (state != DragonFlightState.hovering && actualGlideSpeed < maxGlideSpeed)
			actualGlideSpeed += flapSpeed * flapPower;
	}

}
