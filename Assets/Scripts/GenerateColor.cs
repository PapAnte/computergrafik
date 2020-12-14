using UnityEngine;
using System.IO;

public class GenerateColor : MonoBehaviour
{
    // private attributes
    private float[,] heightMap;
    private float[,] moistureMap;
    private float[,] colorMap;

    // Pathlocations
    string heightmapPath = ".//Assets//Scripts//image//heightmap.png";
    string moisturemapPath = ".//Assets//Scripts//image//moisturemap.png";
    string colorPath = ".//Assets//Scripts//image//colormap.png";

    // Start is called before the first frame update
    void Start()
    {
        GetArrayMoistureMap();
        GetArrayHeightMap();
        //TODO
    }

    void GetArrayMoistureMap()
    {
        Texture2D moisturemap = new Texture2D(1, 1);
        byte[] tmpBytes = File.ReadAllBytes(this.moisturemapPath);
        moisturemap.LoadImage(tmpBytes);
        moistureMap = new float[moisturemap.width, moisturemap.height];
        for (int x = 0; x < moisturemap.width; x++)
        {
            for (int y = 0; y < moisturemap.height; y++)
            {
                Color color = moisturemap.GetPixel(x, y);
                moistureMap[x, y] = color.r;
            }
        }
    }

    void GetArrayHeightMap()
    {
        Texture2D heightmap = new Texture2D(1, 1);
        byte[] tmpBytes = File.ReadAllBytes(this.heightmapPath);
        heightmap.LoadImage(tmpBytes);
        heightMap = new float[heightmap.width, heightmap.height];
        for (int x = 0; x < heightmap.width; x++)
        {
            for (int y = 0; y < heightmap.height; y++)
            {
                Color color = heightmap.GetPixel(x, y);
                heightMap[x, y] = color.r;
            }
        }
    }
}