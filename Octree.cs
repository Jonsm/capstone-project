using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//This class is for (somewhat) efficiently storing branch vectors for collision detection
//Not actually an octree, should change the name
public class Octree {
	class VectorSet {
		public Vector3 pos;
		public Vector3 dir;
		public float rad; //8D

		public VectorSet (Vector3 p_in, Vector3 d_in, float rad_in) {
			pos = p_in;
			dir = d_in;
			rad = rad_in;
		}
	}

	List <VectorSet> [] tree; //where the vecotrs are stored
	//float topSize; //size of the largest cube
	Vector3 startPoint; //center of the largest cube
	float minSize; //size of smallest cube
	int dim;

	public Octree (float size, float resolution, Vector3 center_in) {
		startPoint = center_in - new Vector3 (size, size, size) / 2;
		//topSize = size;
		minSize = size / (Mathf.Floor (size / resolution));
		dim = (int) (size / minSize);
		tree = new List <VectorSet> [(int) Mathf.Pow (dim, 3)]; 
	}

	//add a branch to the tree
	public bool Put (Vector3 pos, Vector3 dir, float rad) {
		VectorSet v = new VectorSet (pos, dir, rad);


		return true;
	}

	//find the index where a vector should go
	private int GetIndex (float size, Vector3 pos, int curr) {
		pos -= startPoint;
		return (int) (pos.x / minSize);
	}
}
