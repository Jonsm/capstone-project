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
	private int leaf_density;
	private int treeSize = 1;

	public static int buildingSize= 1;
	public static int buildingHeight;
	public static float expandChance;
	public static float windows;
	public static bool dome;
	public static float domeRat;
	public static int numChild;
	public static float[] childRadius;
	public static float[] childHeight;

	public static int[] segments;
	public static float[] segmentLength;
	public static float[] upCurve;
	public static float[] maxTurn;
	public static float[] branchChance;
	public static float[] branchDeviation;
	public static int[] leafDensity;



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
			if (big) t.segments =segments;
			else t.segments = new int[] {10,2};
			t.segmentLength = new float[]{trunk*segmentLength[0], 0};
			t.upCurve = upCurve;
			t.maxLeafPercent = .50f;
			t.maxTurn = maxTurn;
			t.branchChance = branchChance;
			t.branchDeviation = branchDeviation;
			l.leafAngleRange = new float[]{40f,90f};
			l.leafDensity = leafDensity;
			l.leafLength = new float[]{trunk / 2, trunk};
			l.leafWidth = new float[]{trunk / 2, trunk};
			l.leaves = leaf;
			treeLoc.transform.position = new Vector3 (hit.point.x,hit.point.y,hit.point.z);
		
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
			float r  = (float)Random.Range(cubeSize*buildingSize/4,cubeSize*buildingSize/2);
			float h =(float)Random.Range(cubeSize*buildingHeight/4,cubeSize*buildingHeight/2);
			int height = Mathf.RoundToInt(h);
			int rad = Mathf.RoundToInt(r);
			cbm.radius = rad;
			cbm.segments = height;
			cbm.segmentHeight = height;
			int max = Mathf.RoundToInt(r/5);
			cbm.maxRad = new float[] {rad + max, rad+ max*1.5f};
			cbm.expandChance = expandChance;
			cbm.windowHeight = windows;
			cbm.numChildren = 0;
			cbm.childRadiusFactor = childRadius;
			cbm.childSegmentFactor = childHeight;

			buildingA.transform.position = new Vector3 (hit.point.x,hit.point.y-10,hit.point.z);
			cbm.BuildMe();
			buildingA.transform.parent = BuildingContainer.transform;
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

			if(noises[new Vector2(x, z)] < .25f && noises[new Vector2(x, z)] >.20){
				makeTrees(v,true);
			}else if (noises[new Vector2(x, z)] < .26){
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
			
			if(noises[new Vector2(x, z)] <.48f &&  noises[new Vector2(x, z)] >.43f){
				makeBuildings(v);
				yield return new WaitForSeconds(.01f);
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
