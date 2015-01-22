using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//Right now this is just a driver for the extrude class. Attach it to a cube or something.
public class RoadMeshController : MonoBehaviour {
	private int [] currentFaces;

	// Use this for initialization
	void Start () {
		Mesh mesh = gameObject.GetComponent <MeshFilter> ().mesh;
		int [] triangles = mesh.triangles;

		//get the positive x face of the cube
		List <int> trianglesList = new List <int> ();
		for (int i = 0; i < triangles.Length; i += 3) {
			int vertsGreaterThanZero = 0;
			for (int j = i; j < i + 3; j++) 
				if (mesh.vertices [triangles [j]].y > 0) vertsGreaterThanZero++;

			if (vertsGreaterThanZero == 3)
				for (int j = i; j < i + 3; j++) trianglesList.Add (triangles [j]);
		}
		currentFaces = trianglesList.ToArray ();

		Vector3 [] vals = new Vector3 [] {(Vector3.up) / 2};
		Extruder.Extrude (mesh, currentFaces, true, Extruder.ExtrudeOffset, vals);
		//TestSplitCylinder (mesh);
	}

	//test function: drags one face out by two units
	void TestDrag (Mesh mesh) {
		Vector3 [] vertices = mesh.vertices;
		for (int i = 0; i < vertices.Length; i++) {
			if (vertices [i].x > 0) vertices [i].x += 2;
		}
		mesh.vertices = vertices; //need to do this for some reason
		mesh.RecalculateBounds ();
	}

	//test out various cylinder meshes
	void TestSplitCylinder (Mesh mesh) {
		Vector3 offset = .05f * Vector3.left;
		Vector3 [] vertices = mesh.vertices;
		Dictionary <Vector3, int> occurrences = new Dictionary <Vector3, int> ();
		for (int i = 0; i < vertices.Length; i++) {
			if (!occurrences.ContainsKey (vertices [i])) occurrences.Add (vertices [i], 1);
			else {
				occurrences [vertices [i]]++;
				if (occurrences [vertices [i]] == 3) Debug.Log (3);
				vertices [i] += occurrences [vertices [i]] * offset;
			}
		}

		mesh.vertices = vertices;
		mesh.RecalculateBounds ();
	}
}
