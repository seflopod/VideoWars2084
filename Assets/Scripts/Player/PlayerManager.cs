using UnityEngine;

public class PlayerManager : IObjectManager
{
	public struct StateBools
	{
		public bool inGame;
		public bool canJump;
		public bool canShoot;
		public bool canThrust;
		public bool reloading;
		public bool fireCooldown;
		public bool thrusting;
		public bool fullThrustRegen;
		public bool dead;
		
		public void Reset()
		{
			inGame = false;
			canJump = true;
			canShoot = true;
			canThrust = true;
			reloading = false;
			fireCooldown = false;
			thrusting = false;
			fullThrustRegen = false;
			dead = false;
		}
	};
	
	private GameObject _prefab;
	private GameObject _playerObj;
	private int _id;
	private PlayerStatsData _stats;
	private PlayerScoreData _score;
	private float _reloadTimer;
	private float _fireTimer;
	private float _thrustRegenTimer;
	private StateBools _state;
	private string _name;
	private Vector3 _moveDirection;
	private Vector3 _fireDirection;
	private bool _isHuman;
	private Material _mat;
	
	public PlayerManager(GameObject prefab, string name, int id, Material mat)
	{
		_prefab = prefab;
		_name = name;
		_id = id;
		_mat = mat;
		
		_state = new StateBools();
		_state.Reset();
		_stats = new PlayerStatsData();
		_score = new PlayerScoreData();
		Credits = 0;
	}
	
	public void SpawnInStage(Vector3 pos)
	{
		if(!_state.inGame)
		{
			_playerObj = (GameObject)GameObject.Instantiate(_prefab, pos,
														Quaternion.identity);
			_playerObj.GetComponent<MeshRenderer>().material = _mat;
			_playerObj.GetComponent<PlayerBehaviour>().Manager = this;
			_reloadTimer = 0.0f;
			_fireTimer = 0.0f;
			_thrustRegenTimer = 0.0f;
			_moveDirection = Vector3.right;
			_fireDirection = _moveDirection;
			_state.inGame = true;
		}
	}
	
	#region movement
	public void Move(Vector3 dir, float dt)
	{
		if(dir == _moveDirection)
			return;
		
		_moveDirection = dir;
		
		Vector3 velNorm = _playerObj.rigidbody.velocity.normalized;
		_moveDirection = (velNorm + dir).normalized;
		_fireDirection = _moveDirection;
		float curSpeedSq = _playerObj.rigidbody.velocity.sqrMagnitude;
		if(velNorm != _moveDirection || curSpeedSq < _stats.moveSpeed * _stats.moveSpeed)
		{
			_playerObj.rigidbody.velocity = _playerObj.rigidbody.velocity + _moveDirection * _stats.moveSpeed;
			if(_playerObj.rigidbody.velocity.sqrMagnitude > _stats.moveSpeed*_stats.moveSpeed)
				_playerObj.rigidbody.velocity = _moveDirection * _stats.moveSpeed;
		}
	}
	
	public void Stop()
	{
		Vector3 vel = _playerObj.rigidbody.velocity;
		vel.x *= 0.4f;
		
		//if the player is moving up the reduction is less than if they are
		//moving down.
		vel.y = (vel.y > 0.0f) ? vel.y*0.8f : vel.y*1.001f;
		_playerObj.rigidbody.velocity = vel;
	}
	#endregion
	
