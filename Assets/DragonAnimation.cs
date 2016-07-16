using UnityEngine;
using System.Collections;

public class DragonAnimation : MonoBehaviour {
	public bool animate;

	public Transform root;

	public Transform[] wingLeft;
	public Transform[] wingRight;
	public AnimationCurve[] wingMovement;
	public Vector2[] wingAmplitudeOffset;
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
			/*if (wingMovement.Length == wingLeft.Length && wingLeft.Length == wingRight.Length) {
				float progress = Mathf.Repeat (wingSpeed * Time.time, 1f);
				for(int i = 0; i<wingMovement.Length; i++){
					wingLeft [i].localRotation = Quaternion.Euler (0, 0, wingAmplitudeOffset[i].x*wingMovement[i].Evaluate(progress)-wingAmplitudeOffset[i].y);//(Mathf.PingPong (wingSpeed * Time.time, 2f) - 0.75f) * 30f);
					wingRight [i].localRotation = Quaternion.Euler (0, 0, -wingAmplitudeOffset[i].x*wingMovement[i].Evaluate(progress)+wingAmplitudeOffset[i].y);//(Mathf.PingPong (wingSpeed * Time.time, 2f) - 0.75f) * -30f);
				}
			} else {
				Debug.LogError ("Number of wing bones is wrong there is "+wingLeft.Length.ToString()+" bones for left wing, "+wingRight.Length.ToString()+" bones for right wing and"+wingMovement.Length.ToString()+" animationCurves!");
			}*/

		}
	}
	//IEnumerator wingsFlap
	[System.Serializable]
	class BoneGroup{
		public string name;
		public Transform[] bones;
		public Transform[] bones_rev;
		public Move[] motions;
		void setBones(float progress, int move_index){
			for(int i = 0; i<bones.Length; i++){
				bones [i].localRotation = Quaternion.Euler ( motions[move_index].axis[i]*(motions[move_index].amplitudeOffset[i].x*motions[move_index].movement[i].Evaluate(progress)-motions[move_index].amplitudeOffset[i].y));//(Mathf.PingPong (wingSpeed * Time.time, 2f) - 0.75f) * 30f);
				if(i<bones_rev.Length){
					bones_rev [i].localRotation = Quaternion.Euler ( motions[move_index].axis[i]*(-motions[move_index].amplitudeOffset[i].x*motions[move_index].movement[i].Evaluate(progress)+motions[move_index].amplitudeOffset[i].y));//(Mathf.PingPong (wingSpeed * Time.time, 2f) - 0.75f) * 30f);

				}
			}
		}
	}
	[System.Serializable]
	class Move{
		public string name;
		public AnimationCurve[] movement;
		public Vector2[] amplitudeOffset;
		public Vector3[] axis;
	}
}
