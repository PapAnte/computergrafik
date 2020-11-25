using UnityEngine;

public class MoistureMap : MonoBehaviour
{
    public int width = 256;
    public int height = 256;
    public float scale = 20f;

    // Start is called before the first frame update
    void Start()
    {
        Renderer renderer = GetComponent<Renderer>();
        renderer.material.mainTexture = GenerateTexture();
    }

    Texture2D GenerateTexture()
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
        string _fullpath = ".//Assets//Scripts//image//image.png";
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
        if (perlin >= 0 && perlin < 0.1)
        {
            return new Color(0, 0, 139);
        }
        if (perlin >= 0.1 && perlin < 0.2)
        {
            return new Color(0, 0, 205);
        }
        if (perlin >= 0.2 && perlin < 0.3)
        {
            return new Color(0, 0, 238);
        }
        if (perlin >= 0.3 && perlin < 0.4)
        {
            return new Color(0, 0, 255);
        }
        if (perlin >= 0.4 && perlin < 0.5)
        {
            return new Color(30, 144, 255);
        }
        if (perlin >= 0.5 && perlin < 0.6)
        {
            return new Color(255, 255, 224);
        }
        if (perlin >= 0.6 && perlin < 0.7)
        {
            return new Color(0, 255, 0);
        }
        if (perlin >= 0.7 && perlin < 0.8)
        {
            return new Color(0, 238, 0);
        }
        if (perlin >= 0.8 && perlin < 0.9)
        {
            return new Color(0, 205, 0);
        }
        if (perlin >= 0.9 && perlin <= 1.0)
        {
            return new Color(0, 139, 0);
        }
        return new Color(139, 129, 076);
    }
}