	#region actions
	/// <summary>
	/// Fires a bullet in direction the player is currently moving.
	/// </summary>
	public void Fire()
	{
		if(_state.canShoot)
		{
			Vector3 pos = _playerObj.transform.position;
			float dPos = _playerObj.rigidbody.velocity.magnitude *
																Time.deltaTime;
			
			//adjust so the bullet is spawned outside of the player body
			if(_fireDirection == Vector3.zero)
				pos.x += _playerObj.transform.localScale.x/2 + 0.5f + dPos;
			else
			{
				pos.x += _moveDirection.x *
							(_playerObj.transform.localScale.x/2 + 0.5f + dPos);
				pos.y += _moveDirection.y *
							(_playerObj.transform.localScale.y/2 + 0.5f + dPos);
			}
			
			//tell the game to spawn the bullet at the proper position
			GameManagerBehaviour.Instance.SpawnBullet(pos, _fireDirection, _id);
			
			_score.bulletsFired++;
			_stats.ammoRemaining--;
			
			//check to see if we need to reload or just put firing on cooldown
			if(_stats.ammoRemaining <= 0)
			{
				_stats.ammoRemaining = 0;
				_reloadTimer = 0.0f;
				_state.reloading = true;
			}
			else
			{
				_fireTimer = 0.0f;
				_state.fireCooldown = true;
			}
			
			_state.canShoot = false;
			
			//play audio
			_playerObj.audio.clip = GameManagerBehaviour.Instance.Sounds["fire"];
			_playerObj.audio.Play();
		}
	}
		
	public void Thrust()
	{
		if(_state.canThrust)
		{
			const int costPerTick = 100;
			if(!_state.fullThrustRegen && _stats.fuelRemaining >= costPerTick)
			{   //continue thrusting
				//Vector3 force = _playerObj.transform.up * _stats.thrustForce;
				Vector3 force = _moveDirection * 2 * _stats.thrustForce;
				_playerObj.rigidbody.AddForce(force, ForceMode.Force);
				_stats.fuelRemaining -= costPerTick;
			}
			
			if(_stats.fuelRemaining <= 0)
			{   //stop thrusting, out of fuel
				_state.canThrust = false;
				_state.thrusting = false;
				_state.fullThrustRegen = true;
				_thrustRegenTimer = -5.0f;
			}
		}
	}
	
	public void EndThrust()
	{
		_state.thrusting = false;
		if(!_state.fullThrustRegen)
			_thrustRegenTimer = 0.0f;
	}
	#endregion
	
	
	public void TakeDamage(int dmg, int shooterId)
	{
		_stats.health -= dmg;
		
		//set the texture based on the amount of health left
		float hPct = _stats.health/(float)_stats.maxHealth;
		if(hPct > 0.8f)
		{
			_playerObj.GetComponent<MeshRenderer>().material.mainTexture =
										GameManagerBehaviour.Instance.healthTextures[0];
		}
		else if(hPct <= 0.8f && hPct > 0.6f)
		{
			_playerObj.GetComponent<MeshRenderer>().material.mainTexture =
										GameManagerBehaviour.Instance.healthTextures[1];
		}
		else if(hPct <= 0.6f && hPct > 0.4f)
		{
			_playerObj.GetComponent<MeshRenderer>().material.mainTexture =
										GameManagerBehaviour.Instance.healthTextures[2];
		}
		else if(hPct <= 0.4f && hPct > 0.2f)
		{
			_playerObj.GetComponent<MeshRenderer>().material.mainTexture =
										GameManagerBehaviour.Instance.healthTextures[3];
		}
		else
		{
			_playerObj.GetComponent<MeshRenderer>().material.mainTexture =
										GameManagerBehaviour.Instance.healthTextures[4];
		}
		
		//play sound for damage
		_playerObj.audio.clip = GameManagerBehaviour.Instance.Sounds["damage"];
		_playerObj.audio.Play();
		
		if(_stats.health <= 0)
		{   //dead
			_state.dead = true;
			++_score.deaths;
			_stats.health = 0;
			TimeOfDeath = Time.time;
			GameManagerBehaviour.Instance.PlayerDied(this, shooterId);
			_playerObj.rigidbody.velocity = Vector3.zero;
			_playerObj.audio.Stop();
			_playerObj.SetActive(false);
		}
	}
	
