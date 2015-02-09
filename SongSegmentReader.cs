using UnityEngine;
using System.Collections;

public class Segment {
	public float time;
	public float charPitch;
	public float power;
	public float bpm;
	public float [] playableLPF;
	public double [] samples_RE;
	public double [] samples_IM;

	private FFTM fftm;
	private float charMin;
	private float charMax;
	private float bassRangeCutoff;
	private int factor;

	public Segment (int start, int end, float [] samples, float time_in, int factor_in, float cmin, 
	                float cmax, float brc) {
		charPitch = power = bpm = 0;
		factor = factor_in;
		time = time_in;
		charMin = cmin;
		charMax = cmax;
		bassRangeCutoff = brc;
		fftm = ScriptableObject.CreateInstance (typeof(FFTM)) as FFTM;

		samples_RE = new double [(end - start) / factor];
		samples_IM = new double [samples_RE.Length];
		for (int i = 0; i < samples_RE.Length; i++) {
			power += Mathf.Pow (samples [i * 2], 2);
			samples_RE [i] = (double) samples [start + i * factor];
		}
		power /= samples_RE.Length;
	}

	public void MakeFF () {
		fftm.Init ((uint) Mathf.Log (samples_RE.Length, 2));
		fftm.Run (samples_RE, samples_IM);
	}

	public void InverseFF () {
		fftm.Run (samples_RE, samples_IM, true);
	}

	//is FFT running (prevents race condition)
	public bool IsWorking () {
		return fftm.working;
	}

	//find characteristic pitch
	public void GetCharPitch () {
		int minCutoff = (int) (charMin * time);
		int maxCutoff = (int) (charMax * time);
		int max = minCutoff;
		float maxPow = (float) samples_RE [max];
		for (int i = minCutoff; i < maxCutoff; i++) {
			float pow = Power (i);
			if (pow > maxPow) {
				maxPow = pow;
				max = i;
			}
		}

		charPitch = (float) max / time;
	}
	
	public void LowPassFilter () {
		//remove all frequencies above bass range cutoff
		int lowPassCutOff = (int) (bassRangeCutoff * time);
		for (int i = lowPassCutOff; i < samples_RE.Length / 2; i++) {
			samples_RE [i] = 0;
			samples_IM [i] = 0;
			samples_RE [samples_RE.Length - i - 1] = 0;
			samples_IM [samples_RE.Length - i - 1] = 0;
		}

		InverseFF ();
	}

	public void MakePlayableLPF () {
		playableLPF = new float[samples_RE.Length * factor];
		SongSegmentReader.SmoothArray (samples_RE, playableLPF, factor);
	}

	private float Power (int i) {
		return Mathf.Pow ((float) samples_RE [i], 2) + Mathf.Pow ((float) samples_IM [i], 2);
	}
}

public class SongSegmentReader : MonoBehaviour {
	public AudioClip song;
	public AudioSource source;
	public int fudgeFactor; //use 1/fudgeFactor samples in FFT; must be power of 2
	public float segmentTime; //length of each segment in seconds
	public float bassRangeCutoff; //cutoff for low pass filter, in hertz
	public float charMin; //minimum frequency for characteristic pitch
	public float charMax; //max frequency for characteristic pitch
	public Segment [] segments;
	
	private int revisedSegmentLength; //power of 2

	void Start () {
		source.clip = song;
		source.Play ();

		//calculate length of each segment
		float time = song.length;
		int sampleSize = song.samples * song.channels;
		int segmentLength = (segmentTime < time) ? (int)(segmentTime * sampleSize / time) : sampleSize;
		revisedSegmentLength = (int) Mathf.Pow (2, Mathf.Round (Mathf.Log (segmentLength, 2)));
		if (revisedSegmentLength > sampleSize) revisedSegmentLength /= 2;
		float revisedSegmentTime = ((float)revisedSegmentLength / sampleSize) * song.length;

		//initialize segments
		float[] samples = new float [song.samples * song.channels];
		song.GetData (samples, 0);		
		segments = new Segment [sampleSize / revisedSegmentLength];
		for (int i = 0; i < segments.Length; i++) {
			int start = i * revisedSegmentLength, end = (i + 1) * revisedSegmentLength;
			segments [i] = new Segment (start, end, samples, revisedSegmentTime, fudgeFactor, 
			                            charMin, charMax, bassRangeCutoff);
		}

		foreach (Segment s in segments) s.MakeFF ();
		StartCoroutine ("SegmentAnalytics");
	}

	//Get information on each segment
	IEnumerator SegmentAnalytics () {
		foreach (Segment s in segments) {
			while (s.IsWorking ()) yield return new WaitForSeconds (.01f);
			s.GetCharPitch ();
			s.LowPassFilter ();
		}

		foreach (Segment s in segments) {
			while (s.IsWorking ()) yield return new WaitForSeconds (.01f);
			s.MakePlayableLPF ();
		}

		Debug.Log ("done");
		yield return null;
	}

	//for debug: plays the song with the low pass filter applied
	private void PlayLowPass () {
		float [] newSong = new float [song.samples * song.channels]; 
		int total = 0;
		for (int i = 0; i < segments.Length; i++) {
			for (int j = 0; j < segments [i].playableLPF.Length; j++) {
				newSong [total + j] = segments [i].playableLPF [j]; 
			}
			total += segments [i].playableLPF.Length;
		}
		source.Stop ();
		song.SetData (newSong, 0);
		source.Play ();
	}

	//Turns a smaller array into a larger one, interpolating all new points
	public static void SmoothArray (double [] small, float [] large, int factor) {
		for (int i = 0; i < small.Length - 1; i++) {
			float v_0 = (float) small [i]; //first value to interpolate
			float v_1 = (float) small [i + 1]; //second value
			int start_point = i * factor;
			int end_point = (i + 1) * factor - 1;
			
			for (int j = start_point; j < end_point; j++) {
				large [j] = Mathf.Lerp (v_0, v_1, (float) j / ((float) end_point - (float) start_point));
			}
		}
	}
}
