using UnityEngine;
using System.Collections;

public class BuildingManager : MonoBehaviour {
	// Use this for initialization
	public GameObject building;
	void Start () {
		CylBuildingMaker cbm = building.GetComponent <CylBuildingMaker> () as CylBuildingMaker;
		cbm.BuildMe ();
	}
}