	public void StepTimers(float dt)
	{
		if(_state.reloading)
		{
			_reloadTimer+=dt;
			if(_reloadTimer >= _stats.reloadTime)
			{
				_state.reloading = false;
				_state.canShoot = true;
				_stats.ammoRemaining = _stats.maxAmmo;
			}
		}
		
		if(_state.fireCooldown)
		{
			_fireTimer+=dt;
			if(_fireTimer >= _stats.rateOfFire)
			{
				_state.fireCooldown = false;
				_state.canShoot = true;
			}
		}
		
		if(_thrustRegenTimer <= 0.0f)
			_thrustRegenTimer += dt;
		
		if(_thrustRegenTimer >= 0.0f)
		{   //delay in thrust regen is over or we never had one
			const int fuelToRegen = 25;
			if(_stats.fuelRemaining < _stats.maxFuel)
				_stats.fuelRemaining += fuelToRegen;
			
			if(_stats.fuelRemaining >= _stats.maxFuel)
			{
				_stats.fuelRemaining = _stats.maxFuel;
			
				if(_state.fullThrustRegen)
				{
					_state.canThrust = true;
					_state.fullThrustRegen = false;
				}
			}
		}
	}
	
	public void ReSpawn(Vector3 spawnPosition)
	{
		_stats.health = _stats.maxHealth;
		_stats.ammoRemaining = _stats.maxAmmo;
		_stats.fuelRemaining = _stats.maxFuel;
		_state.Reset();
		_playerObj.SetActive(true);
		_playerObj.rigidbody.isKinematic = true;
		_playerObj.transform.position = spawnPosition;
		_playerObj.rigidbody.isKinematic = false;
		_playerObj.GetComponent<MeshRenderer>().material.mainTexture =
										GameManagerBehaviour.Instance.healthTextures[0];
	}
	
	public void Kill()
	{
		_state.inGame = false;
		GameObject.DestroyImmediate(_playerObj);
	}
	
	#region IObject_implementation
	public void Disable()
	{
		_playerObj.SetActive(false);
	}
	
	public void Enable()
	{
		_playerObj.SetActive(true);
	}
	
	public int Id { get { return _id; } }
	#endregion
	
	#region score_properties
	public PlayerScoreData ScoreData
	{
		get { return _score; }
		set { _score = value; }
	}
	
	public int Kills
	{
		get { return _score.kills; }
		set { _score.kills = value; }
	}
	
	public int Deaths
	{
		get { return _score.deaths; }
		set { _score.deaths = value; }
	}
	
	public int Score
	{
		get
		{
			int s = Mathf.FloorToInt(31250.0f * _score.kills *
							_score.bulletsHit/_score.bulletsFired - 3125 *
							_score.deaths);
			return (s < 0) ? 0 : s;
		}
	}
	#endregion
	
	#region state_properties
	public StateBools State
	{
		get { return _state; }
		set { _state = value; }
	}
	
	public bool CanShoot
	{
		get { return _state.canShoot; }
		set { _state.canShoot = value; }
	}
	
	public bool CanJump
	{
		get { return _state.canJump; }
		set { _state.canJump = value; }
	}
	
	public bool CanThrust
	{
		get { return _state.canThrust; }
		set { _state.canThrust = value; }
	}
	
	public bool Reloading
	{
		get { return _state.reloading; }
		set { _state.reloading = value; }
	}
	
	public bool FireCooldown
	{
		get { return _state.fireCooldown; }
		set { _state.fireCooldown = value; }
	}
	public bool Thrusting
	{
		get { return _state.thrusting; }
		set { _state.thrusting = value; }
	}
	
	public bool FullThrustRegen
	{
		get { return _state.fullThrustRegen; }
		set { _state.fullThrustRegen = value; }
	}
	
	public bool IsDead
	{
		get { return _state.dead; }
		set { _state.dead = value; }
	}
	
	public bool InGame
	{
		get { return _state.inGame; }
		set { _state.inGame = value; }
	}
	#endregion
	
	public PlayerStatsData Stats
	{
		get { return _stats; }
		set { _stats = value; }
	}
	
	public Vector3 FireDirection
	{
		get { return _fireDirection; }
		set { _fireDirection = value; }
	}
	
	public Vector3 MoveDirection
	{
		get { return _moveDirection; }
	}
	
	public string Name
	{
		get { return _name; }
		set { _name = value; }
	}
	
	public float TimeOfDeath { get; set; }
	
	public GameObject PlayerObject
	{
		get { return _playerObj; }
	}
	
	public bool IsHuman
	{
		get { return _isHuman; }
		set { _isHuman = value; }
	}
	
	public int Credits { get; set; }
}