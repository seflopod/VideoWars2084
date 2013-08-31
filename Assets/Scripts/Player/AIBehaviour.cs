using UnityEngine;
using System.Collections;

public class AIBehaviour : MonoBehaviour
{
	
	public PlayerManager manager;
	private float _rayAngle;
	private float _moveTimer;
	private int _dir;
	
	private void Start()
	{
		_rayAngle = (float)Random.Range(0, 360);
		_moveTimer = 0.0f;
		_dir = 0;
	}
	
	private void Update()
	{
		manager.StepTimers(Time.deltaTime);
		Move();	
	}
	
	private void FixedUpdate()
	{
		TargetAndFire();
	}
	
	private void TargetAndFire()
	{
		Ray ray = new Ray(transform.position, new Vector3(Mathf.Cos(_rayAngle), Mathf.Sin(_rayAngle)));
		RaycastHit hit;
		if(Physics.Raycast(ray, out hit, 25.0f))
		{
			if(hit.collider.gameObject.CompareTag("Player"))
			{
				Vector3 targetPos = hit.collider.gameObject.transform.position;
				Vector3 targetVel = hit.collider.gameObject.rigidbody.velocity;
				
				//attempt to predict next position for target
				//this could be made more accurate by taking into account the
				//distance between the ai and its target.
				//fixed since fire is called in FixedUpdate
				targetPos+=targetVel*Time.fixedDeltaTime;
				float relAng = Mathf.Floor(Vector3.Angle(transform.position, targetPos)/45)*45.0f;
				if(relAng < 0)
					relAng += 360.0f;
				Vector3 relDir = new Vector3(Mathf.Cos(Mathf.PI/180*relAng), Mathf.Sin(Mathf.PI/180*relAng), 0.0f);
				if(relDir != manager.MoveDirection)
					manager.Move(relDir, Time.deltaTime);
				manager.Fire(targetPos);
			}
		}
		_rayAngle += 6.0f;
		while(_rayAngle >= 360.0f)
			_rayAngle -= 360.0f;
	}
	
	private void Move()
	{
		_moveTimer += Time.deltaTime;
		if(_moveTimer >= Random.Range(0.25f, 1.0f))
		{
			_moveTimer = 0.0f;
			_dir = Random.Range(0,8);	
		}
		manager.Move(new Vector3(Mathf.Cos(_dir*Mathf.PI), Mathf.Sin(_dir*Mathf.PI), 0.0f), Time.deltaTime);
	}
}
