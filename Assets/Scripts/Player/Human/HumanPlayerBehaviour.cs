using UnityEngine;

public class HumanPlayerBehaviour : MonoBehaviour
{
	protected HumanPlayerManager _manager;
	
	protected virtual void FixedUpdate()
	{
		Vector3 vel = rigidbody.velocity;
		_manager.FireDirection = vel.normalized;
	}
	
	protected virtual void OnCollisionEnter(Collision c)
	{
		if(c.gameObject.name == "Terrain")
			_manager.CanJump = true;
	}
	
	public virtual PlayerManager Manager
	{
		get { return _manager; }
		set { _manager = (HumanPlayerManager)value; }
	}
}