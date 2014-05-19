using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class HitParticlesSetup{
	public string name;
	public Transform rootTransform;
	public ParticleEmitter[] emitters;
}

public class ParticleController : MonoBehaviour {
	public HitParticlesSetup[] hitParticles;
	List<string> usedTags = new List<string>();
	
	void Start () {
		foreach(HitParticlesSetup hps in hitParticles)
			usedTags.Add(hps.name);
	}

	public void EmitByTag(Vector3 pos, Quaternion rot, string tag){
		if(usedTags.Contains(tag)){
			int index = usedTags.IndexOf(tag);
			hitParticles[index].rootTransform.position = pos;
			hitParticles[index].rootTransform.rotation = rot;
			foreach(ParticleEmitter emitter in hitParticles[index].emitters)
				emitter.Emit();
		}
	}
}
