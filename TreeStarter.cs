using UnityEngine;
using System.Collections;

public class TreeStarter : MonoBehaviour {
	// Use this for initialization
	public GameObject tree;
	public GameObject leaves;
	TreeGenerator [] tg = new TreeGenerator [1];
	TreeLeaves [] tl = new TreeLeaves [1];
	void Start () {
		for (int i = 0; i < tg.Length; i++) {
			GameObject t = Instantiate (tree, Vector3.zero + i * 50 * Vector3.left, Quaternion.identity) as GameObject;
			GameObject l = Instantiate (leaves, Vector3.zero, Quaternion.identity) as GameObject;
			tg [i] = t.GetComponent <TreeGenerator> () as TreeGenerator;
			tl [i] = t.GetComponent <TreeLeaves> () as TreeLeaves;
			tl [i].leaves = l;
		}
	}

	void Update () {
		if (Input.GetKeyDown ("up")) {
			for (int i = 0; i < tg.Length; i++) {
				tg [i].Init ();
				tg [i].pEvent += tl [i].MakeLikeATree;
				StartCoroutine(tg [i].Grow ());
			}
		}
	}
}
