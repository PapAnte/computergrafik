using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerlinTerrain : MonoBehaviour
{
    public float[,] thd;
    private int rac = 0;
    public float rnmt = 0.01f;
    public float multi = 0.01f;
    float perlinNoise = 0f;
    Terrain terrain;
    
    // Start is called before the first frame update
    void Start()
    {
        terrain = GetComponent<Terrain>();
        rac = terrain.terrainData.heightmapResolution;
        thd = new float[rac, rac];

        for (int i = 0; i < rac; i++)
        {
            for (int j = 0; j < rac; j++)
            {
                perlinNoise = Mathf.PerlinNoise(i * rnmt, j * rnmt);
                thd[i, j] = perlinNoise * multi;
            }
        }
        terrain.terrainData.SetHeights(0, 0, thd);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
