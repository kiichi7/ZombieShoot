using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public struct WeaponDynamicData{
	public int ammoInClip;
	public int hasAmmo;
}

[System.Serializable]
public class AnimationBlock{
	public MotionStates motionState;
	public WrapMode warpMode = WrapMode.Loop;
	public AnimationClip[] animations;
}

[System.Serializable]
public class BlendAnimBlock{
	public AnimationClip clip;
	public Transform[] mixWith;
	public float maxWeight = 0.1f;
	public WrapMode warpMode = WrapMode.Once;
	public AnimationBlendMode blendMode = AnimationBlendMode.Additive;
}

public class PlayerController : MonoBehaviour {
	//[HideInInspector]public List<int> weaponIDs = new List<int>();
	//[HideInInspector]public List<int> ammoInClip = new List<int>();
	//[HideInInspector]public List<int> hasAmmo = new List<int>();
	
	public BlendAnimBlock[] recoilAnimations;	//Additive Recoil Animations
	public BlendAnimBlock[] reloadAnimations;	//Additive Recoil Animations
	public AnimationBlock[] animationBlocks;	//Motion State Animations
	
	public ParticleEmitter[] particleOnDamage;
	
	public Transform weaponBone, head, aimPivot;
	public float reticuleSize;
	public Texture2D reticuleTexture;
	public Animation actorAnimation;
	public float hitPoints = 100.0f;
	
	public int credits = 100;
	public int killsAI = 0;
	
	Vector3 targetVelocity;
	bool _grounded;
	//int lastWeaponIndex, lastActiveWeaponID;
	float headLookAngle, aimPivotAngle;
	//int weaponSlots, startWeaponID;
	
	//[HideInInspector] public WeaponInfo activeWeaponInfo;
	[HideInInspector] public WeaponController activeWeaponController;
	[HideInInspector] public WeaponDynamicData weaponDD;
	[HideInInspector] public PickUpWeapnOrAmmo pickUp;
	
	[HideInInspector] public MotionStates motionState = MotionStates.Idle_Simple;
	[HideInInspector] public MotionStates lastMotionState = MotionStates.Idle_Simple;
	[HideInInspector] public float mouseXSpeed, mouseYSpeed, lookAngle, walkSpeed, runSpeed;
	[HideInInspector] public bool aiming = false;
	//[HideInInspector] public WeaponController activeWeapon;
	[HideInInspector] public Vector2 inputAxis1, inputAxis2;
	//[HideInInspector] public int activeWeaponID = 0,/*0 - if no weapon in hand*/ weaponIndex;
	
	CustomInput cInput;
	
	public bool IsAnimationEnabled(string stateName){
		return actorAnimation[stateName].enabled;
	}
	
	public void AddAnimations(){
		foreach(AnimationBlock block in animationBlocks){
			for(int i=0; i<block.animations.Length; i++){
				string clipName = block.motionState.ToString() + "_" + i.ToString();
				actorAnimation.AddClip(block.animations[i], clipName);
				actorAnimation[clipName].wrapMode = block.warpMode;
			}
		}
		
		for(int i= 0; i<recoilAnimations.Length; i++){
			string clipName = "BLEND_Recoil_" + i.ToString();
			actorAnimation.AddClip(recoilAnimations[i].clip, clipName);
			foreach(Transform mt in recoilAnimations[i].mixWith)
				actorAnimation[clipName].AddMixingTransform(mt);
			actorAnimation[clipName].wrapMode = recoilAnimations[i].warpMode;
			actorAnimation[clipName].blendMode = recoilAnimations[i].blendMode;
			actorAnimation[clipName].layer = 1;
		}
		
		for(int i= 0; i<reloadAnimations.Length; i++){
			string clipName = "BLEND_Reload_" + i.ToString();
			actorAnimation.AddClip(reloadAnimations[i].clip, clipName);
			foreach(Transform mt in reloadAnimations[i].mixWith)
				actorAnimation[clipName].AddMixingTransform(mt);
			actorAnimation[clipName].wrapMode = reloadAnimations[i].warpMode;
			actorAnimation[clipName].blendMode = reloadAnimations[i].blendMode;
			actorAnimation[clipName].layer = 1;
		}
		actorAnimation.SyncLayer(0);
	}
	
