using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiamondSquareCreatetexture : MonoBehaviour
{
    public int mRows;
    public int mCols;

    //Größe des Terrains (mSize x mSize)
    public float mSize;

    //Max. Höhe des Terrains
    float mHeight = 1;

    //speichert Vertices des Terrains
    Vector3[] mVerts;
    int mVertCount;

    //speichert die Länge der einzelnen Squares.
    Vector2 divisionSize;

    //boolean ob x und y getauscht worden sind
    bool swapped = false;

    //original mRows und mCols
    int mRows_original;
    int mCols_original;

    // Start is called before the first frame update
    void Start()
    {
        mRows_original = mRows;
        mCols_original = mCols;

        // Debug für ungültigen Wert 0 und > 128 Zeilen/Spalten
        if (mRows == 0 || mCols == 0 || mRows > 128 || mCols > 128 || mRows != mCols || ((mRows & (mRows - 1)) != 0))
        {
            Debug.Log("ungültiger Zeilen/Spalten Wert");

        }
        else if (mCols > mRows)
        {
            mRows = mCols_original;
            mCols = mRows_original;

            swapped = true;

            divisionSize = new Vector2((mSize / mRows), (mSize / mCols));

            CreateTerrain();

        }
        else
        {
            divisionSize = new Vector2((float)(mSize / (float)(mRows_original)), (float)(mSize / (float)(mCols_original)));

            CreateTerrain();
        }
        Debug.Log("Start------------------------------------------------------------------------------------------------");
        CreateTexture();
    }

    void CreateTexture()
    {
        
        // Create a new texture ARGB32 (32 bit with alpha) and no mipmaps
        var texture = new Texture2D(mRows_original, mCols_original, TextureFormat.ARGB32, false);

        //Diese Variable hilft dabei, zu wissen bei welchem Vertice der Matrix wir uns gerade befinden
        int countVertices = 0;

        Color color;
        // pixel farben setzen für jeden Vertice in einer Spalte; das ganze wird Reihe für Reihe durchlaufen
        for (int i = 0; i <= mRows_original; i++)
        {
            for (int j = 0; j <= mCols_original; j++)
            {
                //Der Höhenwert soll sich zwischen 0 und 1 bewegen, das heißt eine 1 ist der höchste Punkt und damit weiß
                //weiß ist im RGB code (255, 255, 255) und schwarz (0, 0, 0); damit kann ich jede Höhe, welche in 
                //y gespeichert ist, mit 255 multiplizieren und erhalte somit eine Grauabstufung von schwarz nach weiß
                // texture.SetPixel(0, 0, Color(1.0, 1.0, 1.0, 0.5));
                color = new Color32((byte)(mVerts[countVertices].y * 255), (byte)(mVerts[countVertices].y * 255), (byte)(mVerts[countVertices].y * 255), 0);
                texture.SetPixel(i, j, color);
                Debug.Log("Check for: " + i + " " + j + " " + color);
                countVertices++;
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

    void CreateTerrain()
    {
        Debug.Log("Start------------------------------------------------------------------------------------------------");
        //creating simple flat terrain with own mesh filter
        // mesh Kalkulation immer mit original mRows and mCols!
        mVertCount = (mRows_original + 1) * (mCols_original + 1);
        mVerts = new Vector3[mVertCount];
        Vector2[] uvs = new Vector2[mVertCount];
        int[] tris = new int[mRows_original * mCols_original * 6];

        float halfSize = mSize * 0.5f;

        Mesh mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        int triOffset = 0;

        for (int i = 0; i <= mRows_original; i++)
        {

            for (int j = 0; j <= mCols_original; j++)
            {

                mVerts[i * (mCols_original + 1) + j] = new Vector3(-halfSize + j * divisionSize.y, 0.0f, halfSize - i * divisionSize.x);
                uvs[i * (mCols_original + 1) + j] = new Vector2((float)i / mRows_original, (float)j / mCols_original);

                //build up triangles
                if (i < mRows_original && j < mCols_original)
                {
                    int topLeft = i * (mCols_original + 1) + j;
                    int botLeft = (i + 1) * (mCols_original + 1) + j;

                    tris[triOffset] = topLeft;
                    tris[triOffset + 1] = topLeft + 1;
                    tris[triOffset + 2] = botLeft + 1;

                    tris[triOffset + 3] = topLeft;
                    tris[triOffset + 4] = botLeft + 1;
                    tris[triOffset + 5] = botLeft;

                    //0,6,12,18... Immer 2 neue Dreiecke pro Viereck
                    triOffset += 6;
                }
            }
        }

        //Debug.Log("mCols_original =" + mCols_original);
        //Debug.Log("mRows_original =" + mRows_original);
        //Random Höhenkalkulation der Rechteck-Eckpunkt Vertices
        mVerts[0].y = Random.Range(0, mHeight);
        mVerts[mCols_original].y = Random.Range(0, mHeight);
        mVerts[mVerts.Length - 1].y = Random.Range(0, mHeight);
        mVerts[mVerts.Length - 1 - mCols_original].y = Random.Range(0, mHeight);

        //Debug.Log("mVerts[mCols_original].y = " + mVerts[mCols_original].y);

        //Start Abfrage für Diamond Square Algorithmus

        //normaler Diamond Square, wenn Anzahl von Spalten und Zeilen gleich sind
        //und diese eine 2er Potenz ist. Bsp: 2,4,8,16... bis 128
        //Mehr Vertices kann Unity nicht berechnen/speichern
        if ((mRows == mCols) && ((mRows & (mRows - 1)) == 0))
        {

            int mDivisions = mRows;

            //start diamond square algo

            // Anzahl der Durchläufe. Bsp: 4x4 Matrix, mDivisions = 4, iterations = logarithmus 4 base 2 = 2.0
            //D.h. es müssen 2x der Diamond Step und 2x der Square Step ausgeführt werden damit alle Vertices berechnet wurden.
            int iterations = (int)Mathf.Log(mDivisions, 2);

            // Anzahl der Vierecke. Zuerst gibt es 1 Viereck
            // Danach gibt es 2 Vierecke pro Zeile, danach 4 Vierecke pro Zeile usw.
            int numSquares = 1;

            //Größe der Vierecke. Zuerst ist das Viereck (mDivisions * mDivisions)
            int squareSize = mDivisions;

            //Schleife wie oft der DiamondSquare ausgeführt werden muss.
            for (int i = 0; i < iterations; i++)
            {
                int row = 0;

                //Schleife für das Wechseln der Zeilen.
                for (int j = 0; j < numSquares; j++)
                {
                    int col = 0;

                    //Schleife für das Wechseln der Splaten.
                    for (int k = 0; k < numSquares; k++)
                    {
                        // Bsp: 2. Durchgang bei mDivision = 4
                        // 1. Aufruf DiamondSquareAlgo(0,0,2,mHeight);
                        // 2. Aufruf DiamondSquareAlgo(0,2,2,mHeight);
                        // Ende der Schleife row += 2
                        // 3. Aufruf DiamondSquareAlgo(2,0,2,mHeight);
                        // 4. Aufruf DiamondSquareAlgo(2,2,2,mHeight);
                        // Ende der Schleife
                        DiamondSquareAlgo(row, col, squareSize, squareSize, mHeight);
                        col += squareSize;
                    }

                    row += squareSize;
                }

                numSquares *= 2;
                squareSize /= 2;

                //can be editable
                mHeight *= 0.5f;
            }
        }

        // Anzahl Spalten und Zeilen ist gerade
        else if ((mRows % 2 == 0) && (mCols % 2 == 0))
        {
            //Bsp: mRows = 8 und mCols = 4 -> iteration = 3
            // Besonderheit doppelt so viele Rows wie Cols -> 2x normaler DiamondSquare
            int iterations = (int)Mathf.Log(mRows, 2);
            int numSquares = 1;
            //Bsp: squareSize_y = 4
            int squareSize_y = mCols;
            int squareSize_x = mRows;

            //Schleife wie oft der DiamondSquare ausgeführt werden muss.
            for (int i = 0; i < iterations; i++)
            {
                int row = 0;

                //Schleife für das Wechseln der Zeilen.
                for (int j = 0; j < numSquares; j++)
                {
                    int col = 0;

                    //Schleife für das Wechseln der Splaten.
                    for (int k = 0; k < numSquares; k++)
                    {
                        if (squareSize_y > squareSize_x)
                        {
                            int temp = squareSize_x;
                            squareSize_x = squareSize_y;
                            squareSize_y = temp;

                            //Anpassung der divisionSize
                            float temp_2 = divisionSize.x;
                            divisionSize.x = divisionSize.y;
                            divisionSize.y = temp_2;

                            if (swapped == false)
                            {
                                swapped = true;
                            }
                            else
                            {
                                swapped = false;
                            }
                        }
                        else if (i == 0 || ((squareSize_x % 2 == 0) && (squareSize_y % 2) == 0))
                        {
                            DiamondSquareAlgo(row, col, squareSize_y, squareSize_x, mHeight);
                            Debug.Log(i + ". Schritt wurde ausgeführt");

                        }
                        else if (squareSize_y == 1)
                        {
                            DiamondSquareAlgo_2_Verts(row, col, squareSize_y, squareSize_x, mHeight);
                        }
                        else
                        {
                            Debug.Log("Ich beginne mit dem " + i + ". Schritt");
                            DiamondSquareAlgo_ungerade_gerade(row, col, squareSize_y, squareSize_x, mHeight);
                        }
                        col += squareSize_y;
                    }

                    row += squareSize_x;
                }

                numSquares *= 2;
                squareSize_y /= 2;

                if ((squareSize_x % 2) == 0)
                {
                    squareSize_x += 1;
                    squareSize_x /= 2;

                }
                else
                {
                    squareSize_x /= 2;
                }

                Debug.Log("squareSize_y = " + squareSize_y);
                Debug.Log("squareSize_x = " + squareSize_x);
                Debug.Log("numSquares = " + numSquares);

                //can be editable
                mHeight *= 0.5f;
            }
        }

        // Debug else, falls keine der Bedingungen eintritt
        else
        {
            Debug.Log("Ungültiger Zeilen und/oder Spaltenwert");
        }

        /* mesh Kalkulierung -> wird am Ende aller Schleifen ausgeführt
        mesh.vertices = mVerts;
        mesh.uv = uvs;
        mesh.triangles = tris;

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        */
        Debug.Log("Start------------------------------------------------------------------------------------------------");
    }

    void DiamondSquareAlgo(int row, int col, int size_y, int size_x, float offset)
    {
        int mDivisions = mRows;
        int topLeft;
        int botLeft;
        int topRight;
        int botRight;

        //Diamond Step
        int halfSize_y = (int)(size_y * 0.5f);
        int halfSize_x = (int)(size_x * 0.5f);
        if (swapped == false)
        {

            Debug.Log("size_y: " + size_y);
            Debug.Log("row: " + row);
            topLeft = ((row * (mRows + 1)) + col);

            //need x_size here!
            botLeft = (((row + size_x) * (mCols + 1)) + col);
            topRight = topLeft + size_y;
            botRight = botLeft + size_y;

            Debug.Log("topLeft: " + topLeft);
            Debug.Log("botLeft: " + botLeft);
            Debug.Log("topRight: " + topRight);
            Debug.Log("botRight: " + botRight);



            int mid = (int)(row + halfSize_y) * (mRows + 1) + (int)(col + halfSize_x);
            Debug.Log("mid: " + mid);
            Debug.Log("topLeft[].y: " + mVerts[topLeft].y);
            Debug.Log("topRight[].y: " + mVerts[topRight].y);
            Debug.Log("botLeft[].y: " + mVerts[botLeft].y);
            Debug.Log("botLeft[].y: " + mVerts[botRight].y);
            mVerts[mid].y = (mVerts[topLeft].y + mVerts[topRight].y + mVerts[botLeft].y + mVerts[botRight].y) * 0.25f + Random.Range(-offset, offset);        //performance boost 

            //Square Step

            //mVerts[oben]
            Debug.Log("mVerts[oben]: " + (topLeft + halfSize_y));
            mVerts[topLeft + halfSize_y].y = (mVerts[topLeft].y + mVerts[topRight].y + mVerts[mid].y) / 3 + Random.Range(-offset, offset);

            //mVerts[links]
            Debug.Log("mVerts[links]: " + (mid - halfSize_y));
            mVerts[mid - halfSize_y].y = (mVerts[topLeft].y + mVerts[botLeft].y + mVerts[mid].y) / 3 + Random.Range(-offset, offset);

            //mVerts[rechts]
            Debug.Log("mVerts[rechts]: " + (mid + halfSize_y));
            mVerts[mid + halfSize_y].y = (mVerts[topLeft + size_y].y + mVerts[botLeft + size_y].y + mVerts[mid].y) / 3 + Random.Range(-offset, offset);

            //mVerts[unten]
            Debug.Log("mVerts[unten]: " + (botLeft + halfSize_y));
            mVerts[botLeft + halfSize_y].y = (mVerts[botLeft].y + mVerts[botLeft + size_y].y + mVerts[mid].y) / 3 + Random.Range(-offset, offset);


        }
        else
        {

            // TODO
            topLeft = mDivisions * size_y + size_y - ((mDivisions + 1) * col);
            botLeft = (mVertCount) - (row * (size_x - 1)) - ((mDivisions + 1) * col + 1);
            topRight = topLeft - (size_y * (mDivisions + 1));
            botRight = (topRight + mDivisions);

            Debug.Log("topLeft_swapped: " + topLeft);
            Debug.Log("botLeft_swapped: " + botLeft);
            Debug.Log("topRight_swapped: " + topRight);
            Debug.Log("botRight_swapped: " + botRight);

            int mid = (topLeft - ((int)(halfSize_y) * (mDivisions + 1))) + (int)(halfSize_x - col);
            Debug.Log("mid_swapped: " + mid);
            mVerts[mid].y = (mVerts[topLeft].y + mVerts[topRight].y + mVerts[botLeft].y + mVerts[botRight].y) * 0.25f + Random.Range(-offset, offset);        //performance boost 

            //Square Step

            //mVerts[links]
            Debug.Log("mVerts[links]: " + (topLeft + (halfSize_y + 1)));
            mVerts[topLeft + (halfSize_y + 1)].y = (mVerts[topLeft].y + mVerts[botLeft].y + mVerts[mid].y) / 3 + Random.Range(-offset, offset);

            //mVerts[oben]
            Debug.Log("mVerts[oben]: " + (mid - (halfSize_y + 1)));
            mVerts[mid - (halfSize_y + 1)].y = (mVerts[topLeft].y + mVerts[topRight].y + mVerts[mid].y) / 3 + Random.Range(-offset, offset);

            //mVerts[unten]
            Debug.Log("mVerts[unten]: " + (mid + (halfSize_y + 1)));
            mVerts[mid + (halfSize_y + 1)].y = (mVerts[botLeft].y + mVerts[botRight].y + mVerts[mid].y) / 3 + Random.Range(-offset, offset);

            //mVerts[rechts]
            Debug.Log("mVerts[rechts]: " + (topRight + (halfSize_y + 1)));
            mVerts[topRight + (halfSize_y + 1)].y = (mVerts[topRight].y + mVerts[botRight].y + mVerts[mid].y) / 3 + Random.Range(-offset, offset);

        }

    }

    void DiamondSquareAlgo_ungerade_gerade(int row, int col, int size_y, int size_x, float offset)
    {
        int topLeft;
        int botLeft;
        int topRight;
        int botRight;

        //size_y ist gerade deshalb int
        int halfSize_y = (int)(size_y * 0.5f);

        //size_x ist ungerade deshalb float
        float halfSize_x = (float)((float)size_x * 0.5f);

        //Die halbe (mCols+1) Länge

        float halfVertsCols = (float)(((float)mCols + 1.0f) * 0.5f);

        if (swapped == false)
        {
            //Änderung hier!!
            topLeft = row * (mCols + 1) + col;

            //Same wie oben auch 
            botLeft = (((row + size_x) * (mCols + 1)) + col);
            topRight = topLeft + size_y;
            botRight = botLeft + size_y;

            //Diamond Step mid_1 and mid_2 haben die gleichen 4 Eckpunkte!

            float mid = ((float)(row * (mCols + 1)) + (float)(halfSize_y * (mRows + 1)) + (float)((float)col + halfSize_x));
            Debug.Log("float mid = " + mid);

            //Ab Hier relevant für swapped == true or false!
            //halfVertsCols new feature !!!
            int mid_1 = ((int)(mid - halfVertsCols));
            Debug.Log("int mid_1 = " + mid_1);
            mVerts[mid_1].y = (mVerts[topLeft].y + mVerts[topRight].y + mVerts[botLeft].y + mVerts[botRight].y) * 0.25f + Random.Range(-offset, offset);

            int mid_2 = ((int)(mid + halfVertsCols));
            Debug.Log("int mid_2 = " + mid_2);
            mVerts[mid_2].y = (mVerts[topLeft].y + mVerts[topRight].y + mVerts[botLeft].y + mVerts[botRight].y) * 0.25f + Random.Range(-offset, offset);

            //Square Step mid_1

            //mVerts[oben]
            Debug.Log("mVerts[oben]: " + (topLeft + halfSize_y));
            mVerts[topLeft + halfSize_y].y = (mVerts[topLeft].y + mVerts[topRight].y + mVerts[mid_1].y) / 3 + Random.Range(-offset, offset);

            //mVerts[links_1]
            Debug.Log("mVerts[links_1]: " + (mid_1 - halfSize_y));
            mVerts[mid_1 - halfSize_y].y = (mVerts[topLeft].y + mVerts[botLeft].y + mVerts[mid_1].y) / 3 + Random.Range(-offset, offset);

            //mVerts[rechts_1]
            Debug.Log("mVerts[rechts_1]: " + (mid_1 + halfSize_y));
            mVerts[mid_1 + halfSize_y].y = (mVerts[topLeft + size_y].y + mVerts[botLeft + size_y].y + mVerts[mid_1].y) / 3 + Random.Range(-offset, offset);

            //Square Step mid_2

            //mVerts[links_2]
            Debug.Log("mVerts[links_2]: " + (mid_2 - halfSize_y));
            mVerts[mid_2 - halfSize_y].y = (mVerts[topLeft].y + mVerts[botLeft].y + mVerts[mid_2].y) / 3 + Random.Range(-offset, offset);

            //mVerts[rechts_2]
            Debug.Log("mVerts[rechts_2]: " + (mid_2 + halfSize_y));
            mVerts[mid_2 + halfSize_y].y = (mVerts[topLeft + size_y].y + mVerts[botLeft + size_y].y + mVerts[mid_2].y) / 3 + Random.Range(-offset, offset);

            //mVerts[unten]
            Debug.Log("mVerts[unten]: " + (botLeft + halfSize_y));
            mVerts[botLeft + halfSize_y].y = (mVerts[botLeft].y + mVerts[botLeft + size_y].y + mVerts[mid_2].y) / 3 + Random.Range(-offset, offset);

        }
        else
        {

            //TODO
            //Diamond Step
            int halfSize = (int)(size_y * 0.5f);
            topLeft = row * (size_y + 1) + col;
            botLeft = (row + mRows) * (size_y + 1) + col;

            float mid = (int)(row + halfSize) * (mRows + 1) + (int)(col + halfSize);
            int mid_1 = (int)(mid - halfSize_x);
            int mid_2 = (int)(mid + halfSize_x);

            // Beide Mit Werte haben die gleichen vier Eckpunkte -> Fehler beim 3. Durchgang im 2.Schritt TODO
            mVerts[mid_1].y = (mVerts[topLeft].y + mVerts[topLeft + size_y].y + mVerts[botLeft].y + mVerts[botLeft + size_y].y) * 0.25f + Random.Range(-offset, offset);
            mVerts[mid_2].y = (mVerts[topLeft].y + mVerts[topLeft + size_y].y + mVerts[botLeft].y + mVerts[botLeft + size_y].y) * 0.25f + Random.Range(-offset, offset);

            //Square Step mid_1

            mVerts[topLeft + halfSize].y = (mVerts[topLeft].y + mVerts[topLeft + size_y].y + mVerts[mid_1].y) / 3 + Random.Range(-offset, offset);
            mVerts[mid_1 - halfSize].y = (mVerts[topLeft].y + mVerts[botLeft].y + mVerts[mid_1].y) / 3 + Random.Range(-offset, offset);
            mVerts[mid_1 + halfSize].y = (mVerts[topLeft + size_y].y + mVerts[botLeft + size_y].y + mVerts[mid_1].y) / 3 + Random.Range(-offset, offset);
            //mVerts[botLeft + halfSize].y = (mVerts[botLeft].y + mVerts[botLeft + size].y + mVerts[mid].y) / 3 + Random.Range(-offset, offset); -> Dieser ist Mid_2

            //Square Step mid_2

            //mVerts[topLeft + halfSize].y = (mVerts[topLeft].y + mVerts[topLeft + size].y + mVerts[mid].y) / 3 + Random.Range(-offset, offset); -> Dieser ist Mid_1
            mVerts[mid_2 - halfSize].y = (mVerts[topLeft].y + mVerts[botLeft].y + mVerts[mid_2].y) / 3 + Random.Range(-offset, offset);
            mVerts[mid_2 + halfSize].y = (mVerts[topLeft + size_y].y + mVerts[botLeft + size_y].y + mVerts[mid_2].y) / 3 + Random.Range(-offset, offset);
            mVerts[botLeft + halfSize].y = (mVerts[botLeft].y + mVerts[botLeft + size_y].y + mVerts[mid_2].y) / 3 + Random.Range(-offset, offset);
        }

    }

    void DiamondSquareAlgo_ungerade_ungerade(int row, int col, int size_x, int size_y, float offset)
    {

    }

    void DiamondSquareAlgo_2_Verts(int row, int col, int size_x, int size_y, float offset)
    {
        int top_vert;
        int bot_vert;

        top_vert = row * (mCols + 1) + col;
        bot_vert = (((row + size_x) * (mCols + 1)) + col);

    }
}
