using UnityEngine;
using System.Collections.Generic;

public class GameManager : Singleton<GameManager>
{
	public struct GameState
	{
		public bool title;
		public bool player1InsertCoin;
		public bool player2InsertCoin;
		public bool player1Play;
		public bool player2Play;
		public bool gameOver;
		public bool demo;
	}
	
	private static int _nextId = 1;
	
	public int maxPlayers = 4;
	
	public Color[] playerColors;
	public Material basePlayerMaterial;
	public GameObject playerPrefab;
	public GameObject aiPrefab;
	public GameObject terrainPrefab;
	public GameObject bulletPrefab;
	public GameObject deathSoundPrefab;
	public Material[] playerMaterials;
	public Material[] bulletMaterials;
	public Texture2D[] healthTextures;
	public SoundsMap[] sounds;
	
	
	private List<IObjectManager> _objects;
	private PlayerManager[] _players;
	private HumanPlayerManager[] _humans;
	private int _localPlayer;
	//private int _localPlayer;
	private BulletManager[] _bullets;
	private int _bulletIdx;
	private Queue<PlayerManager> _toRespawn;
	private Vector3[] _spawnPoints;
	private InputSetupData _player1Keys;
	private InputSetupData _player2Keys;
	private InputSetupData[] _playerKeys;
	private GameState _state;
	private GUIManager _gui;
	
	protected override void Awake()
	{
		base.Awake();
		Object.DontDestroyOnLoad(gameObject);
		_state = new GameState();
		_state.title = true;
		_state.player1InsertCoin = true;
		_state.player2InsertCoin = true;
		_state.player1Play = false;
		_state.player2Play = false;
		_state.gameOver = false;
		_state.demo = false;
	}
	
	private void Start()
	{
		_gui = gameObject.GetComponent<GUIManager>();
		_playerKeys = new InputSetupData[2];
		_playerKeys[0] = new InputSetupData(1);
		_playerKeys[1] = new InputSetupData(2);
		_players = new AIPlayerManager[maxPlayers];
		_humans = new HumanPlayerManager[2];
	}
	
	private void Update()
	{
		TimedEventManager.IncrementTimers(Time.deltaTime);
		for(int i=0;i<_players.Length;++i)
			_players[i].StepTimers(Time.deltaTime);
		ProcessCoinStartInput();
		
		if(_state.title || _state.demo)
		{ 
		}
		else
		{
			if(_state.player1Play)
				ProcessMovementInput(0);
			if(_state.player2Play)
				ProcessMovementInput(1);
			
			if(_state.player1Play)
				ProcessActionInput(0);
			if(_state.player2Play)
				ProcessActionInput(1);
		}
	}
	
	private void LateUpdate()
	{
		//do respawn after one second
		while(_toRespawn.Count > 0 &&
				Time.time - _toRespawn.Peek().TimeOfDeath >= 1.0f)
			_toRespawn.Dequeue().ReSpawn(_spawnPoints[Random.Range(0,_spawnPoints.Length)]);
	}
	
	private void OnLevelWasLoaded(int lvlIdx)
	{
		if(!Application.loadedLevelName.Equals("title"))
			StartBoard();
	}
	
	#region input_handling
	private void ProcessCoinStartInput()
	{
		if(Input.GetKeyUp(_playerKeys[0].coin))
			_humans[0].Credits++;
		if(Input.GetKeyUp(_playerKeys[1].coin))
			_humans[1].Credits++;
		
		bool start1 = false, start2 = false;
		if(!_state.player1Play && Input.GetKeyUp(_playerKeys[0].start) && _humans[0].Credits >= 1)
		{
			_state.player1Play = true;
			if(_state.player1InsertCoin)
			{
				_state.player1InsertCoin = false;
				start1 = true;
			}
		}
		if(!_state.player2Play && Input.GetKeyUp(_playerKeys[1].start) && _humans[1].Credits >= 1)
		{
			_state.player2Play = true;
			if(_state.player2InsertCoin)
			{
				_state.player2InsertCoin = false;
				start2 = true;
			}
		}
		
		if((_state.demo || _state.title) && (_state.player1Play || _state.player2Play))
		{
			StartPlaying();
		}
		else if(start1)
		{
			HumanJoin(0);
		}
		else if(start2)
		{
			HumanJoin(1);
		}
	}
	
	private void ProcessMovementInput(int idx)
	{
		Vector3 moveDirection = Vector3.zero;
		if(Input.GetKey(_playerKeys[idx].up))
			moveDirection.y+=1;
		if(Input.GetKey(_playerKeys[idx].down))
			moveDirection.y-=1;
		if(Input.GetKey(_playerKeys[idx].left))
			moveDirection.x-=1;
		if(Input.GetKey(_playerKeys[idx].right))
			moveDirection.x+=1;
		
		//normalize if two keys hit
		if(moveDirection.x != 0 && moveDirection.y != 0)
			moveDirection = moveDirection.normalized;
		
		if(moveDirection != Vector3.zero)
			_players[0].Move(moveDirection, Time.deltaTime);
	}
	
	private void ProcessActionInput(int idx)
	{
		if(Input.GetKey(_playerKeys[idx].button6))
			_players[idx].Jump();
		
		if(Input.GetKey(_playerKeys[idx].button5))
			_players[idx].Thrust();
		
		if(Input.GetKeyUp(_playerKeys[idx].button5))
			_players[idx].EndThrust();
		
		if(Input.GetKeyDown(_playerKeys[idx].button4))
			_players[idx].Fire();
	}
	#endregion
	//Applies when transitioning to title screen
	private void StartTitle()
	{
		
	}
	
