using UnityEngine;
using System.Collections;

public class BuildingManager : MonoBehaviour {
	// Use this for initialization
	void Start () {
		CylBuildingMaker cbm = gameObject.GetComponent <CylBuildingMaker> () as CylBuildingMaker;
		cbm.BuildMe ();
	}
}
