using UnityEngine;
using System.IO;

public class GenerateMaps : MonoBehaviour
{
    //Auflösung der Fläche in Pixel
    //x_Component = Anzahl der horizontalen Pixel 
    public int x_Component;
    int x_original;

    //y_Component = Anzahl der senkrechten Pixel
    public int y_Component;
    int y_original;

    //Array für die Matrix
    float[,] mPixel;
    int mPixelCount;

    //Höhengrenze
    public int UPPER_BOUND = 1;

    //Eckpunkte des Quadrats mit Höheninformation
    float topLeft;
    float botLeft;
    float topRight;
    float botRight;
    float mid;

    //
    private int width;
    private int height;
    private float scale = 20f;

    // Pathlocations
    string heightmapPath = ".//Assets//Scripts//image//heightmap.png";
    string moisturemapPath = ".//Assets//Scripts//image//moisturemap.png";
    string colorMapPath = ".//Assets//Scripts//image//colormap.png";

    // Start is called before the first frame update
    void Start()
    {
        x_original = x_Component;
        y_original = y_Component;

        // Suche das großte Quadrat aus dem Rechteck,
        // welches die Bedingung 2^n + 1 erfüllt.
        // if Y > X then tausche die Werte

        if (y_Component > x_Component)
        {
            x_Component = y_original;
            y_Component = x_original;
        }

        while ((x_Component & (x_Component - 1)) != 0)
        {
            x_Component += 1;
        }

        // Größe des zu berechneden größten Quadrats
        // y_Component wird gleich x_Component gesetzt
        x_Component += 1;
        y_Component = x_Component;

        //Debug.Log("Folgendes Quadrat wird berechnet: x = " + x_Component + "y = " + y_Component);

        //Bsp: Array mPixel[65,65]
        mPixel = new float[x_Component, y_Component];

        // Höhenkalkulation für die Eckpunkte
        //TopLeft = mPixel[0,0]
        mPixel[0, 0] = Random.Range(-UPPER_BOUND, UPPER_BOUND);
        //TopRight = mPixel[0, 64]
        mPixel[0, (x_Component - 1)] = Random.Range(-UPPER_BOUND, UPPER_BOUND);
        //BotLeft = mPixel[64, 0]
        mPixel[(y_Component - 1), 0] = Random.Range(-UPPER_BOUND, UPPER_BOUND);
        //BotRight = mPixel[64, 64]
        mPixel[(y_Component - 1), (x_Component - 1)] = Random.Range(-UPPER_BOUND, UPPER_BOUND);

        calculate_biggest_Quad();

        Renderer rendererheight = GetComponent<Renderer>();
        rendererheight.material.SetTexture("_HeightMap", CreateTexture());
        Renderer renderermoisture = GetComponent<Renderer>();
        renderermoisture.material.SetTexture("_MoistureMap", GenerateMoisture());
        Renderer renderercolormap = GetComponent<Renderer>();
        renderercolormap.material.SetTexture("_ColorMap", GetColorMap());      
    }

    void Update()
    {
        //Renderer renderer = GetComponent<Renderer>();
        //renderer.material.mainTexture = CalculateColor();
    }

    void calculate_biggest_Quad()
    {

        //Anzahl der Divisions
        int mDivisions = x_Component - 1;

        //start diamond square algo

        // Anzahl der Durchläufe. Bsp: 4x4 Matrix, mDivisions = 4, iterations = logarithmus 4 base 2 = 2.0
        //D.h. es müssen 2x der Diamond Step und 2x der Square Step ausgeführt werden damit alle Vertices berechnet wurden.
        int iterations = (int)Mathf.Log(mDivisions, 2);

        // Anzahl der Vierecke. Zuerst gibt es 1 Viereck
        // Danach gibt es 2 Vierecke pro Zeile, danach 4 Vierecke pro Zeile usw.
        int numSquares = 1;

        //Größe der Vierecke. Zuerst ist das Viereck (mDivisions * mDivisions)
        int squareSize = mDivisions;

        float smothness = (float)UPPER_BOUND;

        //Schleife wie oft der DiamondSquare ausgeführt werden muss.
        for (int i = 0; i < iterations; i++)
        {
            int row = 0;

            //Schleife für das Iterrieren der Zeilen.
            for (int j = 0; j < numSquares; j++)
            {
                int col = 0;

                //Schleife für das Iterrieren der Splaten.
                for (int k = 0; k < numSquares; k++)
                {
                    //Hier könnte noch eine Änderung erfolgen! Zuerst werden alle Diamondsteps
                    //und deren mid-Werte berechnet, damit im SquareSteps die Randwerte mit 4 Werten
                    //anstatt nur 3 Werten berechnet werden können.
                    diamondStep(row, col, squareSize, smothness);
                    squareStep(row, col, squareSize, smothness);

                    //Das nächste Viereck von links nach rechts wird berechnet
                    col += squareSize;
                }

                //Das nächste Viereck von oben nach unten wird berechnet 
                row += squareSize;
            }

            //Anzahl der Vierecke wird verdoppelt
            numSquares *= 2;

            //Größe der Vierecke wird halbiert
            squareSize /= 2;

            //Höhenwert, der zur smothness der Heightmap verantwortlich ist wird * 0.5 genommen.
            //Kann angepasst werden.
            smothness *= 0.5f;
        }
    }

