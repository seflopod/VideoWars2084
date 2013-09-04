using UnityEngine;
using System.Collections;

public class AIBehaviour : MonoBehaviour
{
	
	public AIPlayerManager manager;
	
	private void Update()
	{
		manager.StepTimers(Time.deltaTime);
		manager.Move();
	}
	
	private void FixedUpdate()
	{
		manager.Fire();
	}
	
	private void OnCollisionEnter(Collision c)
	{
		if(c.collider.gameObject.CompareTag("terrain") && Random.Range(0,4) == 1)
			manager.ForceDirectionChange();
	}
}
