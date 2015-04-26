using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using CoherentNoise;
using CoherentNoise.Generation;
using CoherentNoise.Generation.Fractal;
using CoherentNoise.Generation.Combination;
using CoherentNoise.Generation.Voronoi;

public class CubeManager : MonoBehaviour {
	
	public float cubeSize = .5f; //size of each cube
	public float surface = 1; //cutoff value of isosurface
	public int max = 15;
	public int min = -5;
	public float mod1_min = 1;
	public float mod1_max = 1;
	public float mod1_change = 0;
	public float mod1_end = 0;
	public float mod2_min = 1;
	public float mod2_max = 1;
	public float mod2_change = 0;
	public float mod2_end = 0;
	public float mod3_min = 1;
	public float mod3_max = 1;
	public float mod3_change = 0;
	public float mod3_end = 0;
	public float equilibrium;
	static int count = 0;
	public List<GameObject> cubeList;
	public int y_min = -5;
	public int y_max = 15;
	private int [][] range = {new int[] {-10,10}, new int[] {-5,15}, new int[] {-10,10}};
	private int cube_count = 0;
	private Vector3 player = new Vector3 (0f, 0f, 0f);
	private int curr_x = 0;
	private int curr_z = 0;
	private CubeThreader a;
	public int size;
	public bool smoothShade;
	public GameObject object_prefab;
	private Hashtable cubes = new Hashtable();
	private System.Object lock_obj = new System.Object();
	// Use this for initialization
	void Start () {
		range [1] = new int[]{y_min, y_max};
		if (size <= 0) size = 10;
		//Generator s = new ValueNoise(Random.Range (-9000, 9000), null); 
		//int [] [] r = {new int[] {-size,size}, new int[] {-min,max}, new int[] {-size,size}};
		//range = r;
		Generator s = new GradientNoise (UnityEngine.Random.Range (-9000,9000));

		a = new CubeThreader (cubeSize, s,range,surface,size,
		                      mod1_min,mod1_max,mod1_change,mod1_end,
		                      mod2_min,mod2_max, mod2_change,mod2_end,
		                      mod3_min,mod3_max, mod3_change,mod3_end,equilibrium);

		//Creates numCubes new marching cubes and adds them to the list
		for (int i = -4; i < 5; i++) {
			for (int j = -4; j < 5; j++){
				//Each cube should have a range of 30x30, with the cubes centered appropriately based on cubeNumber
				//Height limits can be changed but for now are constant
				//Range is not shifting properly when multiplied by i and j, it doent change inside range
				//This enures a square of cubes centered around the origin.
				Vector2 p = new Vector2 ( i,j);
				cubes.Add (p,false);
				a.addCubes(p);		
			}
		}
		a.Run();
		int count = 0;
		foreach (Vector2 posit in cubes.Keys) {
			if((bool)cubes[posit] == false){
				StartCoroutine(cube_Gen(posit));
			}
		}

		//this.GetComponent<MeshRenderer> ().material.SetColor ("_Color", Color.red);
		StartCoroutine(wait ());

		//InvokeRepeating ("CheckAround", 10.0f, .5f);

	}
	IEnumerator wait(){
		while (cubeList.Count < 81)
				yield return new WaitForSeconds (5.0f);

		for (float i = 0.0f; i < 1; i += .01f) {
			Color color = new Color(0,i,i,1.0f);
			gameObject.GetComponent<MeshRenderer> ().sharedMaterial.SetColor ("_Color", color);
			yield return new WaitForSeconds (.05f);
			Debug.Log ("a");
		}
		
		yield return null;
	}

	IEnumerator cube_Gen(Vector2 posit){
		while (!(bool)(a.generated[posit])) yield return new WaitForSeconds (.01f);
		GameObject cube = Instantiate(object_prefab);
		List<int> tris = new List<int> ();
		List<Vector3> vert = new List<Vector3> ();
		lock(lock_obj){
			tris = (List<int>)a.triangle[posit];
			vert = (List<Vector3>)a.vertices[posit];
			cubeList.Add (cube);
		}
		cube.GetComponent<MarchingCubes>().Go(tris,vert);
		cubes[posit] = true;
		yield return null;
	}

	//Needs to scan around the player creating a circle around them 
	void CheckAround(){
		player = Camera.main.transform.position/(cubeSize*2*size);
		bool up = false;
		Debug.Log (player);
	}
}
