using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

public class GrassScript : MonoBehaviour {
	//NOTE: more clumped grass is not denser in this model. It actually generates the same density, which means less grass overall.
	public static float grassHeight = 4f;
	public static float grassDensity = 2000; //how many times to check perlin noise values
	public static float grassClumping = .15f; //lower is more clumped, 1 is uniformly distributed
	static int additionalGrasses = 20; //number of grasses to randomly place around the initial grass piece
	static float radius = 4; //radius aroudn the initial grass to place them
	static float grassWidth = 4f;

	GameObject cube;
	Dictionary <Vector2, float> perlinNoise;
	float cubeSize;
	Vector2 position;

	List <Vector3> positions = new List <Vector3> ();
	List <int> triangles = new List <int> ();
	List <Vector3> vertices = new List <Vector3> ();
	List <Vector2> uvs = new List <Vector2> ();
	public bool done = false;

	public void Begin (GameObject c, Dictionary <Vector2, float> pn, float cs, Vector2 p) {
		cube = c;
		perlinNoise = pn;
		position = p;
		cubeSize = cs;
		Place ();
		Debug.Log ("grass");

		Thread t = new Thread (BuildMesh);
		t.Start ();
		StartCoroutine ("WaitDone");
	}

	//find where to put grass
	void Place () {
		for (int i = 0; i < grassDensity; i ++) {
			Vector2 v = new Vector2(Random.Range(-170,170),
			                        Random.Range(-170,170));
			v /= 17;

			float val = Interpolate (v) - Random.Range (0f, grassClumping);
			if (val < grassClumping){
				RaycastHit hit;
				if(Physics.Raycast(new Vector3((v.x)*cubeSize + (position.x-1/2)*20*cubeSize,500,
				                               ((v.y)*cubeSize + (position.y-1/2)*20*cubeSize))
				                   ,Vector3.down,out hit) && (hit.collider == cube.GetComponent<Collider>() as Collider)
				   && Vector3.Dot (hit.normal, Vector3.up) > .5f){
					positions.Add (hit.point + (grassHeight / 2) * Vector3.down);

					//make more grasses around the original hit point
					int additional = Mathf.RoundToInt (Mathf.Lerp (0, additionalGrasses, (grassClumping - val) / grassClumping));
					for (int j = 0; j < additional; j++) {
						Vector3 pos = hit.point + Vector3.Cross (radius * Random.insideUnitSphere, hit.normal);
						positions.Add (pos + (grassHeight / 2) * Vector3.down);
					}
				}
			}	
		}
	}

	void BuildMesh () {
		System.Random r = new System.Random ();
		double d = 360;

		foreach (Vector3 v in positions) {
			Vector3 height = Vector3.up * grassHeight;
			Vector3  width = Quaternion.Euler (new Vector3 (0, (float)(d * r.NextDouble ()), 0)) * Vector3.left * grassWidth;
			Vector3 v00 = v;
			Vector3 v01 = v00 + height;
			Vector3 v10 = v00 + width;
			Vector3 v11 = v00 + width + height;

			//make triangles on one side
			vertices.Add (v00);
			vertices.Add (v01);
			vertices.Add (v11);
			vertices.Add (v10);
			triangles.Add (vertices.Count - 4);
			triangles.Add (vertices.Count - 3);
			triangles.Add (vertices.Count - 2);
			triangles.Add (vertices.Count - 4);
			triangles.Add (vertices.Count - 2);
			triangles.Add (vertices.Count - 1);

			//make triangles on other side
			/*vertices.Add (v00);
			vertices.Add (v01);
			vertices.Add (v11);
			vertices.Add (v10);
			triangles.Add (vertices.Count - 1);
			triangles.Add (vertices.Count - 2);
			triangles.Add (vertices.Count - 3);
			triangles.Add (vertices.Count - 1);
			triangles.Add (vertices.Count - 3);
			triangles.Add (vertices.Count - 4);

			uvs.Add (Vector2.zero);
			uvs.Add (Vector2.up);
			uvs.Add (Vector2.one);
			uvs.Add (Vector2.right);*/ //doesn't need other side for shader
			uvs.Add (Vector2.zero);
			uvs.Add (Vector2.up);
			uvs.Add (Vector2.one);
			uvs.Add (Vector2.right);
		}

		done = true;
	}

	//interpolate between the 4 closest points in the perlin noise grid for
	//arbitrary vector v
	float Interpolate (Vector2 v) {
		Vector2 v00 = new Vector2 (Mathf.Floor (v.x), Mathf.Floor (v.y));
		Vector2 v10 = v00 + Vector2.right;
		Vector2 v01 = v00 + Vector2.up;
		Vector2 v11 = v00 + Vector2.right + Vector2.up;

		return (v.x - v00.x) * ((v.y - v00.y) * perlinNoise [v00] + (1 - v.y + v00.y) * perlinNoise [v01])
						+ (1 - v.x + v00.x) * ((v.y - v00.y) * perlinNoise [v10] + (1 - v.y + v00.y) * perlinNoise [v11]);
	}

	IEnumerator WaitDone () {
		while (!done) yield return new WaitForSeconds (.2f);

		gameObject.transform.position = Vector3.zero;
		MeshFilter mf = gameObject.GetComponent <MeshFilter> () as MeshFilter;
		mf.mesh = new Mesh ();
		Mesh mesh = mf.mesh;
		
		mesh.vertices = vertices.ToArray ();
		mesh.triangles = triangles.ToArray ();
		mesh.uv = uvs.ToArray ();
		mesh.RecalculateBounds ();
		mesh.RecalculateNormals ();
		Debug.Log ("MESH " + mesh.vertices.Length);
	}
}
