using UnityEngine;
using System.Collections.Generic;

public class AIPlayerManager : PlayerManager
{
	private int _curTarget;
	private float _moveTimer;
	private float _moveTimerTarget;
	private int _dir;

	public AIPlayerManager(GameObject prefab, string name, int id)
	{
		_prefab = prefab;
		_name = name;
		_id = id;
		_state = new StateBools();
		_state.Reset();
		_stats = new PlayerStatsData();
		_score = new PlayerScoreData();
		_curTarget = -1;
		_moveTimer = 0.0f;
		_moveTimerTarget = 0.0f;
		_dir = 0;
	}
	
	public override void JoinGame(Vector3 pos, Material mat)
	{
		if(!_state.inGame)
		{
			_playerObj = (GameObject)GameObject.Instantiate(_prefab, pos,
														Quaternion.identity);
			_playerObj.GetComponent<MeshRenderer>().material = mat;
			_playerObj.GetComponent<AIBehaviour>().Manager = this;
			_reloadTimer = 0.0f;
			_fireTimer = 0.0f;
			_thrustRegenTimer = 0.0f;
			_moveDirection = Vector3.right;
			_fireDirection = _moveDirection;
			_state.inGame = true;
		}
	}
	
	/// <summary>
	/// Fires a bullet in direction the player is currently moving.
	/// </summary>
	public override void Fire()
	{
		if(_state.canShoot)
		{
			//as AI we need to see if we have a target first
			_fireDirection = FindTarget();
			if(_fireDirection == Vector3.zero) //no target, return
				return;
			
			base.Fire();
		}
	}
	
	public void Move()
	{
		Move (Vector3.zero, Time.deltaTime);
	}
	
	public override void Move(Vector3 dir, float dt)
	{
		_moveTimer += dt;
		if(_moveTimer >= _moveTimerTarget)
		{
			_moveTimer = 0.0f;
			_moveTimerTarget = Random.Range(0.25f, 1.0f);
			_dir = Random.Range(0,8);
		}
				
		base.Move(new Vector3(Mathf.Cos(_dir*Mathf.PI),
								Mathf.Sin(_dir*Mathf.PI),
								0.0f
							),
					dt);
	}
	
	public void ForceDirectionChange()
	{
		_moveTimerTarget = 0.0f;
	}
	
	private Vector3 FindTarget()
	{
		List<GameObject> targets = new List<GameObject>();
		
		for(float ang=0.0f; ang < 360.0f; ang+=45.0f)
		{
			Ray ray = new Ray(_playerObj.transform.position,
							new Vector3(Mathf.Cos(ang), Mathf.Sin(ang), 0.0f));
			RaycastHit hit;
			if(Physics.Raycast(ray, out hit, 50.0f))
			{
				if(hit.collider.gameObject.CompareTag("Player"))
					targets.Add(hit.collider.gameObject);
			}
		}
			
		//see if we our current target is still available
		foreach(GameObject target in targets)
		{
			if(target.GetComponent<PlayerBehaviour>().Manager.Id ==
																_curTarget)
				return target.transform.position;
		}
		
		if(targets.Count == 0)
		{
			_curTarget = -1;
			return Vector3.zero;
		}
		else
		{
			int idx = Random.Range(0,targets.Count);
			_curTarget =
					targets[idx].GetComponent<PlayerBehaviour>().Manager.Id;
			return targets[idx].transform.position;
		}
	}
}