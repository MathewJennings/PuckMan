using UnityEngine;
using System.Collections;

public class NetworkManagerScript : MonoBehaviour {

	private const string typeName = "Pacman!$!$";
	private const string gameName = "PacmanRoom1";
	
	private HostData[] hostList = null;
	
	public GameObject pacman;
	public GameObject blinky;
	public GameObject pinky;
	public GameObject inky;
	public GameObject clyde;

	int cPresses = 0;
	
	private void StartServer()
	{
		Network.InitializeServer(4, 25000, !Network.HavePublicAddress());
		MasterServer.RegisterHost(typeName, gameName);
	}

	// This is called right after server started
	void OnServerInitialized()
	{
		SpawnPlayer ();
	}

	void OnGUI()
	{
		if (!Network.isClient && !Network.isServer)
		{
			if (Input.GetKeyDown(KeyCode.S)) {
				StartServer ();
			}

			if (Input.GetKeyDown (KeyCode.C)) {
				cPresses++;
				if (cPresses == 1) {
					RefreshHostList();
				} else if (hostList != null) {
					if (hostList.Length > 0) {
						JoinServer (hostList[0]);
					}
				}
			}
			/*
			if (GUI.Button(new Rect(100, 100, 250, 100), "Start Server"))
				StartServer();
			if (GUI.Button(new Rect(100, 250, 250, 100), "Refresh Hosts"))
				RefreshHostList();
			
			if (hostList != null)
			{
				for (int i = 0; i < hostList.Length; i++)
				{
					if (GUI.Button(new Rect(400, 100 + (110 * i), 300, 100), hostList[i].gameName))
						JoinServer(hostList[i]);
				}
			}
			*/
		}
	}
	
	private void RefreshHostList()
	{
		MasterServer.RequestHostList(typeName);
	}
	
	void OnMasterServerEvent(MasterServerEvent msEvent)
	{
		if (msEvent == MasterServerEvent.HostListReceived)
			hostList = MasterServer.PollHostList();
	}

	private void JoinServer(HostData hostData)
	{
		Network.Connect(hostData);
	}
	
	void OnConnectedToServer()
	{
		/*DoTheDance ();
		GameObject o1 = GameObject.Find ("OVRPlayerController(Clone)");
		if (o1 == null) {
			print ("YOU'RE A BIG o1 NULL BABY");
		} else {
			GameObject o2 = GameObject.FindWithTag ("Pac Cam").transform.GetChild(0).gameObject;
			if (o2 == null) {
				print ("YOU'RE A BIG o2 NULL BABY");
			} else {
				
			}
		}
		Camera cam = GameObject.Find ("OVRPlayerController(Clone)/OVRCameraRig").GetComponent<Camera>();
		cam.enabled = false;
		Camera cam2 = GameObject.Find ("Camera").GetComponent<Camera>();
		cam2.enabled = true;
*/

		GameObject pacCam = GameObject.Find ("OVRPlayerController(Clone)/OVRCameraRig");
		GameObject specCam = GameObject.Find ("Camera");
		/*pacCam.GetComponent<Camera> ().rect = new Rect (0f, 0f, 0f, 0f);
		pacCam.GetComponent<Camera> ().depth = 2;
		specCam.GetComponent<Camera> ().rect = new Rect (0f, .5f, .5f, 1f);
		specCam.GetComponent<Camera> ().depth = 1;*/
		//pacCam.SetActive (false);

		GameObject.Find ("FakeOVRCameraRig").SetActive (false);
	}
	
	private void SpawnPlayer()
	{
		Network.Instantiate(pacman, new Vector3(.38f, .8f, -6.26f), Quaternion.identity, 0);
		Quaternion q = Quaternion.identity;
		q.eulerAngles = new Vector3(90,0,0);
		Network.Instantiate(blinky, new Vector3(-1.82f, .8f, 3.44f), q, 0);
		Network.Instantiate(pinky, new Vector3(-.22f, .8f, 2.44f), q, 0);
		Network.Instantiate(inky, new Vector3(.88f, .8f, 3.44f), q, 0);
		Network.Instantiate(clyde, new Vector3(2.48f, .8f, 2.44f), q, 0);


	}
}
