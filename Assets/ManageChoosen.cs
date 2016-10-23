using UnityEngine;
using System.Collections;

public enum SelectableObjectStates{
	NotAvailable,
	Available,
	Choosen
}
public class ManageChoosen : MonoBehaviour {
	public Material Available;
	public Material Choosen;
	public ParticleSystem Effect;
	//umarło bo nie można zmieniać kształtów w skrypcie
	/*// Use this for initialization
	void Awake () {
		GameObject effect = Instantiate (Effect.gameObject);
		effect.transform.parent = transform;
		effect.transform.localPosition = Vector3.zero;
		effect.transform.localRotation = Quaternion.identity;
		Effect = effect.GetComponent<ParticleSystem> ();
		ParticleSystem.ShapeModule shape = Effect.shape;
		shape.meshRenderer = GetComponentInChildren<MeshRenderer>();
		Effect.shape = shape;
		effect.SetActive (false);
	}*/

	public void SetState(SelectableObjectStates newstate){
		switch (newstate) {
		case SelectableObjectStates.NotAvailable:
			Effect.gameObject.SetActive (false);
			break;
		case SelectableObjectStates.Available:
			if (!Effect.gameObject.activeSelf) {
				Effect.gameObject.SetActive (true);
			}
			Effect.GetComponent<ParticleSystemRenderer> ().material = Available;
			break;
		case SelectableObjectStates.Choosen:
			if (!Effect.gameObject.activeSelf) {
				Effect.gameObject.SetActive (true);
			}
			Effect.GetComponent<ParticleSystemRenderer> ().material = Choosen;
			break;
		}
	}
}
