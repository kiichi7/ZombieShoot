using UnityEngine;
using System.Collections;

public class AI : MonoBehaviour {
	public float hitPoints = 100f;
	public float stopDistance = 1.5f;
	public float speed = 1.35f;
	public float destroyDelay = 7.0f;
	public Animation actorAnimation;
	public AnimationClip idle;
	public AnimationClip run;
	public AnimationClip[] attackAnimations;
	public AnimationClip[] deathAnimations;
	public Texture2D[] textures;
	public SkinnedMeshRenderer render;
	public Color materialColir;
	
	bool isDead = false;
	Transform target;
	NavMeshAgent nma;
	int attackAnimIndex;
	float damage;
	bool inAttack { get { return actorAnimation[attackAnimations[attackAnimIndex].name].enabled; } }	
	
	void Start () {
		Material material = new Material(Shader.Find("Diffuse"));
		material.SetTexture("_MainTex", textures[Random.Range(0, textures.Length - 1)]);
		material.color = materialColir;
		render.material = material;
		
		nma = gameObject.GetComponent<NavMeshAgent>();
		
		actorAnimation.AddClip(idle, idle.name);
		actorAnimation.AddClip(run, run.name);
		
		foreach(AnimationClip clip in attackAnimations){
			actorAnimation.AddClip(clip, clip.name);
			actorAnimation[clip.name].wrapMode = WrapMode.Once;
		}
		
		foreach(AnimationClip clip in deathAnimations){
			actorAnimation.AddClip(clip, clip.name);
			actorAnimation[clip.name].wrapMode = WrapMode.Once;
		}
		actorAnimation[idle.name].wrapMode = WrapMode.Loop;
		actorAnimation[run.name].wrapMode = WrapMode.Loop;
		attackAnimIndex = Random.Range(0, attackAnimations.Length);
		damage = Random.Range(3.0f, 7.0f);
		target = GameObject.FindGameObjectWithTag("Player").transform;
	}
	

	
	
	void Update () {
		if(Input.GetKeyDown(KeyCode.H)){
			ApplyDamage1(15);
		}
			
			
		if(target){
			if(!isDead){
				nma.destination = target.position;
				nma.updateRotation = DistanceToTarget(target.position) > stopDistance ? true : false;
				nma.speed = DistanceToTarget(target.position) > stopDistance ? speed : 0.0f;
					
				if(DistanceToTarget(target.position) > stopDistance){
					if(!IsAnimationEnabled(run.name))
						actorAnimation.CrossFade(run.name);
				}
					
				if(DistanceToTarget(target.position) <= stopDistance){
					RotateTowards(target.position);
					if(!inAttack)
						Attack(damage + Random.Range(-1.0f, 3.0f));
				}
			}
			else{
				nma.speed = 0.0f;
			}
		}
		else{
				
		}
		
	}
	
	void ApplyDamage1(int dmg){
		if(hitPoints <= dmg){
			isDead = true;
			actorAnimation.CrossFade(deathAnimations[Random.Range(0, deathAnimations.Length)].name);
			//Destroy(gameObject);
		}
		else
			hitPoints -= dmg;
	}
	
	void ApplyDamage(DamageMSG msg){
		if(isDead)
			return;
		if(hitPoints <= msg.damage){
			msg.sendFrom.credits += Random.Range(3, 20);
			msg.sendFrom.killsAI++;
			isDead = true;
			actorAnimation.CrossFade(deathAnimations[Random.Range(0, deathAnimations.Length)].name);
			Invoke("SendDestroyEvent", destroyDelay);
		}
		else
			hitPoints -= msg.damage;
	}
	
	void SendDestroyEvent(){
		networkView.RPC("Destroy", RPCMode.All);
	}
	
	
	void Attack(float finalDamage){
		attackAnimIndex = Random.Range(0, attackAnimations.Length);
		actorAnimation.CrossFade(attackAnimations[attackAnimIndex].name);
		target.gameObject.SendMessage("ApplyDamage", finalDamage, SendMessageOptions.DontRequireReceiver);
	}
	
	public void RotateTowards(Vector3 pos){
		Vector3 targetDirection = new Vector3(pos.x, transform.position.y, pos.z);
		transform.LookAt(targetDirection) ;
	}
	
	public bool IsAnimationEnabled(string stateName){
		return actorAnimation[stateName].enabled;
	}
	
	public float DistanceToTarget(Vector3 pos){
		return Vector3.Distance(transform.position, pos);
	}
	
	[RPC]
	void Destroy(){
		Network.RemoveRPCs(GetComponent<NetworkView>().viewID);
		Network.Destroy(GetComponent<NetworkView>().viewID);
	}
}