	void Start(){
		
		ApplyAnimationState();
		
		if(networkView.isMine){
			Settings settings = FindObjectOfType(typeof(Settings)) as Settings;
			cInput = settings.customInput;
			
			runSpeed = float.Parse(Utils.GetValueWithKey("pl_runspeed"));
			walkSpeed = float.Parse(Utils.GetValueWithKey("pl_walkspeed"));
			mouseXSpeed = float.Parse(Utils.GetValueWithKey("mouseXSpeed"));
			mouseYSpeed = float.Parse(Utils.GetValueWithKey("mouseYSpeed"));
			credits = int.Parse(Utils.GetValueWithKey("pl_creditsonstart"));
			//weaponSlots = int.Parse(Utils.GetValueWithKey("pl_wslots"));
			//startWeaponID = int.Parse(Utils.GetValueWithKey("pl_startweapon"));
			
			//AddWeaponOrAmmo(Utils.GetWeaponData(startWeaponID), 150);
		}
	}

	void Update(){
		if(networkView.isMine){
			inputAxis1 = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
			inputAxis2 = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
			
			UpadateMotionState();
			
			transform.Rotate(0, inputAxis2.x * mouseXSpeed, 0);	
			lookAngle -= inputAxis2.y * mouseYSpeed;
			lookAngle = Utils.ClampAngle(lookAngle, -35.0f, 35.0f);
			
			//activeWeaponID = activeWeapon != null ? activeWeapon.info.id : 0;
			
			if(activeWeaponController && !IsAnimationEnabled("BLEND_Recoil_" + activeWeaponController.info.type) &&
									  !IsAnimationEnabled("BLEND_Reload_" + activeWeaponController.info.type) && 
											  weaponDD.ammoInClip != activeWeaponController.info.clipSize && weaponDD.hasAmmo > 0){
				if(Input.GetKeyDown(KeyCode.R) ||  weaponDD.ammoInClip<=0){
					int needBullets = activeWeaponController.info.clipSize - weaponDD.ammoInClip;
					if(weaponDD.hasAmmo >= needBullets){
						weaponDD.ammoInClip += needBullets;
						//hasAmmo[weaponIndex] -= needBullets;
					}
					else{
						weaponDD.ammoInClip += weaponDD.hasAmmo;
						weaponDD.hasAmmo = 0;
					}
					
					ReloadAnimation();// RPC
				}
			}
			
			if(pickUp && Input.GetKeyDown(cInput.KEY_USE)){
				if(activeWeaponController){
					if(activeWeaponController.info != pickUp.info){
						credits -= pickUp.weaponCost;
						SetWeapon(pickUp.info, pickUp.ammoToAdd);
					}
					else{
						credits -= pickUp.ammoCost;
						weaponDD.hasAmmo += 100;
					}
				}
				else{
					credits -= pickUp.weaponCost;
					SetWeapon(pickUp.info, pickUp.ammoToAdd);
				}
			}
			 // && credits >= pickUp.weaponCost
			if(Input.GetKeyDown(KeyCode.H)){
				ApplyDamage(Random.Range(10, 30));
			}
			
			if(activeWeaponController && Input.GetKeyDown(cInput.KEY_DROP) && !IsAnimationEnabled("BLEND_Reload_" + activeWeaponController.info.type)){
				RemoveWeapon();
			}
			
			/*
			if(Input.GetKeyDown(KeyCode.Q) && weaponIDs.Count > 1 && !IsAnimationEnabled("BLEND_Reload_" + activeWeapon.info.type)){
				aiming = false;
				if(weaponIndex < weaponIDs.Count - 1)
					weaponIndex++;
				else
					weaponIndex = 0;
				SelectWeapon(weaponIndex);
			}
			*/
			
			if(Input.GetMouseButtonDown(1) && activeWeaponController)
				aiming = true;
		
			if(Input.GetMouseButtonUp(1) || !activeWeaponController)
				aiming = false;
			
			if(Input.GetMouseButton(0) && aiming && !IsAnimationEnabled("BLEND_Reload_" + activeWeaponController.info.type)){
				if(Time.time <= activeWeaponController.rateTime || weaponDD.ammoInClip <= 0)
					return;
				weaponDD.ammoInClip--;
				
				RecoilAnimation();// RPC
				activeWeaponController.Attack();
			}
		}
		/*
		if (lastActiveWeaponID != activeWeaponID){
			lastActiveWeaponID = activeWeaponID;
			ApplyAnimationState();
		}
		*/
		
		if(lastMotionState != motionState){
			lastMotionState = motionState;
			ApplyAnimationState();
		}	
	}
	
	void LateUpdate(){
		headLookAngle = aiming ? 0 : lookAngle;
		aimPivotAngle = aiming ? lookAngle : 0;
		
		head.RotateAround(head.position, transform.right, headLookAngle);
		aimPivot.RotateAround(aimPivot.position, transform.right, aimPivotAngle);
	}
	
