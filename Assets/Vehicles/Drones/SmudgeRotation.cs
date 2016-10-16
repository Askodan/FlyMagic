using UnityEngine;
using System.Collections;

public class SmudgeRotation : MonoBehaviour {
	Renderer[] Renderers;
	public float rotationwithFullSmudge;
	void Start(){
		Renderers = GetComponentsInChildren<Renderer> ();
	}
	public void Smudge(float speed){
		float degree = Mathf.Lerp (0f, 0.5f, Mathf.Abs(speed/rotationwithFullSmudge));
		for (int i = 0; i < Renderers.Length; i++) {
			Renderers [i].material.SetColor ("_TintColor", new Color (degree, degree, degree, degree));
		}
	}
}
