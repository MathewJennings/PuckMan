using UnityEngine;
using System.Collections;

public class Menu : VRGUI {

	public GUISkin skin;

	// Use this for initialization
	public override void OnVRGUI()
	{
		GUI.Label(new Rect(Screen.width/2, Screen.height - 100, 800, 100), "Press Space For Menu"); 

	}
}