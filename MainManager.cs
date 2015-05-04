using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class MainManager : MonoBehaviour {
	public AudioSource source;
	public SongGenre getGenre;
	public static AudioClip song;
	public AudioClip debugSong;
	public ShaderManager shaderManager;
	public GameObject loading;
	private Text text;
	private SpectrumAnalyzer sa;
	public SongGenre.Genre genre;
	private float sampleTime; //time of each sample (for charPitches and volumes)
	private SortedDictionary <float, float> [] bandBeats; //hashes beat time to power for each band
	private float [] beatTotalPower; //total power for each band during beats, use to rate "matching"
	private float [] charPitches; //list of characteristic pitches for each sample
	private float [] volumes; //list of voumes for each sample
	private bool done = false;
	public static String pathName;

	private SortedDictionary<float,float> bpm;

	//private float hueRange;
	//private float baseHue;
	//private float minSat;
	//private float maxSat;
	private float minVal;
	private float maxVal;

	//Scale of 1 to 5 as this is multiplied later on by cube size
	private int treeMax;
	private int treeSize;
	private float leaf_density;
	private int buildingMax;
	private int buildingSize;
	private int buildingHeight;
	private float expandChance;
	private float windows;
	private bool dome;
	private float domeRat;
	private int numChild;
	private float childRadius;
	private float childHeight;

	private bool treeLeaves;
	private bool caves = false;
	private float equilibrium = 1;

	private float minVol;
	private float maxVol;



	public GameObject waterTop;
	public GameObject waterBottom;
	private float waterLevel= 200;

	public GameObject camera;
	public GameObject marchingCube;
	public GameObject clouds;
	private CloudsToy.TypePreset cloudScene = CloudsToy.TypePreset.Sunrise; 
	private Color cloudColor;
	private int numClouds;
	public GameObject CubeGenerator;
	//private Color baseCubeColor;

	private GameObject treeParent;
	public GameObject trees;
	//private Color baseTreeColor;

	private GameObject buildingParent;
	public GameObject buildings;
	
	//private Color baseBuildingColor;

	public GameObject RainObject;
	private bool rain;

	// Use this for initialization
	void Start () {
		StartCoroutine ("Spectrum");
	}

	IEnumerator Spectrum(){
		text = loading.GetComponent<Text> ();
		text.text = "Analyzing Song";
		if (song == null) song = debugSong;
		float [] data = new float [song.samples * song.channels];
		song.GetData (data, 0);
		sa = new SpectrumAnalyzer (data, song.length);
		sa.Run ();
		while(sa.done == false)
			yield return new WaitForSeconds (.05f);
		sampleTime = sa.sampleTime;
		bandBeats = sa.bandBeats;
		beatTotalPower = sa.beatTotalPower;
		charPitches = sa.charPitches;
		volumes = sa.volumes;
		yield return  StartCoroutine("findGenre");

	}

	IEnumerator Generate(){

		text.text = "Generating Terrain, Buildings and Trees";
		CubeGenerator = Instantiate (CubeGenerator);
		CubeManager g = CubeGenerator.GetComponent<CubeManager>() as CubeManager;
		g.buildingDensity = buildingMax;
		g.treeDensity = treeMax;
		g.treeLeaves = true;
		g.treeSize = treeSize;
		g.buildingSize = buildingSize;
		g.caves = caves;
		
		//This needs to have an object prefab that has a marching cubes script sent in
		g.object_prefab = marchingCube;
		//Fixed
		g.cubeSize = 20;
		//Fix this but the equilibrium needs to be barren for certain songs not others
		g.equilibrium = equilibrium;
		//Set the  surface here
		g.surface = 1;
		//Set size to 10 
		g.size = 10;
		g.leaf_density = leaf_density;
	
		g.Begin ();
		//Set if the terrain turns to cave or flat and where it happens within the range next
		//mod 1 is in x direction mod 2 is z mod 3 is x as well
		while (g.done == false)
			yield return new WaitForSeconds(.02f);

		yield return StartCoroutine("cloudInitializer");
	}

	IEnumerator cloudInitializer(){
		text.text = "Setting Clouds";
		clouds.GetComponent<CloudsToy> ().CloudPreset = cloudScene;
		clouds.GetComponent<CloudsToy> ().NumberClouds =  2* numClouds;
	
		clouds.GetComponent<CloudsToy> ().EditorRepaintClouds ();
		clouds.SetActive(true);
		yield return StartCoroutine (waterInitializer());

	}

	IEnumerator waterInitializer(){
		GameObject top = Instantiate (waterTop);
		GameObject bottom = Instantiate (waterBottom);
		Vector3 curr = top.transform.position;
		top.transform.position = new Vector3 (curr.x, waterLevel, curr.y);
		bottom.transform.position = new Vector3 (curr.x, waterLevel, curr.y);
		yield return StartCoroutine (startSong ());

	}

	IEnumerator startSong(){
		text.text = "Making it Rain";
		MeshParticleEmitter e = null;
		//rain = false;
		GameObject rainMain = null;
		if (rain == true){
			rainMain = Instantiate (RainObject);
			e = rainMain.GetComponent<MeshParticleEmitter>();
			rainMain.SetActive(true);
		}
		float avg = 0;
		int count = 0;
		foreach(float f in volumes){
			avg += f;
			count++;
		}
		avg = avg / count;
		float min = 0;
		float max = 0;
		if(rain ==	 true){
			min = e.minEmission;
			max = e.maxEmission;
		}


		GameObject cam = Instantiate (camera);
		cam.GetComponent<UnderWater> ().water_level = waterLevel;
		Vector3 down = transform.TransformDirection(Vector3.down);
		RaycastHit hit;
		Physics.Raycast(new Vector3(0,500f,0),down,out hit);
		cam.transform.position = new Vector3 (hit.point.x, hit.point.y + 5, hit.point.z);
		text.text = "Setting Fog";
		FogControl fg = cam.GetComponent<FogControl> () as FogControl;
		GameObject p = Instantiate (new GameObject ());
		source = p.AddComponent<AudioSource> ();
		source.clip = song;
		p.SetActive (true);
		source.Play ();
		shaderManager.Begin (genre, sa, rainMain, fg);
		text.text = "";
		yield return new WaitForSeconds (sampleTime / 2);

		foreach (float f in volumes) {
			if(rain){
				e.minEmission = (min + (f-avg)/50);
				e.maxEmission = (max + (f-avg)/20);
			}
			Debug.Log("Rain");
			yield return new WaitForSeconds (sampleTime);
		}	
		yield return null;
	}


	void Alternative(){
		//hueRange = 90;
		//minSat = .3f;
		//maxSat = .7f;
		minVal = .5f;
		maxVal = .75f;
		treeMax = 2;
		treeSize = 2;
		buildingMax = 2;
		buildingSize = 2;
		treeLeaves = true;
		leaf_density = .5f;
		rain = false;
		equilibrium = 2.5f;

		TreeAndBuilding.buildingHeight = 2;
		TreeAndBuilding.expandChance = .4f;
		TreeAndBuilding.windows = .5f;
		TreeAndBuilding.dome = true;
		TreeAndBuilding.domeRat = .3f;
		TreeAndBuilding.numChild = 0;
		TreeAndBuilding.childRadius = new float[]{0,0};
		TreeAndBuilding.childHeight = new float[]{0,0};

		TreeAndBuilding.segments = new int[]{10,2};
		TreeAndBuilding.segmentLength = new float[]{2,0};
		TreeAndBuilding.upCurve = new float[]{.9f,0};
		TreeAndBuilding.maxTurn = new float[]{20,0};
		TreeAndBuilding.branchChance = new float[]{.45f,0};
		TreeAndBuilding.branchDeviation = new float[]{10,0};

		TreeAndBuilding.leafDensity = new int[]{4,7};
		numClouds = 75;
		clouds.GetComponent<CloudsToy> ().CloudColor = Color.white;
	}

	void Ambient(){
		//hueRange = 30;
		//baseHue = 240;
		//minSat = TreeAndBuilding.1f;
		//maxSat = TreeAndBuilding.3f;
		minVal = .5f;
		maxVal = .8f;
		treeMax = 3;
		treeSize = 5;
		buildingMax = 1;
		buildingSize = 5;
		treeLeaves = true;
		leaf_density = .1f;
		rain = true;
		equilibrium = 1;

		buildingHeight = 2;
		TreeAndBuilding.expandChance = .2f;
		TreeAndBuilding.windows = .1f;
		TreeAndBuilding.dome = false;
		TreeAndBuilding.domeRat = 0;
		TreeAndBuilding.numChild = 6;
		TreeAndBuilding.childRadius = new float[]{.5f,.6f};
		TreeAndBuilding.childHeight = new float[] {1,1.5f};

		TreeAndBuilding.segments = new int[]{20,6};
		TreeAndBuilding.segmentLength = new float[]{3.0f,0};
		TreeAndBuilding.upCurve = new float[]{.9f,0};
		TreeAndBuilding.maxTurn = new float[]{10,0};
		TreeAndBuilding.branchChance = new float[]{.6f,0};
		TreeAndBuilding.branchDeviation = new float[]{0,0};
		TreeAndBuilding.leafDensity = new int[]{8,12};
		cloudScene = CloudsToy.TypePreset.Fantasy;
		numClouds = 50;
		clouds.GetComponent<CloudsToy> ().CloudColor = Color.white;

	}
	void Country(){
//		hueRange = 45;
//		baseHue = 60;
//		minSat = .7f;
//		maxSat = .9f;
		minVal = .8f;
		maxVal = 1;
		treeMax = 3;
		treeSize = 3;
		buildingMax = 1;
		buildingSize = 1;
		treeLeaves = true;
		leaf_density = 1;
		rain = false;
		equilibrium = 1;

		TreeAndBuilding.buildingHeight = 1;
		TreeAndBuilding.expandChance = .5f;
		TreeAndBuilding.windows = .7f;
		TreeAndBuilding.dome = true;
		TreeAndBuilding.domeRat = .5f;
		TreeAndBuilding.numChild = 0;
		TreeAndBuilding.childRadius = new float[]{0,0};
		TreeAndBuilding.childHeight = new float[]{0,0};

		TreeAndBuilding.segments = new int[]{12,3};
		TreeAndBuilding.segmentLength = new float[]{1.0f,0};
		TreeAndBuilding.upCurve = new float[]{.9f,0};
		TreeAndBuilding.maxTurn = new float[]{10,0};
		TreeAndBuilding.branchChance = new float[]{.75f,0};
		TreeAndBuilding.branchDeviation = new float[]{0,0};
		TreeAndBuilding.leafDensity = new int[]{8,10};
		clouds.GetComponent<CloudsToy> ().CloudColor = Color.white;
		numClouds = 50;
	}
	void Electronic(){
//		hueRange = 360;
//		minSat = .7f;
//		maxSat = 1;
		minVal = .8f;
		maxVal = 1;
		treeMax = 1;
		treeSize = 1;
		buildingMax = 2;
		buildingSize = 4;
		treeLeaves = false;
		leaf_density = .8f;
		rain = false;
		equilibrium = 2;

		TreeAndBuilding.buildingHeight = 3;
		TreeAndBuilding.expandChance = .2f;
		TreeAndBuilding.windows = .5f;
		TreeAndBuilding.dome = false;
		TreeAndBuilding.domeRat = 0;
		TreeAndBuilding.numChild = 4;
		TreeAndBuilding.childRadius = new float[] {.2f,.4f};
		TreeAndBuilding.childHeight = new float[]{.2f,.4f};

		TreeAndBuilding.segments = new int[]{14,4};
		TreeAndBuilding.segmentLength = new float[]{.75f,0};
		TreeAndBuilding.upCurve = new float[]{.9f,0};
		TreeAndBuilding.maxTurn = new float[]{10,0};
		TreeAndBuilding.branchChance = new float[]{.7f,0};
		TreeAndBuilding.branchDeviation = new float[]{0,0};
		TreeAndBuilding.leafDensity = new int[]{4,7};
		clouds.GetComponent<CloudsToy> ().CloudColor = Color.white;
		numClouds = 100;
	}

	void Dubstep(){
//		hueRange= 120;
//		minSat = .5f;
//		maxSat = .9f;
		minVal = .7f;
		maxVal = .9f;
		treeMax = 1;
		treeSize = 1;
		buildingMax = 4;
		buildingSize = 4;
		treeLeaves = false;
		leaf_density = .9f;
		rain = true;
		equilibrium = 2;

		TreeAndBuilding.buildingHeight = 1;
		TreeAndBuilding.expandChance = .1f;
		TreeAndBuilding.windows = .5f;
		TreeAndBuilding.dome = true;
		TreeAndBuilding.domeRat = .4f;
		TreeAndBuilding.numChild = 4;
		TreeAndBuilding.childRadius = new float[]{.6f,1};
		TreeAndBuilding.childHeight = new float[]{.2f,.3f};

		TreeAndBuilding.segments = new int[]{14,4};
		TreeAndBuilding.segmentLength = new float[]{1.25f,0};
		TreeAndBuilding.upCurve = new float[]{.8f,.3f};
		TreeAndBuilding.maxTurn = new float[]{10,0};
		TreeAndBuilding.branchChance = new float[]{.5f,0};
		TreeAndBuilding.branchDeviation = new float[]{12,0};
		TreeAndBuilding.leafDensity = new int[]{4,7};
		clouds.GetComponent<CloudsToy> ().CloudColor = Color.white;
		cloudScene = CloudsToy.TypePreset.Sunrise;
		numClouds = 100;
	}
	
	void House(){
//		hueRange= 120;
//		minSat = .7f;
//		maxSat = 1;
		minVal = .4f;
		maxVal = 1;
		treeMax = 1;
		treeSize = 5;
		buildingMax = 2;
		buildingSize = 2;
		treeLeaves = true;
		leaf_density = .5f;
		rain = false;
		equilibrium = 1.5f;

		TreeAndBuilding.buildingHeight = 1;
		TreeAndBuilding.expandChance = .2f;
		TreeAndBuilding.windows = .5f;
		TreeAndBuilding.dome = false;
		TreeAndBuilding.domeRat = 0;
		TreeAndBuilding.numChild = 0;
		TreeAndBuilding.childRadius = new float[]{0,0};
		TreeAndBuilding.childHeight = new float[]{0,0};

		TreeAndBuilding.segments = new int[]{14,4};
		TreeAndBuilding.segmentLength = new float[]{.75f,0};
		TreeAndBuilding.upCurve = new float[]{.75f,.3f};
		TreeAndBuilding.maxTurn = new float[]{10,0};
		TreeAndBuilding.branchChance = new float[]{.7f,0};
		TreeAndBuilding.branchDeviation = new float[]{0,0};
		TreeAndBuilding.leafDensity = new int[]{4,7};
		numClouds = 100;
		clouds.GetComponent<CloudsToy> ().CloudColor = Color.white;
	}
	void Metal(){
//		hueRange= 20;
//		minSat = .3f;
//		maxSat = .4f;
		minVal = 0;
		maxVal = .2f;
		treeMax = 1;
		treeSize = 3;
		buildingMax = 5;
		buildingSize = 3;
		treeLeaves = false;
		leaf_density = 0;
		rain = true;
		equilibrium = 2.5f;

		TreeAndBuilding.buildingHeight = 5;
		TreeAndBuilding.expandChance = .1f;
		TreeAndBuilding.windows = .01f;
		TreeAndBuilding.dome = false;
		TreeAndBuilding.domeRat = 0;
		TreeAndBuilding.numChild = 0;
		TreeAndBuilding.childRadius = new float[]{0,0};
		TreeAndBuilding.childHeight = new float[]{0,0};

		TreeAndBuilding.segments = new int[]{20,4};
		TreeAndBuilding.segmentLength = new float[]{2.0f,0};
		TreeAndBuilding.upCurve = new float[]{.99f,0};
		TreeAndBuilding.maxTurn = new float[]{10,0};
		TreeAndBuilding.branchChance = new float[]{.3f,0};
		TreeAndBuilding.branchDeviation = new float[]{0,0};
		TreeAndBuilding.leafDensity = new int[]{0,0};
		cloudScene = CloudsToy.TypePreset.Stormy;
		numClouds = 200;
		clouds.GetComponent<CloudsToy> ().CloudColor = Color.black;
	}
	void Rap(){
//		hueRange = 10;
//		minSat = 0;
//		maxSat = .2f;
		minVal = 0;
		maxVal = .2f;
		treeMax = 0;
		treeSize = 0;
		buildingMax = 4;
		buildingSize = 4;
		treeLeaves = false;
		caves = true;
		leaf_density = 0;
		rain = false;
		equilibrium = 1;

		TreeAndBuilding.buildingHeight = 3;
		TreeAndBuilding.expandChance = .05f;
		TreeAndBuilding.windows = .3f;
		TreeAndBuilding.dome = false;
		TreeAndBuilding.domeRat = 0;
		TreeAndBuilding.numChild = 0;
		TreeAndBuilding.childRadius = new float[]{0,0};
		TreeAndBuilding.childHeight = new float[] {0,0};

		TreeAndBuilding.segments = new int[]{20,4};
		TreeAndBuilding.segmentLength = new float[]{2.0f,0};
		TreeAndBuilding.upCurve = new float[]{.9f,0};
		TreeAndBuilding.maxTurn = new float[]{0,0};
		TreeAndBuilding.branchChance = new float[]{.3f,0};
		TreeAndBuilding.branchDeviation = new float[]{0,0}; 
		TreeAndBuilding.leafDensity = new int[]{4,7};
		clouds.GetComponent<CloudsToy> ().CloudColor = Color.white;
		numClouds =150;
	}
	void Reggae(){
//		hueRange = 10;
//		minSat = .5f;
//		maxSat = .7f;
		minVal = .7f;
		maxVal = .8f;
		treeMax = 5;
		treeSize = 3;
		buildingMax = 1;
		buildingSize = 2;
		treeLeaves = true;
		leaf_density = 1;
		rain = false;

		TreeAndBuilding.buildingHeight = 1;
		TreeAndBuilding.expandChance = .4f;
		TreeAndBuilding.windows = .1f;
		TreeAndBuilding.dome = true;
		TreeAndBuilding.domeRat = .4f;
		TreeAndBuilding.numChild = 0;
		TreeAndBuilding.childRadius = new float[]{0,0};
		TreeAndBuilding.childHeight = new float[] {0,0};

		TreeAndBuilding.segments = new int[]{20,4};
		TreeAndBuilding.segmentLength = new float[]{1.0f,0};
		TreeAndBuilding.upCurve = new float[]{.75f,0};
		TreeAndBuilding.maxTurn = new float[]{25,0};
		TreeAndBuilding.branchChance = new float[]{.8f,0};
		TreeAndBuilding.branchDeviation = new float[]{25,0};
		TreeAndBuilding.leafDensity = new int[]{8,9};
		numClouds = 25;
		clouds.GetComponent<CloudsToy> ().CloudColor = Color.white;
	}
	void Rock () {
		minVal = .7f;
		maxVal = .8f;
		treeMax = 5;
		treeSize = 3;
		buildingMax = 1;
		buildingSize = 2;
		treeLeaves = true;
		leaf_density = 1;
		rain = false;
		equilibrium = 2.5f;
		CubeThreader.rockiness = 2.5f;

		TreeAndBuilding.buildingHeight = 2;
		TreeAndBuilding.expandChance = .15f;
		TreeAndBuilding.windows = .2f;
		TreeAndBuilding.dome = true;
		TreeAndBuilding.domeRat = .2f;
		TreeAndBuilding.numChild = 0;
		TreeAndBuilding.childRadius = new float[] {0,0};
		TreeAndBuilding.childHeight = new float[] {0,0};

		TreeAndBuilding.segments = new int[]{10,3};
		TreeAndBuilding.segmentLength = new float[]{1.0f,0};
		TreeAndBuilding.upCurve = new float[]{.9f,0};
		TreeAndBuilding.maxTurn = new float[]{2,0};
		TreeAndBuilding.branchChance = new float[]{.3f,0};
		TreeAndBuilding.branchDeviation = new float[]{0,0};
		TreeAndBuilding.leafDensity = new int[]{4,7};
		clouds.GetComponent<CloudsToy> ().CloudColor = Color.white;
		numClouds = 75;
	}

	void Trance(){
//		hueRange= 90;
//		minSat = .4f;
//		maxSat = .8f;
		minVal = .4f;
		maxVal = .8f;
		treeMax = 5;
		treeSize = 2;
		buildingMax = 1;
		buildingSize = 1;
		treeLeaves = true;
		leaf_density = .7f;
		rain = true;
		equilibrium = 1.5f;

		TreeAndBuilding.buildingHeight = 1;
		TreeAndBuilding.expandChance = 0;
		TreeAndBuilding.windows = .5f;
		TreeAndBuilding.dome = false;
		TreeAndBuilding.domeRat = 0;
		TreeAndBuilding.numChild = 2;
		TreeAndBuilding.childRadius = new float[]{.1f,.2f};
		TreeAndBuilding.childHeight = new float[] {1.5f,2f};

		TreeAndBuilding.segments = new int[]{10,3};
		TreeAndBuilding.segmentLength = new float[]{1.0f,0};
		TreeAndBuilding.upCurve = new float[]{.6f,0};
		TreeAndBuilding.maxTurn = new float[]{25,0};
		TreeAndBuilding.branchChance = new float[]{.4f,0};
		TreeAndBuilding.branchDeviation = new float[]{15,0};
		TreeAndBuilding.leafDensity = new int[]{4,7};
		clouds.GetComponent<CloudsToy> ().CloudColor = Color.white;
		numClouds = 100;
	}

	IEnumerator findGenre(){
		text.text = "Analyzing Genre";
		if (pathName != null) {
			yield return StartCoroutine (getGenre.Request(pathName));
			genre = getGenre.genre;
		}


		switch (genre)
		{
			case SongGenre.Genre.Alternative:
				Alternative();
				break;
			case SongGenre.Genre.Ambient:
				Ambient();
				break;
			case SongGenre.Genre.Classical:
				Ambient();
				break;
			case SongGenre.Genre.Folk:
				Country();
				break;
			case SongGenre.Genre.Electronic:
				Electronic();
				break;
			case SongGenre.Genre.House:
				House();
				break;
			case SongGenre.Genre.Dubstep:
				Dubstep();
				break;
			case SongGenre.Genre.Rap:
				Rap();
				break;
			case SongGenre.Genre.Reggae:
				Reggae ();	
				break;
			case SongGenre.Genre.Metal:
				Metal();
				break;
			case SongGenre.Genre.Trance:
				Trance();
				break;
			case SongGenre.Genre.Rock:
				Alternative();
				break;
			case SongGenre.Genre.Unknown: 
				Rock ();
				break;
			default:
				Ambient();
				break;
		
		}	

		yield return StartCoroutine("Generate");
	}

	IEnumerator SetInitial(){
		setWater ();
		fogSet();
		yield return null;

	}
	void setWater(){
		//Set the initial water Color and the initial water fog color
		Color color = Color.blue;
		float height = 20f;
		Color color_under = Color.blue;

		GameObject water = Instantiate (waterTop);
		GameObject water_B = Instantiate (waterBottom);
		water.transform.position =  new Vector3(water.transform.position.x, height, water.transform.position.z);
		water_B.transform.position = new Vector3(water.transform.position.x, height, water.transform.position.z);
		float fog_density = .03f;
		float water_height = -10f;
		waterTop.GetComponent<MeshRenderer> ().sharedMaterial.SetColor ("_Color", color);
		UnderWater under = new UnderWater(color_under,fog_density,water_height);
	}

	void fogSet(){
		FogControl f = camera.GetComponent<FogControl> () as FogControl;
		//Set whether there is fog and the color and such, can update the color if there is fog
		f.fog = false;
		f.fog_color = Color.gray;
		f.fog_density = 0;
	}

	/*
	IEnumerator VolumeBased(){
		int every = 1;
		source.clip = song;
		source.Play ();
		float zeroTime = Time.time;
		float num = 0;


		foreach (float f in volumes) {
			StartCoroutine (LandColor(f));
			StartCoroutine(TreeColor (f));
			StartCoroutine(BuildingColor(f));
			yield return new WaitForSeconds (sampleTime);
			num++;
		}
		yield return null;
	}
	*/
	/*
	IEnumerator LandColor(float f){

		Color color = basicChange (f, baseCubeColor);
		CubeGenerator.GetComponent<MeshRenderer> ().sharedMaterial.SetColor ("_Color", color);
		yield return new WaitForSeconds (.05f);
	}
	IEnumerator TreeColor(float f){
		Color color= basicChange(f, baseTreeColor);
		trees.GetComponent<MeshRenderer> ().sharedMaterial.SetColor ("_Color", color);
		yield return new WaitForSeconds (.05f);
	}
	IEnumerator BuildingColor(float f){
		Color color = basicChange (f, baseBuildingColor);
		buildings.GetComponent<MeshRenderer> ().sharedMaterial.SetColor ("_Color", color);
		yield return new WaitForSeconds (.05f);
	}
	*/
	/* These methods are used to change the colors based on the volume*/

//	Color basicChange(float vol, float ch,Color c){
//		Color color = c;
//		float incr = 1;
//		float h = 1;
//		float s = 1;
//		float v = 1;
//		//HSV colors allow you to keep the brightness the same while changing the color
//		ColorToHSV (color, out h, out s, out v);
//		s = vol / maxVol;
//		color = ColorFromHSV(h,s,v,1);
//		return color;
//	}
//
//	Color rainbowChange(float vol,float ch, Color c){
//		Color color = c;
//		float h = 1;
//		float s = 1;
//		float v = 1;
//		//HSV colors allow you to keep the brightness the same while changing the color
//		ColorToHSV (color,out h, out s, out v);
//
//		c = ColorFromHSV (h, s, v,1);
//		return c;
	}


