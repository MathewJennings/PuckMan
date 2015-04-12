using UnityEngine;
using System.Collections;

public class InkyMovement : MonoBehaviour {

	public float speedMultiplier;
	
	private Rigidbody rb;
	
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	
	void FixedUpdate() {
		float horizontal = Input.GetAxis ("Inky Horizontal");
		float vertical = Input.GetAxis ("Inky Vertical");
		
		Vector3 movement = new Vector3 (horizontal, vertical, 0.0f);
		transform.Translate (movement * speedMultiplier * Time.deltaTime);
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

	void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info) {
		Vector3 syncPosition = Vector3.zero;
		if (stream.isWriting) {
			syncPosition = transform.position;
			stream.Serialize (ref syncPosition);
		} else {
			stream.Serialize(ref syncPosition);
			transform.position = syncPosition;
		}
	}
}
