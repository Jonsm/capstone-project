using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using NAudio;
using NAudio.Wave;


public class MenuScript : MonoBehaviour {

	public GameObject explorer;
	public GameObject music;
	public Canvas quitMenu;
	public Button startText;
	public Button exitText;
	public bool songUp = false;
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
	public Transform[] cubies;
	float[] samples;

	bool isReady = false;


	// Use this for initialization
	void Start () {
		quitMenu = quitMenu.GetComponent<Canvas> ();
		quitMenu.enabled = false;
		startText = startText.GetComponent<Button> ();
		exitText = exitText.GetComponent<Button> ();
	}

	public void StartLevel(){
		new MainManager (song);
		//Application.LoadLevel (1);
	}
	
	public void ExitPress(){
		quitMenu.enabled = true;
		startText.enabled = false;
		exitText.enabled = false;
	}

	public void NoPress(){
		quitMenu.enabled = false;
		startText.enabled = true;
		exitText.enabled = true;
	}

	public void ExitGame(){
		Application.Quit ();
	}

	public void UploadSong(){
		if (men == false){
			startText.enabled = false;
			exitText.enabled = false;
			mp3 = Instantiate (explorer);
			//while (n.GetComponent<Mp3FileFinder>().m_mp3Path.Equals("")) {
			//
			//}
			UnityEngine.Debug.Log(mp3.GetComponent<Mp3FileFinder>().m_mp3Path);
			up = true;
			men = true;
		}
		//Use File Browsing here then an mp3 to wav converter
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
	
		while(!a.isReadyToPlay)
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
	/*
	private void LoadAudio()
	{			
		byte[] songData;
		songData = File.ReadAllBytes(pathName);

		if (!LoadAudioFromData(songData))
		{
			UnityEngine.Debug.LogError("Could not load");
			return;
		}
			
		mWaveOutDevice.Play();
		Resources.UnloadUnusedAssets();

	}

	private void UnloadAudio()
	{
		if (mWaveOutDevice != null)
		{
			mWaveOutDevice.Stop();
		}
		if (mMainOutputStream != null)
		{
			// this one really closes the file and ACM conversion
			mVolumeStream.Close();
			mVolumeStream = null;
			
			// this one does the metering stream
			mMainOutputStream.Close();
			mMainOutputStream = null;
		}
		if (mWaveOutDevice != null)
		{
			mWaveOutDevice.Dispose();
			mWaveOutDevice = null;
		}
	}
	*/

	/*
	private bool LoadAudioFromData(byte[] data)
	{
		try
		{
			MemoryStream tmpStr = new MemoryStream(data);
			mMainOutputStream = new Mp3FileReader(tmpStr);
			mVolumeStream = new WaveChannel32(mMainOutputStream);
			
			mWaveOutDevice = new WaveOut();
			mWaveOutDevice.Init(mVolumeStream);
			
			return true;
		}
		catch (System.Exception ex)
		{
			UnityEngine.Debug.LogWarning("Error! " + ex.Message);
		}
		
		return false;
	}
*/


