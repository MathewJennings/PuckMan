using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ClientHUDTextManager : MonoBehaviour {

	public Text scoreText;
	public Text livesText;
	public Text pelletText;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		GameObject player = GameObject.Find("OVRPlayerController(Clone)");
		if (player != null) {
			scoreText.text = "SCORE: " + player.GetComponent<OVRPlayerController>().getScore();
			livesText.text = "LIVES: " + player.GetComponent<OVRPlayerController>().getLives();
			pelletText.text = "PELLETS: " + player.GetComponent<OVRPlayerController>().getPelletsRemaining();
		}
	}
}
