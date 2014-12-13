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

	private AudioClip mostCommonNote;

	void Start () {
		source.clip = song;
		source.Play ();

		if (playNote) {
			StartCoroutine (SetNote (mostCommonNote, song, source));
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

	/* plays the loudest frequency in the song */
	IEnumerator SetNote (AudioClip output, AudioClip input, AudioSource source) {
		//Initializing songs
		float [] samples = new float [song.samples * song.channels];
		input.GetData (samples, 0);
		output = Instantiate (song) as AudioClip;
		float FFTsampleRate = AudioSettings.outputSampleRate;
		float sampleRate = song.frequency;

		//Initializing fast fourier transform
		double [] samples_RE = new double [samples.Length / fudgeFactor];
		double [] samples_IM = new double [samples.Length / fudgeFactor];
		int size = MakeFF (fudgeFactor, samples, samples_RE, samples_IM);

		//Finding max bin
		float max_pow = 0;
		float max = 0;
		for (int i = 0; i < size / 2; i++) {
			float pow = Mathf.Pow ((float) samples_RE [i], 2) + Mathf.Pow ((float) samples_IM [i], 2);
			if (pow > max_pow) {
				max_pow = pow;
				max = i;
			}
		}
		//Convert to frequency in Hz
		max = max / (input.length * (float) size / (float) samples_RE.Length);
		Debug.Log (max);

		//Create waveform
		for (int i = 0; i < samples.Length; i++) 
			samples [i] = noteVolume * (.5f + .5f * Mathf.Sin (2 * Mathf.PI * max * i / sampleRate));
		output.SetData (samples, 0);

		source.Stop ();
		source.clip = output;
		source.Play ();

		Debug.Log ("done");
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
}
