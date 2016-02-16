using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class ClientRPCDemo : NetworkBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown(KeyCode.G))
			ActivateEvent();
	
	}

	[ClientRpc]
	void RpcActivateEvent(int amount)
	{
		Debug.Log("Event activated!");
		transform.localScale *= amount;
	}

	public void ActivateEvent()
	{
		if (!isServer)
			return;
		
		RpcActivateEvent(2);
	}
}
