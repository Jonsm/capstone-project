using UnityEngine;
using System.Collections;
using Utility;

public class TerrainManager : MonoBehaviour {
	
	public GameObject playerGameObject;
	public Terrain referenceTerrain;
	public int TERRAIN_BUFFER_COUNT = 50;
	public int spread = 1;


	//The higher the numbers, the more hills/mountains there are
	public float numMtns;
	//The lower the numbers in the number range, the higher the hills/mountains will be...
	public float mtnHeight;

	private float HM;
	private float divRange;
	private int[] currentTerrainID;
	private Terrain[] terrainBuffer;
	private DoubleKeyDictionary<int, int, int> terrainUsage;
	private DoubleKeyDictionary<int, int, TerrainData> terrainUsageData;
	private BitArray usedTiles;
	private BitArray touchedTiles;
	private Vector3 referencePosition;
	private Vector2 referenceSize;
	private Quaternion referenceRotation;
	
	// Use this for initialization
	void Start () {
		currentTerrainID = new int[2];
		terrainBuffer = new Terrain[TERRAIN_BUFFER_COUNT];
		terrainUsage = new DoubleKeyDictionary<int, int, int>();
		terrainUsageData = new DoubleKeyDictionary<int, int, TerrainData>();
		usedTiles = new BitArray(TERRAIN_BUFFER_COUNT, false);
		touchedTiles = new BitArray(TERRAIN_BUFFER_COUNT, false);
		
		referencePosition = referenceTerrain.transform.position;
		referenceRotation = referenceTerrain.transform.rotation;
		referenceSize = new Vector2(referenceTerrain.terrainData.size.x, referenceTerrain.terrainData.size.z);

		HM = numMtns;//Random.Range (3, numMtns);
		divRange = mtnHeight; //Random.Range(3,mtnHeight);

		for(int i=0; i<TERRAIN_BUFFER_COUNT; i++)
		{
			TerrainData tData = new TerrainData();
			CopyTerrainDataFromTo(referenceTerrain.terrainData, ref tData);
			terrainBuffer[i] = Terrain.CreateTerrainGameObject(tData).GetComponent<Terrain>();
			terrainBuffer[i].gameObject.active = false;
		}
	}
	
	// Update is called once per frame
	void Update () {
		ResetTouch();
		Vector3 warpPosition = playerGameObject.transform.position;
		TerrainIDFromPosition(ref currentTerrainID, ref warpPosition);
		
		string dbgString = "";
		dbgString = "CurrentID : " + currentTerrainID[0] + ", " + currentTerrainID[1] + "\n\n";
		for(int i=-spread;i<=spread;i++)
		{
			for(int j=-spread;j<=spread;j++)
			{	
				DropTerrainAt(currentTerrainID[0] + i, currentTerrainID[1] + j);
				dbgString += (currentTerrainID[0] + i) + "," + (currentTerrainID[1] + j) + "\n";
			}
		}
		Debug.Log(dbgString);
		ReclaimTiles();
	}
	
	void TerrainIDFromPosition(ref int[] currentTerrainID, ref Vector3 position)
	{
		currentTerrainID[0] = Mathf.RoundToInt((position.x - referencePosition.x )/ referenceSize.x);
		currentTerrainID[1] = Mathf.RoundToInt((position.z - referencePosition.z )/ referenceSize.y);
	}
	
	void DropTerrainAt(int i, int j)
	{
		// Check if terrain exists, if it does, activate it.
		if(terrainUsage.ContainsKey(i, j) && terrainUsage[i,j] != -1)
		{
			// Tile mapped, use it.
		}
		// If terrain doesn't exist, drop it.
		else
		{
			terrainUsage[i,j] = FindNextAvailableTerrainID();
			if(terrainUsage[i,j] == -1) Debug.LogError("No more tiles, failing...");
		}
		if(terrainUsageData.ContainsKey(i,j))
		{
			// Restore the data for this tile
		}
		else
		{
			// Create a new data object
			terrainUsageData[i,j] = CreateNewTerrainData();
		}

		ActivateUsedTile(i, j);
		usedTiles[terrainUsage[i,j]] = true;
		touchedTiles[terrainUsage[i,j]] = true;
	}
	
	TerrainData CreateNewTerrainData()
	{
		TerrainData tData = new TerrainData();
		CopyTerrainDataFromTo(referenceTerrain.terrainData, ref tData);

		float[,] hts = new float[referenceTerrain.terrainData.heightmapWidth, referenceTerrain.terrainData.heightmapHeight];
		for (int i = 0; i < referenceTerrain.terrainData.heightmapWidth; i++)
		{
			for (int k = 0; k < referenceTerrain.terrainData.heightmapHeight; k++)
			{
				hts[i, k] = Mathf.PerlinNoise(((float)i / (float)referenceTerrain.terrainData.heightmapWidth) * HM, ((float)k / (float)referenceTerrain.terrainData.heightmapHeight) * HM)/ divRange;
			}
		}
		Debug.LogWarning("DivRange: " + divRange + " , " + "HTiling: " + HM);
		tData.SetHeights (0, 0, hts);
		return tData;
	}

	TerrainData RandomTerrain()
	{

		return null;
	}
	
	void ResetTouch()
	{
		touchedTiles.SetAll(false);
	}
	
	int CountOnes(BitArray arr)
	{
		int count = 0;
		for(int i=0;i<arr.Length;i++)
		{
			if(arr[i])
				count++;
		}
		return count;
	}
	
	void ReclaimTiles()
	{
		if(CountOnes(usedTiles) > ((spread*2 + 1)*(spread*2 + 1)))
		{
			for(int i=0;i<usedTiles.Length;i++)
			{
				if(usedTiles[i] && !touchedTiles[i])
				{
					usedTiles[i] = false;
					terrainBuffer[i].gameObject.active = false;
				}
			}
		}
	}
	
	void ActivateUsedTile(int i, int j)
	{
		terrainBuffer[terrainUsage[i, j]].gameObject.transform.position = 
									new Vector3(  	referencePosition.x + i * referenceSize.x,
													referencePosition.y,
													referencePosition.z + j * referenceSize.y);
		terrainBuffer[terrainUsage[i, j]].gameObject.transform.rotation = referenceRotation;
		terrainBuffer[terrainUsage[i, j]].gameObject.active = true;
		
		terrainBuffer[terrainUsage[i, j]].terrainData = terrainUsageData[i, j];
	}
	
	int FindNextAvailableTerrainID()
	{
		for(int i=0;i<usedTiles.Length;i++)
			if(!usedTiles[i]) return i;
		return -1;	
	}	
	
	void CopyTerrainDataFromTo(TerrainData tDataFrom, ref TerrainData tDataTo)
	{
		tDataTo.SetDetailResolution(tDataFrom.detailResolution, 8);
		tDataTo.heightmapResolution = tDataFrom.heightmapResolution;
		tDataTo.alphamapResolution = tDataFrom.alphamapResolution;
		tDataTo.baseMapResolution = tDataFrom.baseMapResolution;
		tDataTo.size = tDataFrom.size;
		tDataTo.splatPrototypes = tDataFrom.splatPrototypes;
	}
}
