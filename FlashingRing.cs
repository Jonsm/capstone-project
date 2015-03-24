using UnityEngine;
using System.Collections;

public class FlashingRing : MonoBehaviour {

	private GameObject circle0; 
	private GameObject circle90;
	private GameObject circle180;
	private GameObject circle270;
	float timeToGo;
	// Use this for initialization
	void Start () {
		//Get the current game object and its position and scale
		timeToGo = Time.fixedTime + 2.0f;
		GameObject cylinder = this.gameObject;
		Transform t = gameObject.transform;
		Vector3 pos = t.position;
		float x = pos.x;
		float y = pos.y;
		float z = pos.z;
		//standard height is two and radius is .5
		Vector3 scale = t.localScale;

		circle0 = GameObject.CreatePrimitive (PrimitiveType.Sphere);
		circle90 = GameObject.CreatePrimitive (PrimitiveType.Sphere);
		circle180 = GameObject.CreatePrimitive (PrimitiveType.Sphere);
		circle270 = GameObject.CreatePrimitive (PrimitiveType.Sphere);

		circle0.transform.position = new Vector3 ((x + .5f*scale.x)*1.1f ,y,(z + .5f * scale.z)*1.1f);
		circle0.transform.localScale = new Vector3 (.2f* scale.x, .2f*scale.y,.2f*scale.z);
		circle90.transform.position = new Vector3 ((x - .5f * scale.x)*1.1f, y, (z + .5f * scale.z)*1.1f);
		circle90.transform.localScale = new Vector3 (.2f* scale.x, .2f*scale.y,.2f*scale.z);
		circle180.transform.position = new Vector3 (1.1f*(x - .5f * scale.x), y, (z - .5f * scale.z)*1.1f);
		circle180.transform.localScale = new Vector3 (.2f* scale.x, .2f*scale.y,.2f*scale.z);
		circle270.transform.position = new Vector3 ((x + .5f * scale.x)*1.1f, y, (z - .5f * scale.z)*1.1f);
		circle270.transform.localScale = new Vector3 (.2f* scale.x, .2f*scale.y,.2f*scale.z);

	}
	void FixedUpdate() {
		float scy = this.gameObject.transform.localScale.y;
		if (Time.fixedTime >= timeToGo) {
			circle0.transform.position = 
				new Vector3 (circle0.transform.position.x,circle0.transform.position.y + scy*.2f ,circle0.transform.position.z);
			circle90.transform.position = 
				new Vector3 (circle90.transform.position.x,circle0.transform.position.y + scy*.2f ,circle90.transform.position.z);
			circle180.transform.position = 
				new Vector3 (circle180.transform.position.x,circle0.transform.position.y + scy*.2f ,circle180.transform.position.z);
			circle270.transform.position = 
				new Vector3 (circle270.transform.position.x,circle0.transform.position.y + scy*.2f ,circle270.transform.position.z);
			timeToGo = Time.fixedTime + 2.0f;
		}
	}
}