    //Diamond Step wird durchgeführt
    //Parameter:
    //row: Gibt die Zeile an
    //col: Gibt die Spalte an
    //size: Gibt die Seitenlänge des Vierecks an
    //offset: Gibt den Höhenwert an
    void diamondStep(int row, int col, int size, float offset)
    {
        topLeft = mPixel[row, col];
        botLeft = mPixel[(row + size), col];
        topRight = mPixel[row, (col + size)];
        botRight = mPixel[(row + size), (col + size)];

        //Debug.Log("row dia = " + row);
        //Debug.Log("col dia = " + col);
        //Debug.Log("size dia = " + size);

        int mid_x = ((int)(size * 0.5f) + row);
        int mid_y = ((int)(size * 0.5f) + col);

        //Debug.Log("mid_x = " + mid_x);
        //Debug.Log("mid_y = " + mid_y);

        mPixel[mid_x, mid_y] = ((topLeft + topRight + botLeft + botLeft) * 0.25f + Random.Range(-offset, offset));

        mid = mPixel[mid_x, mid_y];

    }

    //Square Step wird durchgeführt
    //Parameter:
    //row: Gibt die Zeile an
    //col: Gibt die Spalte an
    //size: Gibt die Seitenlänge des Vierecks an
    //offset: Gibt den Höhenwert an
    void squareStep(int row, int col, int size, float offset)
    {
        int halfSize = (int)(size * 0.5f);

        //Debug.Log("halfSize = " + halfSize);

        //Debug.Log("row square = " + row);
        //Debug.Log("col square = " + col);
        //Debug.Log("size square = " + size);

        //Höhenberechnung für die Vertices links und oberhalb des Mittelpunktes eines Vierecks

        //Pixel[oben]
        mPixel[row, (col + halfSize)] = ((topLeft + topRight + mid) / 3 + Random.Range(-offset, offset));
        //Debug.Log("Pixel oben wäre [" + row + "][" + (col + halfSize) + "]");

        //Pixel[links]
        mPixel[(row + halfSize), col] = ((topLeft + botLeft + mid) / 3 + Random.Range(-offset, offset));
        //Debug.Log("Pixel links wäre [" + (row + halfSize) + "][" + col + "]");

        //Werden die Vertices des letzte Vierecks einer Zeile berechnet muss der Vertex rechts des Mittelpunktes
        //auch berechnet werden.
        if ((col > ((y_Component - 1) / size)) && (row <= ((x_Component - 1) / size)))
        {
            //Pixel[rechts]
            mPixel[(row + halfSize), (col + size)] = ((topRight + botRight + mid) / 3 + Random.Range(-offset, offset));

            //Debug.Log("Pixel rechts wäre [" + (row + halfSize) + "][" + (col + size) + "]");
        }

        //Werden die Vertices des letzten Vierecks einer Splate berechnet muss der Vertex unter dem Mittelpunkt
        //auch berechnet werden.
        else if ((row > ((x_Component - 1) / size)) && (col <= ((y_Component - 1) / size)))
        {
            //Pixel[unten]
            mPixel[(row + size), (col + halfSize)] = ((botLeft + botRight + mid) / 3 + Random.Range(-offset, offset));
            //Debug.Log("Pixel unten wäre [" + (row + size) + "][" + (col + halfSize) + "]");

        }

        //Werden die Vertices des letzten Vierecks einer Splate und Zeile berechnet müssen auch die Vertices
        // rechts und unterhalb des Mittelpunktes berechnet werden.
        else if ((row > ((x_Component - 1) / size)) && (col > ((y_Component - 1) / size)) || size == (int)(y_Component - 1))
        {
            //Pixel[unten]
            mPixel[(row + size), (col + halfSize)] = ((botLeft + botRight + mid) / 3 + Random.Range(-offset, offset));
            //Pixel[rechts]
            mPixel[(row + halfSize), (col + size)] = ((topRight + botRight + mid) / 3 + Random.Range(-offset, offset));

            //Debug.Log("Pixel rechts wäre [" + (row + halfSize) + "][" + (col + size) + "]");
            //Debug.Log("Pixel unten wäre [" + (row + size) + "][" + (col + halfSize) + "]");
        }
    }

