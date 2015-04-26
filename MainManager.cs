using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MainManager : MonoBehaviour {

	private AudioClip song;
	private AudioSource source;
	private float sampleTime; //time of each sample (for charPitches and volumes)
	private SortedDictionary <float, float> [] bandBeats; //hashes beat time to power for each band
	private float [] beatTotalPower; //total power for each band during beats, use to rate "matching"
	private float [] charPitches; //list of characteristic pitches for each sample
	private float [] volumes; //list of voumes for each sample
	private bool done = false;

	private SortedDictionary<float,float> bpm;

	private float minVol;
	private float maxVol;

	public GameObject waterTop;
	public GameObject waterBottom;

	public GameObject mainCamera;
	public GameObject marchingCube;
	public GameObject clouds;

	public GameObject CubeGenerator;
	private Color baseCubeColor;

	public GameObject trees;
	private Color baseTreeColor;

	public GameObject buildings;
	private Color baseBuildingColor;
	


	public MainManager(AudioClip a, GameObject cubeGenPre,GameObject treePre, GameObject buildingPre){
		song = a;
		CubeGenerator = cubeGenPre;
		trees = treePre;
		buildings = buildingPre;
	}
	// Use this for initialization
	void Start () {
		float [] data = new float [song.samples * song.channels];
		song.GetData (data, 0);
		SpectrumAnalyzer sa = new SpectrumAnalyzer (data, song.length);
		sa.Run ();
		while (sa.done == false) {
			Debug.Log("Song is being analyzed");
		}
		sampleTime = sa.sampleTime;
		bandBeats = sa.bandBeats;
		beatTotalPower = sa.beatTotalPower;
		charPitches = sa.charPitches;
		volumes = sa.volumes;
		//need to make initial map and set initial values
		setInitial ();

		StartCoroutine ("SetVals");

	}

	void makeTerrain(){
		CubeManager g = CubeGenerator.GetComponent<CubeManager>();

		//This needs to have an object prefab that has a marching cubes script sent in
		g.object_prefab = marchingCube;

		//The size is going to be fixed
		g.cubeSize = 20;
		//Fix this but the equilibrium needs to be barren for certain songs not others
		g.equilibrium = maxVol/minVol;
		//Set the  surface here
		g.surface = 1;

		//Set size to 10 
		g.size = 10;

		//Set if the terrain turns to cave or flat and where it happens within the range next
		//mod 1 is in x direction mod 2 is z mod 3 is x as well

		//mod1 should have the largest range of the 3
		g.mod1_max = 10f;
		g.mod1_min = .5f;
		g.mod1_change = 100;
		g.mod1_end = -100;

		//mod2 should have a medium range (smaller then 1)
		g.mod2_max = 10f;
		g.mod2_min = .5f;
		g.mod2_change = 100;
		g.mod2_end = -100;

		//mod3 should have the smallest range if used, since its extreme and insane
		g.mod3_max = 10f;
		g.mod3_min = .5f;
		g.mod3_change = 100;
		g.mod3_end = -100;
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
		FogControl f = mainCamera.GetComponent<FogControl> () as FogControl;
		//Set whether there is fog and the color and such, can update the color if there is fog
		f.fog = false;
		f.fog_color = Color.gray;
		f.fog_density = 0;
	}

	void makeTrees(){

		TreeGenerator t = trees.GetComponent<TreeGenerator> () as TreeGenerator;
		//Set each component of a tree and Instantiate here
		t.segments = new int[1];
		t.segmentLength = new float[1];
		t.radius = new float[1];
		t.upCurve = new float[1];
		t.maxTurn = new float[1];
		t.branchChance = new float[1];
		t.branchDeviation = new float[1];


		//Scale the building so that it is larger or smaller creating both big and small trees


		//Makes a new Game object and creates the tree by calling init
		//GameObject tree = Instantiate ();
		//tree.GetComponent<TreeGenerator>();
		//tree.GetComponent<TreeGenerator> ().Init ();

	}

	void makeBuildings(){
		CylBuildingMaker cbm = gameObject.GetComponent <CylBuildingMaker> () as CylBuildingMaker;

		//Set each  component of the building object then make it
		cbm.faces = new int[1];
		cbm.radius = 1;
		cbm.segments = 1;
		cbm.segmentHeight = 1;
		cbm.topChances = new float[1];
		cbm.expandChance = 1; 
		cbm.windowChance = 1;
		cbm.maxRad = new float[1];
		cbm.windowHeight = 1;
		cbm.windowInset = 1;

		//In addition rescale the buildings to be larger or smaller.
		cbm.BuildMe ();
	}

	void placeTrees(){

		float cubeSize = CubeGenerator.GetComponent<CubeManager> ().cubeSize;
		int numTrees = 0;
		/*Figure out a formula  for the number of trees that are created*/
		foreach (float f in charPitches) {
			//Call makeTrees() from here 
		}
	}

	void placeBuildings(){
		float cubeSize = CubeGenerator.GetComponent<CubeManager> ().cubeSize;


	}

	void setInitial(){
		int maxBand = 0;
		//Gets the Original Beats per minute
		foreach (SortedDictionary<float,float> band in bandBeats){
			if(band.Count > maxBand){
				bpm = band;
				maxBand = band.Count;
			}
		}
		//Gets the minimum and maximum volumes of the song
		maxVol = 0;
		minVol = 0;
		foreach (float f in volumes) {
			if( f > maxVol) 
				maxVol = f;
			else if (f < minVol){
				minVol = f;
			}
		}
		//Need to set all the max colors for each of the materials here
	}
	

	IEnumerator SetVals(){
		int every = 1;
		source.clip = song;
		source.Play ();
		float zeroTime = Time.time;
		float num = 0;

		/*Controls the terrain change */
		foreach (float f in volumes) {
			while (Time.time - zeroTime < f) yield return new WaitForSeconds (.01f);
			Debug.Log (f);
			if (num % every ==0) {
				StartCoroutine (LandColor(f));
				StartCoroutine(TreeColor (f));
				StartCoroutine(BuildingColor(f));
			}
			num++;
		}
		yield return null;
			yield return new WaitForSeconds(sampleTime);
			Debug.Log("Sending");
	}

	/* These methods are used to change the colors based on the volume*/

	Color basicChange(float f,Color c){
		Color color = c;
		float incr = 1;
		float h = 1;
		float s = 1;
		float v = 1;
		//HSV colors allow you to keep the brightness the same while changing the color
		ColorToHSV (color, out h, out s, out v);
		incr = 1 - ((maxVol-f) / (maxVol-minVol));
		s = incr;
		color = ColorFromHSV(h,s,v,1);
		return color;
	}

	Color rainbowChange(float f, Color c){
		Color color = c;
		float h = 1;
		float s = 1;
		float v = 1;
		//HSV colors allow you to keep the brightness the same while changing the color
		ColorToHSV (color,out h, out s, out v);
		h = UnityEngine.Random.Range (0, 1);
		color = ColorFromHSV (h, s, v,1);
		return color;
	}

	IEnumerator LandColor(float f){
		/*This changes the land color on the beat based on the beat strength at this time*/
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






	public static Color ColorFromHSV(float h, float s, float v, float a = 1)
	{
		// no saturation, we can return the value across the board (grayscale)
		if (s == 0)
			return new Color(v, v, v, a);
		
		// which chunk of the rainbow are we in?
		float sector = h / 60;
		
		// split across the decimal (ie 3.87 into 3 and 0.87)
		int i = (int)sector;
		float f = sector - i;
		
		float p = v * (1 - s);
		float q = v * (1 - s * f);
		float t = v * (1 - s * (1 - f));
		
		// build our rgb color
		Color color = new Color(0, 0, 0, a);
		
		switch(i)
		{
		case 0:
			color.r = v;
			color.g = t;
			color.b = p;
			break;
			
		case 1:
			color.r = q;
			color.g = v;
			color.b = p;
			break;
			
		case 2:
			color.r  = p;
			color.g  = v;
			color.b  = t;
			break;
			
		case 3:
			color.r  = p;
			color.g  = q;
			color.b  = v;
			break;
			
		case 4:
			color.r  = t;
			color.g  = p;
			color.b  = v;
			break;
			
		default:
			color.r  = v;
			color.g  = p;
			color.b  = q;
			break;
		}
		
		return color;
	}
	
	public static void ColorToHSV(Color color, out float h, out float s, out float v)
	{
		float min = Mathf.Min(Mathf.Min(color.r, color.g), color.b);
		float max = Mathf.Max(Mathf.Max(color.r, color.g), color.b);
		float delta = max - min;
		
		// value is our max color
		v = max;
		
		// saturation is percent of max
		if (!Mathf.Approximately(max, 0))
			s = delta / max;
		else
		{
			// all colors are zero, no saturation and hue is undefined
			s = 0;
			h = -1;
			return;
		}
		
		// grayscale image if min and max are the same
		if (Mathf.Approximately(min, max))
		{
			v = max;
			s = 0;
			h = -1;
			return;
		}
		
		// hue depends which color is max (this creates a rainbow effect)
		if (color.r == max)
			h = (color.g - color.b) / delta;         	// between yellow & magenta
		else if (color.g == max)
			h = 2 + (color.b - color.r) / delta; 		// between cyan & yellow
		else
			h = 4 + (color.r - color.g) / delta; 		// between magenta & cyan
		
		// turn hue into 0-360 degrees
		h *= 60;
		if (h < 0 )
			h += 360;
	}
}
