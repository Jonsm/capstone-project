using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class MusicReader : MonoBehaviour {
	public float noteVolume; //how load to play characteristic frequency, range (0, 1)
	public int fudgeFactor; //how much to reduce audio resolution for FFT
	public bool playNote; //whether to play characteristic frequency
	public bool playBeat; //whether to play beat patterns

	public AudioClip song;
	public AudioSource source;

	private AudioClip mostCommonNote;

	void Start () {
		source.clip = song;
		source.Play ();

		if (playNote) {
			StartCoroutine (SetNote (mostCommonNote, song, source));
		} else {

		}
	}

	/* plays the loudest frequency in the song */
	IEnumerator SetNote (AudioClip output, AudioClip input, AudioSource source) {
		//Initializing songs
		float [] samples = new float [song.samples * song.channels];
		input.GetData (samples, 0);
		output = Instantiate (song) as AudioClip;
		float FFTsampleRate = song.frequency / fudgeFactor;
		float sampleRate = song.frequency;

		//Initializing fast fourier transform
		FFT fft = ScriptableObject.CreateInstance (typeof(FFT)) as FFT;
		fft.init ((uint) Mathf.Log (samples.Length / fudgeFactor, 2));
		double [] samples_RE = new double [samples.Length / fudgeFactor];
		double [] samples_IM = new double [samples.Length / fudgeFactor];
		for (int i = 0; i < samples.Length; i+=fudgeFactor) samples_RE [i/4] = (double) samples [i];
		fft.run (samples_RE, samples_IM, false);

		//Finding max frequency
		float max = 0;
		for (int i = 0; i < samples_RE.Length / 2; i++) {
			float pow = Mathf.Pow ((float) samples_RE [i], 2) + Mathf.Pow ((float) samples_IM [i], 2);
			if (pow > max) max = i;
		}
		max = max * fudgeFactor / FFTsampleRate;

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
}
