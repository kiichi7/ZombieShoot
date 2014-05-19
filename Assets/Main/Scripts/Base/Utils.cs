using UnityEngine;
using System.Collections;

public enum MotionStates{
	Idle_Aim,
	Idle_Simple,
	Walk_Forward,
	Walk_Forward_Left,
	Walk_Forward_Right,
	Walk_Back,
	Walk_Back_Left,
	Walk_Back_Right,
	Walk_Left,
	Walk_Right,
	Run_Forward,
	Run_Forward_Left,
	Run_Forward_Right,
	Run_Back,
	Run_Back_Left,
	Run_Back_Right,
	Run_Left,
	Run_Right,
	Jump,
	
}

public struct DamageMSG{
	public float damage;
	public PlayerController sendFrom;
}

public struct PlayerInfo{
	public bool host;
	public string username;
	public NetworkPlayer player;
}

public class WeaponInfo{
	public int id;
	public int type;
	public string filename;
	public string name;
    public float damage;
	public float attackRate;
	public float inaccuracy;
	public int clipSize;
	public int maxAmmo;
}

public struct FileAsset{
	public string fileName;
	public Object asset;
}

public class UI{
	public static void CreateMSG(int width, int height, string text, Texture2D background){
		GUI.BeginGroup(new Rect(Screen.width/2 - width/2, Screen.height/2 - height/2, width, height));
		GUI.DrawTexture(new Rect(0, 0, width, height), background);
		//GUI.Box(new Rect(0, 0, width, height), "");
		GUI.Label(new Rect(0, 0, width, height), (text));
		GUI.EndGroup();
	}
}

public class Utils{
	public static float ClampAngle(float angle, float min, float max){
        if (angle < -360F)
            angle += 360F;
        if (angle > 360F)
            angle -= 360F;
        return Mathf.Clamp(angle, min, max);
    }
	
	public static string CreateRandomString(int _length){
	  	string allowedChars = "abcdefghijkmnopqrstuvwxyzABCDEFGHJKLMNOPQRSTUVWXYZ0123456789";
	  	char[] chars = new char[_length];
		for (int i = 0; i < _length; i++) 
		 	 chars[i] = allowedChars[Random.Range(0, allowedChars.Length)];
		return new string(chars);
	}
	
	public static bool IsStringEmpty(string str){
		for (int i = 0; i < str.Length; i++){
		  if (str[i] != ' ')
		  	 return false;
		}
		return true;
	}
	
	public static string GetValueWithKey(string key){
		Loader loader = MonoBehaviour.FindObjectOfType(typeof(Loader)) as Loader;
		if(loader.CFG.ContainsKey(key))
			return loader.CFG[key];
		return null;
	}
	
	public static WeaponInfo GetWeaponData(int id){
		Loader loader = MonoBehaviour.FindObjectOfType(typeof(Loader)) as Loader;
		WeaponInfo newWeaponInfo = new WeaponInfo();
		foreach(WeaponInfo info in loader.DB_Weapon){
			if(info.id == id){
				newWeaponInfo = info;
				return newWeaponInfo;
			}
		}
		return null;
	}
	
	public static bool IsWebPlayer () {
		return (Application.platform == RuntimePlatform.WindowsWebPlayer 
			||Application.platform == RuntimePlatform.OSXWebPlayer);
	}
	
	public static Texture2D CreateTexture(Color color){
		Texture2D TEMP_texture = new Texture2D(1, 1);
		TEMP_texture.SetPixel(0, 0, color);
		TEMP_texture.Apply();
		return TEMP_texture;
	}
	
}