	void FixedUpdate () {
		if(networkView.isMine){
			targetVelocity = new Vector3(inputAxis1.x, 0, inputAxis1.y);
			if(targetVelocity.sqrMagnitude > 1.0f)
				targetVelocity.Normalize();
			
			targetVelocity = transform.TransformDirection(targetVelocity);
			targetVelocity *= (aiming ? walkSpeed : runSpeed);
			
			Vector3 velocity = rigidbody.velocity;
			Vector3 velocityChange = (targetVelocity - velocity);
			
			velocityChange.x = Mathf.Clamp(velocityChange.x, -10.0f, 10.0f);
	        velocityChange.z = Mathf.Clamp(velocityChange.z, -10.0f, 10.0f);
	        velocityChange.y = 0;

			rigidbody.AddForce(velocityChange, ForceMode.VelocityChange);
			
			rigidbody.AddForce(new Vector3 (0, Physics.gravity.y * rigidbody.mass, 0));
 		}
	}
	//[RPC]
	void RecoilAnimation(){
		int weaponType = activeWeaponController.info.type;
		actorAnimation.Stop("BLEND_Recoil_" + weaponType);
		actorAnimation.Play("BLEND_Recoil_" + weaponType);
		actorAnimation["BLEND_Recoil_" + weaponType].weight = recoilAnimations[weaponType].maxWeight;
	}
	//[RPC]
	void ReloadAnimation(){
		int weaponType = activeWeaponController.info.type;
		actorAnimation.Stop("BLEND_Reload_" + weaponType);
		actorAnimation.CrossFade("BLEND_Reload_" + weaponType);
		actorAnimation["BLEND_Reload_" + weaponType].weight = reloadAnimations[weaponType].maxWeight;	
	}
	
	void ApplyDamage(float dmg){
		if(dmg > hitPoints){
			GameObject[] spawns = GameObject.FindGameObjectsWithTag("Spawnpoint_Player");
			transform.position = spawns[Random.Range(0, spawns.Length - 1)].transform.position;
			hitPoints = 100;
		}
		else{
			foreach(ParticleEmitter emitter in particleOnDamage)
				emitter.Emit();
			hitPoints -= dmg;
		}
	}
	

	void ApplyAnimationState(){
		if(activeWeaponController)
			actorAnimation.CrossFade(motionState.ToString() + "_" + activeWeaponController.info.type.ToString());
		else
			actorAnimation.CrossFade(motionState.ToString() + "_" + 2);	
	}
	
	void UpadateMotionState(){
		if(_grounded){
			if(inputAxis1.y == 0 && inputAxis1.x == 0)
				motionState = aiming ? MotionStates.Idle_Aim : MotionStates.Idle_Simple;
			
			if(inputAxis1.y == 0 && inputAxis1.x > 0)
				motionState = aiming ? MotionStates.Walk_Right : MotionStates.Run_Right;
			
			if(inputAxis1.y == 0 && inputAxis1.x < 0)
				motionState = aiming ? MotionStates.Walk_Left : MotionStates.Run_Left;
	
			if(inputAxis1.y > 0 && inputAxis1.x == 0)
				motionState = aiming ? MotionStates.Walk_Forward : MotionStates.Run_Forward;
	
			if(inputAxis1.y < 0 && inputAxis1.x == 0)
				motionState = aiming ? MotionStates.Walk_Back : MotionStates.Run_Back;
	
			if(inputAxis1.y > 0 && inputAxis1.x > 0)
				motionState = aiming ? MotionStates.Walk_Forward_Right : MotionStates.Run_Forward_Right;
	
			if(inputAxis1.y < 0 && inputAxis1.x < 0)
				motionState = aiming ? MotionStates.Walk_Back_Left : MotionStates.Run_Back_Left;
	
			if(inputAxis1.y < 0 && inputAxis1.x > 0)
				motionState = aiming ? MotionStates.Walk_Back_Right : MotionStates.Run_Back_Right;
	
			if(inputAxis1.y > 0 && inputAxis1.x < 0)
				motionState = aiming ? MotionStates.Walk_Forward_Left : MotionStates.Run_Forward_Left;
			}
		else{
			motionState = MotionStates.Jump;
		}
		
	
	}
	
	void SetWeapon(WeaponInfo info, int ammoToAdd){
		if(activeWeaponController)
			Destroy(activeWeaponController.gameObject);
		Loader loader = FindObjectOfType(typeof(Loader)) as Loader;
		Object weapon = loader.GetAsset(info.filename, loader.FileDB_Weapon);
		GameObject w_go = Instantiate(weapon, weaponBone.position, weaponBone.rotation) as GameObject;
		w_go.transform.parent = weaponBone;
		activeWeaponController = w_go.GetComponent<WeaponController>();
		activeWeaponController.info = info;
		activeWeaponController.owner = gameObject.GetComponent<PlayerController>();
		weaponDD.ammoInClip = info.clipSize;
		weaponDD.hasAmmo = ammoToAdd;
		ApplyAnimationState();
	}
	
