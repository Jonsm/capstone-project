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
	public bool treeLeaves = false;
	public float equilibrium = 1;
	public bool done = false;
	public bool caves = false;
	public Hashtable perlinNoises = new Hashtable();
	public List<GameObject> cubeList;
	public int y_min = -5;
	public int y_max = 15;
	public GameObject TreeBuilding;
	public GameObject tree;
	private GameObject TreeBuilder;
	public GameObject building;
	public int treeDensity = 0;
	public int buildingDensity = 0;
	public int treeSize = 0;
	public int buildingSize = 0;

	private int [][] range = {new int[] {-10,10}, new int[] {0,25}, new int[] {-10,10}};
	private CubeThreader a;
	public int size;
	public bool smoothShade;
	public GameObject object_prefab;
	private Hashtable cubes = new Hashtable();
	// Use this for initialization
	public void Begin() {
		TreeBuilder = Instantiate(TreeBuilding);
		range [1] = new int[]{y_min, y_max};
		if (size <= 0) size = 10;
		//Generator s = new ValueNoise(Random.Range (-9000, 9000), null); 
		//int [] [] r = {new int[] {-size,size}, new int[] {-min,max}, new int[] {-size,size}};
		//range = r;
		Generator s = new GradientNoise (UnityEngine.Random.Range (-9000,9000));

		a = new CubeThreader (cubeSize, s,range,surface,size,equilibrium,max,min,caves);

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
		Debug.Log ("CubeInfo");
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
		perlinNoises = a.noises;


	}
	IEnumerator wait(){
		while (cubeList.Count < 81)
				yield return new WaitForSeconds (0.1f);
		done = true;
		yield return null;
	}

	IEnumerator cube_Gen(Vector2 posit){
		while (!(bool)(a.generated[posit])) yield return new WaitForSeconds (.01f);
		Debug.Log("A");
		GameObject cube = Instantiate(object_prefab);
		List<int> tris = new List<int> ();
		List<Vector3> vert = new List<Vector3> ();
		tris = (List<int>)a.triangle[posit];
		vert = (List<Vector3>)a.vertices[posit];
		cubeList.Add (cube);
		cube.GetComponent<MarchingCubes>().Go(tris,vert);
		cube.transform.parent = gameObject.transform;
		TreeBuilder.GetComponent<TreeAndBuilding> ().Begin (posit,(Dictionary<Vector2,float>)perlinNoises[posit], 
		                                                treeDensity,buildingDensity,cube,tree,building,cubeSize,
		                                             treeLeaves,treeSize,buildingSize);

		cubes[posit] = true;
		yield return null;
	}
}
