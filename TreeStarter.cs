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
			tg.pEvent += MakeLeaves;
			StartCoroutine(tg.Grow ());
		}
	}

	void MakeLeaves (TreeGenerator tg) {
		Debug.Log ("done");
		tl.tg = tg;
		tl.MakeLikeATree ();
	}
}
