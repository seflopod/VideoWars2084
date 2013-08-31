using UnityEngine;
using System.Collections.Generic;

public class GameManager : Singleton<GameManager>
{
	private static int _nextId = 1;
	
	public int maxPlayers = 4;
	
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
	private GameObject[] _players;
	private int _localPlayer;
	//private int _localPlayer;
	private BulletManager[] _bullets;
	private int _bulletIdx;
	private Queue<PlayerManager> _toRespawn;
	private Vector3[] _spawnPoints;
	private InputSetupData _player1Keys;
	private InputSetupData _player2Keys;
	
	private void Start()
	{
		_objects = new List<IObjectManager>();
		_players = new GameObject[maxPlayers];
		_toRespawn = new Queue<PlayerManager>();
		
		GameObject[] spawns = GameObject.FindGameObjectsWithTag("spawn_point");
		_spawnPoints = new Vector3[spawns.Length];
		for(int i=0;i < spawns.Length;++i)
			_spawnPoints[i] = spawns[i].transform.position;
			
		//make local player
		_players[0] = (GameObject)GameObject.Instantiate(playerPrefab, _spawnPoints[Random.Range(0,_spawnPoints.Length)], Quaternion.identity);
		Camera.main.GetComponent<CameraFollow>().target = _players[0].transform;
		_players[0].GetComponent<PlayerBehaviour>().manager = new PlayerManager(_players[0], "Player", _nextId++);
		_players[0].GetComponent<MeshRenderer>().material = playerMaterials[0];
		_localPlayer = 0;	
		
		//add other players
		for(int i=1;i<maxPlayers;++i)
		{
			_players[i] = (GameObject)GameObject.Instantiate(aiPrefab, _spawnPoints[Random.Range(0,_spawnPoints.Length)], Quaternion.identity);
			_players[i].GetComponent<PlayerBehaviour>().manager = new PlayerManager(_players[i], "CPU "+i.ToString(), _nextId++);
			_players[i].GetComponent<AIBehaviour>().manager = _players[i].GetComponent<PlayerBehaviour>().manager;
			_players[i].GetComponent<MeshRenderer>().material = playerMaterials[i+1];
		}
		
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
			((IObjectManager)_bullets[i]).Disable();
			//b.SetActive(false);
		}
		_bulletIdx = 0;
		
		_player1Keys = new InputSetupData(1);
		_player2Keys = new InputSetupData(2);
		
	}
	
	private void Update()
	{
		//check for player one movement
		Vector3 moveDirection = Vector3.zero;
		if(Input.GetKey(_player1Keys.up))
			moveDirection.y+=1;
		if(Input.GetKey(_player1Keys.down))
			moveDirection.y-=1;
		if(Input.GetKey(_player1Keys.left))
			moveDirection.x-=1;
		if(Input.GetKey(_player1Keys.right))
			moveDirection.x+=1;
		
		//normalize if two keys hit
		if(moveDirection.x != 0 && moveDirection.y != 0)
			moveDirection = moveDirection.normalized;
		
		if(moveDirection != Vector3.zero)
			_players[_localPlayer].GetComponent<PlayerBehaviour>().manager.Move(moveDirection, Time.deltaTime);
	}
	
	private void FixedUpdate()
	{
		if(Input.GetKey(_player1Keys.button6))
			_players[_localPlayer].GetComponent<PlayerBehaviour>().manager.Jump();
		
		if(Input.GetKey(_player1Keys.button5))
			_players[_localPlayer].GetComponent<PlayerBehaviour>().manager.Thrust();
		
		if(Input.GetKeyUp(_player1Keys.button5))
			_players[_localPlayer].GetComponent<PlayerBehaviour>().manager.EndThrust();
		
		if(Input.GetKeyDown(_player1Keys.button4))
		{
			Vector3 mousePos = Input.mousePosition;
			Plane xy = new Plane(-Vector3.forward, Vector3.zero);
			Ray ray = Camera.main.ScreenPointToRay(mousePos);
			float dist;
			
			xy.Raycast(ray, out dist);
			Vector3 mouseWorld = ray.GetPoint(dist);
			
			_players[_localPlayer].GetComponent<PlayerBehaviour>().manager.Fire(mouseWorld);
		}
	}
	
	private void LateUpdate()
	{
		_players[_localPlayer].GetComponent<PlayerBehaviour>().manager.StepTimers(Time.deltaTime);
		//do respawn after one second
		while(_toRespawn.Count > 0 &&
				Time.time - _toRespawn.Peek().TimeOfDeath >= 1.0f)
			_toRespawn.Dequeue().ReSpawn(_spawnPoints[Random.Range(0,_spawnPoints.Length)]);
	}
	
	private void OnGUI()
	{
		if(_players.Length > 0)
		{
			for(int i=0; i<_players.Length; ++i)
			{
				PlayerManager p = _players[i].GetComponent<PlayerBehaviour>().manager;
				string[] lbls = new string[5]{ p.Name, "Health", "Fuel", "Ammo",
												"Kills/Deaths" };
				string[] vals = new string[4] {
					p.Stats.health.ToString() + "/" + p.Stats.maxHealth,
					p.Stats.fuelRemaining.ToString() + "/" + p.Stats.maxFuel,
					p.Stats.ammoRemaining.ToString() + "/" + p.Stats.maxAmmo,
					p.Score.kills + "/" + p.Score.deaths };
				
				Rect lbl, data;
				float w=100.0f, h=20.0f, hpad=1.0f;
				float y = (i%2 == 0) ? 0.0f : Screen.height - 5.0f*(h+hpad);
				float x = (i < 2) ? 0.0f : Screen.width - 200.0f;
				lbl = new Rect(x, y, w, h);
				data = new Rect(x + w, y+h+hpad, w, h);
				
				for(int j=0;j<5;++j)
				{
					GUI.Label(lbl, lbls[j]);
					lbl.center = new Vector2(lbl.center.x, lbl.center.y + h + hpad);
					
					if(j==0)
						continue;
					
					GUI.Label(data, vals[j-1]);
					data.center = new Vector2(data.center.x, data.center.y + h + hpad);
				}
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
		int d_id = ((IObjectManager)pm).Id;
		Vector3 d_pos = _players[d_id-1].transform.position;
		GameObject ds = (GameObject)GameObject.Instantiate(deathSoundPrefab, d_pos, Quaternion.identity);
		ds.audio.clip = GetClip("death");
		ds.audio.Play();
		
		//destroy the DeathSound GO when the sound is done
		GameObject.Destroy(ds, 2 * ds.audio.clip.length);
		
		//this can't work for long, find a better way
		if(killerId != ((IObjectManager)pm).Id) //don't want suicide to count
			_players[killerId - 1].GetComponent<PlayerBehaviour>().manager.Score.kills++;
	}
	
	public AudioClip GetClip(string name)
	{
		foreach(SoundsMap sm in sounds)
			if(sm.name == name)
				return sm.sound;
		
		return null;
	}
}