using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TreeAndBuilding : MonoBehaviour {

	public GameObject leaf_object;
	private Vector2 position = new Vector2 (0f,0f);
	private Dictionary<Vector2 , float> noises = new Dictionary<Vector2,float>();
	private int treeDensity = 0;
	private int buildingDensity = 0;
	private float bigNoise;
	private GameObject cube;
	private GameObject tree;
	private GameObject building;
	private float cubeSize;
	private bool leaf = false;
	private int treeSize = 1;
	private int buildingSize = 1;
	private float leaf_density = 0;

	//for sync
	private List <TreeGenerator> treesInProgress = new List <TreeGenerator> ();
	private List <CylBuildingMaker> buildingsInProgress = new List <CylBuildingMaker> ();
	public bool done = false;

	private static GameObject BuildingContainer;
	// Use this for initialization
	public void Begin(Vector2 posit, Dictionary<Vector2,float> noise, int trees, int buildings,GameObject cubeObj, 
	                  GameObject treeObj, GameObject buildingObj,float cubeSz, bool leaves,int treeSz, int buildingSz,
	                  float leaf_d){

		if (BuildingContainer == null)
			BuildingContainer = new GameObject ();
		position = posit;
		noises = noise;
		//float bigNoise = Mathf.PerlinNoise (posit.x, posit.y);
		treeDensity = trees;
		buildingDensity = buildings;
		cube = cubeObj;
		tree = treeObj;
		building = buildingObj;
		cubeSize = cubeSz;
		leaf = leaves;
		treeSize = treeSz;
		buildingSize = buildingSz;
		leaf_density = leaf_density;
		StartCoroutine ("placeTrees");
		StartCoroutine ("placeBuildings");


	}
	void makeTrees(Vector2 v, bool big){
		Vector3 down = transform.TransformDirection(Vector3.down);
		RaycastHit hit;

		//Debug.Log ("Here");
		if(Physics.Raycast(new Vector3((v.x)*cubeSize + (position.x-1/2)*20*cubeSize,300f,
		                               ((v.y)*cubeSize + (position.y-1/2)*20*cubeSize)),down,out hit)
		  								 && (hit.collider == cube.GetComponent<Collider>() as Collider)){
			GameObject treeLoc = Instantiate (tree);
			GameObject leaf = Instantiate(leaf_object);
			TreeGenerator t = treeLoc.GetComponent<TreeGenerator> () as TreeGenerator;
			TreeLeaves l = treeLoc.GetComponent<TreeLeaves>()  as TreeLeaves;
			treesInProgress.Add (t);
			t.pEvent += RemoveTree;
			//MeshCollider c = treeLoc.GetComponent<MeshCollider> () as MeshCollider;
			l.tg = t;
			float trunk = (float)Random.Range(cubeSize*treeSize/24,cubeSize*treeSize/12);
			t.radius = new float[]{trunk,.25f};
			if (big) t.segments = new int[] {15, 2};
			else t.segments = new int[] {10,2};
			t.segmentLength = new float[]{trunk*1.25f,0};
			t.upCurve = new float[]{30,.25f};
			t.maxLeafPercent = .50f;
			t.maxTurn = new float[]{20f,0};
			t.branchChance = new float[]{.8f,0};
			t.branchDeviation = new float[] {0,0};

			l.leafAngleRange = new float[]{40f,90f};
			l.leafDensity = new int[]{2,6};
			l.leafLength = new float[]{trunk / 2, trunk};
			l.leafWidth = new float[]{trunk / 2, trunk};
			l.leaves = leaf;
			treeLoc.transform.position = new Vector3 (hit.point.x,hit.point.y-2,hit.point.z);
		
			//treeLoc.transform.parent = cube.transform;
			t.Init();
			t.pEvent += l.MakeLikeATree;
			StartCoroutine(t.Grow());

			//treeLoc.transform.parent = TreeContainer.transform;
			treeLoc.SetActive(true);
			//c.sharedMesh = treeLoc.GetComponent<MeshFilter>().mesh;
			//treeLoc.GetComponent<CapsuleCollider>().radius = t.
			//t.Init ();
			//Debug.Log ("Enter");
		}

	}
		
	void makeBuildings(Vector2 v){
		Vector3 down = transform.TransformDirection(Vector3.down);
		RaycastHit hit;
		if(Physics.Raycast(new Vector3((v.x)*cubeSize + (position.x-1/2)*20*cubeSize,300f,
		                               ((v.y)*cubeSize + (position.y-1/2)*20*cubeSize))
		                   ,down,out hit) && (hit.collider == cube.GetComponent<Collider>() as Collider)){
			GameObject buildingA = Instantiate (building);
			CylBuildingMaker cbm =  buildingA.GetComponent <CylBuildingMaker> () as CylBuildingMaker;
			//MeshCollider c = buildingA.GetComponent<MeshCollider> () as MeshCollider;
			buildingsInProgress.Add (cbm);
			cbm.pEvent += RemoveBuilding;
			float f  = 2 * (float)Random.Range(cubeSize*buildingSize/2,cubeSize*buildingSize);
			int a = Mathf.RoundToInt(f);
			cbm.radius = a;
			cbm.segments = a;
			cbm.segmentHeight = a;
			int max = Mathf.RoundToInt(f/5);
			cbm.maxRad = new float[] {a+ max, a + max*1.5f};
			cbm.expandChance = .25f;
			cbm.windowHeight = max;
			cbm.numChildren = Random.Range (0, 3);
			cbm.childRadiusFactor = new float[] {.3f,.5f};
			cbm.childSegmentFactor = new float[] {.5f, 1.5f};
					
			cbm.BuildMe();
			buildingA.transform.parent = BuildingContainer.transform;
			buildingA.transform.position = new Vector3 (hit.point.x,hit.point.y,hit.point.z);;
			buildingA.SetActive(true);
			//c.sharedMesh = buildingA.GetComponent<MeshFilter> ().mesh;
		}
	}

	IEnumerator placeTrees(){

		//They all start at zero
		for (int i = 0; i < treeDensity*cubeSize; i ++) {
			Vector2 v = new Vector2(Random.Range(-10,10),
			                        Random.Range(-10,10));
			float x = Mathf.RoundToInt(v.x);
			float z = Mathf.RoundToInt(v.y);

			if(noises[new Vector2(x, z)] < .22f && noises[new Vector2(x, z)] >.21){
				makeTrees(v,true);
			}else if (noises[new Vector2(x, z)] < .23){
				makeTrees(v,false);
			}

		}
		yield return null;
	}

	IEnumerator placeBuildings(){
		//Keep track of other ones close by
		for (int i = 0; i < buildingDensity*cubeSize; i ++) {
			Vector2 v = new Vector2(Random.Range(-10,10),
			                        Random.Range(-10,10));
			float x = Mathf.RoundToInt(v.x);
			float z = Mathf.RoundToInt(v.y);
			
			if(noises[new Vector2(x, z)] <.45f &&  noises[new Vector2(x, z)] >.445f){
				makeBuildings(v);
				yield return new WaitForSeconds(.02f);
			}
		}
		yield return null;
	}

	// Update is called once per frame
	void Update () {
	
	}

	void RemoveTree (TreeGenerator tg) {
		treesInProgress.Remove (tg);
		if (treesInProgress.Count == 0 && buildingsInProgress.Count == 0)
			done = true;
	}

	void RemoveBuilding (CylBuildingMaker cb) {
		buildingsInProgress.Remove (cb);
		if (buildingsInProgress.Count == 0 && treesInProgress.Count == 0)
			done = true;
	}
}
