using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//Right now this is just a driver for the extrude class. Attach it to a cube or something.
public class RoadMeshController : MonoBehaviour {
	private int [] currentFaces;
	public float amt;
	public Vector3 mov;

	// Use this for initialization
	void Start () {
		Mesh mesh = gameObject.GetComponent <MeshFilter> ().mesh;
		int [] triangles = mesh.triangles;

		//get the positive x face of the cube
		List <int> trianglesList = new List <int> ();
		for (int i = 0; i < triangles.Length; i += 3) {
			int vertsGreaterThanZero = 0;
			for (int j = i; j < i + 3; j++) 
				if (mesh.vertices [triangles [j]].y > amt) vertsGreaterThanZero++;

			if (vertsGreaterThanZero == 3)
				for (int j = i; j < i + 3; j++) trianglesList.Add (triangles [j]);
		}
		currentFaces = trianglesList.ToArray ();

		//Vector3 [] vals = new Vector3 [] {mov};
		//Vector3 [] vals2 = new Vector3 [] {-1 * mov};

		//Vector3 [] resizeVals = new Vector3 [] {mov, new Vector3 (1.5f, 1.5f, 1.5f)};
		//Vector3 [] offsetVals = new Vector3 [] {mov, new Vector3 (.2f, 0, 0)};
		//Extruder.Extrude (mesh, currentFaces, false, Extruder.ExtrudeBevel, offsetVals);
		//Extruder.Extrude (mesh, currentFaces, true, Extruder.ExtrudeBevel, offsetVals);

		//Vector3 [] resizeVals = new Vector3 [] {mov, new Vector3 (1.5f, 1.5f, 1.5f)};
		//Vector3 [] offsetVals = new Vector3 [] {mov, new Vector3 (.1f, 0, 0)};
		//Extruder.Extrude (mesh, currentFaces, false, Extruder.ExtrudeWeirdClean, resizeVals);

		//Tiered square pyramid loops
		//for (int i = 0; i < 20; i +=1)
		//{
			//Vector3 [] offsetVals = new Vector3 [] {mov, new Vector3 (10f, 0, 0)};//Makes a tiered square
			//Extruder.Extrude (mesh, currentFaces, true, Extruder.ExtrudeBevel, offsetVals);//Makes a tiered square
		//}

		//Extruder.Extrude (mesh, currentFaces, true, Extruder.ExtrudeOffset, vals2);
		//TestDrag (mesh, currentFaces);
		//Debug.Log (GetUniquePoints (currentFaces));
		//TestSplitCylinder (mesh, new List <int> (triangles));
	}

	//test function: drags one face out by one unit
	void TestDrag (Mesh mesh, int [] triangles) {
		Vector3 [] vertices = mesh.vertices;
		List <int> alreadyMoved = new List <int> ();
		foreach (int i in triangles) {
			if (!alreadyMoved.Contains (i)) {
				vertices [i] += Vector3.left;
				alreadyMoved.Add (i);
			}
		}
		mesh.vertices = vertices; //need to do this for some reason
		mesh.RecalculateBounds ();
	}

	//test out various cylinder meshes
	void TestSplitCylinder (Mesh mesh, List <int> triangles) {
		Vector3 offset = .05f * Vector3.left;
		Vector3 [] vertices = mesh.vertices;
		Dictionary <Vector3, int> occurrences = new Dictionary <Vector3, int> ();
		for (int i = 0; i < vertices.Length; i++) {
			if (!occurrences.ContainsKey (vertices [i])) occurrences.Add (vertices [i], 1);
			else {
				Debug.Log (i);
				if (!triangles.Contains (i)) continue;
				occurrences [vertices [i]]++;
				if (occurrences [vertices [i]] == 3) Debug.Log (3);
				vertices [i] += occurrences [vertices [i]] * offset;
			}
		}

		mesh.vertices = vertices;
		mesh.RecalculateBounds ();
	}

	int GetUniquePoints (int [] triangles) {
		List <int> added = new List <int> ();
		int sum = 0;
		foreach (int i in triangles) {
			if (!added.Contains (i)) {
				sum++;
				added.Add (i);
			}
		}

		return sum;
	}
}
