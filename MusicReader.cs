using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class MusicReader : MonoBehaviour {
	public float noteVolume; //how load to play characteristic frequency, range (0, 1)
	public int fudgeFactor; //how much to reduce audio resolution for FFT
	public bool playNote; //whether to play characteristic frequency
	public bool playBeat; //whether to play beat patterns
	public bool debug;

	public AudioClip song;
	public AudioSource source;
	public double percent;

	private float [] samples;
	private double [] samples_RE;
	private double [] samples_IM;
	private int sampleSize;


	private bool wait = false;

	void Start () {
		source.clip = song;
		source.Play ();

		StartCoroutine(SongMakeFF ());
		if (playBeat) {
			StartCoroutine(LowPassFilter(song, source));
		} else if (playNote) {
			StartCoroutine (SetNote (song, source));
		} else if (debug) {
			StartCoroutine (FFTDebug());
		}
	}

	/* get rid of this later -- for debuggin makeFF */
	IEnumerator FFTDebug () {
		int len = 512;
		int freq = 21;
		float [] samples = new float [len];
		for (int i = 0; i < samples.Length; i++) {
			samples [i] = Mathf.Sin ((float) freq * 2f * Mathf.PI * (float) i / (float) len);
		}
		double [] samples_RE = new double [samples.Length];
		double [] samples_IM = new double [samples.Length];
		MakeFF (1, samples, samples_RE, samples_IM);
	
		float maxPow = 0;
		int max = 0;
		for (int i = 1; i < samples_RE.Length; i++) {
			float pow = Mathf.Pow ((float) samples_RE [i], 2) + Mathf.Pow ((float) samples_IM [i], 2);
			if (pow > maxPow) {
				maxPow = pow;
				max = i;
			}
		}
		Debug.Log (max);

		yield return null;
	}

	IEnumerator LowPassFilter (AudioClip input, AudioSource source) {
		while (!wait) yield return new WaitForSeconds(.01f);
		AudioClip output = Instantiate (song) as AudioClip;
		//calculates total power for the song
		float total_pow = 0;
		float power_remove = 0;
		double [] copy_RE = new double [samples_RE.Length];
		double [] copy_IM = new double [samples_IM.Length];
		Debug.Log ("HERE 1");

		for (int i = 0; i < sampleSize / 2; i++) {
			copy_RE [i] = samples_RE [i];
			copy_IM [i] = samples_IM [i];
			copy_RE [sampleSize - i - 1] = samples_RE [sampleSize - i - 1];
			copy_IM [sampleSize - i - 1] = samples_IM [sampleSize - i - 1];
			total_pow += Mathf.Pow ((float) samples_RE [i], 2) + Mathf.Pow ((float) samples_IM [i], 2);
		}
		Debug.Log ("HERE 2");

		int j = sampleSize/2 - 1;
		while(power_remove <= percent * total_pow){
			power_remove += Mathf.Pow ((float) copy_RE [j], 2) + Mathf.Pow ((float) copy_IM [j], 2);
			copy_RE [j] = 0;
			copy_IM [j] = 0;
			copy_RE [sampleSize - j] = 0;
			copy_IM [sampleSize - j] = 0;
			j--;
		}
		Debug.Log ("HERE 3");

		FFT fft = ScriptableObject.CreateInstance (typeof(FFT)) as FFT;
		fft.init ((uint) Mathf.Log (copy_RE.Length, 2));
		fft.run (copy_RE, copy_IM, true);
		//float [] new_samples = new float [song.samples * song.channels];

		smoothArray (copy_RE, samples, fudgeFactor);
		Debug.Log ("HERE 4");

		output.SetData (samples, 0);

		source.Stop ();
		source.clip = output;
		source.Play ();

		yield return null;
	}


	/* plays the loudest frequency in the song */
	IEnumerator SetNote (AudioClip input, AudioSource source) {
		while (!wait) yield return new WaitForSeconds(.01f);
		AudioClip output = Instantiate (song) as AudioClip;
		float sampleRate = input.frequency;

		//Finding max bin
		float max_pow = 0;
		float max = 0;
		for (int i = 0; i < sampleSize / 2; i++) {
			float pow = Mathf.Pow ((float) samples_RE [i], 2) + Mathf.Pow ((float) samples_IM [i], 2);
			if (pow > max_pow) {
				max_pow = pow;
				max = i;
			}
		}
		//Convert to frequency in Hz
		max = max / (input.length * (float) sampleSize / (float) samples_RE.Length);
		Debug.Log ("MAX " + max);

		//Create waveform
		for (int i = 0; i < samples.Length; i++) 
			samples [i] = noteVolume * (.5f + .5f * Mathf.Sin (2 * Mathf.PI * max * i / sampleRate));
		output.SetData (samples, 0);

		source.Stop ();
		source.clip = output;
		source.Play ();

		yield return null;
	}

	//Generates the fourier transform samples_RE and samples_IM, and returns number of elements.
	//Number of elements in output is different than input because FFT only works on array sizes 2^n.
	int MakeFF (int factor, float [] samples, double [] samples_RE, double [] samples_IM) {
		FFT fft = ScriptableObject.CreateInstance (typeof(FFT)) as FFT;
		fft.init ((uint) Mathf.Log (samples_RE.Length, 2));
		for (int i = 0; i < samples_RE.Length; i++) samples_RE [i] = (double) samples [i * factor];
		fft.run (samples_RE, samples_IM, false);

		int size = samples.Length / factor;
		size = (int) Mathf.Log (size, 2);
		size = (int) Mathf.Pow (2, size);
		return size;
	}

	//Applies the fourier transform for the global variable song
	IEnumerator SongMakeFF (){
		//Initializing songs
		samples = new float [song.samples * song.channels];
		song.GetData (samples, 0);		
		//Initializing fast fourier transform
		samples_RE = new double [samples.Length / fudgeFactor];
		samples_IM = new double [samples.Length / fudgeFactor];
		sampleSize = MakeFF (fudgeFactor, samples, samples_RE, samples_IM);
		wait = true;

		yield return null;
	}

	//Maps the points of a smaller array to a larger one, linear- interpolating in between
	void smoothArray (double [] small, float [] large, int factor) {
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
