using UnityEngine;
using System.Collections;

public class ShaderTest : MonoBehaviour {
	public AudioClip clip;
	public AudioSource source;
	public SongGenre.Genre genre;

	public FogControl fg;
	public ShaderManager sm;
	public TreeGenerator tg;
	public TreeLeaves tl;
	public GameObject leaf;
	public CylBuildingMaker cbm;

	private SpectrumAnalyzer sa;

	// Use this for initialization
	void Start () {
		tl.tg = tg;
		tl.leaves = leaf;
		tg.pEvent += tl.MakeLikeATree;
		tg.Init ();
		StartCoroutine (tg.Grow ());

		cbm.BuildMe ();
		float [] data = new float [clip.samples * clip.channels];
		clip.GetData (data, 0);
		sa = new SpectrumAnalyzer (data, clip.length);
		sa.Run ();
		StartCoroutine ("GetThisParty");
	}
	
	IEnumerator GetThisParty () {
		while (!sa.done) yield return new WaitForSeconds (.1f);

		source.clip = clip;
		source.Play ();
		sm.Begin (genre, sa, null, fg);

		foreach (MeshRenderer mr in MainManager.meshManager) mr.enabled = true;
		yield return null;
	}
}
