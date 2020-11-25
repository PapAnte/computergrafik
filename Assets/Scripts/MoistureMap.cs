using UnityEngine;

public class MoistureMap : MonoBehaviour
{
    public int width = 256;
    public int height = 256;
    public float scale = 20f;

    // Start is called before the first frame update
    void Update()
    {
        Renderer renderer = GetComponent<Renderer>();
        renderer.material.mainTexture = GenerateTexture();
    }

    Texture2D GenerateTexture()
    {
        Texture2D texture = new Texture2D(width, height);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Color color = CalculateColor(x, y);
                texture.SetPixel(x, y, color);
            }
        }

        texture.Apply();
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