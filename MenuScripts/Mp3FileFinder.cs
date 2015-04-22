using UnityEngine;

public class Mp3FileFinder : MonoBehaviour {

	public string m_mp3Path ="";
	public GUISkin thisMetalGUISkin;
	protected FileBrowser m_fileBrowser;
	
	[SerializeField]
	protected Texture2D	m_directoryImage,
	m_fileImage;
	
	protected void OnGUI () {
		GUI.skin = thisMetalGUISkin;
		if (m_fileBrowser != null) {
			m_fileBrowser.OnGUI();
		} else {
			OnGUIMain();
		}
	}
	
	protected void OnGUIMain() {
		GUILayout.BeginHorizontal();
		GUILayout.Label("MP3_File", GUILayout.Width(100));
		GUILayout.FlexibleSpace();
		GUILayout.Label(m_mp3Path ?? "none selected");
		if (GUILayout.Button("...", GUILayout.ExpandWidth(false))) {
			m_fileBrowser = new FileBrowser(
				new Rect(100, 100, 600, 500),
				"Choose Song",
				FileSelectedCallback
				);
			m_fileBrowser.SelectionPattern = "*.mp3";
			m_fileBrowser.DirectoryImage = m_directoryImage;
			m_fileBrowser.FileImage = m_fileImage;
		}
		GUILayout.EndHorizontal();
	}
	
	protected void FileSelectedCallback(string path) {
		m_fileBrowser = null;
		m_mp3Path = path;
	}
}
