using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class HUD : VRGUI {

	private string score = "0";

	// Use this for initialization
	public override void OnVRGUI()
	{
		GUIStyle style = new GUIStyle(GUI.skin.label);
		style.fontSize = 30;
		GUI.contentColor = Color.white;
		GUI.Label (new Rect (Screen.width / 2, Screen.height - 100, 800, 100), "Score: " + score, style); 
	}

	public void setScore(string score) {
		this.score = score;
	}
}