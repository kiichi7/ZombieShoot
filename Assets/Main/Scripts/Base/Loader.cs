using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Text;
using System.IO;


public class Loader : MonoBehaviour {
	public ArrayList DB_Weapon = new ArrayList();
	
	public ArrayList FileDB_Player = new ArrayList();
	public ArrayList FileDB_Weapon = new ArrayList();
	public ArrayList FileDB_Zombies = new ArrayList();
	
	string[] FilePaths_Player;
	string[] FilePaths_Weapon;
	string[] FilePaths_Zombies;
	string[] FilePaths_Levels;
	
	AssetBundle streamedLevel;
	
	public Dictionary<string, string> CFG = new Dictionary<string, string>(); 
	
	[HideInInspector]public WWW XMLData;
	[HideInInspector]public WWW HDData;
	[HideInInspector]public bool fileAssetsLoading;
	[HideInInspector]public string fileOnLoad;
	[HideInInspector]public bool fileLoadError;
	//[HideInInspector]public bool globalLoading;
	
	
	void Start () {
		FilePaths_Player = Directory.GetFiles(Application.dataPath + "/../Packages/Players/", "*.package");
		FilePaths_Weapon = Directory.GetFiles(Application.dataPath + "/../Packages/Weapons/", "*.package");
		FilePaths_Zombies = Directory.GetFiles(Application.dataPath + "/../Packages/AI/", "*.package");
		FilePaths_Levels = Directory.GetFiles(Application.dataPath + "/../Packages/Maps/", "*.map");
		/*	
		StartCoroutine(Get_HardDiskAsset(Directory.GetFiles(Application.dataPath + "/../Packages/Players/", "*.package"), FileDB_Player));
		StartCoroutine(Get_HardDiskAsset(Directory.GetFiles(Application.dataPath + "/../Packages/Weapons/", "*.package"), FileDB_Weapon));
		StartCoroutine(Get_HardDiskAsset(Directory.GetFiles(Application.dataPath + "/../Packages/AI/", "*.package"), FileDB_Zombies));
		*/
		//StartCoroutine(Get_XMLDoc("https://dl.dropbox.com/u/34935968/Server/CFG.xml"));
		StartCoroutine(Get_XMLDoc(Application.dataPath + "/../cfg/CFG.xml"));
		
	}
	
	IEnumerator Get_XMLDoc(string url){
		Debug.Log("Loading XML DB file...");
		XMLData = new WWW("file://" + url);
		yield return XMLData;
		
		if(string.IsNullOrEmpty(XMLData.error)){
			Debug.Log("Done...");
			
			StartCoroutine("Get_AllHardDiskAssets");
			XmlDocument xmlDoc = new XmlDocument();
			xmlDoc.LoadXml(XMLData.text);
			
			XMLData_CreateWeaponDB(xmlDoc.SelectNodes("data/weaponsDB/weapon"));
			XMLData_CreateGameCFG(xmlDoc.SelectSingleNode("data/gameconfig"));
		}
		else{
			Debug.Log("Error");
		}
	}
	
