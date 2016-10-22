using UnityEngine;
using System.Collections;
[System.Serializable]
public class PIDController {
	/// <summary>
	/// The overall amplify.
	/// </summary>
	[Tooltip("Wzmocnienie całego wyjścia")]
	public float overallAmplify =1f;
	/// <summary>
	/// The proportional gain.
	/// </summary>
	[Tooltip("Wzmocnienie członu P")]
	public float proportionalConst;
	/// <summary>
	/// The integral  gain.
	/// </summary>
	[Tooltip("Wzmocnienie członu I")]
	public float integralConst=100f;
	/// <summary>
	/// The max absolute value of the integral. Should be positive
	/// </summary>
	[Tooltip("Ograniczenie siły całki")]
	public float maxIntegralAbs;
	float integral;
	/// <summary>
	/// The derivative gain.
	/// </summary>
	[Tooltip("Wzmocnienie członu D")]
	public float derivativeConst;
	float prevError;
	/// <summary>
	/// Regulate the specified errorValue.
	/// </summary>
	/// <param name="errorValue">Error value.</param>
	public float Regulate(float errorValue){
		integral += errorValue * Time.deltaTime;
		if (Mathf.Abs (integral) > maxIntegralAbs) {
			integral = Mathf.Sign(integral)*maxIntegralAbs;
		}
		float derivative = (errorValue - prevError) / Time.deltaTime;
		prevError = errorValue;
		return (proportionalConst*errorValue+integralConst*integral+derivativeConst*derivative)*overallAmplify;
	}
	/// <summary>
	/// Resets the intergral to value 0.
	/// </summary>
	public void ResetIntergral(){
		integral = 0f;
	}
}
