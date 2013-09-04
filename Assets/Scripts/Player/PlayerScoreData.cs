using UnityEngine;

public class PlayerScoreData
{
	public int kills = 0;
	public int deaths = 0;
    public int bulletsFired = 0;
    public int bulletsHit = 0;
	
	public void Reset()
	{
		kills = 0;
		deaths = 0;
		bulletsFired = 0;
		bulletsHit = 0;
	}
}