	IEnumerator Get_AllHardDiskAssets(){
		fileAssetsLoading = true;
		Debug.Log("Loading Player Assets...");
		foreach (string url in FilePaths_Player){
			fileOnLoad = Path.GetFileNameWithoutExtension(url);
			
			FileAsset newFileAsset = new FileAsset();
			HDData = new WWW ("file://" + url);
			yield return HDData;
			
			if(string.IsNullOrEmpty(HDData.error)){
				newFileAsset.fileName = fileOnLoad;
				newFileAsset.asset = HDData.assetBundle.mainAsset;
				FileDB_Player.Add(newFileAsset);
			}
			else{
				Debug.Log("Error");
				fileAssetsLoading = false;
				fileLoadError = true;
				StopCoroutine("Get_AllHardDiskAssets");
			}
		}
		
		Debug.Log("Done...");
		Debug.Log("Loading Weapon Assets...");
		
		foreach (string url in FilePaths_Weapon){
			fileOnLoad = Path.GetFileNameWithoutExtension(url);
			
			FileAsset newFileAsset = new FileAsset();
			HDData = new WWW ("file://" + url);
			yield return HDData;
			
			if(string.IsNullOrEmpty(HDData.error)){
				newFileAsset.fileName = fileOnLoad;
				newFileAsset.asset = HDData.assetBundle.mainAsset;
				FileDB_Weapon.Add(newFileAsset);
			}
			else{
				Debug.Log("Error");
				fileAssetsLoading = false;
				fileLoadError = true;
				StopCoroutine("Get_AllHardDiskAssets");
			}
		}
		
		Debug.Log("Done...");
		Debug.Log("Loading Zombie Assets...");
		
		foreach (string url in FilePaths_Zombies){
			fileOnLoad = Path.GetFileNameWithoutExtension(url);
			
			FileAsset newFileAsset = new FileAsset();
			HDData = new WWW ("file://" + url);
			yield return HDData;
			
			if(string.IsNullOrEmpty(HDData.error)){
				newFileAsset.fileName = fileOnLoad;
				newFileAsset.asset = HDData.assetBundle.mainAsset;
				FileDB_Zombies.Add(newFileAsset);
			}
			else{
				Debug.Log("Error");
				fileAssetsLoading = false;
				fileLoadError = true;
				StopCoroutine("Get_AllHardDiskAssets");
			}
		}
		
		Debug.Log("Done...");
		Debug.Log("Loading World...");
		
		fileOnLoad = Path.GetFileNameWithoutExtension(FilePaths_Levels[0]);
		HDData = new WWW ("file://" + FilePaths_Levels[0]);
		yield return HDData;
		if(string.IsNullOrEmpty(HDData.error)){
			streamedLevel = HDData.assetBundle;
		}
		else{
			Debug.Log("Error");
			fileAssetsLoading = false;
			fileLoadError = true;
			StopCoroutine("Get_AllHardDiskAssets");
		}
		Debug.Log("Done...");
		fileAssetsLoading = false;
		Debug.Log("Assets Loaded...");
		
		if(!fileLoadError){
			foreach (GameObject go in FindObjectsOfType(typeof (GameObject)))
					go.SendMessage("OnDataWasLoaded", SendMessageOptions.DontRequireReceiver);
		}
	}

	void XMLData_CreateWeaponDB(XmlNodeList nodes){
		foreach (XmlNode node in nodes){
			WeaponInfo newWeaponInfo = new WeaponInfo();
			newWeaponInfo.id = int.Parse(node.Attributes.GetNamedItem("id").Value);
			newWeaponInfo.name = node.SelectSingleNode("name").InnerText;
			newWeaponInfo.filename = node.SelectSingleNode("filename").InnerText;
			newWeaponInfo.type = int.Parse(node.SelectSingleNode("type").InnerText);
			newWeaponInfo.damage = float.Parse(node.SelectSingleNode("damage").InnerText);
			newWeaponInfo.attackRate = float.Parse(node.SelectSingleNode("attack_rate").InnerText);
			newWeaponInfo.inaccuracy = float.Parse(node.SelectSingleNode("inaccuracy").InnerText);
			newWeaponInfo.clipSize = int.Parse(node.SelectSingleNode("clipsize").InnerText);
			newWeaponInfo.maxAmmo = int.Parse(node.SelectSingleNode("maxammo").InnerText);
			DB_Weapon.Add(newWeaponInfo);
		}
	}

	public void UnloadMap(){
		streamedLevel.Unload(true);
		streamedLevel = null;
	}
	void XMLData_CreateGameCFG(XmlNode rootNode){
		XmlNodeList nodes = rootNode.ChildNodes;
		foreach (XmlNode node in nodes)
			CFG.Add(node.Name, node.InnerText);
	}
	
	public Object GetAsset(string fileName, ArrayList fileDB){
		foreach(FileAsset aa in fileDB){
			if(aa.fileName == fileName)
				return aa.asset;
		}
		return null;
	}
}
	/*
	IEnumerator Get_HardDiskAsset(string[] urls, ArrayList fileDB){
		foreach (string url in urls){
			FileAsset newFileAsset = new FileAsset();
			WWW download = new WWW ("file://" + url);
			yield return download;
			
			if(string.IsNullOrEmpty(download.error)){
				newFileAsset.fileName = Path.GetFileNameWithoutExtension(url);
				newFileAsset.asset = download.assetBundle.mainAsset;
				fileDB.Add(newFileAsset);
			}
			else{
				Debug.Log("Error");
			}
		}
	}
	*/
