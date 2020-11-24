using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiamondSquareDiffrentRowsAndCols : MonoBehaviour
{
    public int mRows;
    public int mCols;

    //Größe des Terrains (mSize x mSize)
    public float mSize;

    //Max. Höhe des Terrains
    public float mHeight;

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
        if (mRows == 0 || mCols == 0 || mRows > 128 || mCols > 128)
        {
            Debug.Log("ungültiger Zeilen/Spalten Wert");

        } else if (mCols > mRows)
        {
            mRows = mCols_original;
            mCols = mRows_original;

            swapped = true;

            divisionSize = new Vector2((mSize / mRows), (mSize / mCols));

            CreateTerrain();

        } else
        {
            divisionSize = new Vector2((float)(mSize / (float)(mRows_original)), (float)(mSize / (float)(mCols_original)));

            CreateTerrain();
        }
    }

    void CreateTerrain()
    {

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

        //Random Höhenkalkulation der Rechteck-Eckpunkt Vertices
        mVerts[0].y = Random.Range(-mHeight, mHeight);
        mVerts[mCols_original].y = Random.Range(-mHeight, mHeight);
        mVerts[mVerts.Length - 1].y = Random.Range(-mHeight, mHeight);
        mVerts[mVerts.Length - 1 - mCols_original].y = Random.Range(-mHeight, mHeight);

        //Start Abfrage für Diamond Square Algorithmus

        //normaler Diamond Square, wenn Anzahl von Spalten und Zeilen gleich sind
        //und diese eine 2er Potenz ist. Bsp: 2,4,8,16... bis 256
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
                            } else
                            {
                                swapped = false;
                            }
                        }
                        else if (i == 0 || squareSize_x == (squareSize_y * 2))
                        {
                            DiamondSquareAlgo(row, col, squareSize_y, squareSize_x, mHeight);
                            Debug.Log("1. Schritt wurde ausgeführt");

                        } else
                        {
                            Debug.Log("Ich beginne mit dem 2.Schritt");
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

                } else
                {
                    squareSize_x /= 2;
                }

                //can be editable
                mHeight *= 0.5f;
            }
        }

        // Debug else, falls keine der Bedingungen eintritt
        else
        {
            Debug.Log("Ungültiger Zeilen und/oder Spaltenwert");
        }

        // mesh Kalkulierung -> wird am Ende aller Schleifen ausgeführt
        mesh.vertices = mVerts;
        mesh.uv = uvs;
        mesh.triangles = tris;

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

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


        } else
        {
            topLeft = mDivisions * size_y + size_y - ((mDivisions +1) * col);
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
        int mDivisions = mRows;
        int size = size_y;
        float half_divSize_x = (divisionSize.x * 0.5f);

        Debug.Log("size: " + size);

        //Diamond Step
        int halfSize = (int)(size * 0.5f);
        int topLeft = row * (size + 1) + col;
        int botLeft = (row + mDivisions) * (size + 1) + col;

        Debug.Log("topLeft: " + topLeft);
        Debug.Log("botLeft: " + botLeft);

        float mid = (int)(row + halfSize) * (mDivisions + 1) + (int)(col + halfSize);
        int mid_1 = (int)(mid - half_divSize_x);
        int mid_2 = (int)(mid + half_divSize_x);

        // Beide Mit Werte haben die gleichen vier Eckpunkte -> Fehler beim 3. Durchgang im 2.Schritt TODO
        mVerts[mid_1].y = (mVerts[topLeft].y + mVerts[topLeft + size].y + mVerts[botLeft].y + mVerts[botLeft + size].y) * 0.25f + Random.Range(-offset, offset);
        mVerts[mid_2].y = (mVerts[topLeft].y + mVerts[topLeft + size].y + mVerts[botLeft].y + mVerts[botLeft + size].y) * 0.25f + Random.Range(-offset, offset);

        //Square Step mid_1

        mVerts[topLeft + halfSize].y = (mVerts[topLeft].y + mVerts[topLeft + size].y + mVerts[mid_1].y) / 3 + Random.Range(-offset, offset);
        mVerts[mid_1 - halfSize].y = (mVerts[topLeft].y + mVerts[botLeft].y + mVerts[mid_1].y) / 3 + Random.Range(-offset, offset);
        mVerts[mid_1 + halfSize].y = (mVerts[topLeft + size].y + mVerts[botLeft + size].y + mVerts[mid_1].y) / 3 + Random.Range(-offset, offset);
        //mVerts[botLeft + halfSize].y = (mVerts[botLeft].y + mVerts[botLeft + size].y + mVerts[mid].y) / 3 + Random.Range(-offset, offset); -> Dieser ist Mid_2

        //Square Step mid_2

        //mVerts[topLeft + halfSize].y = (mVerts[topLeft].y + mVerts[topLeft + size].y + mVerts[mid].y) / 3 + Random.Range(-offset, offset); -> Dieser ist Mid_1
        mVerts[mid_2 - halfSize].y = (mVerts[topLeft].y + mVerts[botLeft].y + mVerts[mid_2].y) / 3 + Random.Range(-offset, offset);
        mVerts[mid_2 + halfSize].y = (mVerts[topLeft + size].y + mVerts[botLeft + size].y + mVerts[mid_2].y) / 3 + Random.Range(-offset, offset);
        mVerts[botLeft + halfSize].y = (mVerts[botLeft].y + mVerts[botLeft + size].y + mVerts[mid_2].y) / 3 + Random.Range(-offset, offset);

        Debug.Log("Ich bin fertig mit dem 2. Schritt");

    }

    void DiamondSquareAlgo_ungerade_ungerade(int row, int col, int size_x, int size_y, float offset)
    {

    }
}
