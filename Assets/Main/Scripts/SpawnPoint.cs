using UnityEngine;
using System.Collections;

public class SpawnPoint : MonoBehaviour {
	public Color color;
	public float size;
	
	void OnDrawGizmos(){
		Gizmos.color = color;
		Gizmos.DrawWireSphere(transform.position, size);
	}
}
