using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ds_heigtmap : MonoBehaviour
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

    //Bool swapped wenn y>x

    //bool swapped = false;

    //Höhengrenze
    int UPPER_BOUND = 1;

    //Eckpunkte des Quadrats mit Höheninformation
    float topLeft;
    float botLeft;
    float topRight;
    float botRight;
    float mid;

    // Start is called before the first frame update
    void Start()
    {

        x_original = x_Component;
        y_original = y_Component;

        // Suche das großte Quadrat aus dem Rechteck,
        // welches die Bedingung 2^n + 1 erfüllt.
        // if Y > X then swapp values

        if (y_Component > x_Component)
        {
            x_Component = y_original;
            y_Component = x_original;
            //swapped = true;
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
        CreateTexture();
    }

    void calculate_biggest_Quad()
    {
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
                    // Bsp: 2. Durchgang bei mDivision = 4
                    // 1. Aufruf DiamondSquareAlgo(0,0,2,mHeight);
                    // 2. Aufruf DiamondSquareAlgo(0,2,2,mHeight);
                    // Ende der Schleife row += 2
                    // 3. Aufruf DiamondSquareAlgo(2,0,2,mHeight);
                    // 4. Aufruf DiamondSquareAlgo(2,2,2,mHeight);
                    // Ende der Schleife
                    diamondStep(row, col, squareSize, smothness);
                    squareStep(row, col, squareSize, smothness);
                    col += squareSize;
                }

                row += squareSize;
            }

            numSquares *= 2;
            squareSize /= 2;

            //can be editable
            smothness *= 0.5f;
        }
    }

    //Diamond Step wird durchgeführt
    //Parameter:
    //size: Die Größe der quadratischen Matrix
    //increment: Die Schrittweite (2^n)
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

    void squareStep(int row, int col, int size, float offset)
    {
        int halfSize = (int)(size * 0.5f);

        //Debug.Log("halfSize = " + halfSize);

        //Debug.Log("row square = " + row);
        //Debug.Log("col square = " + col);
        //Debug.Log("size square = " + size);

        //Pixel[oben]
        mPixel[row, (col + halfSize)] = ((topLeft + topRight + mid) / 3 + Random.Range(-offset, offset));
        //Debug.Log("Pixel oben wäre [" + row + "][" + (col + halfSize) + "]");

        //Pixel[links]
        mPixel[(row + halfSize), col] = ((topLeft + botLeft + mid) / 3 + Random.Range(-offset, offset));
        //Debug.Log("Pixel links wäre [" + (row + halfSize) + "][" + col + "]");

        if ((col > ((y_Component - 1) / size)) && (row <= ((x_Component - 1) / size))) 
        {
            //Pixel[rechts]
            mPixel[(row + halfSize), (col + size)] = ((topRight + botRight + mid) / 3 + Random.Range(-offset, offset));

            //Debug.Log("Pixel rechts wäre [" + (row + halfSize) + "][" + (col + size) + "]");
        }
        else if ((row > ((x_Component - 1) / size)) && (col <= ((y_Component - 1) / size)))
        {
            //Pixel[unten]
            mPixel[(row + size), (col + halfSize)] = ((botLeft + botRight + mid) / 3 + Random.Range(-offset, offset));
            //Debug.Log("Pixel unten wäre [" + (row + size) + "][" + (col + halfSize) + "]");

        } else if ((row > ((x_Component - 1) / size)) && (col > ((y_Component - 1) / size)) || size == (int)(y_Component- 1))
        {
            //Pixel[unten]
            mPixel[(row + size), (col + halfSize)] = ((botLeft + botRight + mid) / 3 + Random.Range(-offset, offset));
            //Pixel[rechts]
            mPixel[(row + halfSize), (col + size)] = ((topRight + botRight + mid) / 3 + Random.Range(-offset, offset));

            //Debug.Log("Pixel rechts wäre [" + (row + halfSize) + "][" + (col + size) + "]");
            //Debug.Log("Pixel unten wäre [" + (row + size) + "][" + (col + halfSize) + "]");
        }
    }

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

    void CreateTexture()
    {

        // Create a new texture ARGB32 (32 bit with alpha) and no mipmaps
        var texture = new Texture2D(y_original, x_original, TextureFormat.ARGB32, false);

        Color color;

        float[] _MinMaxHeight = MinMaxHeight();

        float totaler_abstand = _MinMaxHeight[1] - _MinMaxHeight[0];

        Debug.Log("totaler_Abstand = " + totaler_abstand);

        // pixel farben setzen für jeden Vertice in einer Spalte; das ganze wird Reihe für Reihe durchlaufen
        for (int i = 0; i < y_original; i++)
        {
            for (int j = 0; j < x_original; j++)
            {
                float ac_height = mPixel[i, j];

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

        //definierte Pixel anwenden
        texture.Apply();

        // connect texture to material of GameObject this script is attached to
        GetComponent<Renderer>().material.mainTexture = texture;
        //GetComponent<Renderer>().material.SetTexture("_NewTexture", texture);

        // ------------------------------------------------------------------------------------------
        string _fullpath = "Assets//Scripts//image//image.png";
        byte[] _bytes = texture.EncodeToPNG();
        System.IO.File.WriteAllBytes(_fullpath, _bytes);
        Debug.Log(_bytes.Length / 1024 + "Kb was saved as: " + _fullpath);
        // ------------------------------------------------------------------------------------------ /
    }
}
