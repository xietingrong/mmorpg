using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class login : MonoBehaviour {

	// Use this for initialization
	void Start () {
		Network.NetClient.Instance.Init("127.0.0.1",8000);
		Network.NetClient.Instance.Connect();
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
