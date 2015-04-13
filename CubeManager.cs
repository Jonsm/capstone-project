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

	static int count = 0;
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
		if (size <= 0) size = 10;
		//Generator s = new ValueNoise(Random.Range (-9000, 9000), null); 
		//int [] [] r = {new int[] {-size,size}, new int[] {-min,max}, new int[] {-size,size}};
		//range = r;
		Generator s = new GradientNoise (UnityEngine.Random.Range (-9000,9000));

		a = new CubeThreader (cubeSize, s,range,surface,size);

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
		//InvokeRepeating ("CheckAround", 10.0f, .5f);

	}

	IEnumerator cube_Gen(Vector2 posit){
		while (!(bool)(a.generated[posit])) yield return new WaitForSeconds (.01f);
		GameObject cube = Instantiate(object_prefab);
		List<int> tris = new List<int> ();
		List<Vector3> vert = new List<Vector3> ();
		lock(lock_obj){
			tris = (List<int>)a.triangle[posit];
			vert = (List<Vector3>)a.vertices[posit];
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

		if (Math.Floor((double)player.x) != curr_x) {
			for (int i = -1; i < 1; i++) {
				for (int j = -1; j < 1; j++){
					if(!cubes.ContainsKey(new Vector2(curr_x+i,curr_z+j))){
						up = true;
						Vector2 p = new Vector2 ( curr_x + i,curr_z + j);
						cubes.Add (p,false);
						a.addCubes(p);	
						Debug.Log ("x");
					}
				
				}
			}
			curr_x =(int) Math.Floor((double)player.x);
		}
		if (Math.Floor ((double)player.z) != curr_z) {
			for (int i = -1; i < 1; i++) {
				for (int j = -1; j < 1; j++){
					if(!cubes.ContainsKey(new Vector2(curr_x+i,curr_z+j))){
						up = true;
						Vector2 p = new Vector2 (curr_x + i,curr_z + j);
						cubes.Add (p,false);
						a.addCubes(p);	
					}	
				}
			}
			curr_z = (int)Math.Floor((double)player.z);
		}

		if (up == true) 
			a.Run ();

		foreach (Vector2 posit in cubes.Keys) {
			if((bool)cubes[posit] == false){
				StartCoroutine(cube_Gen(posit));
			}
		}
		//Need to Check out the cubes around him and for each integer that it goes up by check
	}

	/* private void min_x(){
		for (int i = z_min -1 ; i < z_max+1; i++) {
			Vector2 p = new Vector2 (x_min-1 , i);
			if (cubes.ContainsKey(p) == false){
				cubes.Add (p, false);
				a.addCubes (p);	
			}
		}
		x_min -=1;
	}
	private void max_x(){
		for (int i = z_min -1; i < z_max +1; i++) {
			Vector2 p = new Vector2 (x_max+1 , i);
			if (cubes.ContainsKey(p) == false){
				cubes.Add (p, false);
				a.addCubes (p);	
			}
		}
		x_max += 1;
	}
	private void min_z(){
		for (int i = x_min-1; i < x_max+1; i++) {
			Vector2 p = new Vector2 (i,z_min-1);
			if (cubes.ContainsKey(p) == false){
				cubes.Add (p, false);
				a.addCubes (p);	
			}
		}
		z_min -= 1;
	}
	private void max_z(){
		for (int i = x_min-1; i < x_max+1; i++) {
			Vector2 p = new Vector2 (i,z_max+1);
			if (cubes.ContainsKey(p) == false){
				cubes.Add (p, false);
				a.addCubes (p);	
			}
		}
		z_max += 1;
	}

	// Update is called once per frame
	//MATH IS INCORRECT
	void New_Cubes () {
		//Use this to add cubes as the person nears the ledges.
		player = Camera.main.transform.position;
		if (Math.Abs((x_min * cubeSize*size) - player.x ) < (20)) {
			min_x();
		}
		if ((x_max * cubeSize*size) - player.x < (20)) {
			max_x();
		}
		if (Math.Abs ((z_min * cubeSize*size) - player.z) < 20) {
			min_z();
		}
		if ((z_max * cubeSize*size) - player.z < 20) {
			max_z ();
		}
		a.Run ();
		foreach (Vector2 posit in a.generated.Keys) {
			if (a.generated.ContainsKey(posit) && !(bool)(cubes[posit])){
				Debug.Log("new cube");
				StartCoroutine(cube_Gen(posit));
			}
		}

	}
	*/
}
