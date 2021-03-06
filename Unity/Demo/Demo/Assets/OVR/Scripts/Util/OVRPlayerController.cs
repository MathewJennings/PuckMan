/************************************************************************************

Copyright   :   Copyright 2014 Oculus VR, LLC. All Rights reserved.

Licensed under the Oculus VR Rift SDK License Version 3.2 (the "License");
you may not use the Oculus VR Rift SDK except in compliance with the License,
which is provided at the time of installation or download, or which
otherwise accompanies this software in either electronic or hard copy form.

You may obtain a copy of the License at

http://www.oculusvr.com/licenses/LICENSE-3.2

Unless required by applicable law or agreed to in writing, the Oculus VR SDK
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.

************************************************************************************/

using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Controls the player's movement in virtual reality.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class OVRPlayerController : MonoBehaviour
{
	public NetworkView networkView;
	/// <summary>
	/// The rate acceleration during movement.
	/// </summary>
	public float Acceleration = 0.1f;

	/// <summary>
	/// The rate of damping on movement.
	/// </summary>
	public float Damping = 0.3f;

	/// <summary>
	/// The rate of additional damping when moving sideways or backwards.
	/// </summary>
	public float BackAndSideDampen = 0.5f;

	/// <summary>
	/// The force applied to the character when jumping.
	/// </summary>
	public float JumpForce = 0.3f;

	/// <summary>
	/// The rate of rotation when using a gamepad.
	/// </summary>
	public float RotationAmount = 1.5f;

	/// <summary>
	/// The rate of rotation when using the keyboard.
	/// </summary>
	public float RotationRatchet = 45.0f;

	/// <summary>
	/// If true, reset the initial yaw of the player controller when the Hmd pose is recentered.
	/// </summary>
	public bool HmdResetsY = true;

	/// <summary>
	/// If true, tracking data from a child OVRCameraRig will update the direction of movement.
	/// </summary>
	public bool HmdRotatesY = true;

	/// <summary>
	/// Modifies the strength of gravity.
	/// </summary>
	public float GravityModifier = 0.379f;
	
	/// <summary>
	/// If true, the OVRPlayerController will use the player's profile data for height, eye depth, etc.
	/// </summary>
	public bool useProfileData = true;

	protected CharacterController Controller = null;
	protected OVRCameraRig CameraRig = null;

	private float MoveScale = 1.0f;
	private Vector3 MoveThrottle = Vector3.zero;
	private float FallSpeed = 0.0f;
	private OVRPose? InitialPose;
	private float InitialYRotation = 0.0f;
	private float MoveScaleMultiplier = 1.0f;
	private float RotationScaleMultiplier = 1.0f;
	private bool  SkipMouseRotation = false;
	private bool  HaltUpdateMovement = false;
	private bool prevHatLeft = false;
	private bool prevHatRight = false;
	private float SimulationRate = 60f;

	//pacman specific variable
	private int score = 0;
	public int getScore() {return score;}
	private int lives = 3;
	public int getLives() {return lives;}
	public int totalNumberPellets = 179;
	public int pelletsRemaining = 179;
	public int getPelletsRemaining() {return pelletsRemaining;}
	private int timer = 0;
	private int tStamp = 0;
	private bool isSuper = false;
	private int superKills = 0;
	private Color blinkyOriginal;
	private Color clydeOriginal;
	private Color inkyOriginal;
	private Color pinkyOriginal;
	public bool getSuper() {
		return isSuper;
	}
	private List<GameObject> inactiveGhosts = new List<GameObject>();
	public AudioClip waka;
	public AudioClip cherry;

	void Awake()
	{
		networkView = GetComponent<NetworkView>();

		Controller = gameObject.GetComponent<CharacterController>();

		if(Controller == null)
			Debug.LogWarning("OVRPlayerController: No CharacterController attached.");

		// We use OVRCameraRig to set rotations to cameras,
		// and to be influenced by rotation
		OVRCameraRig[] CameraRigs = gameObject.GetComponentsInChildren<OVRCameraRig>();

		if(CameraRigs.Length == 0)
			Debug.LogWarning("OVRPlayerController: No OVRCameraRig attached.");
		else if (CameraRigs.Length > 1)
			Debug.LogWarning("OVRPlayerController: More then 1 OVRCameraRig attached.");
		else
			CameraRig = CameraRigs[0];

		InitialYRotation = transform.rotation.eulerAngles.y;
	}

	void OnEnable()
	{
		OVRManager.display.RecenteredPose += ResetOrientation;

		if (CameraRig != null)
		{
			CameraRig.UpdatedAnchors += UpdateTransform;
		}
	}

	void OnDisable()
	{
		OVRManager.display.RecenteredPose -= ResetOrientation;

		if (CameraRig != null)
		{
			CameraRig.UpdatedAnchors -= UpdateTransform;
		}
	}

	void OnTriggerEnter(Collider other) {
		if (networkView.isMine) {
			if (other.gameObject.tag == "Teleporter") {
				if (transform.localPosition.x < -13) {
					transform.Translate (Vector3.right * 26, Space.World);
				} else {
					transform.Translate (Vector3.left * 26, Space.World);
				}
			} else if (other.gameObject.tag == "Pellet") {
				GetComponent<AudioSource> ().PlayOneShot (waka);
				other.gameObject.SetActive (false);
				SetScore(score + 100);
				SetPelletsRemaining(pelletsRemaining - 1);
				updateDisplayText ();
			} else if (other.gameObject.tag == "Power Pellet") {
				GetComponent<AudioSource> ().PlayOneShot (waka);
				other.gameObject.SetActive (false);
				SetScore (score + 500);
				tStamp = timer;
				SetPelletsRemaining(pelletsRemaining - 1);
				updateDisplayText ();
				isSuper = true;
				ChangeGhostColor(isSuper);
			} else if (other.gameObject.tag == "Cherry") {
				other.gameObject.SetActive (false);
				GetComponent<AudioSource> ().PlayOneShot (cherry);
				SetScore (score + 1000);
				updateDisplayText ();
			} else if (other.gameObject.tag == "Ghost") {
				if (isSuper) {
					//other.gameObject.SetActive (false);
					string ghostName = other.gameObject.name;
					switch (ghostName) {
					case "Blinky(Clone)" :
						SetGhostDisabled(0);
						break;
					case "Pinky(Clone)" :
						SetGhostDisabled(1);
						break;
					case "Inky(Clone)" :
						SetGhostDisabled(2);
						break;
					case "Clyde(Clone)" :
						SetGhostDisabled(3);
						break;
					}
					superKills++;
					if (superKills == 1) {
						SetScore (score + 2000);
					} else if (superKills == 2) {
						SetScore (score + 4000);
					} else if (superKills == 3) {
						SetScore (score + 8000);
					} else {
						SetScore (score + 16000);
					}
				} else {
					SetLives(lives-1);
					resetPositions ();
					if (lives == 0) {
						resetGame ();			
					}
				}
				updateDisplayText ();
			}
		}
	}

	void OnControllerColliderHit(ControllerColliderHit hit) {
		Collider other = hit.collider;
		if (other == null)
			return;
		
		if (other.gameObject.tag == "Ghost") {
			if (isSuper) {
				//other.gameObject.SetActive (false);
				string ghostName = other.gameObject.name;
				switch (ghostName) {
				case "Blinky(Clone)" :
					SetGhostDisabled(0);
					break;
				case "Pinky(Clone)" :
					SetGhostDisabled(1);
					break;
				case "Inky(Clone)" :
					SetGhostDisabled(2);
					break;
				case "Clyde(Clone)" :
					SetGhostDisabled(3);
					break;
				}
				superKills++;
				if (superKills == 1) {
					SetScore (score + 2000);
				} else if (superKills == 2) {
					SetScore (score + 4000);
				} else if (superKills == 3) {
					SetScore (score + 8000);
				} else {
					SetScore (score + 16000);
				}
			} else {
				SetLives (lives-1);
				resetPositions ();
				if (lives == 0) {
					resetGame ();			
				}
			}
			updateDisplayText ();
		}
	}

	[RPC] void resetGame() {
		// TODO: print out game over message. Reset to beginning state

		// Reset dead ghosts
		foreach (GameObject ghost in inactiveGhosts) {
			ghost.SetActive (true);
			if (ghost.name.Equals ("Blinky(Clone)")) {
				colorChange(ghost.GetComponentInChildren<Renderer>(), blinkyOriginal);
			} else if (ghost.name.Equals ("Pinky(Clone)")) {
				colorChange(ghost.GetComponentInChildren<Renderer>(), pinkyOriginal);
			} else if (ghost.name.Equals ("Inky(Clone)")) {
				colorChange(ghost.GetComponentInChildren<Renderer>(), inkyOriginal);
			} else if (ghost.name.Equals ("Clyde(Clone)")) {
				colorChange(ghost.GetComponentInChildren<Renderer>(), clydeOriginal);
			}
		}
		resetPositions ();
		//reset all pellets
		SetPelletsRemaining (totalNumberPellets);
		foreach (Transform child in GameObject.Find ("Pellets").transform) {
			child.gameObject.SetActive (true);
		}
		foreach (Transform child in GameObject.Find ("Power Pellets").transform) {
			child.gameObject.SetActive (true);
		}
		SetScore (0);
		SetLives (3);
		updateDisplayText ();
		if (networkView.isMine) {
			networkView.RPC("resetGame", RPCMode.OthersBuffered);
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

	void Start() {
		InvokeRepeating ("IncreaseTimer", 1.0F, 1.0F);
		GameObject[] ghosts = GameObject.FindGameObjectsWithTag ("Ghost");
		foreach (GameObject ghost in ghosts) {
			if (ghost.name.Equals ("Blinky(Clone)")) {
				blinkyOriginal = ghost.GetComponentInChildren<Renderer>().material.color;
			} else if (ghost.name.Equals ("Pinky(Clone)")) {
				pinkyOriginal = ghost.GetComponentInChildren<Renderer>().material.color;
			} else if (ghost.name.Equals ("Inky(Clone)")) {
				inkyOriginal = ghost.GetComponentInChildren<Renderer>().material.color;
			} else if (ghost.name.Equals ("Clyde(Clone)")) {
				clydeOriginal = ghost.GetComponentInChildren<Renderer>().material.color;
			}
		}
	}

	void IncreaseTimer() {
		timer++;
	}
	
	[RPC] void SetScore(int newScore) {
		score = newScore;
		if (networkView.isMine)
			networkView.RPC("SetScore", RPCMode.OthersBuffered, newScore);
	}

	[RPC] void SetLives(int newLives) {
		lives = newLives;
		if (networkView.isMine)
			networkView.RPC("SetLives", RPCMode.OthersBuffered, newLives);
	}

	[RPC] void SetPelletsRemaining(int newPellets) {
		pelletsRemaining = newPellets;
		if (networkView.isMine)
			networkView.RPC("SetPelletsRemaining", RPCMode.OthersBuffered, newPellets);
	}

	[RPC] void ChangeGhostColor(bool isSuper)
	{
		GameObject[] ghosts = GameObject.FindGameObjectsWithTag ("Ghost");
		foreach (GameObject ghost in ghosts) {
			if (ghost.name.Equals ("Blinky(Clone)")) {
				if (isSuper) {
					ghost.GetComponentInChildren<Renderer>().material.color = Color.blue;
				} else {
					ghost.GetComponentInChildren<Renderer>().material.color = blinkyOriginal;
				}
			} else if (ghost.name.Equals ("Pinky(Clone)")) {
				if (isSuper) {
					ghost.GetComponentInChildren<Renderer>().material.color = Color.blue;
				} else {
					ghost.GetComponentInChildren<Renderer>().material.color = pinkyOriginal;
				}
			} else if (ghost.name.Equals ("Inky(Clone)")) {
				if (isSuper) {
					ghost.GetComponentInChildren<Renderer>().material.color = Color.blue;
				} else {
					ghost.GetComponentInChildren<Renderer>().material.color = inkyOriginal;
				}
			} else if (ghost.name.Equals ("Clyde(Clone)")) {
				if (isSuper) {
					ghost.GetComponentInChildren<Renderer>().material.color = Color.blue;
				} else {
					ghost.GetComponentInChildren<Renderer>().material.color = clydeOriginal;
				}
			}
		}		
		if (networkView.isMine)
			networkView.RPC("ChangeGhostColor", RPCMode.OthersBuffered, isSuper);
	}

	[RPC] void SetGhostDisabled(int whichGhost)
	{
		string ghostName = "";
		switch (whichGhost) {
		case 0 :
			ghostName = "Blinky(Clone)";
			break;
		case 1 :
			ghostName = "Pinky(Clone)";
			break;
		case 2 :
			ghostName = "Inky(Clone)";
			break;
		case 3 :
			ghostName = "Clyde(Clone)";
			break;
		}
		GameObject[] ghosts = GameObject.FindGameObjectsWithTag ("Ghost");
		foreach (GameObject ghost in ghosts) {
			if (ghost.name.Equals(ghostName)) {
				ghost.SetActive (false);				
				inactiveGhosts.Add (ghost);
			}
		}
		if (networkView.isMine) {
			networkView.RPC ("SetGhostDisabled", RPCMode.OthersBuffered, whichGhost);
		}
	}	

	void colorChange(Renderer rd, Color color) {
		if (isSuper) {
			rd.material.color = Color.blue;
		} else {
			rd.material.color = color;
		}
	}

	void changeGhostsToSuper() {
		GameObject[] ghosts = GameObject.FindGameObjectsWithTag ("Ghost");
		foreach (GameObject ghost in ghosts) {
			if (ghost.name.Equals ("Blinky(Clone)")) {
				colorChange(ghost.GetComponentInChildren<Renderer>(), blinkyOriginal);
			} else if (ghost.name.Equals ("Pinky(Clone)")) {
				colorChange(ghost.GetComponentInChildren<Renderer>(), pinkyOriginal);
			} else if (ghost.name.Equals ("Inky(Clone)")) {
				colorChange(ghost.GetComponentInChildren<Renderer>(), inkyOriginal);
			} else if (ghost.name.Equals ("Clyde(Clone)")) {
				colorChange(ghost.GetComponentInChildren<Renderer>(), clydeOriginal);
			}
		}
	}

	void resetPositions() {
		transform.position = new Vector3(0.38f, 0.8f, -6.26f);
		transform.rotation = Quaternion.Euler(0,90,0);

		GameObject[] ghosts = GameObject.FindGameObjectsWithTag ("Ghost");
		foreach (GameObject ghost in ghosts) {
			if (ghost.name.Equals ("Blinky(Clone)")) {
				ghost.GetComponent<Transform>()
					.position = new Vector3(-1.82f, 0.8f, 3.44f);
			} else if (ghost.name.Equals ("Pinky(Clone)")) {
				ghost.GetComponent<Transform>()
					.position = new Vector3(-0.22f, 0.8f, 2.44f);
			} else if (ghost.name.Equals ("Inky(Clone)")) {
				ghost.GetComponent<Transform>()
					.position = new Vector3(0.88f, 0.8f, 3.44f);
			} else if (ghost.name.Equals ("Clyde(Clone)")) {
				ghost.GetComponent<Transform>()
					.position = new Vector3(2.48f, 0.8f, 2.44f);
			}
		}
	}

	void updateDisplayText() {
		GameObject.Find ("OVRCameraRig").GetComponent<HUD> ().setScore (score.ToString());
		GameObject.Find ("OVRCameraRig").GetComponent<HUD> ().setRemainingPellets(pelletsRemaining.ToString());
		GameObject.Find ("OVRCameraRig").GetComponent<HUD> ().setRemainingLives(lives.ToString());
	}

	protected virtual void Update()
	{
		if (useProfileData)
		{
			if (InitialPose == null)
			{
				InitialPose = new OVRPose()
				{
					position = CameraRig.transform.localPosition,
					orientation = CameraRig.transform.localRotation
				};
			}

			var p = CameraRig.transform.localPosition;
			p.y = OVRManager.profile.eyeHeight - 0.5f * Controller.height;
			p.z = OVRManager.profile.eyeDepth;
			CameraRig.transform.localPosition = p;
		}
		else if (InitialPose != null)
		{
			CameraRig.transform.localPosition = InitialPose.Value.position;
			CameraRig.transform.localRotation = InitialPose.Value.orientation;
			InitialPose = null;
		}

		UpdateMovement();

		Vector3 moveDirection = Vector3.zero;

		float motorDamp = (1.0f + (Damping * SimulationRate * Time.deltaTime));

		MoveThrottle.x /= motorDamp;
		MoveThrottle.y = (MoveThrottle.y > 0.0f) ? (MoveThrottle.y / motorDamp) : MoveThrottle.y;
		MoveThrottle.z /= motorDamp;

		moveDirection += MoveThrottle * SimulationRate * Time.deltaTime;

		// Gravity
		if (Controller.isGrounded && FallSpeed <= 0)
			FallSpeed = ((Physics.gravity.y * (GravityModifier * 0.002f)));
		else
			FallSpeed += ((Physics.gravity.y * (GravityModifier * 0.002f)) * SimulationRate * Time.deltaTime);

		moveDirection.y += FallSpeed * SimulationRate * Time.deltaTime;

		// Offset correction for uneven ground
		float bumpUpOffset = 0.0f;

		if (Controller.isGrounded && MoveThrottle.y <= 0.001f)
		{
			bumpUpOffset = Mathf.Max(Controller.stepOffset, new Vector3(moveDirection.x, 0, moveDirection.z).magnitude);
			moveDirection -= bumpUpOffset * Vector3.up;
		}

		Vector3 predictedXZ = Vector3.Scale((Controller.transform.localPosition + moveDirection), new Vector3(1, 0, 1));

		// Move contoller
		Controller.Move(moveDirection);

		Vector3 actualXZ = Vector3.Scale(Controller.transform.localPosition, new Vector3(1, 0, 1));

		if (predictedXZ != actualXZ)
			MoveThrottle += (actualXZ - predictedXZ) / (SimulationRate * Time.deltaTime);

		if (timer - tStamp >= 12) {
			if(isSuper) {
				isSuper = false;
				ChangeGhostColor(isSuper);
			}
			isSuper = false;
			superKills = 0;
		}

		if (Input.GetKeyDown (KeyCode.Backspace)) {
			resetGame ();
		}
	}

	public virtual void UpdateMovement()
	{
		if (HaltUpdateMovement)
			return;

		bool moveForward = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow);
		bool moveLeft = Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow);
		bool moveRight = Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow);
		bool moveBack = Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow);

		bool dpad_move = false;

		if (OVRGamepadController.GPC_GetButton(OVRGamepadController.Button.Up))
		{
			moveForward = true;
			dpad_move   = true;

		}
		if (OVRGamepadController.GPC_GetButton(OVRGamepadController.Button.Down))
		{
			moveBack  = true;
			dpad_move = true;
		}

		MoveScale = 1.0f;

		if ( (moveForward && moveLeft) || (moveForward && moveRight) ||
			 (moveBack && moveLeft)    || (moveBack && moveRight) )
			MoveScale = 0.70710678f;

		// No positional movement if we are in the air
		if (!Controller.isGrounded)
			MoveScale = 0.0f;

		MoveScale *= SimulationRate * Time.deltaTime;

		// Compute this for key movement
		float moveInfluence = Acceleration * 0.1f * MoveScale * MoveScaleMultiplier;

		// Run!
		if (dpad_move || Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
			moveInfluence *= 2.0f;

		Quaternion ort = transform.rotation;
		Vector3 ortEuler = ort.eulerAngles;
		ortEuler.z = ortEuler.x = 0f;
		ort = Quaternion.Euler(ortEuler);

		if (moveForward)
			MoveThrottle += ort * (transform.lossyScale.z * moveInfluence * Vector3.forward);
		if (moveBack)
			MoveThrottle += ort * (transform.lossyScale.z * moveInfluence * BackAndSideDampen * Vector3.back);
		if (moveLeft)
			MoveThrottle += ort * (transform.lossyScale.x * moveInfluence * BackAndSideDampen * Vector3.left);
		if (moveRight)
			MoveThrottle += ort * (transform.lossyScale.x * moveInfluence * BackAndSideDampen * Vector3.right);

		bool curHatLeft = OVRGamepadController.GPC_GetButton(OVRGamepadController.Button.LeftShoulder);

		Vector3 euler = transform.rotation.eulerAngles;

		if (curHatLeft && !prevHatLeft)
			euler.y -= RotationRatchet;

		prevHatLeft = curHatLeft;

		bool curHatRight = OVRGamepadController.GPC_GetButton(OVRGamepadController.Button.RightShoulder);

		if(curHatRight && !prevHatRight)
			euler.y += RotationRatchet;

		prevHatRight = curHatRight;

		//Use keys to ratchet rotation
		if (Input.GetKeyDown(KeyCode.Q))
			euler.y -= RotationRatchet;

		if (Input.GetKeyDown(KeyCode.E))
			euler.y += RotationRatchet;

		if (Input.GetKeyDown (KeyCode.Space)) {
			OVRManager.display.RecenterPose ();
		}
		float rotateInfluence = SimulationRate * Time.deltaTime * RotationAmount * RotationScaleMultiplier;

		if (!SkipMouseRotation)
			euler.y += Input.GetAxis("Mouse X") * rotateInfluence * 3.25f * .5f;

		moveInfluence = SimulationRate * Time.deltaTime * Acceleration * 0.1f * MoveScale * MoveScaleMultiplier;

#if !UNITY_ANDROID // LeftTrigger not avail on Android game pad
		moveInfluence *= 1.0f + OVRGamepadController.GPC_GetAxis(OVRGamepadController.Axis.LeftTrigger);
#endif

		float leftAxisX = OVRGamepadController.GPC_GetAxis(OVRGamepadController.Axis.LeftXAxis);
		float leftAxisY = OVRGamepadController.GPC_GetAxis(OVRGamepadController.Axis.LeftYAxis);

		if(leftAxisY > 0.0f)
			MoveThrottle += ort * (leftAxisY * moveInfluence * Vector3.forward);

		if(leftAxisY < 0.0f)
			MoveThrottle += ort * (Mathf.Abs(leftAxisY) * moveInfluence * BackAndSideDampen * Vector3.back);

		if(leftAxisX < 0.0f)
			MoveThrottle += ort * (Mathf.Abs(leftAxisX) * moveInfluence * BackAndSideDampen * Vector3.left);

		if(leftAxisX > 0.0f)
			MoveThrottle += ort * (leftAxisX * moveInfluence * BackAndSideDampen * Vector3.right);

		float rightAxisX = OVRGamepadController.GPC_GetAxis(OVRGamepadController.Axis.RightXAxis);

		euler.y += rightAxisX * rotateInfluence;

		transform.rotation = Quaternion.Euler(euler);
	}

	/// <summary>
	/// Invoked by OVRCameraRig's UpdatedAnchors callback. Allows the Hmd rotation to update the facing direction of the player.
	/// </summary>
	public void UpdateTransform(OVRCameraRig rig)
	{
		Transform root = CameraRig.trackingSpace;
		Transform centerEye = CameraRig.centerEyeAnchor;


		if (HmdRotatesY)
		{
			Vector3 prevPos = root.position;
			Quaternion prevRot = root.rotation;

			transform.rotation = Quaternion.Euler(0.0f, centerEye.rotation.eulerAngles.y, 0.0f);

			root.position = prevPos;
			root.rotation = prevRot;
		}
	}

	/// <summary>
	/// Jump! Must be enabled manually.
	/// </summary>
	public bool Jump()
	{
		if (!Controller.isGrounded)
			return false;

		MoveThrottle += new Vector3(0, JumpForce, 0);

		return true;
	}

	/// <summary>
	/// Stop this instance.
	/// </summary>
	public void Stop()
	{
		Controller.Move(Vector3.zero);
		MoveThrottle = Vector3.zero;
		FallSpeed = 0.0f;
	}

	/// <summary>
	/// Gets the move scale multiplier.
	/// </summary>
	/// <param name="moveScaleMultiplier">Move scale multiplier.</param>
	public void GetMoveScaleMultiplier(ref float moveScaleMultiplier)
	{
		moveScaleMultiplier = MoveScaleMultiplier;
	}

	/// <summary>
	/// Sets the move scale multiplier.
	/// </summary>
	/// <param name="moveScaleMultiplier">Move scale multiplier.</param>
	public void SetMoveScaleMultiplier(float moveScaleMultiplier)
	{
		MoveScaleMultiplier = moveScaleMultiplier;
	}

	/// <summary>
	/// Gets the rotation scale multiplier.
	/// </summary>
	/// <param name="rotationScaleMultiplier">Rotation scale multiplier.</param>
	public void GetRotationScaleMultiplier(ref float rotationScaleMultiplier)
	{
		rotationScaleMultiplier = RotationScaleMultiplier;
	}

	/// <summary>
	/// Sets the rotation scale multiplier.
	/// </summary>
	/// <param name="rotationScaleMultiplier">Rotation scale multiplier.</param>
	public void SetRotationScaleMultiplier(float rotationScaleMultiplier)
	{
		RotationScaleMultiplier = rotationScaleMultiplier;
	}

	/// <summary>
	/// Gets the allow mouse rotation.
	/// </summary>
	/// <param name="skipMouseRotation">Allow mouse rotation.</param>
	public void GetSkipMouseRotation(ref bool skipMouseRotation)
	{
		skipMouseRotation = SkipMouseRotation;
	}

	/// <summary>
	/// Sets the allow mouse rotation.
	/// </summary>
	/// <param name="skipMouseRotation">If set to <c>true</c> allow mouse rotation.</param>
	public void SetSkipMouseRotation(bool skipMouseRotation)
	{
		SkipMouseRotation = skipMouseRotation;
	}

	/// <summary>
	/// Gets the halt update movement.
	/// </summary>
	/// <param name="haltUpdateMovement">Halt update movement.</param>
	public void GetHaltUpdateMovement(ref bool haltUpdateMovement)
	{
		haltUpdateMovement = HaltUpdateMovement;
	}

	/// <summary>
	/// Sets the halt update movement.
	/// </summary>
	/// <param name="haltUpdateMovement">If set to <c>true</c> halt update movement.</param>
	public void SetHaltUpdateMovement(bool haltUpdateMovement)
	{
		HaltUpdateMovement = haltUpdateMovement;
	}

	/// <summary>
	/// Resets the player look rotation when the device orientation is reset.
	/// </summary>
	public void ResetOrientation()
	{
		if (HmdResetsY)
		{
			Vector3 euler = transform.rotation.eulerAngles;
			euler.y = InitialYRotation;
			transform.rotation = Quaternion.Euler(euler);
		}
	}
}

