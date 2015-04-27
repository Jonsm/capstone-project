using UnityEngine;
using System.Collections;

public class UnderWater : MonoBehaviour {
	public float water_level;
	private bool under;
	private Color normal_color;
	private Color water_color;
	private bool init_fog;
	private float init_density;

	// Use this for initialization
	public UnderWater(){
		normal_color = RenderSettings.fogColor;
		water_color = new Color(.22f, .65f, .77f, .5f);
		init_fog = RenderSettings.fog;
		init_density = RenderSettings.fogDensity;
	}
	public UnderWater(Color water_color,float init_density,float water){
		normal_color = RenderSettings.fogColor;
		this.water_color = water_color;
		init_fog = RenderSettings.fog;
		init_density = RenderSettings.fogDensity;
		water_level = water;
	}
		
	// Update is called once per frame
	void Update () {
		if(transform.position.y < water_level != under){
			under = transform.position.y < water_level;
			if (under) 
				setWater();
			if (!under)
				setNormal ();
		}
	}
	void setNormal(){
		RenderSettings.fogColor = normal_color;
		RenderSettings.fogDensity = init_density;
	}
	void setWater(){
		RenderSettings.fogColor = water_color;
		RenderSettings.fogDensity = 0.03f;
		RenderSettings.fog = true;
	}
}
