using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

[System.Serializable]
public class UIRect{
	public Color rectColor;
	public Texture2D uRight;
	public Texture2D uLeft;
	public Texture2D bRight;
	public Texture2D bLeft;
}
public class CameraUI : MonoBehaviour {
	public UIRect camRect;
	public Texture2D recTexture;
	public Texture2D centerTexture;
	public Color recColor = Color.red;
	public Color centerColor = Color.red;
	
	System.DateTime timer = new System.DateTime();
	//int hours, minutes, seconds;
	
	void Start(){
		
	}
	
	void Update () {
		 timer = timer.AddSeconds(Time.deltaTime);
	}
	
	void OnGUI(){
		GUI.color = camRect.rectColor;
		GUI.DrawTexture(new Rect(10, 10, 100, 100), camRect.uLeft, ScaleMode.ScaleAndCrop, true, 1.0F);
		GUI.DrawTexture(new Rect(10, Screen.height - 110, 100, 100), camRect.bLeft, ScaleMode.ScaleAndCrop, true, 1.0F);
		GUI.DrawTexture(new Rect(Screen.width - 110, 10, 100, 100), camRect.uRight, ScaleMode.ScaleAndCrop, true, 1.0F);
		GUI.DrawTexture(new Rect(Screen.width - 110, Screen.height - 110, 100, 100), camRect.bRight, ScaleMode.ScaleAndCrop, true, 1.0F);
		GUI.color = recColor;
		GUI.DrawTexture(new Rect(Screen.width - 150, 30, 130, 50), recTexture, ScaleMode.ScaleAndCrop, true, 2.0F);
		GUI.color = centerColor;
		GUI.DrawTexture(new Rect(Screen.width/2 - 50, Screen.height/2-50, 100, 100), centerTexture, ScaleMode.ScaleAndCrop, true, 1.0F);
		GUI.Label(new Rect(Screen.width - 180, Screen.height - 70, 150, 100), 
			"<b>" +
			"<size=35>" + 
			timer.ToString("HH:mm:ss")+
			"</size>" +
			"</b>");
	}
}
