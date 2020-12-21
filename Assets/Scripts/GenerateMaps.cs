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
    int Zaehler = 0;
    float[] topLeft;
    float[] botLeft;
    float[] topRight;
    float[] botRight;
    float[] mid;

    //
    private int width;
    private int height;
    private float scale = 20f;

    public Material skyboxMaterial;

    // Pathlocations
    string heightmapPath = ".//Assets//Scripts//image//heightmap.png";
    string moisturemapPath = ".//Assets//Scripts//image//moisturemap.png";
    string colorMapLandPath = ".//Assets//Scripts//image//colormap_land_1.png";
    string colorMapWaterPath = ".//Assets//Scripts//image//colormap_water.png";
    string colorNormalMap1Path = ".//Assets//Scripts//image//normal_map_1.png";
    string colorNormalMap2Path = ".//Assets//Scripts//image//normal_map_2.png";

    string skyboxTexturePath = ".//Assets//Scripts//image//universeStarWars.png";

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

        Renderer renderer = GetComponent<Renderer>();
        renderer.material.SetTexture("_HeightMap", CreateTexture());
        renderer.material.SetTexture("_MoistureMap", GenerateMoisture());
        renderer.material.SetTexture("_ColorMapLand", GetColorMapLand());
        renderer.material.SetTexture("_ColorMapWater", GetColorMapWater());
        renderer.material.SetTexture("_NormalMap1", GetNormalMap1());
        renderer.material.SetTexture("_NormalMap2", GetNormalMap2());

        // Material generieren und als Skybox-Material festlegen
        skyboxMaterial = new Material(Shader.Find("Skybox/Panoramic"));
        skyboxMaterial.mainTexture = GetSkyboxTexture();
        RenderSettings.skybox = skyboxMaterial;
    }

    void Update()
    {
        transform.Rotate(0, -0.04f, 0, Space.World);
    }

    void calculate_biggest_Quad()
    {
        int mDivisions = x_Component - 1;

        //start diamond square algo

        // Anzahl der Durchläufe. Bsp: 4x4 Matrix, 
        // mDivisions = 4, iterations = logarithmus 4 base 2 = 2.0
        // D.h. es müssen 2x der Diamond Step und 2x der Square Step 
        // ausgeführt werden damit alle Vertices berechnet wurden.
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
            int row_dia = 0;
            int row_square = 0;
            int anzahl = numSquares * numSquares;

            topLeft = new float[anzahl];
            topRight = new float[anzahl];
            botLeft = new float[anzahl];
            botRight = new float[anzahl];
            mid = new float[anzahl];

            //Schleife für das Iterrieren der Zeilen.
            for (int j = 0; j < numSquares; j++)
            {
                int col_dia = 0;

                //Schleife für das Iterrieren der Splaten des DiamondSteps
                for (int k = 0; k < numSquares; k++)
                {
                    diamondStep(row_dia, col_dia, squareSize, smothness);

                    //Das nächste Viereck von links nach rechts wird berechnet
                    col_dia += squareSize;
                }

                //Das nächste Viereck von oben nach unten wird berechnet 
                row_dia += squareSize;
            }

            Zaehler = 0;

            //Schleife für das Iterrieren der Zeilen.
            for (int j = 0; j < numSquares; j++)
            {
                int col_square = 0;

                //Schleife für das Iterrieren der Splaten des DiamondSteps
                for (int k = 0; k < numSquares; k++)
                {
                    squareStep(row_square, col_square, squareSize, smothness, numSquares);

                    //Das nächste Viereck von links nach rechts wird berechnet
                    col_square += squareSize;
                }

                //Das nächste Viereck von oben nach unten wird berechnet
                row_square += squareSize;
            }

            //Anzahl der Vierecke wird verdoppelt
            numSquares *= 2;

            //Größe der Vierecke wird halbiert
            squareSize /= 2;

            //Höhenwert, der zur smothness der Heightmap verantwortlich ist wird * 0.5 genommen.
            //Kann angepasst werden.
            smothness *= 0.5f;

            Zaehler = 0;
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

        //Debug.Log("Zähler Dia Step = " + Zaehler);

        topLeft[Zaehler] = mPixel[row, col];
        botLeft[Zaehler] = mPixel[(row + size), col];
        topRight[Zaehler] = mPixel[row, (col + size)];
        botRight[Zaehler] = mPixel[(row + size), (col + size)];

        int mid_x = ((int)(size * 0.5f) + row);
        int mid_y = ((int)(size * 0.5f) + col);

        mPixel[mid_x, mid_y] = ((topLeft[Zaehler] 
                                + topRight[Zaehler] 
                                + botLeft[Zaehler] 
                                + botLeft[Zaehler]) 
                                * 0.25f 
                                + Random.Range(-offset, offset));

        mid[Zaehler] = mPixel[mid_x, mid_y];

        Zaehler += 1;

    }

    //Square Step wird durchgeführt
    //Parameter:
    //row: Gibt die Zeile an
    //col: Gibt die Spalte an
    //size: Gibt die Seitenlänge des Vierecks an
    //offset: Gibt den Höhenwert an
    //numSquares: Gibt die Anzahl der Vierecke pro Zeile an
    void squareStep(int row, int col, int size, float offset, int numSquares)
    {
        int halfSize = (int)(size * 0.5f);

        //Höhenberechnung für die Vertices links, rechts unterhalb und oberhalb des Mittelpunktes eines Vierecks
        if (row == 0 && col == 0)
        {
            int mid_right = (Zaehler + (numSquares - 1));
            int mid_bottom = ((numSquares * numSquares) - numSquares + Zaehler);

            //Pixel[oben]
            mPixel[row, (col + halfSize)] = ((topLeft[Zaehler] 
                                            + topRight[Zaehler] 
                                            + mid[Zaehler] 
                                            + mid[mid_bottom]) 
                                            * 0.25f 
                                            + Random.Range(-offset, offset));

            //Pixel[links]
            mPixel[(row + halfSize), col] = ((topLeft[Zaehler] 
                                            + botLeft[Zaehler] 
                                            + mid[Zaehler] 
                                            + mid[mid_right]) 
                                            * 0.25f 
                                            + Random.Range(-offset, offset));

        }
        else if (row != 0 && col == 0)
        {
            int mid_right = (Zaehler + (numSquares - 1));
            int mid_up = (Zaehler - numSquares);

            //Pixel[oben]
            mPixel[row, (col + halfSize)] = ((topLeft[Zaehler] 
                                            + topRight[Zaehler] + mid[Zaehler] 
                                            + mid[mid_up]) 
                                            * 0.25f 
                                            + Random.Range(-offset, offset));

            //Pixel[links]
            mPixel[(row + halfSize), col] = ((topLeft[Zaehler] 
                                            + botLeft[Zaehler] 
                                            + mid[Zaehler] 
                                            + mid[mid_right]) 
                                            * 0.25f 
                                            + Random.Range(-offset, offset));
        }
        else if (row == 0 && col != 0)
        {
            int mid_bottom = ((numSquares * numSquares) - numSquares + Zaehler);
            int mid_left = (Zaehler - 1);

            //Pixel[oben]
            mPixel[row, (col + halfSize)] = ((topLeft[Zaehler] 
                                            + topRight[Zaehler] 
                                            + mid[Zaehler] 
                                            + mid[mid_bottom]) 
                                            * 0.25f 
                                            + Random.Range(-offset, offset));

            //Pixel[links]
            mPixel[(row + halfSize), col] = ((topLeft[Zaehler] 
                                            + botLeft[Zaehler] 
                                            + mid[Zaehler] 
                                            + mid[mid_left]) 
                                            * 0.25f 
                                            + Random.Range(-offset, offset));

        }
        else
        {
            int mid_up = (Zaehler - numSquares);
            int mid_left = (Zaehler - 1);

            //Pixel[oben]
            mPixel[row, (col + halfSize)] = ((topLeft[Zaehler] 
                                            + topRight[Zaehler] 
                                            + mid[Zaehler] 
                                            + mid[mid_up]) 
                                            * 0.25f 
                                            + Random.Range(-offset, offset));

            //Pixel[links]
            mPixel[(row + halfSize), col] = ((topLeft[Zaehler] 
                                            + botLeft[Zaehler] 
                                            + mid[Zaehler] 
                                            + mid[mid_left]) 
                                            * 0.25f 
                                            + Random.Range(-offset, offset));
        }

        if ((row >= ((x_Component - 1) - size)) && 
            (col >= ((y_Component - 1) - size)) || 
            ((x_Component - 1) == size))
        {
            int mid_up = (Zaehler - ((numSquares * numSquares) - numSquares));
            int mid_left = (Zaehler - (numSquares - 1));

            //Pixel[rechts]
            mPixel[(row + halfSize), (col + size)] = ((topRight[Zaehler] 
                                                    + botRight[Zaehler] 
                                                    + mid[Zaehler] 
                                                    + mid[mid_left]) 
                                                    * 0.25f 
                                                    + Random.Range(-offset, offset));

            //Pixel[unten]
            mPixel[(row + size), (col + halfSize)] = ((botLeft[Zaehler] + botRight[Zaehler] 
                                                    + mid[Zaehler] 
                                                    + mid[mid_up]) * 0.25f 
                                                    + Random.Range(-offset, offset));
        }
        else if ((col >= ((y_Component - 1) - size)) && 
                (row < ((x_Component - 1) - size)))
        {
            int mid_left = (Zaehler - (numSquares - 1));
            int mid_bottom = (Zaehler + numSquares);

            //Pixel[rechts]
            mPixel[(row + halfSize), (col + size)] = ((topRight[Zaehler] 
                                                    + botRight[Zaehler] 
                                                    + mid[Zaehler] 
                                                    + mid[mid_left]) 
                                                    * 0.25f
                                                    + Random.Range(-offset, offset));

            //Pixel[unten]
            mPixel[(row + size), (col + halfSize)] = ((botLeft[Zaehler] 
                                                    + botRight[Zaehler] 
                                                    + mid[Zaehler] 
                                                    + mid[mid_bottom]) 
                                                    * 0.25f 
                                                    + Random.Range(-offset, offset));

        }
        else if ((row >= ((x_Component - 1) - size)) && 
                (col < ((y_Component - 1) - size)))
        {
            int mid_up = (Zaehler - ((numSquares * numSquares) - numSquares));
            int mid_right = (Zaehler + 1);

            //Pixel[rechts]
            mPixel[(row + halfSize), (col + size)] = ((topRight[Zaehler] 
                                                    + botRight[Zaehler] + mid[Zaehler]
                                                    + mid[mid_right]) 
                                                    * 0.25f 
                                                    + Random.Range(-offset, offset));

            //Pixel[unten]
            mPixel[(row + size), (col + halfSize)] = ((botLeft[Zaehler] 
                                                    + botRight[Zaehler] + mid[Zaehler] 
                                                    + mid[mid_up]) 
                                                    * 0.25f 
                                                    + Random.Range(-offset, offset));

        }
        else
        {
            int mid_right = (Zaehler + 1);
            int mid_bottom = (Zaehler + numSquares);

            //Pixel[rechts]
            mPixel[(row + halfSize), (col + size)] = ((topRight[Zaehler] 
                                                    + botRight[Zaehler] + mid[Zaehler] 
                                                    + mid[mid_right]) 
                                                    * 0.25f 
                                                    + Random.Range(-offset, offset));

            //Pixel[unten]
            mPixel[(row + size), (col + halfSize)] = ((botLeft[Zaehler] 
                                                    + botRight[Zaehler] + mid[Zaehler] 
                                                    + mid[mid_bottom]) 
                                                    * 0.25f 
                                                    + Random.Range(-offset, offset));
        }

        Zaehler += 1;

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

        // pixel farben setzen für jeden Vertice in einer Spalte; 
        // das ganze wird Reihe für Reihe durchlaufen
        for (int i = 0; i < y_original; i++)
        {
            for (int j = 0; j < x_original; j++)
            {
                float ac_height = mPixel[i, j];

                //Die Höhenwerte aller Pixel/Vertices werden zwischen 0 und 1 normiert!
                mPixel[i, j] = ((ac_height - _MinMaxHeight[0]) / totaler_abstand);

                // Der Höhenwert soll sich zwischen 0 und 1 bewegen, 
                // das heißt eine 1 ist der höchste Punkt und damit weiß
                // weiß ist im RGB code (255, 255, 255) und schwarz (0, 0, 0); 
                // damit kann ich jede Höhe,
                // welche in y gespeichert ist, 
                // mit 255 multiplizieren und 
                // erhalte somit eine Grauabstufung von schwarz nach weiß
                // texture.SetPixel(0, 0, Color(1.0, 1.0, 1.0, 0.5));
                color = new Color32((byte)(mPixel[i, j] * 255), (byte)(mPixel[i, j] * 255), 
                        (byte)(mPixel[i, j] * 255), 255);
                texture.SetPixel(i, j, color);
                //Debug.Log("Check for: " + i + " " + j + " " + color);
            }
        }

        // definierte Pixel anwenden
        texture.Apply();

        // Verbindet die Textur mit dem Material von GameObject, 
        // an das dieses Skript angehängt ist
        GetComponent<MeshRenderer>().material.mainTexture = texture;

        // ----------------------------------------------------------------------------------------
        string _fullpath = heightmapPath;
        byte[] _bytes = texture.EncodeToPNG();
        System.IO.File.WriteAllBytes(_fullpath, _bytes);
        Debug.Log(_bytes.Length / 1024 + "Kb was saved as: " + _fullpath);
        // ----------------------------------------------------------------------------------------
        return texture;   
    }

    // generiert die MoistureMaP
    // Output: Texture2D texture
    Texture2D GenerateMoisture()
    {
        // Laden der HeightMap um deren Dimensionen zu bestimmen
        try
        {
            Texture2D heightmap = new Texture2D(1, 1);
            byte[] tmpBytes = File.ReadAllBytes(this.heightmapPath);
            heightmap.LoadImage(tmpBytes);
            this.width = heightmap.width;
            this.height = heightmap.height;
        }
        // Existiert die HeightMap unter dem angegeben Pfad nicht, 
        // wird die geworfene Exception abgefangen
        catch (FileNotFoundException)
        {
            Debug.Log("File HeightMapMap.png not found!");
        }
        // Erstellen der texture in die die MoistureMap gespeichert wird
        Texture2D texture = new Texture2D(width, height, TextureFormat.ARGB32, false);
        // Durchläuft die komplette Weite und Höhe und bestimmt je Pixel die Farbe
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // In der Variablen wird die in der Funktion 
                // CalculateColorMoisture generierte Farbe gespeichert
                Color color = CalculateColorMoisture(x, y);
                // Der Pixel an der Stelle (x,y) bekommt die vorher generierte Farbe
                texture.SetPixel(x, y, color);
            }
        }
        // Apply the changes to the texture and upload the updated texture to the GPU
        texture.Apply();

        // Generiert ein Bild aus texture
        // ----------------------------------------------------------------------------------------
        string _fullpath = this.moisturemapPath;
        byte[] _bytes = texture.EncodeToPNG();
        System.IO.File.WriteAllBytes(_fullpath, _bytes);
        Debug.Log(_bytes.Length / 1024 + "Kb was saved as: " + _fullpath);
        // ----------------------------------------------------------------------------------------
        return texture;
    }

    // Berechnet die Farbe mit PerlinNoise
    // Paramter:
    // x : X-Wert der MoistureMap
    // y : y-Wert der MositureMap
    // Output: Color
    Color CalculateColorMoisture(int x, int y)
    {
        // Pixelkoordinaten sind ganze Zahlen, 
        // daher müssen diese in Dezimalzahlen umgewandelt werden, 
        // damit wir unterschiedliche Werte aus PerlinNoise bekommen
        // Es wird mit scale multipliziert, damit wir eine dichtere PerlinNoise bekommen
        float xCoord = (float)x / width * scale;
        float yCoord = (float)y / height * scale;
        // Berechnung der PerlinNoise-Wertes mit den x und y Koordinaten
        float perlin = Mathf.PerlinNoise(xCoord, yCoord);
        return new Color(perlin, perlin, perlin);
    }

    // Läd ein Bild aus dem angegeben Pfad und speichert dieses in texture
    // Output: Texture2D
    Texture2D GetColorMapLand()
    {
        Texture2D colormap = new Texture2D(1, 1);
        byte[] tmpBytes = File.ReadAllBytes(this.colorMapLandPath);
        colormap.LoadImage(tmpBytes);

        return colormap;
    }

    // Läd ein Bild aus dem angegeben Pfad und speichert dieses in texture
    // Output: Texture2D
    Texture2D GetColorMapWater()
    {
        Texture2D colormap = new Texture2D(1, 1);
        byte[] tmpBytes = File.ReadAllBytes(this.colorMapWaterPath);
        colormap.LoadImage(tmpBytes);

        return colormap;
    }

    // Läd ein Bild aus dem angegeben Pfad und speichert dieses in texture
    // Output: Texture2D
    Texture2D GetNormalMap1()
    {
        Texture2D normalmap = new Texture2D(1, 1);
        byte[] tmpBytes = File.ReadAllBytes(this.colorNormalMap1Path);
        normalmap.LoadImage(tmpBytes);

        return normalmap;
    }

    // Läd ein Bild aus dem angegeben Pfad und speichert dieses in texture
    // Output: Texture2D
    Texture2D GetNormalMap2()
    {
        Texture2D normalmap = new Texture2D(1, 1);
        byte[] tmpBytes = File.ReadAllBytes(this.colorNormalMap2Path);
        normalmap.LoadImage(tmpBytes);

        return normalmap;
    }

    // Läd ein Bild aus dem angegeben Pfad und speichert dieses in texture
    // Output: Texture2D
    Texture2D GetSkyboxTexture()
    {
        Texture2D skyboxTexture = new Texture2D(1, 1);
        byte[] tmpBytes = File.ReadAllBytes(this.skyboxTexturePath);
        skyboxTexture.LoadImage(tmpBytes);

        return skyboxTexture;
    }
}