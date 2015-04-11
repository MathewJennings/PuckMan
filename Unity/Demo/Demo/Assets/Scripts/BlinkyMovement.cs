﻿using UnityEngine;
using System.Collections;

public class BlinkyMovement : MonoBehaviour {

	public float speedMultiplier;

	private Rigidbody rb;

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void FixedUpdate() {
		float horizontal = Input.GetAxis ("Blinky Horizontal");
		float vertical = Input.GetAxis ("Blinky Vertical");

		Vector3 movement = new Vector3 (horizontal, vertical, 0.0f);
		transform.Translate (movement * speedMultiplier * Time.deltaTime);
	}
}
