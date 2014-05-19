using UnityEngine;
using System.Collections;

public class WeaponController : MonoBehaviour {
	public Transform raycastPoint;
	public ParticleEmitter[] emitters;
	public ParticleEmitter trailEmitter;
	public AudioSource audioSource;
	public AudioClip attackSound;
	public LayerMask hitLayers;
	public WeaponInfo info;
	
	[HideInInspector]public float rateTime;
	[HideInInspector]public PlayerController owner;
	
	ParticleController _PC;
	
	void Start(){
		_PC = FindObjectOfType(typeof(ParticleController)) as ParticleController;
		audioSource.clip = attackSound;
	}
	
	public void Attack(){
		if(!_PC)
			return;
		
		float x = Random.Range(-info.inaccuracy, info.inaccuracy);
		float y = Random.Range(-info.inaccuracy, info.inaccuracy);

		RaycastHit hit;
		Vector3 fwd = raycastPoint.TransformDirection(Vector3.forward) + new Vector3(x, y, 0);
        
		if (Physics.Raycast(raycastPoint.position, fwd, out hit, 1000.0f, hitLayers)){
           	if(Vector3.Distance(trailEmitter.gameObject.transform.position, hit.point) > 5){
				trailEmitter.gameObject.transform.LookAt(hit.point);
				trailEmitter.Emit();
			}
			
			Vector3 pos = hit.point;
			Quaternion rot = Quaternion.FromToRotation(Vector3.up, hit.normal);
			string colliderTag = hit.collider.tag;
			
			_PC.EmitByTag(pos, rot, colliderTag);
			
			DamageMSG tempMSG = new DamageMSG();
			switch(colliderTag){
				case "HitBox_Head":
					tempMSG.damage = 100;
					tempMSG.sendFrom = owner;
					hit.collider.transform.root.SendMessageUpwards ("ApplyDamage", tempMSG, SendMessageOptions.DontRequireReceiver);
				break;
			
				case "HitBox_Body":
					tempMSG.damage = info.damage * 1.7f;
					tempMSG.sendFrom = owner;
					hit.collider.transform.root.SendMessageUpwards ("ApplyDamage", tempMSG, SendMessageOptions.DontRequireReceiver);
				break;
			}
		}
		foreach(ParticleEmitter pe in emitters)
			pe.Emit();
		
		audioSource.Stop();
		audioSource.Play();
		
		rateTime = Time.time + info.attackRate;
	}
	
	
}
