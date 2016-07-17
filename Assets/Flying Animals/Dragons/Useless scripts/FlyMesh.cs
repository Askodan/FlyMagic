using UnityEngine;
using System.Collections;
[System.Serializable]
public struct Wingllider{
	public string name{ get; set; }
	public Transform bone{ get; set; }
	public BoxCollider collider{ get; set; }
	public float maxArea{ get; set; }
	public float minArea{ get; set; }
	public void init(){
		collider.transform.SetParent (bone);

		float[] areas = new float[3];
		areas [0] = collider.size.x * collider.size.y;
		areas [1] = collider.size.x * collider.size.z;
		areas [2] = collider.size.z * collider.size.y;
		maxArea = Mathf.Max (areas);
		minArea = Mathf.Min (areas);
	}
}
public class FlyMesh : MonoBehaviour {
	Wingllider[] winglliders;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
