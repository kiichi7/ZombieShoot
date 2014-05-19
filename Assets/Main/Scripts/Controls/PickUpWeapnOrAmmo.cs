using UnityEngine;
using System.Collections;

public class PickUpWeapnOrAmmo : MonoBehaviour {
	public int weaponID = 1;
	public int ammoToAdd = 30;
	public int weaponCost;
	public int ammoCost;
	public WeaponInfo info;
	public float distanceThreshold = 15.0f;
	
	void Start () {
		info = (WeaponInfo)(Utils.GetWeaponData(weaponID));
		if(ammoToAdd > info.maxAmmo)
			ammoToAdd = info.maxAmmo;
	}
	
	void OnGUI(){
		Vector3 screenPosition = Camera.main.WorldToScreenPoint(transform.position);
		Vector3 cameraRelative = Camera.main.transform.InverseTransformPoint(transform.position);	
		float distanceToCamera = Vector3.Distance(transform.position, Camera.main.transform.position);
		if (cameraRelative.z > 0 && distanceToCamera < distanceThreshold){
			Rect position = new Rect(screenPosition.x, Screen.height - screenPosition.y, 200, 200);
			GUI.Label(position, "<color=#00FF7F>" +
								"<b>" +
								"<size=16>"+info.name + 
								" Cost: " + weaponCost +
								"</size>" +
								"</b>" +
								"</color>");
		}
	}

}
