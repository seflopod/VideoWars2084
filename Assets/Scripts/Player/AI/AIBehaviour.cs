using UnityEngine;
using System.Collections.Generic;

public class AIBehaviour : PlayerBehaviour
{
	private float _moveTimer;
	private float _moveTimerTarget;
	private Vector3 _moveDir;
	
	private int _curTarget;
	
	private void Start()
	{
		_moveTimer = 0.0f;
		_moveTimerTarget = 1.0f;
		_moveDir = Vector3.zero;
		
		_curTarget = -1;
	}
	
	private void Update()
	{
		float dt = Time.deltaTime;
		_manager.StepTimers(dt);
		PickMoveDirection(dt);
		_manager.Move(_moveDir, dt);
	}
	
	protected override void FixedUpdate()
	{
		Vector3 target = FindTarget();
		if(_curTarget != -1)
		{
			_manager.FireDirection = target;
			base.FixedUpdate();
			_manager.Fire();
		}
	}
	
	protected override void OnCollisionEnter(Collision c)
	{
		if(c.collider.gameObject.name == "Terrain")
		{
			_manager.CanJump = true;
			if(Random.Range(0,4) == 1)
				_moveTimerTarget = 0.0f;
		}			
	}
	
	private void PickMoveDirection(float dt)
	{
		_moveTimer += dt;
		if(_moveTimer >= _moveTimerTarget)
		{
			_moveTimer = 0.0f;
			_moveTimerTarget = Random.Range(0.25f, 1.0f);
			int aiDir = Random.Range(0,8);
			_moveDir = new Vector3(Mathf.Cos(aiDir*Mathf.PI),
								Mathf.Sin(aiDir*Mathf.PI),
								0.0f);
		}
	}
	
	private Vector3 FindTarget()
	{
		List<GameObject> targets = new List<GameObject>();
		Vector3 origin = gameObject.transform.position;
		origin.z -= 1.0f;
		float r = 50.0f;
		float dist = 2.0f;
		Vector3 dir = Vector3.forward;
		
		//this will do a ray cast in the shape of a sphere with radius r.
		//use this from along the z-axis to intersect the plane we occupy with
		//a circle.
		RaycastHit[] allHits = Physics.SphereCastAll(origin, r, dir, dist);
		if(allHits.Length != 0)
		{
			foreach(RaycastHit hit in allHits)
			{
				GameObject target = hit.collider.gameObject;
				if(target.CompareTag("Player"))
				{
					//is this our current target?  if it is just return that pos
					if(target.GetComponent<PlayerBehaviour>().Manager.Id == _curTarget)
						return target.transform.position;
					targets.Add(target);
				}
			}
		}
		
		if(targets.Count == 0)
		{
			_curTarget = -1;
			return Vector3.zero;
		}
		else
		{
			int idx = Random.Range(0,targets.Count);
			PlayerBehaviour pb = targets[idx].GetComponent<PlayerBehaviour>();
			_curTarget = pb.Manager.Id;
			return targets[idx].transform.position;
		}
	}
}