	void RemoveWeapon(){
		Destroy(activeWeaponController.gameObject);
		actorAnimation.CrossFade(motionState.ToString() + "_" + 2);	
	}
	
	void OnGUI(){
		if(networkView.isMine){
			//GUI.Label(new Rect(30, Screen.height - 100, 300, 30), "Motion State: " + motionState.ToString());
			GUI.Label(new Rect(30, Screen.height - 130, 300, 30), "Credits: " + 
				"<color=lime>" +
				"<b>" + 
				credits + 
				"</b>" +
				"</color>");
			
			GUI.Label(new Rect(30, Screen.height - 150, 300, 30), "Health: " + 
				"<color=lime>" +
				"<b>" + 
				hitPoints.ToString("f0") + 
				"</b>" +
				"</color>");
			
			if(activeWeaponController)
				GUI.Label(new Rect(30, Screen.height - 170, 300, 30), "Weapon: " + 
					"<color=lime>" + 
					activeWeaponController.info.name + 
					"</color>" + 
					" | " + 
					"<b>" +
					"<color=lime>" + 
					weaponDD.ammoInClip + 
					"/" + weaponDD.hasAmmo + 
					"</color>" + 
					"</b>");
			
			if(pickUp){
				if(activeWeaponController)
					GUI.Label(new Rect(30, Screen.height - 210, 300, 30), 
						"<color=lime>" + 
						"<size=18>" + (credits >= pickUp.weaponCost ? 
						("Press " + cInput.KEY_USE + " to buy " + (pickUp.info != activeWeaponController.info ? pickUp.info.name : "ammo")) : "Not enough credits!") +
						"</size>" +
						"</color>");
				else
					GUI.Label(new Rect(30, Screen.height - 210, 300, 30), 
						"<color=lime>" + 
						"<size=18>" + (credits >= pickUp.weaponCost ? 
						("Press " + cInput.KEY_USE + " to buy " + pickUp.info.name) : "Not enough credits!") +
						"</size>" +
						"</color>");
			}
			
			GUI.Label(new Rect(30, Screen.height - 50, 300, 30), 
						"<b>" +
						"<color=lime>" + 
						"<size=18>" + "You Kill " + killsAI + " Zombies" +
						"</size>" +
						"</color>" +
						"</b>");
			
			/*GUI.color = Color.red;
			if(aiming)
				GUI.DrawTexture(new Rect(Screen.width/2-reticuleSize/2, Screen.height/2 - reticuleSize/2, reticuleSize, 
				reticuleSize), reticuleTexture);*/
		}
	}
	
	void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info) {
        Vector3 t_pos = Vector3.zero;
		Quaternion t_rot = Quaternion.identity;
		float t_la = 0.0f;
		int t_ms = 0;
        //int t_wi = 0;
		bool t_aiming = false;
		float t_hitPoints = 0;
		if (stream.isWriting) {
            t_pos = transform.position;
			t_rot = transform.rotation;
			t_la = lookAngle;
			t_ms = (int)motionState;
			//t_wi = activeWeaponID;
			t_aiming = aiming;
         	t_hitPoints = hitPoints;
			stream.Serialize(ref t_pos);
			stream.Serialize(ref t_rot);
			stream.Serialize(ref t_la);
			stream.Serialize(ref t_ms);
			//stream.Serialize(ref t_wi);
			stream.Serialize(ref t_aiming);
			stream.Serialize(ref t_hitPoints);
        } 
		else {
			stream.Serialize(ref t_pos);
			stream.Serialize(ref t_rot);
			stream.Serialize(ref t_la);
			stream.Serialize(ref t_ms);
			//stream.Serialize(ref t_wi);
			stream.Serialize(ref t_aiming);
			stream.Serialize(ref t_hitPoints);
            transform.position = t_pos;
			transform.rotation = t_rot;
			lookAngle = t_la;
			motionState = (MotionStates)t_ms;
			//activeWeaponID = t_wi;
			aiming = t_aiming;
			hitPoints = t_hitPoints;
        }
    }
	
	void OnTriggerEnter(Collider otherCollider) {
		switch(otherCollider.tag){
			case "Pickup":
				pickUp = otherCollider.gameObject.GetComponent<PickUpWeapnOrAmmo>();
				break;
		}
	}
	
	void OnTriggerExit(Collider otherCollider){
		pickUp = null;
	}
		
	void OnCollisionStay(Collision collision){
		if(networkView.isMine){
	        foreach(ContactPoint cp in collision.contacts){
				if(Vector3.Angle(cp.normal, Vector3.up) < 30)
					_grounded = true;
			} 
		}
    }
	
	void OnCollisionExit () {
		if(networkView.isMine){
        	_grounded = false;  
		}
    }
}


