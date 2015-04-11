using UnityEngine;
using System.Collections;

public class PinkyMovement : MonoBehaviour {

	public float speedMultiplier;
	
	private Rigidbody rb;
	
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	
	void FixedUpdate() {
		float horizontal = Input.GetAxis ("Pinky Horizontal");
		float vertical = Input.GetAxis ("Pinky Vertical");
		
		Vector3 movement = new Vector3 (horizontal, vertical, 0.0f);
		transform.Translate (movement * speedMultiplier * Time.deltaTime);

		if (horizontal == 1) {
			if (vertical == 1) {
				transform.localEulerAngles = new Vector3(0.0f, 135, 0.0f);
			} else if (vertical == 0) {
				transform.localEulerAngles = new Vector3(0.0f, 90, 0.0f);
			} else {
				transform.localEulerAngles = new Vector3(0.0f, 45, 0.0f);
			}
		} else if (horizontal == 0) {

		} else {

		}
	}

	void OnTriggerEnter(Collider other) {
		if (other.gameObject.tag == "Teleporter") {
			if (transform.localPosition.x < -13) {
				transform.Translate (Vector3.right * 26, Space.World);
			} else {
				transform.Translate (Vector3.left * 26, Space.World);
			}
		}
	}
}