	//Applies when the game transitions from title/demo to humans playing
	private void StartPlaying()
	{
		for(int i=0;i<_humans.Length;++i)
			if(_humans[i].Credits >= 1 && _players[i].Id != _humans[i].Id)
				HumanJoin(i);
		
		_state.demo = false;
		_state.title = false;
		
		//reset all scores
		for(int i=0;i<_players.Length;++i)
			_players[i].Score.Reset();
		
		//reload level to make sure game is reset with humans
		Application.LoadLevel(1); //should be a playable level
	}
	
	//Applies when showing the demo
	private void StartDemo()
	{
		//remove any humans
		for(int i=0;i<_humans.Length;++i)
		{
			if(_humans[i] == _players[i])
			{
				_humans[i].Kill();
				_players[i] = new AIPlayerManager(aiPrefab, "some name", _nextId++);
			}
		}
		
		//populate with AIPlayers
		AIJoin();
	}
	
	//Applies to every time a playable level is started
	private void StartBoard()
	{
		_toRespawn = new Queue<PlayerManager>();
		
		//find all spawn points and populate the spawn point array
		GameObject[] spawns = GameObject.FindGameObjectsWithTag("spawn_point");
		_spawnPoints = new Vector3[spawns.Length];
		for(int i=0;i < spawns.Length;++i)
			_spawnPoints[i] = spawns[i].transform.position;
		
		//create and spawn all bullets
		GameObject go = new GameObject("BulletContainer");
		go.transform.position = Vector3.zero;
		
		_bullets = new BulletManager[100];
		for(int i=0;i<100;++i)
		{
			GameObject b = (GameObject)GameObject.Instantiate(bulletPrefab);
			b.transform.localScale = 0.2f * Vector3.one;
			b.transform.parent = go.transform;
			_bullets[i] = new BulletManager(b, _nextId++);
			b.GetComponent<BulletBehaviour>().manager = _bullets[i];
			_bullets[i].Disable();
		}
		_bulletIdx = 0;
		
		//add all players to the board
		for(int i=0;i<_players.Length;++i)
		{
			Material mat = new Material(basePlayerMaterial);
			mat.color = playerColors[i];
			_players[i].JoinGame(_spawnPoints[Random.Range(0,_spawnPoints.Length)], mat);
		}
	}
	
	private void CreatePlayers()
	{
		_humans = new HumanPlayerManager[2];
		_humans[0] = new HumanPlayerManager(playerPrefab, "some name", _nextId++);
		_humans[1] = new HumanPlayerManager(playerPrefab, "some name", _nextId++);
		for(int i=0; i < maxPlayers; ++i)
		{
			_players[i] = new AIPlayerManager(aiPrefab, "some name", _nextId++);
		}
	}
	
	private void HumanJoin(int idx)
	{
		_players[idx].Kill();
		Material mat = new Material(basePlayerMaterial);
		mat.color = playerColors[idx];
		_humans[idx].JoinGame(_spawnPoints[Random.Range(0,_spawnPoints.Length)], mat);
		_players[idx] = _humans[idx];
	}
	
	private void AIJoin()
	{
		for(int i=0; i< maxPlayers; ++i)
		{
			if(((AIPlayerManager)_players[i]) != null)
			{
				Material mat = new Material(basePlayerMaterial);
				mat.color = playerColors[i];
				_players[i].JoinGame(_spawnPoints[Random.Range(0,_spawnPoints.Length)], mat);
			}
		}
	}
	
	public void SpawnBullet(Vector3 pos, Vector3 velDir, int shooterId)
	{
		
		if(_bulletIdx >= 100) //just to limit the number of times we need to use mod
			_bulletIdx%=100;
		_bullets[_bulletIdx++].Spawn(pos, velDir, shooterId, bulletMaterials[shooterId]);
	}
	
	public void PlayerDied(PlayerManager pm, int killerId)
	{
		_toRespawn.Enqueue(pm);
		//play sound for death
		int d_id = pm.Id;
		Vector3 d_pos = Vector3.zero;
		for(int i=0;i<_players.Length;++i)
		{
			if(_players[i].Id == d_id)
			{
				d_pos = _players[i].PlayerObject.transform.position;
			}
		}
		GameObject ds = (GameObject)GameObject.Instantiate(deathSoundPrefab, d_pos, Quaternion.identity);
		ds.audio.clip = GetClip("death");
		ds.audio.Play();
		
		//destroy the DeathSound GO when the sound is done
		GameObject.Destroy(ds, 2 * ds.audio.clip.length);
		
		//this can't work for long, find a better way
		if(killerId != d_id) //don't want suicide to count
		{
			for(int i=0;i<_players.Length;++i)
			{
				if(_players[i].Id == killerId)
				{
					_players[i].Score.kills++;
					break;
				}
			}
		}
	}
	
	public AudioClip GetClip(string name)
	{
		foreach(SoundsMap sm in sounds)
			if(sm.name == name)
				return sm.sound;
		
		return null;
	}
	
	public PlayerManager[] Players { get { return _players; } }
	public GameState State { get { return _state; } }
}