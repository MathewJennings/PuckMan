using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {

	public int speedMultiplier;
	public Vector3 eulerAngleVelocity;

	private Quaternion cameraRotation;
	private Rigidbody rb;

	// Use this for initialization
	void Start () {
		rb = GetComponent<Rigidbody> ();
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		//cameraRotation = camera.rightEyeAnchor.transform.localRotation;
		//transform.rotation = cameraRotation;

		if (Input.GetKey("w")) {
			rb.MovePosition(transform.position + transform.up * speedMultiplier * Time.deltaTime);
		} else if (Input.GetKey("s")) {
			rb.MovePosition(transform.position + transform.up * speedMultiplier * Time.deltaTime * -1);
		}
		if (Input.GetKey("a")) {
			Quaternion deltaRotation = Quaternion.Euler (eulerAngleVelocity * Time.deltaTime);
			rb.MoveRotation(rb.rotation * deltaRotation);
		} else if (Input.GetKey("d")) {
			Quaternion deltaRotation = Quaternion.Euler (eulerAngleVelocity * Time.deltaTime * -1);
			rb.MoveRotation(rb.rotation * deltaRotation);
		}
	}
}
