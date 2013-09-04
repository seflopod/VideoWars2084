using UnityEngine;
using System.Collections;

public class AIBehaviour : PlayerBehaviour
{
	
	public AIPlayerManager _manager;
	
	private void Update()
	{
		_manager.StepTimers(Time.deltaTime);
		_manager.Move();
	}
	
	protected override void FixedUpdate()
	{
		base.FixedUpdate();
		_manager.Fire();
	}
	
	protected override void OnCollisionEnter(Collision c)
	{
		base.OnCollisionEnter(c);
		
		if(c.collider.gameObject.name == "Terrain" && Random.Range(0,4) == 1)
			_manager.ForceDirectionChange();
	}
	
	public override PlayerManager Manager
	{
		get { return _manager; }
		set { _manager = (AIPlayerManager)value; }
	}
}
