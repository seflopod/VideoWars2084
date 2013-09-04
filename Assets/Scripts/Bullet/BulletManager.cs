using UnityEngine;
using System.Collections;

public class BulletManager : IObjectManager
{
	private int _id;
	private GameObject _bulletObj;
	
	public BulletManager(GameObject bulletObj, int id)
	{
		_bulletObj = bulletObj;
		_id = id;
	}
	
	public void Spawn(Vector3 pos, Vector3 velDir, int shooterId, Material mat)
	{
		_bulletObj.transform.position = pos;
		_bulletObj.rigidbody.velocity = velDir * 40.0f;
		_bulletObj.SetActive(true);
		_bulletObj.GetComponent<MeshRenderer>().material = mat;
		ShooterId = shooterId;
	}
	
	public void Disable()
	{
		_bulletObj.SetActive(false);
	}
	
	public void Enable()
	{
		_bulletObj.SetActive(true);
	}
	
	public int Id { get { return _id; } }
	
	public int ShooterId { get; set; }
}
