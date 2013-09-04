using UnityEngine;
using System.Collections;

public class PlayerBehaviour : MonoBehaviour
{
	private PlayerManager manager;
	
	protected virtual void FixedUpdate()
	{
		Vector3 vel = rigidbody.velocity;
		manager.FireDirection = vel.normalized;
	}
	
	protected virtual void OnCollisionEnter(Collision c)
	{
		if(c.gameObject.name == "Terrain")
			manager.CanJump = true;
	}
	
	public virtual PlayerManager Manager
	{
		get { return _manager; }
		set { _manager = value; }
	}
}
