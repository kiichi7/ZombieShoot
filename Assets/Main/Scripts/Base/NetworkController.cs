using UnityEngine;
using System.Collections;
using System.IO;

public class NetworkController : MonoBehaviour{
	public string connectIP = "127.0.0.1";
	public int connectPORT = 7777;
	public int serverPORT = 7777;
	public int maxPlayers = 8;
	public ArrayList playerList = new ArrayList();
	
	public int mapIndex = 0;
	public GameObject[] playerPrefabs;
	
	
	//AssetBundle map;
	int _llPrefix;
	Loader _loader;
	
	[HideInInspector] public string username;
	[HideInInspector] public string[] mapsURLs;
	[HideInInspector] public WWW mapLoad;
	
	void Start () {
		DontDestroyOnLoad(gameObject);
		//mapsURLs = Directory.GetFiles(Application.dataPath + "/../Packages/Maps/", "*.map");
		_loader = FindObjectOfType(typeof(Loader)) as Loader;
		//Application.LoadLevel(1);
	}

	public void Connect(){
		Network.Connect(connectIP, connectPORT);
		Debug.Log("Connecting to " + connectIP + ":" + connectPORT);  
	}
	
	public void Disconnect(int _TIMEOUT){
		Network.Disconnect(_TIMEOUT);
		Debug.Log("Disconnected");
	}
	
	public void CloseConnection(NetworkPlayer player, string username){
		Network.CloseConnection(player, true);
		Debug.Log("Connection closed for player " + username);
	}
	
	public void StartServer(){
		Network.InitializeSecurity();
    	Network.InitializeServer(maxPlayers, serverPORT, !Network.HavePublicAddress());	
		networkView.RPC("LoadLevel", RPCMode.AllBuffered, mapIndex, _llPrefix++);
	}
	/*
	public void SpawnPlayer(){
		GameObject spawnPoint = GameObject.Find("Spawnpoint");
		GameObject actor = Network.Instantiate(playerPrefabs[0], spawnPoint.transform.position, spawnPoint.transform.rotation, 0) as GameObject;
		foreach (GameObject go in FindObjectsOfType(typeof (GameObject)))
			go.SendMessage("OnPlayerSpawn", actor, SendMessageOptions.DontRequireReceiver);
	}*/
	/*
	public void UnloadMap(){
		map.Unload(true);
		map = null;
	}*/

	[RPC]
	IEnumerator LoadLevel(int index, int levelPrefix){
		Network.isMessageQueueRunning = false;
		_llPrefix = levelPrefix;
		Network.SetLevelPrefix(levelPrefix);
		//if(map)
			//UnloadMap();
		//mapLoad = new WWW ("file://" + mapsURLs[index]);
		//yield return mapLoad;
		//map = mapLoad.assetBundle;
		Application.LoadLevel("Streamed");
		yield return null;
		yield return null;
		Network.isMessageQueueRunning = true;
		foreach (GameObject go in FindObjectsOfType(typeof (GameObject)))
			go.SendMessage("OnNetworkLoadedLevel", SendMessageOptions.DontRequireReceiver);	
	}

	[RPC]
	void AddPlayerToList(NetworkPlayer player, string username, bool host){
		PlayerInfo newPlayerInfo = new PlayerInfo();
		
		newPlayerInfo.player = player;
		newPlayerInfo.username = username;
		newPlayerInfo.host = host;
		
		playerList.Add(newPlayerInfo);
		Debug.Log("Add Palyer: " + username);
	}
	
	[RPC]
	void RemovePlayerFromList(NetworkPlayer player){
		foreach (PlayerInfo playerInstance in playerList) {
			if (player == playerInstance.player){	
				playerList.Remove(playerInstance);
				break;
			}
		}
	}
	
	[RPC]
	void SpawnPlayer(NetworkViewID viewID){
		GameObject spawnPoint = GameObject.FindGameObjectWithTag("Spawnpoint_Player");
		Object player = _loader.GetAsset("Test_Player_Male", _loader.FileDB_Player);
		GameObject actor = Instantiate(player, spawnPoint.transform.position, spawnPoint.transform.rotation) as GameObject;
		
		actor.networkView.viewID = viewID;
		actor.name = actor.networkView.isMine ? "LocalPlayer" : "RemotePlayer";
		actor.GetComponent<PlayerController>().AddAnimations();
		
		if(actor.networkView.isMine){
			foreach (GameObject go in FindObjectsOfType(typeof (GameObject)))
				go.SendMessage("OnPlayerSpawn", actor, SendMessageOptions.DontRequireReceiver);
		}
		else{
			Destroy(actor.GetComponent<Rigidbody>());
			Destroy(actor.GetComponent<SphereCollider>());
			Destroy(actor.GetComponent<NavMeshObstacle>());
		}

	}
	
	[RPC]
	public void SpawnAI(NetworkViewID viewID){
		GameObject spawnPoint = GameObject.FindGameObjectWithTag("Spawnpoint_AI");
		Object enemy = _loader.GetAsset("zombie_male_1", _loader.FileDB_Zombies);
		GameObject actor = Instantiate(enemy, spawnPoint.transform.position, spawnPoint.transform.rotation) as GameObject;
		actor.networkView.viewID = viewID;
		actor.name = "AI";
	}
	
	void OnNetworkLoadedLevel(){
		if(_loader.GetAsset("Test_Player_Male", _loader.FileDB_Player))
			networkView.RPC("SpawnPlayer", RPCMode.AllBuffered, Network.AllocateViewID());
	}
	
	void OnServerInitialized() {
		username = PlayerPrefs.GetString("USERNAME");
		networkView.RPC("AddPlayerToList",RPCMode.AllBuffered, Network.player, username, true);
		Debug.Log("Server initialized and ready");
	}
	
	void OnConnectedToServer() {
		username = PlayerPrefs.GetString("USERNAME");
		networkView.RPC("AddPlayerToList",RPCMode.AllBuffered, Network.player, username, false);
		Debug.Log("Player connected from: " + Network.player.ipAddress +":" + Network.player.port);
	}
	
	void OnPlayerDisconnected(NetworkPlayer player) {
		Debug.Log("Clean up after player " + player);
		networkView.RPC("RemovePlayerFromList", RPCMode.All, player);
		Network.RemoveRPCs(player);
		Network.DestroyPlayerObjects(player);
	}
	
	void OnDisconnectedFromServer(NetworkDisconnection info) {
        playerList.Clear();
		Application.LoadLevel(1);
		
		if (Network.isServer)
            Debug.Log("Local server connection disconnected");
        else
            if (info == NetworkDisconnection.LostConnection)
                Debug.Log("Lost connection to the server");
            else
                Debug.Log("Successfully diconnected from the server");
    }
	
	void OnDataWasLoaded(){
		Application.LoadLevel(1);
	}
}
