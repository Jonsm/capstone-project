using UnityEngine;
using System.Collections;

public class TwinPlanetSystem : MonoBehaviour {

	public bool twin_planet;
	public bool planet_moon;
	public bool mini_moons;

	public float planet_scale;
	public float distance;
	
	private GameObject planet1;
	private GameObject planet2;
	private GameObject moon;
	private GameObject mini_moon;

	// Use this for initialization
	void Start () {
		planet1 = GameObject.CreatePrimitive (PrimitiveType.Sphere);
		float x = planet1.transform.localScale.x;
		float y = planet1.transform.localScale.y;
		float z = planet1.transform.localScale.z;
		planet1.transform.position = new Vector3 (distance,distance,distance);
		planet1.transform.localScale = new Vector3 (x*planet_scale,y*planet_scale,z*planet_scale);
		if (twin_planet) {
				planet2 = GameObject.CreatePrimitive (PrimitiveType.Sphere);
				planet2.transform.localScale = new Vector3 (x * planet_scale, y * planet_scale, z * planet_scale);
		} else if (planet_moon && mini_moons) {
			moon = GameObject.CreatePrimitive (PrimitiveType.Sphere);
			moon.transform.localScale = new Vector3 (x * planet_scale*.33f, y * planet_scale*.33f, z * planet_scale*.33f);
			moon.transform.position = new Vector3(distance + 3* planet_scale/2,distance, distance);
			mini_moon = GameObject.CreatePrimitive (PrimitiveType.Sphere);
			mini_moon.transform.localScale = new Vector3 (x * planet_scale*.33f*.33f, y * planet_scale*.33f*.33f, z * planet_scale*.33f *.33f);
			mini_moon.transform.position = new Vector3((distance + 3* planet_scale/2 + (3*planet_scale*.33f)/2),distance, distance);

		}else if(planet_moon){
			moon = GameObject.CreatePrimitive (PrimitiveType.Sphere);
			moon.transform.localScale = new Vector3 (x * planet_scale*.33f, y * planet_scale*.33f, z * planet_scale*.33f);
			moon.transform.position = new Vector3(distance + 3* planet_scale/2,distance, distance);
		}
	}
	
	// Update is called once per frame
	void Update () {
	if (planet_moon && mini_moons) {
			moon.transform.RotateAround(planet1.transform.position, Vector3.up, 20 * Time.deltaTime);
			mini_moon.transform.RotateAround(moon.transform.position, Vector3.left, 60 * Time.deltaTime);
		}else if (planet_moon){
			moon.transform.RotateAround(planet1.transform.position, Vector3.up, 20 * Time.deltaTime);
		}
	}
}