/*
	void SelectWeapon(int index){
		if(activeWeapon)
			Destroy(activeWeapon.gameObject);
		Loader loader = FindObjectOfType(typeof(Loader)) as Loader;
		weaponIndex = index;
		WeaponInfo newWeaponInfo = (WeaponInfo)(Utils.GetWeaponData(weaponIDs[index]));
		Object weapon = loader.GetAsset(newWeaponInfo.filename, loader.FileDB_Weapon);
		GameObject w_go = Instantiate(weapon, weaponBone.position, weaponBone.rotation) as GameObject;
		w_go.transform.parent = weaponBone;
		activeWeapon = w_go.GetComponent<WeaponController>();
		activeWeapon.info = newWeaponInfo;
	}
	*/
	/*
	public void AddWeaponOrAmmo(WeaponInfo info, int ammoToAdd){
		if(!weaponIDs.Contains(info.id)){
			if(weaponIDs.Count == weaponSlots){
				weaponIDs[weaponIndex] = info.id;
				ammoInClip[weaponIndex] = info.clipSize;
				hasAmmo[weaponIndex] = ammoToAdd;
				
				SelectWeapon(weaponIndex);
			}
			
			if(weaponIDs.Count < weaponSlots && weaponIDs.Count != 0){
				weaponIDs.Add(info.id);
				ammoInClip.Add(info.clipSize);
				hasAmmo.Add(ammoToAdd);
			}
			
			if(weaponIDs.Count == 0){
				weaponIDs.Add(info.id);
				ammoInClip.Add(info.clipSize);
				hasAmmo.Add(ammoToAdd);
				
				SelectWeapon(0);
			}
		}
		else{
			int index = weaponIDs.IndexOf(info.id);
			
			Debug.Log(index);
			if((hasAmmo[index] + ammoToAdd) > info.maxAmmo)
				hasAmmo[index] = info.maxAmmo;
			else
				hasAmmo[index] += ammoToAdd;
			Debug.Log("Alredy has this weapon (Getting  ammo)");
		}
	}
	*/
	/*
	public void RemoveWeapon(int index){
		if(weaponIDs.Count != 1){
			if(index != weaponIDs.Count-1){
				weaponIDs.RemoveAt(index);
				ammoInClip.RemoveAt(index);
				hasAmmo.RemoveAt(index);
				SelectWeapon(index);
			}
			else{
				weaponIDs.RemoveAt(index);
				ammoInClip.RemoveAt(index);
				hasAmmo.RemoveAt(index);
				SelectWeapon(index-1);
			}	
		}
		else{
			weaponIDs.RemoveAt(0);
			ammoInClip.RemoveAt(0);
			hasAmmo.RemoveAt(0);
			Destroy(activeWeapon.gameObject);
		}
	}
	*/
	/*
	public void AddWeaponOrAmmo(int id, int ammoToAdd){
		WeaponInfo temp_wd = (WeaponInfo)(Utils.GetWeaponData(id));
		if(!weaponIDs.Contains(id)){
			if(weaponIDs.Count == weaponSlots){
				weaponIDs[weaponIndex] = id;
				ammoInClip[weaponIndex] = temp_wd.clipSize;
				hasAmmo[weaponIndex] = ammoToAdd;
				
				SelectWeapon(weaponIndex);
			}
			
			if(weaponIDs.Count < weaponSlots && weaponIDs.Count != 0){
				weaponIDs.Add(id);
				ammoInClip.Add(temp_wd.clipSize);
				hasAmmo.Add(ammoToAdd);
			}
			
			if(weaponIDs.Count == 0){
				weaponIDs.Add(id);
				ammoInClip.Add(temp_wd.clipSize);
				hasAmmo.Add(ammoToAdd);
				
				SelectWeapon(0);
			}
		}
		else{
			if((hasAmmo[weaponIndex] + ammoToAdd) > temp_wd.maxAmmo)
				hasAmmo[weaponIndex] = temp_wd.maxAmmo;
			else
				hasAmmo[weaponIndex] += ammoToAdd;
			Debug.Log("Alredy has this weapon (Getting  ammo)");
		}
	}
	 */
