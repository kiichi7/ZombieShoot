using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class UI_MMenu : MonoBehaviour {
	public Color banner_color;
	
	Texture2D banner_texture;
	string menuState;
	string bunner_message;
	NetworkController _NC;
	Settings _settings;

	void Start () {
		_NC = FindObjectOfType(typeof(NetworkController)) as NetworkController;
		_settings = FindObjectOfType(typeof(Settings)) as Settings;
		_NC.username = Utils.CreateRandomString(7);
		banner_texture = Utils.CreateTexture(banner_color);

		/*StartCoroutine(DownloadString("https://dl.dropbox.com/u/34935968/Server/bunnertext.txt"));*/
	}
	
	IEnumerator DownloadString(string url){
		WWW download = new WWW(url);
		yield return download;
		if(!string.IsNullOrEmpty(download.error))
			bunner_message = "Error loading banner";
		else
			bunner_message = download.text;
	}
	
	void OnGUI(){
		if(_NC.mapLoad != null && !_NC.mapLoad.isDone)
			UI.CreateMSG(100, 40, (_NC.mapLoad.progress * 100).ToString("f0") + "%", banner_texture);
			
		if(Network.peerType == NetworkPeerType.Disconnected){
			TimeAndDateGI();
			BannerGI();
			switch (menuState){
				case "MainMenu" : MainMenu();
					break;
				case "GameCreateMenu" : GameCreateMenu();
					break;
				case "Settings" : Settings();
					break;
				case "SetName" : SetPlayerName();
					break;				
				default : SetPlayerName();
					break;
				/*case "SetPlayerName" ;
					break;*/
			}
		}
	}
	
	void MainMenu(){
		GUI.BeginGroup(new Rect(Screen.width / 2 - 150, Screen.height / 2 - 100, 300, 200));
	    GUI.Box(new Rect(0, 0, 300, 200), "Main Menu");
		
		if(GUI.Button(new Rect(50, 30, 200, 30), "Start Game"))
			menuState = "GameCreateMenu";
		
		if(GUI.Button(new Rect(50, 65, 200, 30), "Settings"))
			menuState = "Settings";
		
		if(GUI.Button(new Rect(50, 100, 200, 30), "Set Name"))
			menuState = "SetName";
		
		if(!Utils.IsWebPlayer()){
			if(GUI.Button(new Rect(50, 135, 200, 30), "Quit"))
				Application.Quit();
		}
		
		GUI.EndGroup();
	}
	
	void Settings(){
		GUI.BeginGroup(new Rect(10, 100, 300, 300));
		GUI.Box(new Rect(0, 0, 300, 300), "Settings");
		GUI.Label(new Rect(30, 20, 150, 20), _settings.GetResolutionInString(_settings.currentResolution));
		for(int i=0; i<_settings.resolutions.Length; i++){
			if(GUI.Button(new Rect(30, 60 + i*20, 150, 20), _settings.GetResolutionInString(_settings.resolutions[i])))
				_settings.SetResolution(i, false);
		}
		
		if(GUI.Button(new Rect(30, 230, 150, 30), "Back To Menu"))
			menuState = "MainMenu";
		GUI.EndGroup();
	}
	
	void GameCreateMenu(){
		GUI.BeginGroup(new Rect(Screen.width / 2 - 250, Screen.height/2 - 115, 500, 230));
	  	GUI.Box(new Rect(0, 0, 500, 230), "Menu");
		GUI.Label(new Rect(10, 20, 150, 20), "Connection Settings");
		GUI.Label(new Rect(10, 45, 50, 20), "IP:");
		GUI.Label(new Rect(10, 70, 50, 20), "PORT:");
			
		_NC.connectIP = GUI.TextField(new Rect(65, 45, 150, 20), _NC.connectIP);
		_NC.connectPORT = int.Parse(GUI.TextField(new Rect(65, 70, 70, 20), _NC.connectPORT.ToString()));
			
		if(GUI.Button(new Rect(230, 45, 150, 30), "Connect"))
			_NC.Connect();
			
		GUI.Label(new Rect(10, 100, 300, 400), "Server Settings");
			
		GUI.Label(new Rect(10, 120, 50, 20), "PORT:");
		GUI.Label(new Rect(10, 145, 50, 20), "Max Pl:");
			
		_NC.serverPORT = int.Parse(GUI.TextField(new Rect(65, 120, 70, 20), _NC.serverPORT.ToString()));
		_NC.maxPlayers = int.Parse(GUI.TextField(new Rect(65, 145, 70, 20), _NC.maxPlayers.ToString()));
			
		if(GUI.Button(new Rect(230, 120, 150, 30), "Start Server"))
			_NC.StartServer();
		GUI.Label(new Rect(10, 270, 50, 20), _NC.mapIndex.ToString());	
		
		if(GUI.Button(new Rect(30, 180, 150, 30), "Back To Menu"))
			menuState = "MainMenu";
		
		GUI.EndGroup();
	}
	
	void SetPlayerName(){
		GUI.BeginGroup(new Rect(Screen.width / 2 - 100, Screen.height / 2 - 60, 200, 120));
	    GUI.Box(new Rect(0, 0, 200, 120), "Enter Name");
		_NC.username = GUI.TextField(new Rect(10, 20, 180, 20), _NC.username);
		if(!Utils.IsStringEmpty(_NC.username)){
			if(GUI.Button(new Rect(10, 45, 50, 30), "OK")){
				PlayerPrefs.SetString("USERNAME",_NC.username);
				menuState = "MainMenu";
			}
		}
		else{
			GUI.Label(new Rect(10, 45, 180, 30), "Enter correct name!");
		}
		GUI.EndGroup();	
	}
	
	void BannerGI(){
		GUI.BeginGroup(new Rect(0, Screen.height-60, Screen.width, 50));
		GUI.DrawTexture(new Rect(0, 0, Screen.width, 50), banner_texture);
		GUI.Label(new Rect(10, 15, Screen.width, 30), bunner_message);
		GUI.EndGroup();
	}
	
	void TimeAndDateGI(){
		GUI.BeginGroup(new Rect(0, 10, Screen.width, 50));
		GUI.DrawTexture(new Rect(0, 0, Screen.width, 50), banner_texture);
		GUI.Label(new Rect(Screen.width / 2 - 150, 10, 300, 30), "<color=lime>" +
																"<b>" +
																"<size=25>"+System.DateTime.Now.ToString()+
																"</size>" +
																"</b>" +
																"</color>");
		GUI.EndGroup();
	}
}

///This in old part of OnGUI
/*
		if(!_loader.XMLData.isDone || !string.IsNullOrEmpty(_loader.XMLData.error)){
			UI.CreateMSG(100, 40, string.IsNullOrEmpty(_loader.XMLData.error) ? "LoadingXML..." : "Error!", banner_texture);
			return;
		}
		
		if(_loader.fileAssetsLoading || _loader.fileLoadError){
			UI.CreateMSG(200, 40, !_loader.fileLoadError ? "Loading file: " + _loader.fileOnLoad : "Error!", banner_texture);
			return;
		}
		*/
		
		
		
		/*
		if(!_loader.XMLData.isDone)
			UI.CreateMSG(100, 40, ("LoadingXML..."), banner_texture);
		else{
			if(_loader.XMLData.error == null)
				menuState = "SetPlayerName";
			else
				UI.CreateMSG(100, 40, "Error!", banner_texture);
		}
		*/