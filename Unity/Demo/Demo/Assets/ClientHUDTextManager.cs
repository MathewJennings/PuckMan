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
		scoreText.text = "TITS1";
		livesText.text = "TITS2";
		pelletText.text = "TITS3";
	}
}
