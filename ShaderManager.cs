using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ShaderManager : MonoBehaviour {
	Renderer renderer;
	public Material skybox;
	public Material terrain;
	public Material leaves;
	public Material building;
	//public Material trunk;

	private AudioSource source;
	private SongGenre.Genre genre;
	private SpectrumAnalyzer sa;
	private GameObject rain;
	private FogControl fog;
	
	private Color mainSkyColor;
	private Color secondSkyColor;
	private Color groundColor;
	private Color leafColor;
	private float hRange; //range in hue around which the color varies
	private float lRange; //range in lightness around which color varies, for volume
	private float bpRange; //amount to vary sine period (With volume)
	private float sinePower; //how visible sines are

	private float [] pitch;
	private float [] volume;
	private float averagePitch;
	private float averageVolume;

	private SortedDictionary <float, float> beats;
	private List <float> beatsList;
	private float beatPeriod;
	
	private float startTime;
	private int currentBeat;

	public void Begin (SongGenre.Genre g, SpectrumAnalyzer saa, 
	                   GameObject rain, FogControl fg) {
		//Initialize constants
		genre = g;
		sa = saa;
		this.rain = rain;
		fog = fg;
		startTime = Time.time;
		source = fog.gameObject.GetComponent <AudioSource> () as AudioSource;

		//get shared materials
		renderer = gameObject.GetComponent <MeshRenderer> () as Renderer;
		skybox = matchSharedMaterial (skybox);
		terrain = matchSharedMaterial (terrain);
		leaves = matchSharedMaterial (leaves);
		building = matchSharedMaterial (building);

		GetRanges ();
		GetMainColors ();
		InvokeRepeating ("Animate", 0, .05f);
		InvokeRepeating ("Beats", 0, .05f);
	}

	void Animate () {
		float time = Time.time - startTime;
		int pos = (int) (time / sa.sampleTime);
		if (pos == sa.charPitches.Length) return;
		float val = Mathf.Lerp (sa.charPitches [pos], sa.charPitches [pos + 1], 
		                        (time - pos * sa.sampleTime) / sa.sampleTime);
		float vol = Mathf.Lerp (sa.volumes [pos], sa.volumes [pos + 1],
		                        (time - pos * sa.sampleTime) / sa.sampleTime);

		Vector3 skyHSV1 = RgbHsv.RGBToHSV (mainSkyColor);

		//mod by pitch
		Vector3 skyHSV12 = new Vector3 ();
		skyHSV12 = new Vector3 (ModFloat (skyHSV1.x + Mathf.Lerp(-1 * hRange, hRange, 
                               (val - pitch [0]) / (pitch [1] - pitch [0]))),
                 				skyHSV1.y, skyHSV1.z);

		//mod by volume
		float vShift = Mathf.Lerp (-1 * lRange, lRange, (vol - volume [0]) / (volume [1] - volume [0]));
		skyHSV12.y = Mathf.Clamp (skyHSV12.y - vShift, 0, 1);
		skyHSV12.z = Mathf.Clamp (skyHSV12.z + vShift, 0, 1);

		Color newSky = RgbHsv.HSVToRGB (skyHSV12);
		skybox.SetColor ("_Color", newSky);
		terrain.SetColor ("_BeatColor", newSky);
	}

	void Beats () {
		float time = Time.time - startTime;

		if (currentBeat == beatsList.Count) return;
		int pos = (int) (time / sa.sampleTime);
		float vol = Mathf.Lerp (sa.volumes [pos], sa.volumes [pos + 1],
		                        (beatsList [currentBeat] - pos * sa.sampleTime) / sa.sampleTime);
		float bp = Mathf.Lerp (1 - bpRange, 1 + bpRange, (vol - volume [0]) / (volume [1] - volume [0]));
		terrain.SetFloat ("_SinePeriod", bp);

		if (time > beatsList [currentBeat]) {
			Debug.Log ("Beat");
			currentBeat++;
			terrain.SetFloat ("_BeatTime", startTime + beatsList [currentBeat]);

			float strength = sinePower * (vol - volume [0]) / (volume [1] - volume [0]);
			terrain.SetFloat ("_BeatStrength", Mathf.Pow (strength, 2));
		}
	}

	//gets average and min max pitch and volume and bpm
	void GetRanges () {
		float minVolume = float.MaxValue;
		float maxVolume = 0;
		float minPitch = float.MaxValue;
		float maxPitch = 0;
		float sumVolume = 0;
		float sumPitch = 0;

		for (int i = 0; i < sa.volumes.Length; i++) {
			if (minVolume > sa.volumes [i]) minVolume = sa.volumes [i];
			if (maxVolume < sa.volumes [i]) maxVolume = sa.volumes [i];
			if (minPitch > sa.charPitches [i]) minPitch = sa.charPitches [i];
			if (maxPitch < sa.charPitches [i]) maxPitch = sa.charPitches [i];

			sumVolume += sa.volumes [i];
			sumPitch += sa.charPitches [i] * sa.volumes [i];
		}

		volume = new float [] {minVolume, maxVolume};
		pitch = new float [] {minPitch, maxPitch};
		averagePitch = sumPitch / sumVolume;
		averageVolume = sumVolume / sa.volumes.Length;

		int mostBeats = 0;
		for (int i = 0; i < sa.bandBeats.Length; i++) {
			if (sa.bandBeats [i].Count > sa.bandBeats [mostBeats].Count) mostBeats = i;
		}
		beats = sa.bandBeats [mostBeats];
		beatsList = new List <float> (sa.bandBeats [mostBeats].Keys);
		beatsList.Sort ();
		beatPeriod = (beatsList [beatsList.Count - 1] - beatsList [0]) / (beatsList.Count - 1);
		terrain.SetFloat ("_BeatPeriod", beatPeriod);

//		beatMax = 0;
//		beatMin = float.MaxValue;
//		for (int i = 0; i < beatsList.Count; i++) {
//			float pow = beats [beatsList [i]];
//			if (pow > beatMax) beatMax = pow;
//			if (pow < beatMin) beatMin = pow;
//		}

		//Debug.Log ("AVG VOLUME " + averageVolume);
		//Debug.Log ("AVG PITCH " + averagePitch);
	}

	void GetMainColors () {
		//get center color
		float middlePitch = 440; //middle c
		float lowPitch = 200;
		float highPitch = 800;

		if (averagePitch < middlePitch) {
			mainSkyColor = Vector4.Lerp (Color.red, Color.yellow, 
			                             (middlePitch - averagePitch) / (middlePitch - lowPitch));
		} else {
			mainSkyColor = Vector4.Lerp (Color.yellow, Color.blue, 
			                             (averagePitch - middlePitch) / (highPitch - middlePitch));
		}

		//assign range and other colors
		Vector3 groundHSV = new Vector3 (.1f, .8f, .7f); //random range, percentage, percentage
		Vector3 skySecondaryHSV = new Vector3 (.2f, .5f, .5f);
		float skySValue = .3f;
		float leafDifference = .5f;
		hRange = .17f;
		lRange = .15f;
		bpRange = .5f;
		sinePower = 1;

		switch (genre) {
		case SongGenre.Genre.Alternative:
			hRange = .35f;
			groundHSV = new Vector3 (.2f, .5f, 1.4f);
			skySecondaryHSV = new Vector3 (.5f, .5f, .5f);
			skySValue = .6f;
			break;
		case SongGenre.Genre.Ambient:
			groundHSV = new Vector3 (.2f, .5f, .4f);
			break;
		case SongGenre.Genre.Classical:
			leafDifference = .2f;
			break;
		case SongGenre.Genre.Dubstep:
			groundHSV = new Vector3 (.2f, .5f, .4f);
			leafDifference = .2f;
			hRange = .2f;
			skySValue = .4f;
			break;
		case SongGenre.Genre.Electronic:
			groundHSV = new Vector3 (.2f, .5f, .4f);
			leafDifference = .2f;
			hRange = .35f;
			skySValue = .7f;
			break;
		case SongGenre.Genre.Folk:
			groundHSV = new Vector3 (.2f, .9f, 1.1f);
			leafDifference = .05f;
			hRange = .1f;
			break;
		case SongGenre.Genre.House:
			leafDifference = .2f;
			break;
		case SongGenre.Genre.Metal:
			leafDifference = .2f;
			groundHSV = new Vector3 (.2f, .5f, .4f);
			sinePower = .3f;
			break;
		case SongGenre.Genre.Rap:
			leafDifference = .1f;
			sinePower = .3f;
			break;
		case SongGenre.Genre.Reggae:
			groundHSV = new Vector3 (.2f, .5f, 1.4f);
			hRange = .35f;
			skySValue = .7f;
			sinePower = .5f;
			break;
		case SongGenre.Genre.Rock:
			sinePower = .5f;
			break;
		case SongGenre.Genre.Trance:
			groundHSV = new Vector3 (.2f, .5f, 1.4f);
			skySecondaryHSV = new Vector3 (.5f, .5f, .5f);
			hRange = .5f;
			skySValue = 1;
			break;
		case SongGenre.Genre.Unknown:
			break;
		}

		Vector3 mainHSV = RgbHsv.RGBToHSV (mainSkyColor);
		mainHSV.y = skySValue;
		Vector3 groundHSV2 = new Vector3 (ModFloat (mainHSV.x + Random.Range (0, groundHSV.x)),
		                                  Mathf.Clamp (mainHSV.y * groundHSV.y, 0, 1),
		                                  Mathf.Clamp (mainHSV.z * groundHSV.z, 0, 1));
		Vector3 secondaryHSV2 = new Vector3 (ModFloat (mainHSV.x + Random.Range (0, skySecondaryHSV.x)),
		                                  Mathf.Clamp (mainHSV.y * skySecondaryHSV.y, 0, 1),
		                                  Mathf.Clamp (mainHSV.z * skySecondaryHSV.z, 0, 1));
		Vector3 leafHSV = new Vector3 (ModFloat (groundHSV2.x + leafDifference), .9f, groundHSV2.z);
		secondSkyColor = RgbHsv.HSVToRGB (secondaryHSV2);
		groundColor = RgbHsv.HSVToRGB (groundHSV2);
		leafColor = RgbHsv.HSVToRGB (leafHSV);
		mainSkyColor = RgbHsv.HSVToRGB (mainHSV);

		skybox.SetColor ("_Color", mainSkyColor);
		skybox.SetColor ("_Color1", secondSkyColor);
		terrain.color = groundColor;
		leaves.SetColor ("_Color", leafColor);
		fog.fog_color = mainSkyColor;
		fog.fog_change = true;
	}

	//clamps float between 0 and 1 (for color adjustment)
	float ModFloat (float f) {
		if (f < 0) return f + 1;
		else if (f > 1) return f - 1;
		else return f;
	}

	Material matchSharedMaterial (Material m) {
		foreach (Material m1 in renderer.sharedMaterials)
			if (m1.name == m.name) return m1;

		return null;
	}
}
