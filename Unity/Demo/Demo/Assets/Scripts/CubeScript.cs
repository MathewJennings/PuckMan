using UnityEngine;
using System.Collections;

public class NewBehaviourScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKey(KeyCode.W))
			GetComponent<Rigidbody>().MovePosition(GetComponent<Rigidbody>().position + Vector3.forward * 1 * Time.deltaTime);
		
		if (Input.GetKey(KeyCode.S))
			GetComponent<Rigidbody>().MovePosition(GetComponent<Rigidbody>().position - Vector3.forward * 1 * Time.deltaTime);
		
		if (Input.GetKey(KeyCode.D))
			GetComponent<Rigidbody>().MovePosition(GetComponent<Rigidbody>().position + Vector3.right * 1 * Time.deltaTime);
		
		if (Input.GetKey(KeyCode.A))
			GetComponent<Rigidbody>().MovePosition(GetComponent<Rigidbody>().position - Vector3.right * 1 * Time.deltaTime);
	
	}

	void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info)
	{
		Vector3 syncPosition = Vector3.zero;
		if (stream.isWriting)
		{
			syncPosition = GetComponent<Rigidbody>().position;
			stream.Serialize(ref syncPosition);
		}
		else
		{
			stream.Serialize(ref syncPosition);
			GetComponent<Rigidbody>().position = syncPosition;
		}
	}
}
