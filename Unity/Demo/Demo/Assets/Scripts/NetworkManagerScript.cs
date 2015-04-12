using UnityEngine;
using System.Collections;

public class NetworkManagerScript : MonoBehaviour {

	private const string typeName = "UniqueGameName";
	private const string gameName = "RoomNameWithMatAndFelix";
	
	private HostData[] hostList;
	
	public GameObject pacman;
	
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
		DoTheDance ();
		GameObject.Find ("OVRPlayerController/OVRCameraRig").SetActive (false);
	}

	public IEnumerator DoTheDance() {
		yield return new WaitForSeconds(1); // waits 1 seconds
	}
	
	private void SpawnPlayer()
	{
		Network.Instantiate(pacman, new Vector3(0f, 5f, 0f), Quaternion.identity, 0);

	}
}
