using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Extruder : MonoBehaviour {
	//event type for custom extrude function. toChange is the list to pull, vertices is the mesh's vertices,
	//and vals is an array of special parameters (i.e. size, offset, etc.)
	public delegate void ExtrudeType (List <int> toChange, Vector3 [] vertices, Vector3 [] vals);

	//Extrudes a face (triangles) in the direction offset
	public static void Extrude (Mesh mesh, int [] faces, bool hardEdge, ExtrudeType ex, Vector3 [] vals) {
		Vector3 [] vertices = mesh.vertices;
		int [] triangles = mesh.triangles;
		
		//key = face vertices, value = edge vertices with same coords
		Dictionary <int, List <int>> faceToSides = new Dictionary<int, List<int>> ();
		foreach (int i in faces) {
			for (int j = 0; j < vertices.Length; j++) {
				if (vertices [j] == vertices [i] && j != i) {
					if (!faceToSides.ContainsKey (i)) faceToSides.Add (i, new List <int> ()) ;
					if (!faceToSides [i].Contains (j)) faceToSides [i].Add (j);
				}
			}
		}
		Debug.Log (faceToSides.Count);
		//new triangles and vertices arrays (new geometry is appended at the end)
		int verticesToAdd = 0;
		foreach (List <int> l in faceToSides.Values) verticesToAdd += l.Count;
		if (hardEdge) verticesToAdd *= 2;
		Vector3 [] newVertices = new Vector3 [vertices.Length + verticesToAdd];
		int [] newTriangles = new int [triangles.Length + 3 * 2 * faceToSides.Count]; 
		System.Array.Copy (vertices, newVertices, vertices.Length);
		System.Array.Copy (triangles, newTriangles, triangles.Length);
		int vertIndex = vertices.Length; //current position in new vertices array
		int triIndex = triangles.Length; //current position new triangles array

		//copy the seam where the faces connect instead of joining to it
		Dictionary <int, int> copies = new Dictionary <int, int> (); //maps original to copy
		if (hardEdge) {
			foreach (List <int> l in faceToSides.Values) {
				foreach (int i in l) {
					AddVertex (newVertices, ref vertIndex, vertices [i], null);
					copies.Add (i, vertIndex - 1);
				}
			}
		}

		//data structures for loop
		List <int> visited = new List <int> (); //visited vertices
		List <int> addedVertices = new List <int> ();
		int prev = -1, curr = -1;

		//make sure that curr, prev are going clockwise (so normals align)
		IEnumerator enumerator = faceToSides.Keys.GetEnumerator ();
		enumerator.MoveNext ();
		prev = (int) enumerator.Current;
		curr = GetClockwiseEdge (mesh, prev, faces);

		//loops around the edge, vertex by vertex, adding new geometry
		while (!visited.Contains (curr)) {
			//add the new geometry
			//if (faceToSides [curr].Count == 2) { //hard edge (DIFFERENT from hardEdge boolean)
				int s1 = -1, s2 = -1; //s1 = vertex behind curr, s2 = vertex behind prev
				foreach (int i in faceToSides [curr]) {
					foreach (int j in faceToSides [prev]) {
						if (AreConnected (mesh, i, j)) {
							s1 = i;
							s2 = j;
						}
					}
				}

				if (hardEdge) {
					s1 = copies [s1];
					s2 = copies [s2];
				}

				AddVertex (newVertices, ref vertIndex, newVertices [s1], addedVertices);
				AddVertex (newVertices, ref vertIndex, newVertices [s2], addedVertices);
				AddTriangle (newTriangles, ref triIndex, s2, s1, vertIndex - 2);
				AddTriangle (newTriangles, ref triIndex, s2, vertIndex - 2, vertIndex - 1);
			//} else if (faceToSides [curr].Count == 1) { //soft edge

			//} else throw new UnityException ("Invalid Shape!");

			//go to next vertex
			visited.Add (curr);
			prev = curr;
			curr = GetClockwiseEdge (mesh, curr, faces);
		}

		//move the faces using the delegate
		List <int> toChange = new List <int> ();
		toChange.AddRange (faces);
		toChange.AddRange (addedVertices);
		ex (toChange, newVertices, vals);

		mesh.vertices = newVertices;
		mesh.triangles = newTriangles;
		mesh.RecalculateBounds ();
		mesh.RecalculateNormals ();
	}

	//Returns true if vertices at indices a and b are connected
	private static bool AreConnected (Mesh mesh, int a, int b) {
		if (a == b) return false;
		for (int i = 0; i < mesh.triangles.Length; i += 3) {
			int matches = 0;
			for (int j = i; j < i + 3; j++) {
				if (mesh.triangles [j] == a || mesh.triangles [j] == b) matches++;
			}
			if (matches == 2) return true;
		}
		return false;
	}

	//takes a point on the edge of a group of triangles, returns the next 
	//edge point travelling clockwise
	private static int GetClockwiseEdge (Mesh mesh, int p1, int [] faces) {
		for (int i = 0; i < faces.Length; i++) {
			if (faces [i] == p1) {
				int p2 = NextPt (i, faces);

				bool otherWay = false;
				for (int j = 0; j < faces.Length; j++) {
					if (faces [j] == p2 && NextPt (j, faces) == p1) otherWay = true;
				}
				if (!otherWay) return p2;
			}
		}
		return -1;
	}

	//adds a triangle to the end of triangles array, increments pos
	private static void AddTriangle (int [] triangles, ref int pos, int a, int b, int c) {
		triangles [pos] = a;
		triangles [pos + 1] = b;
		triangles [pos + 2] = c;
		pos += 3;
	}

	//adds a vertex to the end of vertices array, increments pos
	private static void AddVertex (Vector3 [] vertices, ref int pos, Vector3 vertex, List <int> visited) {
		vertices [pos] = vertex;
		if (visited != null) visited.Add (pos);
		pos++;
	}

	//gets the next point, clockwise, in a triangle
	private static int NextPt (int index, int [] faces) {
		if ((index + 1) % 3 == 0) return faces [index - 2];
		else return faces [index + 1];
	}

	//basic extrude function: pulls the face in a straight line. Takes only one parameter in vals, offset
	public static void ExtrudeOffset (List <int> toChange, Vector3 [] vertices, Vector3 [] vals) {
		List <int> alreadyExtruded = new List <int> ();
		foreach (int i in toChange) {
			if (!alreadyExtruded.Contains (i)) {
				vertices [i] += vals [0];
				alreadyExtruded.Add (i);
			}
		}
	}
}
