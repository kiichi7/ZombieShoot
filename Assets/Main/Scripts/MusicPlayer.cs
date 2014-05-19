using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class MusicPlayer : MonoBehaviour {
	List<AudioClip> music = new List<AudioClip>();
	public AudioSource source;
	bool showUI;
	string[] urls;
	
	void Start(){
		DontDestroyOnLoad(gameObject);
		urls = Directory.GetFiles(Application.dataPath + "/../Music/", "*.ogg");
		StartCoroutine(LoadMusic(urls)); 
	}
	
	void Update(){
		if(Input.GetKeyDown(KeyCode.M))
			showUI = !showUI;
	}
	
	IEnumerator LoadMusic(string[] urls){
		foreach (string url in urls){
			WWW download = new WWW ("file://" + url);
			yield return download;
			music.Add(download.GetAudioClip(false));
		}
	}
	
	void OnGUI(){
		if(!showUI)
			return;
		GUI.BeginGroup(new Rect(30, 50, 300, 200));
		GUI.Box(new Rect(0, 0, 300, 200), "Music");
		if(music.Count > 0){
			for(int i=0; i<urls.Length; i++){
				GUI.Label(new Rect(10, 20 + i*20, 150, 20), Path.GetFileName(urls[i]));
				if(GUI.Button(new Rect(170, 20 + i*20, 100, 20), "Play>>")){
					source.Stop();
					source.clip = music[i];
					source.Play();
				}
			}
		}
		else{
			GUI.Label(new Rect(10, 20, 150, 20), "Loading...");
		}
		GUI.EndGroup();
	}
	
}
