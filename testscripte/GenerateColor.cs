﻿using UnityEngine;
using System.IO;

public class GenerateColor : MonoBehaviour
{
    // private attributes
    private float[,] heightMap;
    private float[,] moistureMap;
    private float[,] colorMap;
    private int width;
    private int height;
    private float scale = 20f;

    // Pathlocations
    string heightmapPath = ".//Assets//Scripts//image//heightmap.png";
    string moisturemapPath = ".//Assets//Scripts//image//moisturemap.png";
    string colorLandPath = ".//Assets//Scripts//image//colormap_land.png";
    string colorWaterPath = ".//Assets//Scripts//image//colormap_water.png";

    // Start is called before the first frame update
    void Start()
    {
        bool check = true;
        do
        {
            check = GetArrayHeightMap();
        } while (check != true);
        GenerateMoisture();
        do
        {
            check = GetArrayMoistureMap();
        } while (check != true);
        Renderer renderer = GetComponent<Renderer>();
        renderer.material.mainTexture = CalculateColor();
    }

    bool GetArrayHeightMap()
    {
        Texture2D heightmap = new Texture2D(1, 1);
        try
        {
            byte[] tmpBytes = File.ReadAllBytes(this.heightmapPath);
            heightmap.LoadImage(tmpBytes);
            this.width = heightmap.width;
            this.height = heightmap.height;
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
        catch (FileNotFoundException)
        {
            Debug.Log("File HeightMapMap.png not found!");
            return false;
        }
        return true;
    }

    Texture2D GenerateMoisture()
    {
        Texture2D texture = new Texture2D(width, height, TextureFormat.ARGB32, false);
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Color color = CalculateColorMoisture(x, y);
                texture.SetPixel(x, y, color);
            }
        }

        texture.Apply();

        // generate image from Texture
        string _fullpath = this.moisturemapPath;
        byte[] _bytes = texture.EncodeToPNG();
        System.IO.File.WriteAllBytes(_fullpath, _bytes);
        Debug.Log(_bytes.Length / 1024 + "Kb was saved as: " + _fullpath);

        return texture;
    }

    Color CalculateColorMoisture(int x, int y)
    {
        float xCoord = (float)x / width * scale;
        float yCoord = (float)y / height * scale;

        float perlin = Mathf.PerlinNoise(xCoord, yCoord);
        return new Color(perlin, perlin, perlin);
    }

    bool GetArrayMoistureMap()
    {
        Texture2D moisturemap = new Texture2D(1, 1);
        try
        {
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
        catch (FileNotFoundException)
        {
            Debug.Log("File MoistureMap.png not found!");
            return false;
        }
        return true;
    }

    Texture2D CalculateColor()
    {
        Texture2D texture = new Texture2D(width, height, TextureFormat.ARGB32, false);
        Texture2D colormap_land = new Texture2D(1, 1);
        byte[] tmpBytes = File.ReadAllBytes(this.colorLandPath);
        colormap_land.LoadImage(tmpBytes);

        Texture2D colormap_water = new Texture2D(1, 1);
        tmpBytes = File.ReadAllBytes(this.colorWaterPath);
        colormap_water.LoadImage(tmpBytes);

        for (int i = 0; i < this.width; i++)
        {
            for (int j = 0; j < this.height; j++)
            {
                float x = moistureMap[i, j];
                float y = heightMap[i, j];

                if (y <= 0.6)
                {
                    x = (((1 - x) * 100) * colormap_land.width) / 100;
                    y = ((((float)0.6 - (y - (float)0.4)) * 100) * colormap_land.height) / 100;
                    Color color = colormap_land.GetPixel((int)x, (int)y);
                    texture.SetPixel(i, j, color);
                }
                else
                {
                    x = (((1 - x) * 100) * colormap_water.width) / 100;
                    y = ((((float)0.4 - y) * 100) * colormap_water.height) / 100;
                    Color color = colormap_water.GetPixel((int)x, (int)y);
                    texture.SetPixel(i, j, color);
                }
            }
        }

        texture.Apply();
        return texture;
    }
}