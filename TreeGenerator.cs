﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TreeGenerator : MonoBehaviour {
	//each is a coordinate pair: [# in current, change in branch]
	public int [] segments; //how many segments main trunk has, absolute change
	public float [] segmentLength; //length of each segment, % change
	public float [] radius; //how big is the base of the trunk, % change
	public float [] upCurve; //angle range by which tree grows upward, % change
	public float [] maxTurn; //maximum turn radius in degrees, probably set to < 20
	public float [] branchChance; //chance of branching at a segment
	public float [] branchDeviation; //amount branch-trunk angle will deviate from 90
	public float maxLeafPercent; //maximum width relative to size of base that leaves will grow

	public GameObject branch;
	public Mesh mfMesh;

	//hashses position of branch tips to direction
	public Dictionary <Vector3, Vector3> points = new Dictionary<Vector3, Vector3> ();
	//for synchronization, branches is a list of branches still generating
	public delegate void PopTree (TreeGenerator t);
	public event PopTree pEvent;
	private List <TreeGenerator> branches = new List <TreeGenerator> ();
	private Vector3 lastPoint = Vector3.zero;
	
	private Mesh mesh;
	private int center; //center of faces
	private int [] faces; //faces of part being extruded
	private float widthLossFactor; //amount to extrude it by
	private static GameObject TreeContainer;
	private static float waitTime = .2f;

	//set the trunk to the right size
	public void Init () {
		gameObject.GetComponent<MeshRenderer> ().enabled = false;
		MainManager.meshManager.Add(gameObject.GetComponent<MeshRenderer> ());
		//add the top faces to faces and set their height
		if (TreeContainer == null) {
			TreeContainer = new GameObject ();
		}
		gameObject.transform.parent = TreeContainer.transform;
		MeshFilter mf = gameObject.GetComponent <MeshFilter> () as MeshFilter;
		mf.mesh = mfMesh;
		mesh = gameObject.GetComponent <MeshFilter> ().mesh;
		//mesh.MarkDynamic ();

		int [] triangles = mesh.triangles;
		Vector3 [] vertices = mesh.vertices;

		List <int> facesList = new List <int> ();
		for (int i = 0; i < triangles.Length; i += 3) {
			int occurrences = 0;
			for (int j = i; j < i + 3; j++)
				if (mesh.vertices [triangles [j]].y > 0) occurrences++;

			if (occurrences == 3)
				for (int j = i; j < i + 3; j++) facesList.Add (triangles [j]);
		}
		faces = facesList.ToArray ();

		//set the size of the base to radius
		float currentRadius = 0;
		int curr = 0;
		while (currentRadius == 0) {
			Vector3 v = vertices [curr];
			currentRadius += Mathf.Sqrt (Mathf.Pow (v.x, 2) + Mathf.Pow (v.z, 2));
			curr++;
		}

		Matrix4x4 resize = new Matrix4x4 ();
		resize.SetTRS (Vector3.zero, Quaternion.identity, 
		               new Vector3 (radius [0] / currentRadius, 1, radius [0] / currentRadius));
		for (int i = 0; i < vertices.Length; i++) {
			vertices [i] = resize.MultiplyPoint3x4 (vertices [i]);
		}

		//scale the first top face
		widthLossFactor = radius [0] / segments [0];
		float scale = (radius [0] - widthLossFactor) / radius [0];
		resize.SetTRS (Vector3.zero, Quaternion.identity, 
		               new Vector3 (scale, 1, scale));
		for (int i = 0; i < vertices.Length; i++) {
			if (vertices [i].y > 0) 
				vertices [i] = resize.MultiplyPoint3x4 (vertices [i]);
		}

		//change the height of top face
		Vector3 heightFactor = new Vector3 (0, segmentLength [0] - vertices [faces [0]].y, 0);
		for (int i = 0; i < vertices.Length; i++) {
			if (vertices [i].y > 0) vertices [i] += heightFactor;
		}

		//find the center (any point in more than 2 triangles)
		center = -1;
		for (int i = 0; i < 3; i++) {
			int occurrences = 0;
			for (int j = 3; j < 9; j++) {
				if (faces [i] == faces [j]) occurrences++;
			}
			if (occurrences == 2) center = faces [i];
		}

		mesh.vertices = vertices;
		mesh.RecalculateBounds ();
		mesh.RecalculateNormals ();

		//stuff for leaves
		/*if (maxLeafPercent > 1) {
			Vector3 pos = gameObject.transform.TransformPoint (mesh.vertices [center] + .95f * Vector3.down * segmentLength [0]);
			Vector3 dir = gameObject.transform.TransformVector (Vector3.up * segmentLength [0]);
			points.Add (pos, dir);
		}*/
		if (maxLeafPercent > 1)
			lastPoint = gameObject.transform.TransformPoint (mesh.vertices [center] + Vector3.down * segmentLength [0]);
		branches.Add (this);
	}

	public IEnumerator Grow(){
		Vector3 [] parameters = new Vector3 [] {new Vector3 (0, segmentLength [0], 0),
			new Vector3 (widthLossFactor, 0, 0)};
		float currRadius = radius [0] - widthLossFactor;
		
		for (int i = 0; i < segments [0] - 1; i++) {
			//rotate by a random amount
			Vector3 rotVector = RandomNormalCurveVector (maxTurn [0]);
			Quaternion rotation = Quaternion.Euler (rotVector);
			parameters [0] = rotation * parameters [0];

			//rotate upward
			Vector3 localY = gameObject.transform.InverseTransformVector (Vector3.up);
			float rad = Random.Range (0, upCurve [0]) * Mathf.PI / 180;
			parameters [0] = Vector3.RotateTowards (parameters [0], localY, rad, 0);

			//make note for leaves
//			if (currRadius < maxLeafPercent * radius [0]) {
//				Vector3 pos = gameObject.transform.TransformPoint (mesh.vertices [center]);
//				Vector3 dir2 = gameObject.transform.TransformVector (parameters [0]);
//				points.Add (pos, dir2);
//			}
			if (currRadius < maxLeafPercent * radius [0]) {
				if (lastPoint == Vector3.zero) {
					lastPoint = gameObject.transform.TransformPoint (mesh.vertices [center]);
				} else {
					Vector3 pos = gameObject.transform.TransformPoint (mesh.vertices [center]);
					points.Add (pos, lastPoint - pos);
					lastPoint = pos;
				}
			}

			Extruder.Extrude (mesh, faces, false, Extruder.ExtrudeRotate, parameters, false);

			currRadius -= widthLossFactor;

			float willBranch = Random.Range (0f, 1f);
			if (willBranch < branchChance [0] && i != segments [0] - 2 && segments [0] > 2) {;
				MakeNewBranch (gameObject.transform.TransformPoint (mesh.vertices [center]), 
				               parameters [0], .9f * currRadius, i + 1);
			}

			yield return new WaitForSeconds(waitTime);
		}
		SharpenPoint (parameters [0]);
		mesh.RecalculateNormals ();
		mesh.RecalculateBounds ();
		yield return null;

	}

	private void MakeNewBranch (Vector3 pos, Vector3 dir, float rad, int segs) {

		if (rad < .05f || segments [0] - segs - segments [1] <= 0) return;

		//Calculate rotation
		Vector3 transformedDir = gameObject.transform.TransformVector (dir.normalized);
		Vector3 rand = Random.insideUnitSphere;
		Vector3 branchDir = Vector3.Cross (rand, transformedDir).normalized;
		float rads = Random.Range (0, branchDeviation [0]) * Mathf.PI / 180;
		branchDir = Vector3.RotateTowards (branchDir, transformedDir, 
		                                   rads, 0);
		rads = Random.Range (0, upCurve [0]) * Mathf.PI / 180;
		branchDir = Vector3.RotateTowards (branchDir, Vector3.up, 
		                                   rads, 0);

		//make that branch
		GameObject newBranch = Instantiate (branch, pos, 
		           Quaternion.FromToRotation (Vector3.up, branchDir)) as GameObject;
		newBranch.transform.position += branchDir * .5f;

		TreeGenerator tg = newBranch.GetComponent <TreeGenerator> () as TreeGenerator;
		tg.segments = new int [] {segments [0] - segs - segments [1], segments [1]};
		tg.radius = new float [] {rad * (1 - radius [1]), radius [1]};
		tg.segmentLength = ApplyIncrements (segmentLength);
		tg.upCurve = ApplyIncrements (upCurve);
		tg.maxTurn = ApplyIncrements (maxTurn);
		tg.branchChance = ApplyIncrements (branchChance);
		tg.mfMesh = mfMesh;
		tg.branchDeviation = ApplyIncrements (branchDeviation);
		tg.points = points;
		tg.maxLeafPercent = (maxLeafPercent * radius [0]) / tg.radius [0];

		//synchronization
		tg.pEvent = RemoveBranch;
		branches.Add (tg);

		tg.Init ();
		tg.StartCoroutine("Grow");
	}

	//takes the top face of the trunk and replaces it with a single point (like a cone)
	private void SharpenPoint (Vector3 dir) {
		//get rotation
		Vector3 rotVector = RandomNormalCurveVector (maxTurn [0]);
		Quaternion rotation = Quaternion.Euler (rotVector);
		dir = rotation * dir;

		Vector3 [] vertices = mesh.vertices;
		int [] triangles = mesh.triangles;

		Vector3 centerCoords = vertices [center];

		HashSet <Vector3> vectorSet = new HashSet <Vector3> ();
		Dictionary <int, int> trianglesLookup = new Dictionary<int, int> ();

		foreach (int i in faces)
			if (!vectorSet.Contains (vertices [i])) vectorSet.Add (vertices [i]);
		
		//only keep triangles with one edge touching extruded face
		HashSet <int> vertsToKeep = new HashSet <int> ();
		for (int i = 0; i < triangles.Length; i += 3) {
			int occurrences = 0;
			for (int j = i; j < i + 3; j++)
				if (vectorSet.Contains (vertices [triangles [j]])) occurrences++;
		
			if (occurrences == 1) {
				for (int j = i; j < i + 3; j++)
					if (vectorSet.Contains (vertices [triangles [j]])) vertsToKeep.Add (triangles [j]);
			}
		}

		//copy the vertices into a new array without the top face
		Vector3 [] newVertices = new Vector3 [vertices.Length - vectorSet.Count - faces.Length / 3 + 50];
		int k = 0; //counter for new array
		for (int i = 0; i < vertices.Length; i++) {
			Vector3 point = vertices [i];

			if (vectorSet.Contains (point)) {
				if (vertsToKeep.Contains (i)) {
					trianglesLookup [i] = k;
					newVertices [k] = point;
					k++;
				}
			} else {
				trianglesLookup [i] = k;
				newVertices [k] = point;
				k++;
			}
		}

		//reconstruct triangles array
		int [] newTriangles = new int [triangles.Length - 2 * faces.Length];
		k = 0;
		for (int i = 0; i < triangles.Length; i += 3) {
			int occurrences = 0;
			for (int j = i; j < i + 3; j++) 
				if (!trianglesLookup.ContainsKey (triangles [j])) occurrences++;

			if (occurrences == 0) {
				for (int j = i; j < i + 3; j++) {
					newTriangles [k] = trianglesLookup [triangles [j]];
					k++;
				}
			}
		}

		//drag in the triangles to a point, and apply the new values
		foreach (int i in vertsToKeep) {
			newVertices [trianglesLookup [i]] = centerCoords;
		}

		mesh.vertices = newVertices;
		mesh.triangles = newTriangles;

		Vector3 pos = gameObject.transform.TransformPoint (centerCoords);
		points.Add (pos, lastPoint - pos);
		RemoveBranch (this);
	}

	private Vector3 RandomNormalCurveVector (float range) {
		return new Vector3 (
			Random.Range (0f, range) * Random.Range (-1f * range, range) / range,
			Random.Range (0f, range) * Random.Range (-1f * range, range) / range,
			Random.Range (0f, range) * Random.Range (-1f * range, range) / range
		);
	}

	private float [] ApplyIncrements (float [] start) {
		return new float [] {start [0] * (1 - start [1]), start [1]};
	}

	//removes branch from the branches list, indicating it's done.
	private void RemoveBranch (TreeGenerator t) {
		branches.Remove (t);
		if (branches.Count == 0 && pEvent != null) {
			pEvent (this);
		}
	}
}
