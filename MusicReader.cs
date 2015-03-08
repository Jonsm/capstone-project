using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class MusicReader : MonoBehaviour {
	public AudioClip song;
	public AudioSource source;
	public GameObject indicator;
	public GameObject indicator2;

	void Start () {
		float [] data = new float [song.samples * song.channels];
		song.GetData (data, 0);
		SpectrumAnalyzer sa = new SpectrumAnalyzer (data, song.length);
		sa.Run ();
		indicator2.SetActive (false);
		//StartCoroutine (DebugBPF (sa));
		StartCoroutine (DebugBeats (sa));
	}

	void Update () {
		if (Input.GetKeyDown ("space")) {
			indicator2.SetActive (true);
		}
		if (Input.GetKeyUp ("space")) {
			indicator2.SetActive (false);
		}
	}

	//flashes indicator along with beat
	IEnumerator DebugBeats (SpectrumAnalyzer sa) {
		indicator.SetActive (false);
		while (!sa.done) yield return new WaitForSeconds (.01f);
		Debug.Log (sa.bandBeats [1].Count);
		source.clip = song;
		source.Play ();
		float zeroTime = Time.time;
		foreach (float f in sa.bandBeats [0].Keys) {
			while (Time.time - zeroTime < f) yield return new WaitForSeconds (.01f);
			Debug.Log (f);
			StartCoroutine ("FlashIndicator");
		}
		yield return null;
	}

	//plays band passed clip
	IEnumerator DebugBPF (SpectrumAnalyzer sa) {
		while (!sa.done) yield return new WaitForSeconds (.01f);
		AudioClip copy = Instantiate (song) as AudioClip;
		source.Stop ();
		copy.SetData (sa.band, 0);
		source.clip = copy;
		source.Play ();

		yield return null;
	}

	IEnumerator FlashIndicator () {
		indicator.SetActive (true);
		yield return new WaitForSeconds (.05f);
		indicator.SetActive (false);
	}
}
