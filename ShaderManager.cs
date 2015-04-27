using UnityEngine;
using System.Collections;

public class ShaderManager : MonoBehaviour {
	public string genre;
	public SpectrumAnalyzer sa;

	private bool begun = false;

	public void Begin (string g, SpectrumAnalyzer saa) {
		genre = g;
		sa = saa;
		begun = true;
	}

	void Update () {
	
	}
}
