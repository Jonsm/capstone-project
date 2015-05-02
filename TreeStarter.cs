using UnityEngine;
using System.Collections;

public class TreeStarter : MonoBehaviour {
	// Use this for initialization
	public GameObject tree;
	TreeGenerator [] tg = new TreeGenerator [2];
	TreeLeaves [] tl = new TreeLeaves [2];
	void Start () {
		GameObject t = Instantiate (tree, Vector3.zero, Quaternion.identity) as GameObject;
		GameObject t2 = Instantiate (tree, Vector3.left * 10, Quaternion.identity) as GameObject;
		tg [0] = t.GetComponent <TreeGenerator> () as TreeGenerator;
		tl [0] = t.GetComponent <TreeLeaves> () as TreeLeaves;
		tg [1] = t2.GetComponent <TreeGenerator> () as TreeGenerator;
		tl [1] = t.GetComponent <TreeLeaves> () as TreeLeaves;
	}

	void Update () {
		if (Input.GetKeyDown ("up")) {
			for (int i = 0; i < tg.Length; i++) {
				tg [i].Init ();
				if (i == 0) tg [i].pEvent += MakeLeaves;
				StartCoroutine(tg [i].Grow ());
			}
		}
	}

	void MakeLeaves (TreeGenerator tgi) {
		Debug.Log ("done");
		tl [0].tg = tg [0];
		tl [0].MakeLikeATree ();
	}
}
