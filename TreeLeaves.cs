using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TreeLeaves : MonoBehaviour {
	public List <Vector3> tips;
	public GameObject leafBall;
	public float ballRadius;

	private int randomLocs = 3; //how many random locations to try when placing ball

	private float sqrRadius;

	//go through all the branch tips, placing leaves and removing tips that have leaves from the list.
	//try out positions randomly to find the position where placing one leaf ball covers the most tips.
	public void MakeLikeATree () {
		sqrRadius = ballRadius * ballRadius;

		while (tips.Count > 0) {
			int centerLoc = Random.Range (0, tips.Count);
			List <Vector3> [] coveredBranches = GetCoveredLeaves (centerLoc);

			for (int i = 0; i < randomLocs - 1; i++) {
				int centerLocTmp = Random.Range (0, tips.Count);
				List <Vector3> [] coveredBranchesTmp = GetCoveredLeaves (centerLocTmp);

				if (coveredBranchesTmp [0].Count > coveredBranches [0].Count) {
					centerLoc = centerLocTmp;
					coveredBranches = coveredBranchesTmp;
				}
			}

			GameObject lb = Instantiate (leafBall, tips [centerLoc], Quaternion.identity) as GameObject;
			lb.transform.parent = gameObject.transform;
			lb.transform.localScale = new Vector3 (ballRadius, ballRadius, ballRadius);
			tips = coveredBranches [1];
		}
	}

	//returns list of leaves covered by a ball centered at tip at position i
	List <Vector3> [] GetCoveredLeaves (int i) {
		List <Vector3> covered = new List <Vector3> ();
		List <Vector3> alt = new List <Vector3> ();
		foreach (Vector3 v in tips) {
			if ((tips [i] - v).sqrMagnitude <= sqrRadius) covered.Add (v);
			else alt.Add (v);
		}

		return new List <Vector3> [] {covered, alt};
	}
}
