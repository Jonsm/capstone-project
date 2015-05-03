#if UNITY_EDITOR_WIN
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using NAudio;
using NAudio.Wave;

public class MenuScript : MonoBehaviour {
	//Need Tree, Building, CubeManager prefabs
	public GameObject explorer;
	public Button selectSong;
	public RawImage hover;

	private bool songUp = false;
	private string pathName = "";
	private GameObject mp3;
	private bool up = false;
	private bool path = false;
	private bool done = false;
	private bool men = false;
	private IWavePlayer mWaveOutDevice;
	private WaveStream mMainOutputStream;
	private WaveChannel32 mVolumeStream;
	private AudioFileReader audioFileReader;
	private AudioSource source;
	private AudioClip song;
	public int sampleCount;
	float[] samples;

	bool isReady = false;


	// Use this for initialization
	void Start () {
		//selectSong = selectSong.GetComponent<Button> ();
		hover = hover.GetComponent<RawImage> ();
		hover.enabled = false;
		//selectSong.enabled = true;
	}
	//Calls the main Manager that starts the level
	public void StartLevel(){

		MainManager.pathName = pathName;
		MainManager.song = song;
		gameObject.SetActive(false);
		Application.LoadLevel (1);

	}

	public void onHover (){

		hover.enabled = true;
	}
	public void offHover(){
		hover.enabled = false;
	}


	public void UploadSong() {
		UnityEngine.Debug.Log ("True");
		if (men == false){
			mp3 = Instantiate (explorer);
			UnityEngine.Debug.Log(mp3.GetComponent<Mp3FileFinder>().m_mp3Path);
			up = true;
			men = true;
		}
	}

	void Update(){
		if(up == true && path == false){
			if(!(mp3.GetComponent<Mp3FileFinder>().m_mp3Path.Equals(""))){
				pathName = mp3.GetComponent<Mp3FileFinder>().m_mp3Path;
				path = true;
				UnityEngine.Debug.Log (pathName);

			}
		}
		if(path == true && done == false){
			StartCoroutine("Mus");
			done = true;
		}
		if (done == true) {
			StartLevel();
		}
	}

	IEnumerator Mus(){
		samples = new float[sampleCount];
		char[] chars = new char[3] {pathName[pathName.Length - 3], pathName[pathName.Length - 2], pathName[pathName.Length - 1]};
	
		string ext = new string(chars);
		
		if(pathName[pathName.Length - 3] == "mp3"[0])
		{
			Directory.CreateDirectory(System.IO.Path.GetTempPath() + @"\MusicalDefense");

			Mp3ToWav(pathName, System.IO.Path.GetTempPath() + @"\MusicalDefense\currentsong.wav");
			ext = "wav";
		}
		else
		{
			Directory.CreateDirectory(System.IO.Path.GetTempPath() + @"\MusicalDefense");
			File.WriteAllBytes(System.IO.Path.GetTempPath() + @"\MusicalDefense\currentsong." + ext, File.ReadAllBytes(pathName));
		}
	
		WWW www = new WWW("file://" + System.IO.Path.GetTempPath() + @"\MusicalDefense\currentsong." + ext);
		AudioClip a = www.audioClip;
	
		while(a.loadState != AudioDataLoadState.Loaded)
		{
			UnityEngine.Debug.Log("still in loop");
			yield return www; 
		}
		song = a;
		isReady = true;
	}

	public static void Mp3ToWav(string mp3File, string outputFile)
	{
		using (Mp3FileReader reader = new Mp3FileReader(mp3File))
		{
			WaveFileWriter.CreateWaveFile(outputFile, reader);
		}
	}

}
#endif