    //Die Funktion MinMaxHeight findet die Minimale und Maximale berechnete Höhe aller Vertices
    //Output: float array
    //Output-Parameter: float[0] = minimum
    //Output-Parameter: float[1] = maximum
    float[] MinMaxHeight()
    {
        float min = mPixel[0, 0];
        float max = mPixel[0, 0];

        for (int x = 0; x < x_Component; x++)
        {
            for (int y = 0; y < y_Component; y++)
            {
                min = System.Math.Min(mPixel[x, y], min);
                max = System.Math.Max(mPixel[x, y], max);
            }
        }
        return new float[2] { min, max };
    }

    Texture2D CreateTexture()
    {

        // Create a new texture ARGB32 (32 bit with alpha) and no mipmaps
        var texture = new Texture2D(y_original, x_original, TextureFormat.ARGB32, false);

        Color color;

        //Maximum und Minimum Höhe wird gespeichert 
        float[] _MinMaxHeight = MinMaxHeight();

        //totaler_abstand enthält die Entfernung zwischen Max und Min.
        float totaler_abstand = _MinMaxHeight[1] - _MinMaxHeight[0];

        //Debug.Log("totaler_Abstand = " + totaler_abstand);

        // pixel farben setzen für jeden Vertice in einer Spalte; das ganze wird Reihe für Reihe durchlaufen
        for (int i = 0; i < y_original; i++)
        {
            for (int j = 0; j < x_original; j++)
            {
                float ac_height = mPixel[i, j];

                //Die Höhenwerte aller Pixel/Vertices werden zwischen 0 und 1 normiert!
                mPixel[i, j] = ((ac_height - _MinMaxHeight[0]) / totaler_abstand);

                //Der Höhenwert soll sich zwischen 0 und 1 bewegen, das heißt eine 1 ist der höchste Punkt und damit weiß
                //weiß ist im RGB code (255, 255, 255) und schwarz (0, 0, 0); damit kann ich jede Höhe, welche in 
                //y gespeichert ist, mit 255 multiplizieren und erhalte somit eine Grauabstufung von schwarz nach weiß
                // texture.SetPixel(0, 0, Color(1.0, 1.0, 1.0, 0.5));
                color = new Color32((byte)(mPixel[i, j] * 255), (byte)(mPixel[i, j] * 255), (byte)(mPixel[i, j] * 255), 255);
                texture.SetPixel(i, j, color);
                //Debug.Log("Check for: " + i + " " + j + " " + color);
            }
        }

        // definierte Pixel anwenden
        texture.Apply();

        // connect texture to material of GameObject this script is attached to
        GetComponent<MeshRenderer>().material.mainTexture = texture;
        //GetComponent<Renderer>().material.SetTexture("_NewTexture", texture);

        // ------------------------------------------------------------------------------------------
        string _fullpath = heightmapPath;
        byte[] _bytes = texture.EncodeToPNG();
        System.IO.File.WriteAllBytes(_fullpath, _bytes);
        Debug.Log(_bytes.Length / 1024 + "Kb was saved as: " + _fullpath);
        // ------------------------------------------------------------------------------------------ /
        return texture;   
    }

    // 
    Texture2D GenerateMoisture()
    {
        try
        {
            Texture2D heightmap = new Texture2D(1, 1);
            byte[] tmpBytes = File.ReadAllBytes(this.heightmapPath);
            heightmap.LoadImage(tmpBytes);
            this.width = heightmap.width;
            this.height = heightmap.height;
        }
        catch (FileNotFoundException)
        {
            Debug.Log("File HeightMapMap.png not found!");
        }
        Texture2D texture = new Texture2D(width, height, TextureFormat.ARGB32, false);
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Color color = CalculateColorMoisture(x, y);
                texture.SetPixel(x, y, color);
            }
        }
        // Apply the changes to the texture and upload the updated texture to the GPU
        texture.Apply();

        // generate image from Texture
        // ------------------------------------------------------------------------------------------
        string _fullpath = this.moisturemapPath;
        byte[] _bytes = texture.EncodeToPNG();
        System.IO.File.WriteAllBytes(_fullpath, _bytes);
        Debug.Log(_bytes.Length / 1024 + "Kb was saved as: " + _fullpath);
        // ------------------------------------------------------------------------------------------ /
        return texture;
    }

    // 
    Color CalculateColorMoisture(int x, int y)
    {
        float xCoord = (float)x / width * scale;
        float yCoord = (float)y / height * scale;

        float perlin = Mathf.PerlinNoise(xCoord, yCoord);
        return new Color(perlin, perlin, perlin);
    }

    // 
    Texture2D GetColorMap()
    {
        Texture2D colormap = new Texture2D(1, 1);
        byte[] tmpBytes = File.ReadAllBytes(this.colorMapPath);
        colormap.LoadImage(tmpBytes);

        return colormap;
    }
}