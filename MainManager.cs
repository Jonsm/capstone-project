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

	//private float hueRange;
	//private float baseHue;
	//private float minSat;
	//private float maxSat;
	private float minVal;
	private float maxVal;
	private float treeMax;
	private float buildingMax;
	private bool treeLeaves;

	private float minVol;
	private float maxVol;

	public GameObject waterTop;
	public GameObject waterBottom;

	public GameObject mainCamera;
	public GameObject marchingCube;
	public GameObject clouds;

	public GameObject CubeGenerator;
	//private Color baseCubeColor;

	private GameObject treeParent;
	public GameObject trees;
	//private Color baseTreeColor;

	private GameObject buildingParent;
	public GameObject buildings;
	//private Color baseBuildingColor;
	


	public MainManager(AudioClip a, GameObject cubeGenPre,GameObject treePre, GameObject buildingPre){
		song = a;
		CubeGenerator = cubeGenPre;
		trees = treePre;
		buildings = buildingPre;
	}
	// Use this for initialization
	void Start () {
		StartCoroutine ("Spectrum");
	}

	IEnumerator Spectrum(){
		float [] data = new float [song.samples * song.channels];
		song.GetData (data, 0);
		SpectrumAnalyzer sa = new SpectrumAnalyzer (data, song.length);
		sa.Run ();
		sampleTime = sa.sampleTime;
		bandBeats = sa.bandBeats;
		beatTotalPower = sa.beatTotalPower;
		charPitches = sa.charPitches;
		volumes = sa.volumes;
		while(sa.done == false)
			yield return new WaitForSeconds (.05f);

		yield return StartCoroutine("Generate");

	}

	IEnumerator Generate(){
		CubeGenerator = Instantiate (CubeGenerator);
		CubeManager g = CubeGenerator.GetComponent<CubeManager>() as CubeManager;
		
		//This needs to have an object prefab that has a marching cubes script sent in
		g.object_prefab = marchingCube;
		//Fixed
		g.cubeSize = 20;
		//Fix this but the equilibrium needs to be barren for certain songs not others
		g.equilibrium = maxVol/minVol;
		//Set the  surface here
		g.surface = 1;
		//Set size to 10 
		g.size = 10;
		//Set if the terrain turns to cave or flat and where it happens within the range next
		//mod 1 is in x direction mod 2 is z mod 3 is x as well


		while (g.done == false)
			yield return new WaitForSeconds(.05f);

		yield return  StartCoroutine("findGenre");
	}


	void Alternative(){
		//hueRange = 90;
		//minSat = .3f;
		//maxSat = .7f;
		minVal = .5f;
		maxVal = .75f;
		treeMax = 15;
		buildingMax = 20;
		treeLeaves = true;
	}

	void Ambient(){
		//hueRange = 30;
		//baseHue = 240;
		//minSat = .1f;
		//maxSat = .3f;
		minVal = .5f;
		maxVal = .8f;
		treeMax = 40;
		buildingMax = 1;
		treeLeaves = true;
	}
	void Country(){
//		hueRange = 45;
//		baseHue = 60;
//		minSat = .7f;
//		maxSat = .9f;
		minVal = .8f;
		maxVal = 1;
		treeMax = 35;
		buildingMax = 5;
		treeLeaves = true;
	}
	void Electronic(){
//		hueRange = 360;
//		minSat = .7f;
//		maxSat = 1;
		minVal = .8f;
		maxVal = 1;
		treeMax = 10;
		buildingMax = 35;
		treeLeaves = false;
	}

	void Dubstep(){
//		hueRange= 120;
//		minSat = .5f;
//		maxSat = .9f;
		minVal = .7f;
		maxVal = .9f;
		treeMax = 5;
		buildingMax = 40;
		treeLeaves = false;
	}
	
	void House(){
//		hueRange= 120;
//		minSat = .7f;
//		maxSat = 1;
		minVal = .4f;
		maxVal = 1;
		treeMax = 10;
		buildingMax = 25;
		treeLeaves = true;

	}
	void Metal(){
//		hueRange= 20;
//		minSat = .3f;
//		maxSat = .4f;
		minVal = 0;
		maxVal = .2f;
		treeMax = 5;
		buildingMax = 50;
		treeLeaves = false;
	}
	void Rap(){
//		hueRange = 10;
//		minSat = 0;
//		maxSat = .2f;
		minVal = 0;
		maxVal = .2f;
		treeMax = 0;
		buildingMax = 40;
		treeLeaves = false;
	}
	void Reggae(){
//		hueRange = 10;
//		minSat = .5f;
//		maxSat = .7f;
		minVal = .7f;
		maxVal = .8f;
		treeMax = 50;
		buildingMax = 4;
		treeLeaves = true;
	}

	void Trance(){
//		hueRange= 90;
//		minSat = .4f;
//		maxSat = .8f;
		minVal = .4f;
		maxVal = .8f;
		treeMax = 50;
		buildingMax = 0;
		treeLeaves = true;

	}

	IEnumerator findGenre(){
		string gen = "";
		switch (gen)
		{
			case "Alternative":
				Alternative();
				break;
			case "Ambient":
				Ambient();
				break;
			case "Classical":
				Ambient();
				break;
			case "Country":
				Country();
				break;
			case "Dance & EDM":
				Electronic();
				break;
			case "Deep House":
				House();
				break;
			case "Drum & Bass":
				Dubstep();
				break;
			case "Dubstep":
				Dubstep();
				break;
			case "Electronic":
				Electronic();
				break;
			case "Folk and Singer-Songwriter":
				Country ();
				break;
			case "Hip Hop & Rap":
				Rap();
				break;
			case "House":
				House();
				break;
			case "Indie":
				House ();
				break;
			case "Jazz & Blues":
				Ambient ();
				break;
			case "Latin":
				Reggae ();	
				break;
			case "Metal":
				Metal();
				break;
			case "Piano":
				Ambient();
				break;
			case "Pop":
				Electronic();
				break;
			case "R&B & Soul":
				Reggae ();
				break;
			case "Reggae":
				Reggae();
				break;
			case "Reggaeton":
				Reggae();	
				break;
			case "Rock":
				Alternative();
				break;
			case "Techno":
				Electronic();
				break;
			case "Trance":
				Trance();
				break;
			case "Trap":
				Dubstep();
				break;
			case "Trip Hop":
				Trance();
				break;
			case "World":
				Country();
				break;
			default:
				Ambient();
				break;
		
		}	
		yield return null;
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
		FogControl f = mainCamera.GetComponent<FogControl> () as FogControl;
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

	Color basicChange(float vol, float ch,Color c){
		Color color = c;
		float incr = 1;
		float h = 1;
		float s = 1;
		float v = 1;
		//HSV colors allow you to keep the brightness the same while changing the color
		ColorToHSV (color, out h, out s, out v);
		s = vol / maxVol;
		color = ColorFromHSV(h,s,v,1);
		return color;
	}

	Color rainbowChange(float vol,float ch, Color c){
		Color color = c;
		float h = 1;
		float s = 1;
		float v = 1;
		//HSV colors allow you to keep the brightness the same while changing the color
		ColorToHSV (color,out h, out s, out v);

		c = ColorFromHSV (h, s, v,1);
		return c;
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
