using UnityEngine;
using System.IO;

public class MoistureMap : MonoBehaviour
{
    // private attributes
    private int width = 0;
    private int height = 0;
    private float scale = 20f;

    // Pathlocations
    string heightmapPath = ".//Assets//Scripts//image//heightmap.png";
    string moisturemapPath = ".//Assets//Scripts//image//moisturemap.png";

    // Start is called before the first frame update
    void Start()
    {
        GetHeightmap();
        Renderer renderer = GetComponent<Renderer>();
        renderer.material.mainTexture = GenerateMoisture();
    }

    void GetHeightmap()
    {
        Texture2D heightmap = new Texture2D(1, 1);
        byte[] tmpBytes = File.ReadAllBytes(this.heightmapPath);
        heightmap.LoadImage(tmpBytes);
        this.width = heightmap.width;
        this.height = heightmap.height;
    }

    Texture2D GenerateMoisture()
    {
        Texture2D texture = new Texture2D(width, height, TextureFormat.ARGB32, false);
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Color color = CalculateColor(x, y);
                texture.SetPixel(x, y, color);
            }
        }

        texture.Apply();
        GetComponent<Renderer>().material.shader = Shader.Find("Diffuse");
        GetComponent<Renderer>().material.mainTexture = texture;

        // generate image from Texture
        string _fullpath = this.moisturemapPath;
        byte[] _bytes = texture.EncodeToPNG();
        System.IO.File.WriteAllBytes(_fullpath, _bytes);
        Debug.Log(_bytes.Length / 1024 + "Kb was saved as: " + _fullpath);

        return texture;
    }

    Color CalculateColor (int x, int y)
    {
        float xCoord = (float)x / width * scale;
        float yCoord = (float)y / height * scale;
        
        float perlin = Mathf.PerlinNoise(xCoord, yCoord);
        return new Color(perlin, perlin, perlin);
    }
}