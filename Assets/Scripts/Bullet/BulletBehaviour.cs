using UnityEngine;
using System.Collections;

public class BulletBehaviour : MonoBehaviour {

	public BulletManager manager;
	
	private void Update()
	{
		if(rigidbody.velocity.sqrMagnitude < 25.0f)
			gameObject.SetActive(false);
	}
	
	private void OnCollisionEnter(Collision c)
	{
		PlayerBehaviour pb = c.gameObject.GetComponent<PlayerBehaviour>();
		if(pb != null)
		{
			pb.manager.TakeDamage(Mathf.FloorToInt(rigidbody.velocity.sqrMagnitude / 8), manager.ShooterId);
			gameObject.SetActive(false);
		}
	}
}
