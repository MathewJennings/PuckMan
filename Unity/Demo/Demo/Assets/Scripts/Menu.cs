using UnityEngine;
using System.Collections;

public class Menu : VRGUI {

	public GUISkin skin;

	// Use this for initialization
	public override void OnVRGUI()
	{
		GUI.skin = skin;
		
		GUI.Label(new Rect(0f, 0f, 600f, 100f), "Time: " + Time.time);
		
		GUI.Label(new Rect(Screen.width/2 - 400, Screen.height - 100, 800, 100), "Press Space For Menu"); 
	}
}