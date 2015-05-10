using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CoherentNoise;
using CoherentNoise.Generation;
using CoherentNoise.Generation.Fractal;
using CoherentNoise.Generation.Combination;
using CoherentNoise.Generation.Voronoi;

//This actually makes the cube objects
public class MarchingCubes : MonoBehaviour {

	private List<int> triangle = new List<int> ();
	private List<Vector3> vertice = new List<Vector3> ();
	// Use this for initialization
	public void Go (List<int> triangles,List<Vector3> vertices) {

		gameObject.GetComponent<MeshRenderer> ().enabled = false;
		MainManager.meshManager.Add (gameObject.GetComponent<MeshRenderer> ());
		Mesh mesh = gameObject.GetComponent <MeshFilter> ().mesh;
		MeshCollider collide = gameObject.GetComponent<MeshCollider> ();
		triangle = triangles;
		vertice = vertices;
		mesh.vertices = vertice.ToArray ();
		mesh.triangles = triangle.ToArray ();
		mesh.RecalculateBounds ();
		mesh.RecalculateNormals ();
		collide.sharedMesh = mesh;	
	}

}
