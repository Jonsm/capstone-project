using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CylBuildingMaker : MonoBehaviour {
	public int [] faces;
	public Mesh mesh;
	public float radius;
	public int segments;
	public int segmentHeight;

	public float [] topChances; //array of chance for flat roof, slanted roof, domed roof
	public float expandChance;
	public float windowChance;
	public float [] maxRad;
	public float windowHeight;
	public float windowInset;

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

		AddSegments ();
		MakeRoof ();
		mesh.RecalculateBounds ();
		mesh.RecalculateNormals ();
	}

	void AddSegments () {
		float rad = radius;
		float currHeight = 0;
		float actualHeight = 0;

		//go through each segment, only extruding when radius changes (because extrusion is slow)
		for (int i = 0; i < segments; i++) {
			//decide whether to bevel it
			float newRad = rad;
			if (rad == radius) {
				if (Random.Range (0f, 1f) <= expandChance) newRad = Random.Range (maxRad [0], maxRad [1]);
			} else {
				if (Random.Range (0f, 1f) <= expandChance) newRad = radius;
			}
			bool isWindow = (Random.Range (0f, 1f) <= windowChance);
			List <int> windowVerts = null;

			//extrude it when it needs to be beveled
			if (newRad != rad) {
				if (i == 0) Debug.Log (i);
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
	}

	void MakeRoof () {

	}
}
