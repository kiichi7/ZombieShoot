using UnityEngine;
using System.Collections;

public class UI_Game : MonoBehaviour {
	public bool showControls;
	public bool showInventory;
	
	bool ready;
	NetworkController _NC;
	//Loader _loader;
	//PlayerController owner;
		
	void Update(){
		if(Input.GetKeyDown(KeyCode.Escape)) 
			showControls = !showControls;
		if(Input.GetKeyDown(KeyCode.I)) 
			showInventory = !showInventory;
		if(Input.GetKeyDown(KeyCode.F) && Network.isServer) 
			_NC.gameObject.networkView.RPC("SpawnAI", RPCMode.AllBuffered, Network.AllocateViewID());
	}

	void OnGUI () {
		if(Network.peerType == NetworkPeerType.Disconnected || !ready)
			return;
		
		GUI.color = Color.red;
		//GUI.Label(new Rect(10, 10, 150, 20), "Press esc to " + (showControls? "hide" : "show").ToString() + " menu");
		
		GUI.color = Color.white;
		if(showControls){
			GUI.BeginGroup(new Rect(30, 50, 300, 200));
			GUI.Box(new Rect(0, 0, 300, 200), "Menu");
			GUI.Label(new Rect(10, 20, 150, 20), "Peer Type: " + Network.peerType.ToString());
			if(GUI.Button(new Rect(30, 80, 240, 30), Network.isClient ? "Disconnect" : "Stop Server" ))
				_NC.Disconnect(150);
			GUI.EndGroup();
			
			GUI.BeginGroup(new Rect(Screen.width - 350, 50, 300, 200));
			GUI.Box(new Rect(0, 0, 300, 200), "Players");
			
			for(int i=0; i<_NC.playerList.Count; i++){
				GUI.Label(new Rect(10, 20 + i*20, 150, 20), "Name: " + ((PlayerInfo)(_NC.playerList[i])).username + " Ping: " + Network.GetAveragePing(((PlayerInfo)(_NC.playerList[i])).player));
				if(((PlayerInfo)(_NC.playerList[i])).host)
					GUI.Label(new Rect(170,  20 + i*20, 70, 20), "Host");
				else if(Network.isServer)
					if(GUI.Button(new Rect(170,  20 + i*20, 70, 20), "Kick"))
						_NC.CloseConnection(((PlayerInfo)(_NC.playerList[i])).player, ((PlayerInfo)(_NC.playerList[i])).username); 
			}

			GUI.EndGroup();
		}
		
		/*else if(showInventory){
	
			GUI.BeginGroup(new Rect(30, 50, 300, 200));
			GUI.Box(new Rect(0, 0, 300, 200), "Add Weapon");
			for(int i=0; i<_loader.DB_Weapon.Count; i++){
				if(GUI.Button(new Rect(10, 20 + i*20, 150, 20), "    "+((WeaponInfo)(_loader.DB_Weapon[i])).name))
					owner.AddWeaponOrAmmo(((WeaponInfo)(_loader.DB_Weapon[i])), 100);
			}
			
			GUI.EndGroup();
			
			GUI.BeginGroup(new Rect(370, 50, 300, 200));
				
			GUI.Box(new Rect(0, 0, 300, 200), "Weapons:");
			
			if(owner.activeWeaponID != 0){
				GUI.Box(new Rect(10, 20 + owner.weaponIndex*20, 150, 20),"");
			
				if(GUI.Button(new Rect(170, 20 + owner.weaponIndex*20, 50, 20),"Drop"))
					owner.RemoveWeapon(owner.weaponIndex);
				
				for(int i=0; i<owner.weaponIDs.Count; i++)
					GUI.Label(new Rect(10, 20 + i*20, 150, 20), owner.weaponIDs[i].ToString()/*+((WeaponInfo)(Utils.GetWeaponData(owner.weaponIDs[i]))).name);
				
			}
			else
				GUI.Label(new Rect(10, 20, 150, 20),"No Weapon Data");
			GUI.EndGroup();
		}*/
	}
	
	void OnNetworkLoadedLevel(){
		showControls = false;
		showInventory = false;
		ready = true;
		_NC = FindObjectOfType(typeof(NetworkController)) as NetworkController;
	//	_loader = FindObjectOfType(typeof(Loader)) as Loader;
	}
	/*
	void OnPlayerSpawn(GameObject player){
		owner = player.GetComponent<PlayerController>();
	}*/
}
