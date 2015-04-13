using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Diagnostics;

public class MenuScript : MonoBehaviour {

	public GameObject explorer;
	public Canvas quitMenu;
	public Button startText;
	public Button exitText;
	public bool songUp = false;
	public AudioClip song;
	public AudioSource source;
	private string pathName = @"C:\";
	private Mp3FileFinder mp3;

	// Use this for initialization
	void Start () {
		quitMenu = quitMenu.GetComponent<Canvas> ();
		quitMenu.enabled = false;
		startText = startText.GetComponent<Button> ();
		exitText = exitText.GetComponent<Button> ();
	}

	public void StartLevel(){
		Application.LoadLevel (1);
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

	public void getPath(){

	}

	public void UploadSong(){
		startText.enabled = false;
		exitText.enabled = false;
		Instantiate (explorer);
		//Use File Browsing here then an mp3 to wav converter
	}
	
}
