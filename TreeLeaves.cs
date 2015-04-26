using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TreeLeaves : MonoBehaviour {
	public GameObject leafBall;
	public TreeGenerator tg;

	private int randomLocs = 3; //how many random locations to try when placing ball

	private Dictionary <Vector3, Vector3> tips;

	//go through all the branch tips, placing leaves and removing tips that have leaves from the list.
	//try out positions randomly to find the position where placing one leaf ball covers the most tips.
	public void MakeLikeATree () { //and leaf
		tips = new Dictionary <Vector3, Vector3> (tg.tips);
		List <Vector3> tipsList = new List <Vector3> (tips.Keys);

		while (tips.Count > 0) {
			Vector3 radius = Random.Range (tg.leafSizeRange [0], tg.leafSizeRange [1]) * new Vector3 (1, .5f, 1);

			Vector3 centerLoc = tipsList [Random.Range (0, tipsList.Count)];
			List <Vector3> [] coveredBranches = GetCoveredLeaves (centerLoc, radius);

			for (int i = 0; i < randomLocs - 1; i++) {
				Vector3 centerLocTmp = tipsList [Random.Range (0, tipsList.Count)];
				List <Vector3> [] coveredBranchesTmp = GetCoveredLeaves (centerLocTmp, radius);

				if (coveredBranchesTmp [0].Count > coveredBranches [0].Count) {
					centerLoc = centerLocTmp;
					coveredBranches = coveredBranchesTmp;
				}
			}

			GameObject lb = Instantiate (leafBall, centerLoc, Quaternion.identity) as GameObject;
			lb.transform.parent = gameObject.transform;
			lb.transform.localScale = 2 * radius;
			lb.transform.rotation = Quaternion.FromToRotation (Vector3.up, tips [centerLoc]);

			foreach (Vector3 v in coveredBranches [0]) tips.Remove (v);
			tipsList = coveredBranches [1];
		}
	}

	//returns list of leaves covered by a ball centered at tip at position i
	List <Vector3> [] GetCoveredLeaves (Vector3 center, Vector3 radius) {
		List <Vector3> covered = new List <Vector3> ();
		List <Vector3> alt = new List <Vector3> ();
		Quaternion rotation = Quaternion.FromToRotation (tips [center], Vector3.up);
		foreach (Vector3 v in tips.Keys) {
			if (IsInsideEllipse (rotation * (center - v), radius)) {
				covered.Add (v);
			}
			else alt.Add (v);
		}

		return new List <Vector3> [] {covered, alt};
	}

	//returns whether a vector v, starting at center of ellipse, is inside ellipse
	//with dimensions transform
	bool IsInsideEllipse (Vector3 v, Vector3 transform) {
		return (v.x * v.x / transform.x / transform.x 
		        + v.y * v.y / transform.y / transform.y
				+ v.z * v.z / transform.z / transform.z < 1);
	}
}
