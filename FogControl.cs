using UnityEngine;
using System.Collections;

public class FogControl : MonoBehaviour {

	public bool fog;
	public float fog_density;
	public Color fog_color;
	public bool fog_change = false;
	// Use this for initialization
	void Start () {
		RenderSettings.fog = fog;
		RenderSettings.fogColor = fog_color;
		RenderSettings.fogDensity = fog_density;

	}
	
	// Update is called once per frame
	void Update () {
		if(fog_change == true){
			RenderSettings.fog = fog;
			RenderSettings.fogColor = fog_color;
			RenderSettings.fogDensity = fog_density;
		}
	}
}
