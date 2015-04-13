using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TriVert {

	public List <int> triangles = new List<int> ();
	public List<Vector3> vertices = new List<Vector3> ();

	public TriVert(List <int> triangles,List<Vector3> vertices){
		this.triangles = triangles;
		this.vertices = vertices;
	}

}
