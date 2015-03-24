using UnityEngine;
using System.Collections;

//wasd to move forward, backward, sideways, space to move up, shift to move down
//mouse to look around
public class CameraMovementTMP : MonoBehaviour {
//<<<<<<< HEAD
	//private float forceMultiplier = 1000;
	//private float dragForce = 5;
//=======
	private float forceMultiplier = 65;
	private float dragForce = 2;
	private bool canJump = true;
//>>>>>>> 70161d8350f414e905e14a00705a34c59f7c018e

	void Start () {
		Physics.gravity = new Vector3 (0, -10.0f, 0);
		gameObject.GetComponent<Rigidbody>().drag = dragForce;
	}

	void LateUpdate () {
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
		} else if (Input.GetKey("e") && canJump == true){

			canJump = false;
			gameObject.GetComponent<Rigidbody>().AddForce (forceMultiplier *100* Vector3.up);
		}

		//rotate view
		Vector2 mp = Input.mousePosition;
		float pitch = Mathf.Lerp (-89, 89, 1 - mp.y / Screen.height);
		float yaw = Mathf.Lerp (-180, 180, ProperMod (mp.x, Screen.width) / Screen.width);
		GetComponent<Rigidbody>().rotation = Quaternion.Euler (new Vector3 (pitch, yaw, 0));
	}

	void OnCollisionEnter(Collision collision) {
		canJump = true;
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
