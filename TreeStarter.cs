using UnityEngine;
using System.Collections;

public class TreeStarter : MonoBehaviour {
	// Use this for initialization
	TreeGenerator tg;
	TreeLeaves tl;
	void Start () {
		tg = gameObject.GetComponent <TreeGenerator> () as TreeGenerator;
		tl = gameObject.GetComponent <TreeLeaves> () as TreeLeaves;
	}

	void Update () {
		if (Input.GetKeyDown ("up")) {
			tg.Init ();
			tg.Grow ();
			tl.tg = tg;
			tl.MakeLikeATree ();
		}
	}
}
