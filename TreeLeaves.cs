using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TreeLeaves : MonoBehaviour {
	//public GameObject leafBall;
	public TreeGenerator tg;

	//private int randomLocs = 3; //how many random locations to try when placing ball

	private Dictionary <Vector3, Vector3> points;

	public float [] leafLength;
	public float [] leafWidth;
	public float [] leafAngleRange;
	public int [] leafDensity;
	public GameObject leaves;

	//go through all the branch tips, placing leaves and removing tips that have leaves from the list.
	//try out positions randomly to find the position where placing one leaf ball covers the most tips.
	/*public void MakeLikeATree () { //and leaf
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
	}*/

	public void MakeLikeATree (TreeGenerator tg)  { //and leaf
		points = tg.points;

		//create a bunch of triangles
		List <int> triangles = new List <int> ();
		List <Vector3> vertices = new List <Vector3> ();
		foreach (Vector3 v in points.Keys) {

			int numLeaves = Random.Range (leafDensity [0], leafDensity [1] + 1);
			for (int i = 0; i < numLeaves; i++) {
				Vector3 pos = Vector3.Lerp (v, v + points [v], (float) i / numLeaves);
				MakeLeaf (pos, vertices, triangles);
			}
		}

		//assign the mesh
		leaves.transform.position = Vector3.zero;
		MeshFilter mf = leaves.GetComponent <MeshFilter> () as MeshFilter;
		mf.mesh = new Mesh ();
		Mesh mesh = mf.mesh;
		if (mesh != null)
			Debug.Log ("mesh");
		mesh.vertices = vertices.ToArray ();
		mesh.triangles = triangles.ToArray ();
		Debug.Log (mesh.vertices.Length);
		mesh.RecalculateBounds ();
		mesh.RecalculateNormals ();
	}

	//adds a leaf to arbitrary list of vertices at point v
	void MakeLeaf (Vector3 v, List <Vector3> vertices, List <int> triangles) {
		float length = Random.Range (leafLength [0], leafLength [1]);
		float width = Random.Range (leafWidth [0], leafWidth [1]);
		Vector3 dir = Vector3.down * length;
		Quaternion rand = Quaternion.Euler (new Vector3 (Random.Range (leafAngleRange [0], leafAngleRange [1]),
		                                                 Random.Range (0, 360), 0));
		dir = rand * dir;
		Vector3 tangent = Vector3.Cross (dir, Random.onUnitSphere).normalized * width / 2;
		
		//add one side of leaf
		vertices.Add (v);
		vertices.Add (v + dir + tangent);
		vertices.Add (v + dir - tangent);
		triangles.Add (vertices.Count - 1);
		triangles.Add (vertices.Count - 2);
		triangles.Add (vertices.Count - 3);
		
		//add other side of leaf
		vertices.Add (v);
		vertices.Add (v + dir - tangent);
		vertices.Add (v + dir + tangent);
		triangles.Add (vertices.Count - 1);
		triangles.Add (vertices.Count - 2);
		triangles.Add (vertices.Count - 3);
	}
}
