
private var gameName = "PacManUncontrollablyGuzzlingHotSauce";
private var refreshing:boolean; 
private var hostData: HostData[];

public var cam1: Camera;
public var cam2: Camera;
public var cam3: Camera;


function startServer() {
	Network.InitializeServer(2, 25001, !Network.HavePublicAddress);
	MasterServer.RegisterHost(gameName, "Tutorial Game Name", "This is a tutorial game");
}

function refreshHostLists() {
	MasterServer.RequestHostList(gameName);
	refreshing = true;
}

function Update() {
	if(refreshing) {
		if (MasterServer.PollHostList().Length > 0) {
			refreshing = false;
			hostData = MasterServer.PollHostList();
		}
	}
	if (!Network.isClient && !Network.isServer) {
		if(Input.GetKeyDown('=')) {
			startServer();
			GameObject.Find("/OVRPlayerController/OVRCameraRig").active = true;
			cam1.enabled = true;
     		cam2.enabled = true;
			GameObject.Find("/Camera").active = false;
     		cam3.enabled = false;
		}
		if(Input.GetKeyDown('0')) {
			refreshHostLists();
			if (hostData) {
				Network.Connect(hostData[0]);
				GameObject.Find("/OVRPlayerController/OVRCameraRig").active = false;
				cam1.enabled = false;
	     		cam2.enabled = false;
				GameObject.Find("Camera").active = true;
	     		cam3.enabled = true;
			}
		}
	}
}
private var playerCount: int = 0;
function OnPlayerConnected(player: NetworkPlayer) {
		Debug.Log("Player " + playerCount++ + 
			      " connected from " + player.ipAddress + 
	              ":" + player.port);
		Network.Instantiate(GameObject.Find("OVRPlayerController"), GameObject.Find("OVRPlayerController").transform.position, GameObject.Find("OVRPlayerController").transform.quaternion, 0);
		Network.Instantiate(GameObject.Find("Inky"), GameObject.Find("Inky").transform.position, GameObject.Find("Inky").transform.quaternion, 0);
		Network.Instantiate(GameObject.Find("Clyde"), GameObject.Find("Clyde").transform.position, GameObject.Find("Clyde").transform.quaternion, 0);
		Network.Instantiate(GameObject.Find("Blinky"), GameObject.Find("Blinky").transform.position, GameObject.Find("Blinky").transform.quaternion, 0);
		Network.Instantiate(GameObject.Find("Pinky"), GameObject.Find("Pinky").transform.position, GameObject.Find("Pinky").transform.quaternion, 0);
}

function OnConnectedToServer() {
		Debug.Log("Connected to server");
		Destroy(GameObject.Find("OVRPlayerController"));
		Destroy(GameObject.Find("Inky"));
		Destroy(GameObject.Find("Clyde"));
		Destroy(GameObject.Find("Blinky"));
		Destroy(GameObject.Find("Pinky"));
	}