using UnityEngine;
using System.Collections;

public class GUIManager : MonoBehaviour
{
	public Texture titleTexture;
	
	private void Start()
	{
	}
	
	private void OnGUI()
	{
		if(GameManager.Instance.State.title)
			DisplayTitle();
	}
	
	private void DisplayTitle()
	{
		float tw = titleTexture.width;
		float th = titleTexture.height;
		float ratio = 1.0f;
		if(tw > Screen.width)
			ratio = Screen.width / tw;
		if(Screen.height / th < ratio)
			ratio = Screen.height / th;
		tw*=ratio;
		th*=ratio;
		float x = (Screen.width - tw) / 2.0f;
		float y = (Screen.height - th) / 2.0f;
		Rect pos = new Rect(x, y, tw, th);
		GUI.DrawTexture(pos, titleTexture);
	}
	
	private void ShowAllPlayerInfo()
	{
		/*if(GameManager.Instance.Players.Length > 0)
		{
			for(int i=0; i<GameManager.Instance.Players.Length; ++i)
			{
				PlayerManager p = GameManager.Instance.Players[i].GetComponent<PlayerBehaviour>().manager;
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
		}*/
	}
}