//	public static Color ColorFromHSV(float h, float s, float v, float a = 1)
//	{
//		// no saturation, we can return the value across the board (grayscale)
//		if (s == 0)
//			return new Color(v, v, v, a);
//		
//		// which chunk of the rainbow are we in?
//		float sector = h / 60;
//		
//		// split across the decimal (ie 3.87 into 3 and 0.87)
//		int i = (int)sector;
//		float f = sector - i;
//		
//		float p = v * (1 - s);
//		float q = v * (1 - s * f);
//		float t = v * (1 - s * (1 - f));
//		
//		// build our rgb color
//		Color color = new Color(0, 0, 0, a);
//		
//		switch(i)
//		{
//		case 0:
//			color.r = v;
//			color.g = t;
//			color.b = p;
//			break;
//			
//		case 1:
//			color.r = q;
//			color.g = v;
//			color.b = p;
//			break;
//			
//		case 2:
//			color.r  = p;
//			color.g  = v;
//			color.b  = t;
//			break;
//			
//		case 3:
//			color.r  = p;
//			color.g  = q;
//			color.b  = v;
//			break;
//			
//		case 4:
//			color.r  = t;
//			color.g  = p;
//			color.b  = v;
//			break;
//			
//		default:
//			color.r  = v;
//			color.g  = p;
//			color.b  = q;
//			break;
//		}
//		
//		return color;
//	}
//	
//	public static void ColorToHSV(Color color, out float h, out float s, out float v)
//	{
//		float min = Mathf.Min(Mathf.Min(color.r, color.g), color.b);
//		float max = Mathf.Max(Mathf.Max(color.r, color.g), color.b);
//		float delta = max - min;
//		
//		// value is our max color
//		v = max;
//		
//		// saturation is percent of max
//		if (!Mathf.Approximately(max, 0))
//			s = delta / max;
//		else
//		{
//			// all colors are zero, no saturation and hue is undefined
//			s = 0;
//			h = -1;
//			return;
//		}
//		
//		// grayscale image if min and max are the same
//		if (Mathf.Approximately(min, max))
//		{
//			v = max;
//			s = 0;
//			h = -1;
//			return;
//		}
//		
//		// hue depends which color is max (this creates a rainbow effect)
//		if (color.r == max)
//			h = (color.g - color.b) / delta;         	// between yellow & magenta
//		else if (color.g == max)
//			h = 2 + (color.b - color.r) / delta; 		// between cyan & yellow
//		else
//			h = 4 + (color.r - color.g) / delta; 		// between magenta & cyan
//		
//		// turn hue into 0-360 degrees
//		h *= 60;
//		if (h < 0 )
//			h += 360;
//	}
//}
