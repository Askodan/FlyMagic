using UnityEngine;
using System.Collections;

public class DragonAnimation : MonoBehaviour {
	public bool animate;

	public Transform root;

	public Transform[] wingLeft;
	public Transform[] wingRight;
	public AnimationCurve[] wingMovement;

	public float wingSpeed;

	public Transform[] legLeft;
	public Transform[] legRight;

	public Transform[] armgLeft;
	public Transform[] armRight;

	public Transform[] head;
	public Transform[] tail;

	public Transform jaw;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if (animate) {
			//wings
			if (wingMovement.Length == wingLeft.Length && wingLeft.Length == wingRight.Length) {
				float progress = Mathf.PingPong (wingSpeed * Time.time, 1f);
				for(int i = 0; i<wingMovement.Length; i++){
					wingLeft [i].localRotation = Quaternion.Euler (0, 0, wingMovement[i].Evaluate(Time.time));//(Mathf.PingPong (wingSpeed * Time.time, 2f) - 0.75f) * 30f);
					wingRight [i].localRotation = Quaternion.Euler (0, 0, -wingMovement[i].Evaluate(Time.time));//(Mathf.PingPong (wingSpeed * Time.time, 2f) - 0.75f) * -30f);
				}
			} else {
				Debug.LogError ("Number of wing bones is wrong there is "+wingLeft.Length.ToString()+" bones for left wing, "+wingRight.Length.ToString()+" bones for right wing and"+wingMovement.Length.ToString()+" animationCurves!");
			}
		}
	}
}
