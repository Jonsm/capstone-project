using UnityEngine;
using System.Collections;

public class CountTris {
	public TriVert[] tri_vert;
	public int count;

	public CountTris(TriVert[] tris, int count){
		this.tri_vert = tris;
		this.count = count;
	}
}
