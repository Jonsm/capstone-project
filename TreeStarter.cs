using UnityEngine;
using System.Collections;

public class TreeStarter : MonoBehaviour {
	// Use this for initialization
	TreeGenerator tg;
	void Start () {
		tg = gameObject.GetComponent <TreeGenerator> () as TreeGenerator;
	}

	void Update () {
		if (Input.GetKeyDown ("up")) {
			tg.Init ();
			tg.Grow ();
		}
	}
}
