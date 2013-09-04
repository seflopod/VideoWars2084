using UnityEngine;
using System.Collections;

public class AIBehaviour : MonoBehaviour
{
	
	public AIPlayerManager _manager;
	
	private void Update()
	{
		_manager.StepTimers(Time.deltaTime);
		_manager.Move();
	}
	
	private void FixedUpdate()
	{
		Vector3 vel = rigidbody.velocity;
		_manager.FireDirection = vel.normalized;
		_manager.Fire();
	}
	
	private void OnCollisionEnter(Collision c)
	{
		if(c.collider.gameObject.name == "Terrain")
		{
			_manager.CanJump = true;
			if(Random.Range(0,4) == 1)
				_manager.ForceDirectionChange();
		}			
	}
	
	public PlayerManager Manager
	{
		get { return _manager; }
		set { _manager = (AIPlayerManager)value; }
	}
}
