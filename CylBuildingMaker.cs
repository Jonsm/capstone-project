using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CylBuildingMaker : MonoBehaviour {
	public GameObject child;
	public float radius;
	public int segments;
	public int segmentHeight;
	public bool hasDome;

	public float domeRatio; //how flat is the dome? 0 is flat, 1 is a sphere, 2 is ellipse...
	public float expandChance;
	public float windowChance;
	public float [] maxRad;
	public float windowHeight;

	public int numChildren;
	public float [] childRadiusFactor; //how much to reduce radius of children?
	public float [] childSegmentFactor; //how much to multiply height of children

	private float topRad;
	private static float domeSegments = 8;
	private static float windowInset = .5f;
	private static float maxTries = 5;
	private int [] faces;
	private Mesh mesh;

	public void BuildMe () {
		mesh = gameObject.GetComponent <MeshFilter> ().mesh;
		Vector3 [] vertices = mesh.vertices;
		int [] triangles = mesh.triangles;

		//find top faces
		List <int> facesList = new List <int> ();
		for (int i = 0; i < triangles.Length; i += 3) {
			int occurrences = 0;
			for (int j = i; j < i + 3; j++)
				if (mesh.vertices [triangles [j]].y > .5f) occurrences++;
			
			if (occurrences == 3)
				for (int j = i; j < i + 3; j++) facesList.Add (triangles [j]);
		}
		faces = facesList.ToArray ();

		//find radius and rescale
		//also reassign uvs to 0,0
		Vector2 [] uv = mesh.uv;
		float prevRadius = -1;
		int curr = 0;
		do {
			curr++;
			prevRadius = (vertices [curr] - vertices [curr].y * Vector3.up).magnitude;
		} while (vertices [curr].z == 0 && vertices [curr].x == 0);

		float factor = radius / prevRadius;
		Matrix4x4 trs = new Matrix4x4 ();
		trs.SetTRS (Vector3.zero, Quaternion.identity, new Vector3 (factor, 1, factor));
		for (int i = 0; i < vertices.Length; i++) {
			uv [i] = Vector2.zero;
			vertices [i] = trs.MultiplyPoint3x4 (vertices [i]);
		}

		mesh.vertices = vertices;
		mesh.triangles = triangles;
		mesh.uv = uv;

		AddSegments();
		if (hasDome) MakeDome ();
		if (numChildren > 0) MakeChildren ();
		mesh.RecalculateBounds ();
		mesh.RecalculateNormals ();
	}

	void AddSegments () {
		float rad = radius;
		float currHeight = 0;
		float actualHeight = 0;
		bool justBeveled = true;

		//go through each segment, only extruding when radius changes (because extrusion is slow)
		for (int i = 0; i < segments; i++) {
			//decide whether to bevel it
			float newRad = rad;
			if (!justBeveled) {
				if (rad == radius) {
					if (Random.Range (0f, 1f) <= expandChance) newRad = Random.Range (maxRad [0], maxRad [1]);
				} else {
					if (Random.Range (0f, 1f) <= expandChance) newRad = radius;
				}
			}
			if (rad != newRad) justBeveled = true;
			else justBeveled = false;

			bool isWindow = (Random.Range (0f, 1f) <= windowChance);
			List <int> windowVerts = null;

			//extrude it when it needs to be beveled
			if (newRad != rad) {
				//catch up to the point where it bevels
				Vector3 [] diff = new Vector3 [] {(actualHeight - currHeight) * Vector3.up};
				float randH = Random.Range (1f, 2f);
				if (actualHeight != currHeight) Extruder.Extrude (mesh, faces, true, Extruder.ExtrudeOffset, diff, false);
				actualHeight += randH * segmentHeight;
				currHeight = actualHeight;

				//bevel
				diff = new Vector3 [] {segmentHeight * randH * Vector3.up, 
									   new Vector3 (newRad / rad, 0, newRad / rad)};
				windowVerts = Extruder.Extrude (mesh, faces, true, Extruder.ExtrudeResize, diff, false);
			} else { 
				actualHeight += segmentHeight;
			}

			//extrude it when it needs a window (so there is new geometry) or use beveled vertices
			if (isWindow) {
				if (newRad == rad) {
					Vector3 [] diff = new Vector3 [] {(actualHeight - currHeight) * Vector3.up};
					if (actualHeight != currHeight) Extruder.Extrude (mesh, faces, true, Extruder.ExtrudeOffset, diff, false);
					actualHeight += windowHeight;
					currHeight = actualHeight;

					float inset = (rad - windowInset) / rad;
					diff = new Vector3 [] {Vector3.zero, new Vector3 (inset, 0, inset)};
					Extruder.Extrude (mesh, faces, true, Extruder.ExtrudeResize, diff, false);

					diff = new Vector3 [] {windowHeight * Vector3.up};
					windowVerts = Extruder.Extrude (mesh, faces, true, Extruder.ExtrudeOffset, diff, false);

					diff = new Vector3 [] {Vector3.zero, new Vector3 (1 / inset, 0, 1 / inset)};
					Extruder.Extrude (mesh, faces, true, Extruder.ExtrudeResize, diff, false);
				}

				//change UV coordinates so mesh can be recolored
				Vector2 [] uv = mesh.uv;
				foreach (int j in windowVerts) uv [j] = new Vector2 (1, 1);
				mesh.uv = uv;
			}

			rad = newRad;
		}

		if (actualHeight != currHeight) {
			Vector3 [] diff = new Vector3 [] {(actualHeight - currHeight) * Vector3.up};
			Extruder.Extrude (mesh, faces, true, Extruder.ExtrudeOffset, diff, false);
		}
		topRad = rad;
	}

	//puts a dome on top of the building
	void MakeDome () {
		//extrude the faces in a circular pattern
		float rad = topRad;
		float step = domeRatio * topRad / (domeSegments + 1);
		Vector3 [] parameters = new Vector3 [] {step * Vector3.up, Vector3.zero};
		for (int i = 0; i < domeSegments; i++) {
			bool hardEdge = (i == 0);
			float inset = Mathf.Sqrt (Mathf.Pow (topRad, 2) - Mathf.Pow ((float) (i + 1) * step / domeRatio, 2));
			float resize = inset / rad;
			rad = inset;
			parameters [1] = new Vector3 (resize, 0, resize);

			Extruder.Extrude (mesh, faces, hardEdge, Extruder.ExtrudeResize, parameters, false);
		}

		//pull out the top point
		Vector3 [] vertices = mesh.vertices;
		Vector3 center = Vector3.zero;
		for (int i = 0; i < 3; i++) {
			int occurrences = 0;
			for (int j = 3; j < 9; j++) {
				if (vertices [faces [j]] == vertices [faces [i]]) occurrences++;
			}
			if (occurrences == 2) center = vertices [faces [i]];
		}

		HashSet <int> facesSet = new HashSet <int> (faces);
		foreach (int i in facesSet) {
			if (vertices [i] == center) vertices [i] += parameters [0];
		}

		mesh.vertices = vertices;
	}

	//buildings can have children
	void MakeChildren () {
		List <int []> forbiddenAngles = new List <int []> ();
		for (int i = 0; i < numChildren; i++) {
			//calculate place to put the child building
			float newRad = Random.Range (childRadiusFactor [0], childRadiusFactor [1]) * radius;
			float angleSub = (360 * newRad) / (Mathf.PI * Mathf.PI * radius);

			int angle = 0; 
			bool forbidden = true;
			int tries = 0;
			while (forbidden && tries < maxTries) {
				forbidden = false;
				tries++;
				angle = Random.Range (0, 360);
				foreach (int [] range in forbiddenAngles) {
					if (angle - angleSub / 2 > range [0] && angle - angleSub / 2 < range [1] ||
					    angle + angleSub / 2 > range [0] && angle + angleSub / 2 < range [1]) forbidden = true;
				}
			}
			if (tries == maxTries) continue;

			//instantiate
			Vector3 v = Quaternion.Euler (0, angle, 0) * (radius * Vector3.left);
			GameObject childO = Instantiate (child, v, Quaternion.identity) as GameObject;
			CylBuildingMaker ccb = childO.GetComponent <CylBuildingMaker> () as CylBuildingMaker;
			forbiddenAngles.Add (new int [] {(int) (angle - angleSub / 2), (int) (angle + angleSub / 2)});

			//grow the new building
			ccb.radius = newRad;
			ccb.segments = (int) Random.Range (childSegmentFactor [0] * segments, childSegmentFactor [1] * segments);
			ccb.segmentHeight = segmentHeight;
			ccb.hasDome = hasDome;
			ccb.domeRatio = domeRatio;
			ccb.expandChance = expandChance;
			ccb.windowChance = windowChance;
			ccb.maxRad = new float [] {maxRad [0] * newRad / radius, maxRad [1] * newRad / radius};
			ccb.windowHeight = windowHeight;
			ccb.numChildren = 0;

			ccb.BuildMe ();
		}
	}
}
