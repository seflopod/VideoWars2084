using UnityEngine;
using System.Collections.Generic;

public class GameManagerBehaviour : Singleton<GameManagerBehaviour>
{
	public enum GameState
	{
		NotPlaying = 0x01,
		DemoPlaying = 0x02,
		PlayerPlaying = 0x03
	};
	
	private static int _nextId = 1;
	
	public int maxPlayers = 8;	
	public Material basePlayerMaterial;
	public Material baseBulletMaterial;
	public Color[] playerColors;
	
	public GameObject mainCameraPrefab;
	public GameObject playerPrefab;
	public GameObject bulletPrefab;
	public GameObject deathSoundPrefab;
	
	public Texture2D[] healthTextures;
	public SoundsMap[] sounds;
	
	private PlayerManager[] _players;
	private BulletManager[] _bullets;
	private Queue<PlayerManager> _toRespawn;
	private InputSetupData[] _playerKeys;
	private GameState _state;
	
	private int _bulletIdx;
	private int _curStage;
	private Vector3[] _spawnPoints;
	private Dictionary<string, AudioClip> _soundMap;
	
	#region monobehaviour_funcs
	protected override void Awake()
	{
		base.Awake();
		Object.DontDestroyOnLoad(gameObject);
	}
	
	private void Start()
	{
		_toRespawn = new Queue<PlayerManager>(2*maxPlayers);
		_curStage = 0;
		
		_soundMap = new Dictionary<string, AudioClip>();
		foreach(SoundsMap s in sounds)
		{
			_soundMap[s.name] = s.sound;
		}
		
		CreatePlayers();
		CreateBullets();
	}
	
	private void Update()
	{
	}
	
	private void OnLevelWasLoaded(int lvlIdx)
	{
		if(!Application.loadedLevelName.Equals("title"))
			StartStage();
	}
	#endregion
	
	#region startup_funcs
	private void CreatePlayers()
	{
		_players = new PlayerManager[maxPlayers];
		_playerKeys = new InputSetupData[2];
		for(int i=0;i<maxPlayers;++i)
		{
			Material mat = new Material(basePlayerMaterial);
			mat.color = playerColors[i];
			_players[i] = new PlayerManager(playerPrefab, "CPU", i, mat);
			
			if(i < 2)
				_playerKeys[i] = new InputSetupData(i+1);
		}
	}
	
	private void CreateBullets()
	{
		_bullets = new BulletManager[100];
		for(int i=0;i<100;++i)
		{
			_bullets[i] = new BulletManager(bulletPrefab, i);
			_bullets[i].Disable();
		}
		_bulletIdx = 0;
	}
	
	private void StartTitle()
	{
		_state = GameState.NotPlaying;
		EndBoard();
		Application.LoadLevel("title");
	}
	
	private void StartDemoPlaying()
	{
		_state = GameState.DemoPlaying;
		EndBoard();
		Application.LoadLevel(_curStage);
	}
	
	private void StartPlayerPlaying()
	{
		_state = GameState.PlayerPlaying;
		EndBoard();
		Application.LoadLevel(_curStage);
	}
	
	private void HumanJoin(int idx)
	{
		_players[idx].IsHuman = true;
		
		if(_state != GameState.PlayerPlaying)
			StartPlayerPlaying();
		else
			_toRespawn.Enqueue(_players[idx]);
	}
	
	private void StartStage()
	{
		//clear the respawn queue, though this may be redundant
		_toRespawn.Clear();
		
		//find all spawn points and populate the spawn point array
		//this will shuffle the found spawn points as well
		GameObject[] spawns = GameObject.FindGameObjectsWithTag("spawn_point");
		_spawnPoints = new Vector3[spawns.Length];
		_spawnPoints[0] = spawns[0].transform.position;
		for(int i=1;i < spawns.Length;++i)
		{
			int j = Random.Range(0, i);
			if(j != i)
				_spawnPoints[i] = _spawnPoints[j];
			_spawnPoints[j] = spawns[i].transform.position;
		}
				
		int spwnIdx = 0;
		for(int i=0;i<maxPlayers;++i)
			_players[i].SpawnInStage(_spawnPoints[spwnIdx++ % _spawnPoints.Length]);
		
	}
	#endregion
	
	private void EndBoard()
	{}
	
	#region input_handling
	private void ProcessCoinStartInput()
	{
		for(int i=0;i<2;++i)
		{
			if(Input.GetKeyUp(_playerKeys[i].coin))
				_players[i].Credits++;
			
			if(!_players[i].IsHuman &&
				Input.GetKeyUp(_playerKeys[i].start) &&
				_players[i].Credits >= 1)
				HumanJoin(i);
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
			//_players[idx].Jump();
		
		if(Input.GetKey(_playerKeys[idx].button5))
			_players[idx].Thrust();
		
		if(Input.GetKeyUp(_playerKeys[idx].button5))
			_players[idx].EndThrust();
		
		if(Input.GetKeyDown(_playerKeys[idx].button4))
			_players[idx].Fire();
	}
	#endregion
	
	public void SpawnBullet(Vector3 pos, Vector3 velDir, int shooterId)
	{
		if(_bulletIdx >= 100) //just to limit the number of times we need to use mod
			_bulletIdx%=100;
		Material mat = new Material(baseBulletMaterial);
		mat.color = playerColors[shooterId];
		_bullets[_bulletIdx++].Spawn(pos, velDir, shooterId, mat);
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
		GameObject ds = (GameObject)GameObject.Instantiate(deathSoundPrefab,
											pm.PlayerObject.transform.position,
											Quaternion.identity);
		ds.audio.clip = _soundMap["death"];
		ds.audio.Play();
		
		//destroy the DeathSound GO when the sound is done
		GameObject.Destroy(ds, 2 * ds.audio.clip.length);
		
		//don't want suicide to count
		if(killerId != pm.Id)
			_players[killerId].Kills++;
	}
	
	#region properties
	public GameState State { get { return _state; } }
	public PlayerManager[] Players { get { return _players; } }
	public Dictionary<string, AudioClip> Sounds { get { return _soundMap; } }
	#endregion
}