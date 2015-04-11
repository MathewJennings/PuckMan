using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class HUD : VRGUI {

	private string score = "0";
	private string remainingPellets = "";
	private string remainingLives = "3";

	// Use this for initialization
	public override void OnVRGUI()
	{
		remainingPellets = GameObject.Find ("OVRPlayerController").GetComponent<OVRPlayerController> ().pelletsRemaining.ToString();
		GUIStyle style = new GUIStyle(GUI.skin.label);
		style.fontSize = 30;
		GUI.contentColor = Color.white;
		GUI.Label (new Rect (100, 50, 800, 100), "Score: " + score, style); 
		GUI.Label (new Rect (300, 50, 800, 100), "Remaining Pellets: " + remainingPellets, style); 
		GUI.Label (new Rect (100, 75, 800, 100), "Lives: " + remainingLives, style);
	}

	public void setScore(string score) {
		this.score = score; 
	}

	public void setRemainingPellets(string remainingPellets) {
		this.remainingPellets = remainingPellets; 
	}

	public void setRemainingLives(string remainingLives) {
		this.remainingLives = remainingLives; 
	}
}