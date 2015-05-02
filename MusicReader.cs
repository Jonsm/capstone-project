using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class MusicReader : MonoBehaviour {

	public AudioClip song;
	public AudioSource source;
	public GameObject indicator;
	public GameObject indicator2;
	public GameObject shaders;

	private float startTime;
	/*
	void Start () {
		startTime = Time.time;
		float [] data = new float [song.samples * song.channels];
		song.GetData (data, 0);
		SpectrumAnalyzer sa = new SpectrumAnalyzer (data, song.length);
		sa.Run ();
		indicator2.SetActive (false);
		//StartCoroutine (DebugBPF (sa));
		StartCoroutine (DebugBeats (sa));
		//StartCoroutine (DebugFreqs (sa));
		StartCoroutine (TestGenre ());
		StartCoroutine (TestVolumeTexture (sa));
	}

	//flashes indicator along with beat
	IEnumerator DebugBeats (SpectrumAnalyzer sa) {
		int every = 1;

		indicator.SetActive (false);
		while (!sa.done) yield return new WaitForSeconds (.01f);
		Debug.Log ("TIME = " + (Time.time - startTime));
	
		int max = 0;
		float maxPow = -1 * float.MaxValue;
		for (int i = 0; i < sa.beatTotalPower.Length; i++) {
			if (sa.beatTotalPower [i] > maxPow) {
				maxPow = sa.beatTotalPower [i];
				max = i;
			}
		}

		source.clip = song;
		source.Play ();
		float zeroTime = Time.time;
		float num = 0;
		foreach (float f in sa.bandBeats [max].Keys) {
			while (Time.time - zeroTime < f) yield return new WaitForSeconds (.01f);
			Debug.Log (f);
			if (num % every ==0) StartCoroutine ("FlashIndicator");
			num++;
		}
		yield return null;
	}

	IEnumerator DebugFreqs (SpectrumAnalyzer sa) {
		while (!sa.done) yield return new WaitForSeconds (.01f);
		Debug.Log (sa.charPitches.Length);

		source.clip = song;
		source.Play ();

		yield return new WaitForSeconds (sa.sampleTime / 2);
		foreach (float f in sa.volumes) {
			Debug.Log (f);
			yield return new WaitForSeconds (sa.sampleTime);
		}
		yield return null;
	}

	IEnumerator FlashIndicator () {
		indicator.SetActive (true);
		yield return new WaitForSeconds (.05f);
		indicator.SetActive (false);
	}

	IEnumerator TestGenre () {
		SongGenre sg = gameObject.GetComponent <SongGenre> () as SongGenre;
		yield return StartCoroutine(sg.Request ("Assets/Binaries/SampleSongs/Darude-Sandstorm (www.myfreemusic.cc).mp3"));
		Debug.Log (sg.genre);
		yield return null;
	}

	IEnumerator TestVolumeTexture (SpectrumAnalyzer sa) {
		SongGenre sg = gameObject.GetComponent <SongGenre> () as SongGenre;
		yield return StartCoroutine(sg.Request ("Assets/Binaries/SampleSongs/Darude-Sandstorm (www.myfreemusic.cc).mp3"));
		while (!sa.done) yield return new WaitForSeconds (.01f);
		ShaderManager sm = shaders.GetComponent <ShaderManager> () as ShaderManager;
		sm.Begin (sg.genre, sa);

	}
	*/
	/*
	 * //plays band passed clip
	IEnumerator DebugBPF (SpectrumAnalyzer sa) {
		while (!sa.done) yield return new WaitForSeconds (.01f);
		AudioClip copy = Instantiate (song) as AudioClip;
		source.Stop ();
		copy.SetData (sa.band, 0);
		source.clip = copy;
		source.Play ();
		
		yield return null;
	}*/
}
