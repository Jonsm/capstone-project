using UnityEngine;
using System.Collections;

//wasd to move forward, backward, sideways, space to move up, shift to move down
//mouse to look around
public class CameraMovementTMP : MonoBehaviour {
	private float forceMultiplier = 40;
	private float dragForce = 2;

	void Start () {
		gameObject.GetComponent<Rigidbody>().drag = dragForce;
	}

	void Update () {
		//compute forward, left, and right (relative to camera's rotation)
		Vector3 forward = gameObject.transform.forward;
		forward = new Vector3 (forward.x, 0, forward.z);
		forward = forward.normalized;
		Vector3 left = Vector3.Cross (forward, Vector3.up);
		Vector3 right = Vector3.Cross (forward, Vector3.down);

		//forward backward
		if (Input.GetKey ("w")) {
			gameObject.GetComponent<Rigidbody>().AddForce (forceMultiplier * forward);
		} else if (Input.GetKey ("s")) {
			gameObject.GetComponent<Rigidbody>().AddForce (-1 * forceMultiplier * forward);
		}

		//left right
		if (Input.GetKey ("a")) {
			gameObject.GetComponent<Rigidbody>().AddForce (forceMultiplier * left);
		} else if (Input.GetKey ("d")) {
			gameObject.GetComponent<Rigidbody>().AddForce (forceMultiplier * right);
		}

		//up down
		if (Input.GetKey ("space")) {
			gameObject.GetComponent<Rigidbody>().AddForce (forceMultiplier * Vector3.up);
		} else if (Input.GetKey (KeyCode.LeftShift)) {
			gameObject.GetComponent<Rigidbody>().AddForce (forceMultiplier * Vector3.down);
		}

		//rotate view
		Vector2 mp = Input.mousePosition;
		float pitch = Mathf.Lerp (-89, 89, 1 - mp.y / Screen.height);
		float yaw = Mathf.Lerp (-180, 180, ProperMod (mp.x, Screen.width) / Screen.width);
		GetComponent<Rigidbody>().rotation = Quaternion.Euler (new Vector3 (pitch, yaw, 0));
	}

	//returns a % b that works on negative numbers
	float ProperMod (float a, float b) {
		if (a > 0) return a % b;
		else {
			int quotient = (int) (a / b);
			return a - (quotient - 1) * b;
		}
	}
}
