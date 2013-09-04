using UnityEngine;
using System.Collections.Generic;

public class HumanPlayerManager : PlayerManager
{
	public HumanPlayerManager(GameObject prefab, string name, int id)
	{
		_prefab = prefab;
		_name = name;
		_id = id;
		_state = new StateBools();
		_state.Reset();
		_stats = new PlayerStatsData();
		_score = new PlayerScoreData();
		Credits = 0;
	}
	
	public override void JoinGame(Vector3 pos, Material mat)
	{
		if(!_state.inGame)
		{
			_playerObj = (GameObject)GameObject.Instantiate(_prefab, pos,
														Quaternion.identity);
			_playerObj.GetComponent<MeshRenderer>().material = mat;
			_playerObj.GetComponent<PlayerBehaviour>().Manager = this;
			_reloadTimer = 0.0f;
			_fireTimer = 0.0f;
			_thrustRegenTimer = 0.0f;
			_moveDirection = Vector3.right;
			_fireDirection = _moveDirection;
			_state.inGame = true;
		}
	}
	
	public int Credits { get; set; }
}