using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {
	public bool canControl = true;
    public Transform target;
	public Vector3 normalOffset;
	public Vector3 aimOffset;
   
	Vector3 offset;
	PlayerController owner;

    void LateUpdate () {
	   if(!canControl)
			return;
		
		if (target) {
			offset = Vector3.MoveTowards(offset, owner.aiming ? aimOffset : normalOffset, Time.deltaTime * 5);
	       	Quaternion rotation = Quaternion.Euler(owner.lookAngle, target.eulerAngles.y, 0);
	 		transform.rotation = rotation;
			Vector3 position = rotation * new Vector3(offset.x, 0, -offset.z) + target.position + new Vector3(0, offset.y, 0);
	 		transform.position = position;
		}
	}

	void OnPlayerSpawn(GameObject player){
		target = player.transform;
		owner = player.GetComponent<PlayerController>();
	}
}