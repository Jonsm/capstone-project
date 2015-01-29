using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Extruder : MonoBehaviour {
	//event type for custom extrude function. toChange is the list to pull, vertices is the mesh's vertices,
	//and vals is an array of special parameters (i.e. size, offset, etc.)
	public delegate void ExtrudeType (List <int> toChange, Mesh mesh, Vector3 [] vals);

	//Extrudes a face (triangles) in the direction offset
	public static void Extrude (Mesh mesh, int [] faces, bool hardEdge, ExtrudeType ex, Vector3 [] vals) {
		Vector3 [] vertices = mesh.vertices;
		int [] triangles = mesh.triangles;
		
		//key = face vertices, value = edge vertices with same coords
		Dictionary <int, List <int>> faceToSides = new Dictionary<int, List<int>> ();
		foreach (int i in faces) {
			int occurrences = 0;
			foreach (int j in faces) if (j == i) occurrences++;

			if (occurrences <= 2 && !faceToSides.ContainsKey (i)) {
				faceToSides.Add (i, new List <int> ()) ;

				for (int j = 0; j < vertices.Length; j++) {
					if (vertices [j] == vertices [i] && j != i) faceToSides [i].Add (j);
				}
			}
		}

		//new triangles and vertices arrays (new geometry is appended at the end)
		int verticesToAdd = 0;
		//the algorithm will make the array bigger than it needs to be. This will tell it how much to remove.
		int verticesToRemove = 0;
		foreach (int i in faceToSides.Keys) {
			int count = faceToSides [i].Count;
			if (count > 0) verticesToAdd += count;
			else {
				verticesToAdd += 4;
				verticesToRemove += 4;
			}
		}
		if (hardEdge) verticesToAdd *= 2;
		Vector3 [] newVertices = new Vector3 [vertices.Length + verticesToAdd];
		int [] newTriangles = new int [triangles.Length + 3 * 2 * faceToSides.Count]; 
		System.Array.Copy (vertices, newVertices, vertices.Length);
		System.Array.Copy (triangles, newTriangles, triangles.Length);
		int vertIndex = vertices.Length; //current position in new vertices array
		int triIndex = triangles.Length; //current position new triangles array

		//copy the seam where the faces connect instead of joining to it
		Dictionary <int, int> copies = new Dictionary <int, int> (); //maps original to copy
		Dictionary <int, int> addedVertices = new Dictionary<int, int> ();
		foreach (List <int> l in faceToSides.Values) {
			foreach (int i in l) {
				if (hardEdge) {
					AddVertex (newVertices, ref vertIndex, vertices [i]);
					copies.Add (i, vertIndex - 1);
				}
				AddVertex (newVertices, ref vertIndex, vertices [i]);
				addedVertices.Add (i, vertIndex - 1);
			}
		}

		//data structures for loop
		List <int> visited = new List <int> (); //visited vertices
		int prev = -1, curr = -1;

		//make sure that curr, prev are going clockwise (so normals align)
		IEnumerator enumerator = faceToSides.Keys.GetEnumerator ();
		enumerator.MoveNext ();
		prev = (int) enumerator.Current;
		curr = GetClockwiseEdge (mesh, prev, faces);

		//loops around the edge, vertex by vertex, adding new geometry
		while (!visited.Contains (curr)) {
			int next = GetClockwiseEdge (mesh, curr, faces);

			//add the new geometry
			int s1 = -1, s2 = -1; //s1 = vertex behind curr, s2 = vertex behind prev
			int cCurr = -1, cPrev = -1; //cCurr = vertex at same pos as prev, cPrev '' prev

			foreach (int i in faceToSides [curr]) {
				foreach (int j in faceToSides [prev]) {
					if (AreConnected (mesh, i, j)) {
						s1 = i;
						s2 = j;
					}
				}
			}

			//if it is not on the edge
			if (s1 == -1 && s2 == -1) {
				s1 = vertIndex;
				AddVertex (newVertices, ref vertIndex, vertices [curr]);
				cCurr = vertIndex;
				addedVertices.Add (vertIndex - 1, vertIndex);
				AddVertex (newVertices, ref vertIndex, vertices [curr]);
				s2 = vertIndex;
				AddVertex (newVertices, ref vertIndex, vertices [prev]);
				cPrev = vertIndex;
				addedVertices.Add (vertIndex - 1, vertIndex);
				AddVertex (newVertices, ref vertIndex, vertices [prev]);
				verticesToRemove -= 4;
			}

			cCurr = addedVertices [s1];
			cPrev = addedVertices [s2];
			
			if (hardEdge && copies.ContainsKey (s1) && copies.ContainsKey (s2)) {
				s1 = copies [s1];
				s2 = copies [s2];
			}

			AddTriangle (newTriangles, ref triIndex, s2, s1, cCurr);
			AddTriangle (newTriangles, ref triIndex, s2, cCurr, cPrev);

			//go to next vertex
			visited.Add (curr);
			prev = curr;
			curr = next;
		}

		//delete extraneous entries at the end of the array
		Vector3 [] newNewVertices = new Vector3 [newVertices.Length - verticesToRemove];
		System.Array.Copy (newVertices, newNewVertices, newNewVertices.Length);
		mesh.vertices = newNewVertices;
		mesh.triangles = newTriangles;

		//move the faces using the delegate
		List <int> toChange = new List <int> ();
		toChange.AddRange (faces);
		toChange.AddRange (addedVertices.Values);
		ex (toChange, mesh, vals);

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
	private static void AddVertex (Vector3 [] vertices, ref int pos, Vector3 vertex) {
		vertices [pos] = vertex;
		//if (visited != null) visited.Add (pos);
		pos++;
	}

	//gets the next point, clockwise, in a triangle
	private static int NextPt (int index, int [] faces) {
		if ((index + 1) % 3 == 0) return faces [index - 2];
		else return faces [index + 1];
	}

	//basic extrude function: pulls the face in a straight line. Takes only one parameter in vals, offset
	public static void ExtrudeOffset (List <int> toChange, Mesh mesh, Vector3 [] vals) {
		List <int> alreadyExtruded = new List <int> ();
		Vector3 [] vertices = mesh.vertices;
		foreach (int i in toChange) {
			if (!alreadyExtruded.Contains (i)) {
				vertices [i] += vals [0];
				alreadyExtruded.Add (i);
			}
		}
		mesh.vertices = vertices;
	}

	//shrinks/expands the extruded faces. Vals has two parameters: 1 = offset, 2 = {inset, 0, 0}.
	public static void ExtrudeBevel (List <int> toChange, Mesh mesh, Vector3 [] vals) {
		List <int> topFaces = new List <int> ();
		int [] triangles = mesh.triangles;
		Vector3 [] vertices = mesh.vertices;

		//if a whole triangle is in toChange, add its vertices to edgeFaces
		for (int i = 0; i < triangles.Length; i += 3) {
			int occurrences = 0;
			for (int j = i; j < i + 3; j++) {
				if (toChange.Contains (triangles [i])) occurrences++;
			}

			if (occurrences == 3) {
				for (int j = i; j < i + 3; j++) topFaces.Add (triangles [j]);
			}
		}

		List <int> visited = new List <int> ();
		int [] TFArray = topFaces.ToArray ();
		Vector3 [] vertices2 = new Vector3 [vertices.Length];
		System.Array.Copy (vertices, vertices2, vertices.Length);
		int prev = topFaces [0];
		int curr = GetClockwiseEdge (mesh, prev, TFArray);

		while (!visited.Contains (curr)) {
			int next = GetClockwiseEdge (mesh, curr, TFArray);

			//find the bisector
			Vector3 a1 = vertices [prev].normalized - vertices [curr].normalized;
			Vector3 a2 = vertices [next].normalized - vertices [curr].normalized;
			Vector3 bisector = (a1 + a2).normalized * vals [1].x;

			//move the vertices
			for (int i = 0; i < vertices2.Length; i++) {
				if (toChange.Contains (i)) 
					if (vertices2 [i] == vertices [curr]) vertices2 [i] += bisector;
			}

			visited.Add (curr);
			prev = curr;
			curr = next;
		}

		List <int> alreadyMoved = new List <int> ();
		foreach (int i in toChange) {
			if (!alreadyMoved.Contains (i)) {
				vertices2 [i] += vals [0];
				alreadyMoved.Add (i);
			}
		}

		mesh.vertices = vertices2;
	}

	//Simply resizes the extruded face about the center of geometry. Takes two parameters: 1 = offset, 2 = transform (x, y, z)
	public static void ExtrudeResize (List <int> toChange, Mesh mesh, Vector3 [] vals) {
		List <int> alreadyExtruded = new List <int> ();
		Vector3 [] vertices = mesh.vertices;
		Vector3 center = Vector3.zero; //center of geometry
		foreach (int i in toChange) center += vertices [i] / toChange.Count;

		foreach (int i in toChange) {
			if (!alreadyExtruded.Contains (i)) {
				//resize about center
				Vector3 offset = vertices [i] - center;
				offset.x *= vals [1].x;
				offset.y *= vals [1].y;
				offset.z *= vals [1].z;

				vertices [i] = center + offset;
				vertices [i] += vals [0];
				alreadyExtruded.Add (i);
			}
		}

		mesh.vertices = vertices;
	}
}
