using UnityEngine;
using System.Collections;

public class LaserPointer : MonoBehaviour {
	public bool calculate = true;
	public LineRenderer lineRenderer;
	public LayerMask hitLayers;
	public Transform pointer;
	public LensFlare flare;
	
	void FixedUpdate(){
		if(!calculate)
			return;
		RaycastHit hit;
		Vector3 fwd = transform.TransformDirection(Vector3.forward);
		Vector3 pos;
		
		if (Physics.Raycast(transform.position, fwd, out hit, 1000.0f, hitLayers)){
			pos = new Vector3(0, 0, hit.distance);
			flare.brightness = 1 / hit.distance;
		}
		else{
			pos = new Vector3(0, 0, 1000.0f);
			flare.brightness = 0;
		}
		pointer.localPosition = pos;
		lineRenderer.SetPosition(1, pos);
		
		
	}
}
