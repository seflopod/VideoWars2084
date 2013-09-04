using UnityEngine;
using System.Collections;

public class PlayerBehaviour : MonoBehaviour
{
	public PlayerManager manager;
	
	private void FixedUpdate()
	{
		Vector3 vel = rigidbody.velocity;
		manager.FireDirection = vel.normalized;
	}
	
	private void OnCollisionEnter(Collision c)
	{
		if(c.gameObject.name == "Terrain")
			manager.CanJump = true;
	}
}
