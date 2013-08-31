using UnityEngine;
using System.Collections;

public class PlayerBehaviour : MonoBehaviour
{
	public PlayerManager manager;
	
	private void FixedUpdate()
	{
		Vector3 vel = rigidbody.velocity;
		/*while(vel.sqrMagnitude > 2048.0f)
			vel /= 2.0f;*/
		//rigidbody.velocity = vel;
		manager.FireDirection = vel.normalized;
		
		/*RaycastHit hit;
		LayerMask t = LayerMask.NameToLayer("terrain");
		if(Physics.Raycast(transform.position, rigidbody.velocity, out hit, 0.1f, t))
		{
			if(hit.collider)
			rigidbody.velocity = Vector3.zero;
		}*/
	}
	
	private void OnCollisionEnter(Collision c)
	{
		if(c.gameObject.name == "Terrain")
			manager.CanJump = true;
	}
}
