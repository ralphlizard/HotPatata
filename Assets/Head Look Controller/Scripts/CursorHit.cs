using UnityEngine;
using System.Collections;

public class CursorHit : MonoBehaviour {
	
	public HeadLookController headLook;
	private float offset = 1.5f;
	
	// Update is called once per frame
	void LateUpdate () {		
		headLook.target = transform.position;
	}
}
