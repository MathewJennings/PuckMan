
private var gameName = "PacManUncontrollablyGuzzlingHotSauce";
private var refreshing:boolean; 
private var hostData: HostData[];

public var pacman: Transform;
public var inky: Transform;
public var clyde: Transform;
public var blinky: Transform;
public var pinky: Transform;

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
		}
		if(Input.GetKeyDown('0')) {
			refreshHostLists();
			if (hostData) {
				Network.Connect(hostData[0]);
			}
		}
	}
}

private var playerCount: int = 0;
function OnPlayerConnected(player: NetworkPlayer) {
		Debug.Log("Player " + playerCount++ + 
			      " connected from " + player.ipAddress + 
	              ":" + player.port);
		Network.Instantiate(pacman, new Vector3(0.38, 0.8, -6.26), Quaternion.identity, 0);
		Network.Instantiate(inky, new Vector3(0.88, 0.8, 3.44), Quaternion.identity, 0);
		Network.Instantiate(clyde, new Vector3(2.48, 0.8, 2.44), Quaternion.identity, 0);
		Network.Instantiate(blinky, new Vector3(-1.82, 0.8, 3.44), Quaternion.identity, 0);
		Network.Instantiate(pinky, new Vector3(-0.22, 0.8, 2.44), Quaternion.identity, 0);
		
		GameObject.Find("/OVRPlayerController/OVRCameraRig").SetActive(true);
		GameObject.Find("/Camera").SetActive(false);
}

	
function OnConnectedToServer() {
	Debug.Log("Connected to server");
	// Send local player name to server ...
	GameObject.Find("/OVRPlayerController/OVRCameraRig").SetActive(false);
	GameObject.Find("/Camera").SetActive(true);
}