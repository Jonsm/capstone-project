using UnityEngine;
using System.Collections;

public class ShaderManager : MonoBehaviour {
	private SongGenre.Genre genre;
	private SpectrumAnalyzer sa;
	private GameObject rain;
	private FogControl fog;

	private bool begun = false;
	private Color MainSkyColor;
	private Color SecondSkyColor;
	private Color MainGroundColor;
	private int hsvRange; //range in degrees around which the color varies

	private float [] pitch;
	private float [] volume;
	private float averagePitch;
	private float averageVolume;

	public void Begin (SongGenre.Genre g, SpectrumAnalyzer saa, 
	                   GameObject rain, FogControl fg) {
		//Initialize constants
		genre = g;
		sa = saa;
		this.rain = rain;
		fog = fg;
		begun = true;

		GetRanges ();
		GetMainColors ();
	}

	void Update () {
	
	}

	//gets average and min max pitch and volume
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
			sumPitch += sa.charPitches [i];
		}

		volume = new float [] {minVolume, maxVolume};
		pitch = new float [] {minPitch, maxPitch};
		averagePitch = sumPitch / sa.charPitches.Length;
		averageVolume = sumVolume / sa.volumes.Length;
		Debug.Log ("AVG VOLUME " + averageVolume);
		Debug.Log ("AVG PITCH " + averagePitch);
	}

	void GetMainColors () {

	}
}
