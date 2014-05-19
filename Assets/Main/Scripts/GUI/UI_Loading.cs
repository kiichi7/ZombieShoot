using UnityEngine;
using System.Collections;

public class UI_Loading : MonoBehaviour {
	Loader _loader;
	Texture2D banner_texture;
	
	void Start () {
		_loader = FindObjectOfType(typeof(Loader)) as Loader;
		banner_texture = Utils.CreateTexture(Color.grey);
	}
	
	
	void OnGUI () {
		if(!_loader.XMLData.isDone || !string.IsNullOrEmpty(_loader.XMLData.error)){
			UI.CreateMSG(250, 40, string.IsNullOrEmpty(_loader.XMLData.error) ? "LoadingXML..." : "Error!", banner_texture);
		}
		
		if(_loader.fileAssetsLoading || _loader.fileLoadError){
			UI.CreateMSG(250, 40, !_loader.fileLoadError ? "Loading file: " + _loader.fileOnLoad + " " 
				+ ( _loader.HDData.progress * 100).ToString("f0") + " %": "Error!", banner_texture);
		}
	}
	
}
