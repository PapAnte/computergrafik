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
    //bool swapped = false;

    //original mRows und mCols
    int mRows_original;
    int mCols_original;

    // Start is called before the first frame update
    void Start()
    {
        mRows_original = mRows;
        mCols_original = mCols;

        // Debug für ungültigen Wert 0 und > 128 Zeilen/Spalten
        // edit this: || mRows != mCols || ((mRows & (mRows - 1)) != 0)
        if (mRows == 0 || mCols == 0 || mRows > 128 || mCols > 128 )
        {
            Debug.Log("ungültiger Zeilen/Spalten Wert");

        } else if (mCols > mRows)
        {
            mRows = mCols_original;
            mCols = mRows_original;

            //swapped = true;

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

        //Debug.Log("mCols_original =" + mCols_original);
        //Debug.Log("mRows_original =" + mRows_original);
        //Random Höhenkalkulation der Rechteck-Eckpunkt Vertices
        mVerts[0].y = Random.Range(-mHeight, mHeight);
        mVerts[mCols_original].y = Random.Range(-mHeight, mHeight);
        mVerts[mVerts.Length - 1].y = Random.Range(-mHeight, mHeight);
        mVerts[mVerts.Length - 1 - mCols_original].y = Random.Range(-mHeight, mHeight);

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
            int numSquares_y = 1;
            int numSquares_x = 1;
            //Bsp: squareSize_y = 4
            int squareSize_y = mCols;
            float squareSize_x = (float)mRows;
            int squareSize_x_edited = mRows;
            int setrow = 0;

            //Schleife wie oft der DiamondSquare ausgeführt werden muss.
            for (int i = 0; i < iterations; i++)
            {
                int row = 0;

                //Schleife für das Wechseln der Zeilen.
                for (int j = 0; j < numSquares_x; j++)
                {
                    int col = 0;

                    //Schleife für das Wechseln der Splaten.
                    for (int k = 0; k < numSquares_y; k++)
                    {
                        
                        if (i == 0 || ((squareSize_x % 2 == 0) && (squareSize_y % 2) == 0))
                        {
                            squareSize_x_edited = (int)squareSize_x;
                            setrow = squareSize_x_edited;

                            DiamondSquareAlgo(row, col, squareSize_y, squareSize_x_edited, mHeight);
                            Debug.Log(i + ". Schritt wurde ausgeführt");

                        }
                        else if (squareSize_y == 1)
                        {
                            int int_row = 1;
                            int float_row = 1;

                            //check squareSize is int or float
                            if((squareSize_x % 1) == 0)
                            {
                                int_row = (int)squareSize_x;
                            } else
                            {
                                float_row = (int)(squareSize_x - 0.5f);
                            }

                            if ( ( j == 0 || ((j % 2) == 0)) )
                            {
                                squareSize_x_edited = (int)(squareSize_x - 0.5f);

                                if ((squareSize_x % 1) != 0)
                                {
                                    setrow = (int)(squareSize_x + 0.5f);
                                }
                                else
                                {
                                    setrow = (int)squareSize_x;

                                }
                                Debug.Log("Schritte : " + j + "squareSize_x_edited (if) = " + squareSize_x_edited);
                                Debug.Log("Schritte : " + j + "squareSize_x (if)= " + squareSize_x);
                                Debug.Log("setrow (if) = " + setrow);

                                if (((int_row % 2) == 0) || ((float_row % 2) == 0))
                                {
                                    Debug.Log("Ich beginne mit dem " + i + ". Schritt (Abbruch_row_gerade)");
                                    DiamondSquareAlgo_Abbruch_row_gerade(row, col, squareSize_y, squareSize_x_edited, mHeight);
                                } else
                                {
                                    Debug.Log("Ich beginne mit dem " + i + ". Schritt (Abbruch_row_ungerade)");
                                    DiamondSquareAlgo_Abbruch_row_ungerade(row, col, squareSize_y, squareSize_x_edited, mHeight);
                                }

                            } else if ((j % 2) != 0)
                            {
                                squareSize_x_edited = (int)(squareSize_x - 0.5f);

                                if ((squareSize_x % 1) != 0)
                                {
                                    setrow = (int)(squareSize_x - 0.5f);
                                } else
                                {
                                    setrow = (int)squareSize_x;
                                }

                                Debug.Log("Schritte : " + j + "squareSize_x_edited (elseif) = " + squareSize_x_edited);
                                Debug.Log("Schritte : " + j + "squareSize_x (elseif)= " + squareSize_x);
                                Debug.Log("setrow (elseif) = " + setrow);

                                if (((int_row % 2) == 0) || ((float_row % 2) == 0))
                                {
                                    Debug.Log("Ich beginne mit dem " + i + ". Schritt (Abbruch_row_gerade)");
                                    DiamondSquareAlgo_Abbruch_row_gerade(row, col, squareSize_y, squareSize_x_edited, mHeight);

                                } else
                                {
                                    Debug.Log("Ich beginne mit dem " + i + ". Schritt (Abbruch_row_ungerade)");
                                    DiamondSquareAlgo_Abbruch_row_ungerade(row, col, squareSize_y, squareSize_x_edited, mHeight);
                                }
                            } 
                            else
                            {
                                squareSize_x_edited = (int)squareSize_x;
                                setrow = squareSize_x_edited;
                            }        
                        }
                        else
                        {
                            
                            squareSize_x_edited = (int)squareSize_x;
                            setrow = squareSize_x_edited;

                            if ( ((squareSize_y % 2) != 0) && ((squareSize_x_edited % 2) == 0))
                            {                 
                                Debug.Log("Ich beginne mit dem " + i + ". Schritt (u/g)");
                                DiamondSquareAlgo_ungerade_gerade(row, col, squareSize_y, squareSize_x_edited, mHeight);

                            } else if (((squareSize_y % 2) != 0) && ((squareSize_x_edited % 2) != 0))
                            {
                                Debug.Log("Ich beginne mit dem " + i + ". Schritt (u/u)");
                                DiamondSquareAlgo_ungerade_ungerade(row, col, squareSize_y, squareSize_x_edited, mHeight);

                            } else
                            {
                                Debug.Log("Ich beginne mit dem " + i + ". Schritt (g/u)");
                                DiamondSquareAlgo_gerade_ungerade(row, col, squareSize_y, squareSize_x_edited, mHeight);
                            }
                  
                        }
                        col += squareSize_y;
                    }

                    row += setrow;
                    //Debug.Log("row = " + row);
                }

                numSquares_y *= 2;
                numSquares_x *= 2;

                if (((squareSize_y % 2) != 0) || squareSize_y == 2)
                {
                    squareSize_y = 1;
                    numSquares_y = mCols + 1;
                }
                else
                {
                    squareSize_y /= 2;
                }

                squareSize_x /= 2.0f;

                //Debug.Log("squareSize_y = " + squareSize_y);
                //Debug.Log("squareSize_x = " + squareSize_x);
                //Debug.Log("numSquares_y = " + numSquares_y);
                //Debug.Log("numSquares_x = " + numSquares_x);

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

    //worked 25.11.2020 (Bsp: 10 x 6 Matrix)
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

        //Debug.Log("size_y: " + size_y);
        //Debug.Log("row: " + row);
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
        //Debug.Log("topLeft[].y: " + mVerts[topLeft].y);
        //Debug.Log("topRight[].y: " + mVerts[topRight].y);
        //Debug.Log("botLeft[].y: " + mVerts[botLeft].y);
        //Debug.Log("botLeft[].y: " + mVerts[botRight].y);
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

    //worked 25.11.2020 (Bsp: 10 x 4 Matrix)
    void DiamondSquareAlgo_gerade_ungerade(int row, int col, int size_y, int size_x, float offset)
    {

        //size_y ist gerade deshalb int
        int halfSize_y = (int)(size_y * 0.5f);
        //Debug.Log("halfSize_y = " + halfSize_y);

        //size_x ist ungerade deshalb float
        float halfSize_x = (float)((float)size_x * 0.5f);
        //Debug.Log("halfSize_x = " + halfSize_x);

        //Die halbe (mCols+1) Länge

        float halfVertsCols = (float)(((float)mCols + 1.0f) * 0.5f);

            //Änderung hier!!
        int topLeft = row * (mCols + 1) + col;

            //Same wie oben auch 
        int botLeft = (((row + size_x) * (mCols + 1)) + col);
        int topRight = topLeft + size_y;
        int botRight = botLeft + size_y;

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

    //worked 25.11.2020 (Bsp: 12 x 6 and 14 x 6 Matrix)
    void DiamondSquareAlgo_ungerade_gerade(int row, int col, int size_y, int size_x, float offset)
    {

        //size_y ist gerade deshalb int
        int halfSize_y = (int)(size_y * 0.5f);
        //Debug.Log("halfSize_y = " + halfSize_y);

        //size_x ist ungerade deshalb float
        float halfSize_x = (float)((float)size_x * 0.5f);
        //Debug.Log("halfSize_x = " + halfSize_x);

        //Die halbe (mCols+1) Länge

        float halfVertsCols = (float)(((float)mCols + 1.0f) * 0.5f);

        //Änderung hier!!
        int topLeft = row * (mCols + 1) + col;

        //Same wie oben auch 
        int botLeft = (((row + size_x) * (mCols + 1)) + col);
        int topRight = topLeft + size_y;
        int botRight = botLeft + size_y;

        //Diamond Step mid_1 and mid_2 haben die gleichen 4 Eckpunkte!

        float mid = ((float)(row * (mCols + 1)) + (float)((float)((float)size_y * 0.5f) * (mRows + 1)) + (float)((float)col + halfSize_x));
        Debug.Log("float mid = " + mid);

        //halfVertsCols new feature !!!
        int mid_1 = ((int)(mid - 0.5f));
        Debug.Log("int mid_1 = " + mid_1);
        mVerts[mid_1].y = (mVerts[topLeft].y + mVerts[topRight].y + mVerts[botLeft].y + mVerts[botRight].y) * 0.25f + Random.Range(-offset, offset);

        int mid_2 = ((int)(mid + 0.5f));
        Debug.Log("int mid_2 = " + mid_2);
        mVerts[mid_2].y = (mVerts[topLeft].y + mVerts[topRight].y + mVerts[botLeft].y + mVerts[botRight].y) * 0.25f + Random.Range(-offset, offset);

        //Square Step mid_1

        //mVerts[oben_1]
        Debug.Log("mVerts[oben_1]: " + (topLeft + halfSize_y));
        mVerts[topLeft + halfSize_y].y = (mVerts[topLeft].y + mVerts[topRight].y + mVerts[mid_1].y) / 3 + Random.Range(-offset, offset);

        //mVerts[links_1]
        Debug.Log("mVerts[links_1]: " + (mid_1 - halfSize_y));
        mVerts[mid_1 - halfSize_y].y = (mVerts[topLeft].y + mVerts[botLeft].y + mVerts[mid_1].y) / 3 + Random.Range(-offset, offset);

        //mVerts[unten_1]
        Debug.Log("mVerts[unten_1]: " + (botLeft + halfSize_y));
        mVerts[botLeft + halfSize_y].y = (mVerts[botLeft].y + mVerts[botRight].y + mVerts[mid_1].y) / 3 + Random.Range(-offset, offset);

        //Square Step mid_2

        //mVerts[oben_2]
        Debug.Log("mVerts[links_2]: " + (topLeft + halfSize_y + 1));
        mVerts[topLeft + halfSize_y + 1].y = (mVerts[topLeft].y + mVerts[botLeft].y + mVerts[mid_2].y) / 3 + Random.Range(-offset, offset);

        //mVerts[rechts_2]
        Debug.Log("mVerts[rechts_2]: " + (mid_2 + halfSize_y));
        mVerts[mid_2 + halfSize_y].y = (mVerts[topRight].y + mVerts[botRight].y + mVerts[mid_2].y) / 3 + Random.Range(-offset, offset);

        //mVerts[unten]
        Debug.Log("mVerts[unten_2]: " + (botLeft + halfSize_y + 1));
        mVerts[botLeft + halfSize_y + 1].y = (mVerts[botLeft].y + mVerts[botRight].y + mVerts[mid_2].y) / 3 + Random.Range(-offset, offset);

    }

    //worked 25.11.2020 (Bsp: 10 x 6 Matrix)
    void DiamondSquareAlgo_ungerade_ungerade(int row, int col, int size_y, int size_x, float offset)
    {

        //size_y ist gerade deshalb int
        float halfSize_y = (float)((float)size_y * 0.5f);
        //Debug.Log("halfSize_y = " + halfSize_y);

        //size_x ist ungerade deshalb float
        float halfSize_x = (float)((float)size_x * 0.5f);
        //Debug.Log("halfSize_x = " + halfSize_x);

        //Die halbe (mCols+1) Länge

        float halfVertsCols = (float)(((float)mCols + 1.0f) * 0.5f);

        //Änderung hier!!
        int topLeft = row * (mCols + 1) + col;

        //Same wie oben auch 
        int botLeft = (((row + size_x) * (mCols + 1)) + col);
        int topRight = topLeft + size_y;
        int botRight = botLeft + size_y;

        //Diamond Step mid_1 and mid_2 haben die gleichen 4 Eckpunkte!

        float mid = ((float)(row * (mCols + 1)) + (float)((halfSize_y * (mRows + 1)) + (float)((float)col + halfSize_x)));
        Debug.Log("float mid = " + mid);

        //halfVertsCols new feature !!!
        int mid_1 = ((int)(mid - halfVertsCols - 0.5f));
        Debug.Log("int mid_1 = " + mid_1);
        mVerts[mid_1].y = (mVerts[topLeft].y + mVerts[topRight].y + mVerts[botLeft].y + mVerts[botRight].y) * 0.25f + Random.Range(-offset, offset);

        int mid_2 = ((int)(mid - halfVertsCols + 0.5f));
        Debug.Log("int mid_2 = " + mid_2);
        mVerts[mid_2].y = (mVerts[topLeft].y + mVerts[topRight].y + mVerts[botLeft].y + mVerts[botRight].y) * 0.25f + Random.Range(-offset, offset);

        int mid_3 = ((int)(mid + halfVertsCols  - 0.5f));
        Debug.Log("int mid_3 = " + mid_3);
        mVerts[mid_3].y = (mVerts[topLeft].y + mVerts[topRight].y + mVerts[botLeft].y + mVerts[botRight].y) * 0.25f + Random.Range(-offset, offset);

        int mid_4 = ((int)(mid + halfVertsCols  + 0.5f));
        Debug.Log("int mid_4 = " + mid_4);
        mVerts[mid_4].y = (mVerts[topLeft].y + mVerts[topRight].y + mVerts[botLeft].y + mVerts[botRight].y) * 0.25f + Random.Range(-offset, offset);


        //Square Step mid_1

        //mVerts[oben_1]
        Debug.Log("mVerts[oben_1]: " + (topLeft + (int)(halfSize_y - 0.5f)));
        mVerts[topLeft + (int)(halfSize_y - 0.5f)].y = (mVerts[topLeft].y + mVerts[topRight].y + mVerts[mid_1].y) / 3 + Random.Range(-offset, offset);

        //mVerts[links_1]
        Debug.Log("mVerts[links_1]: " + (mid_1 - (int)(halfSize_y - 0.5f)));
        mVerts[mid_1 - (int)(halfSize_y - 0.5f)].y = (mVerts[topLeft].y + mVerts[botLeft].y + mVerts[mid_1].y) / 3 + Random.Range(-offset, offset);

        //Square Step mid_2

        //mVerts[oben_2]
        Debug.Log("mVerts[oben_2]: " + (topLeft + (int)(halfSize_y + 0.5f)));
        mVerts[topLeft + (int)(halfSize_y + 0.5f)].y = (mVerts[topLeft].y + mVerts[topRight].y + mVerts[mid_2].y) / 3 + Random.Range(-offset, offset);

        //mVerts[rechts_1]
        Debug.Log("mVerts[rechts_1]: " + (mid_2 + (int)(halfSize_y - 0.5f)));
        mVerts[mid_2 + (int)(halfSize_y - 0.5f)].y = (mVerts[topRight].y + mVerts[botRight].y + mVerts[mid_2].y) / 3 + Random.Range(-offset, offset);

        //Square Step mid_3

        //mVerts[links_2]
        Debug.Log("mVerts[links_2]: " + (mid_3 - (int)(halfSize_y - 0.5f)));
        mVerts[mid_3 - (int)(halfSize_y - 0.5f)].y = (mVerts[topLeft].y + mVerts[botLeft].y + mVerts[mid_3].y) / 3 + Random.Range(-offset, offset);

        //mVerts[unten_1]
        Debug.Log("mVerts[unten_1]: " + (botLeft + (int)(halfSize_y - 0.5f)));
        mVerts[botLeft + (int)(halfSize_y - 0.5f)].y = (mVerts[botLeft].y + mVerts[botRight].y + mVerts[mid_3].y) / 3 + Random.Range(-offset, offset);

        //Square Step mid_4

        //mVerts[rechts_2]
        Debug.Log("mVerts[rechts_2]: " + (mid_4 + (int)(halfSize_y - 0.5f)));
        mVerts[mid_4 + (int)(halfSize_y - 0.5f)].y = (mVerts[topRight].y + mVerts[botRight].y + mVerts[mid_4].y) / 3 + Random.Range(-offset, offset);

        //mVerts[unten_2]
        Debug.Log("mVerts[unten_2]: " + (botLeft + (int)(halfSize_y + 0.5f)));
        mVerts[botLeft + (int)(halfSize_y + 0.5f)].y = (mVerts[botLeft].y + mVerts[botRight].y + mVerts[mid_4].y) / 3 + Random.Range(-offset, offset);

    }

    //worked 25.11.2020 (Bsp: 10 x 6 Matrix)
    void DiamondSquareAlgo_Abbruch_row_gerade(int row, int col, int size_y, int size_x, float offset)
    {
        int top_vert;
        int bot_vert;

        top_vert = row * (mCols + 1) + col;
        bot_vert = (((row + size_x) * (mCols + 1)) + col);

        Debug.Log("topVert = " + top_vert);
        Debug.Log("botVert = " + bot_vert);

        int mid = (int)((top_vert + bot_vert) * 0.5f);

        Debug.Log("mid = " + mid);

        mVerts[mid].y = (mVerts[top_vert].y + mVerts[bot_vert].y) *0.5f + Random.Range(-offset, offset);

    }

    void DiamondSquareAlgo_Abbruch_row_ungerade(int row, int col, int size_y, int size_x, float offset)
    {
        int top_vert;
        int bot_vert;

        top_vert = row * (mCols + 1) + col;
        bot_vert = (top_vert + (3 * (mCols + 1)));

        Debug.Log("topVert = " + top_vert);
        Debug.Log("botVert = " + bot_vert);

        int mid_1 = (top_vert + (mCols + 1));
        int mid_2 = (bot_vert - (mCols + 1));

        Debug.Log("mid_1 = " + mid_1);
        Debug.Log("mid_2 = " + mid_2);

        mVerts[mid_1].y = (mVerts[top_vert].y + mVerts[bot_vert].y) * 0.5f + Random.Range(-offset, offset);
        mVerts[mid_2].y = (mVerts[top_vert].y + mVerts[bot_vert].y) * 0.5f + Random.Range(-offset, offset);

    }

    // TODO!!!!!!
    void DiamondSquareAlgo_Abbruch_col_gerade(int row, int col, int size_y, int size_x, float offset)
    {
        int top_vert;
        int bot_vert;

        top_vert = row * (mCols + 1) + col;
        bot_vert = (((row + size_x) * (mCols + 1)) + col);

        Debug.Log("topVert = " + top_vert);
        Debug.Log("botVert = " + bot_vert);

        int mid = (int)((top_vert + bot_vert) * 0.5f);

        Debug.Log("mid = " + mid);

        mVerts[mid].y = (mVerts[top_vert].y + mVerts[bot_vert].y) * 0.5f + Random.Range(-offset, offset);

    }

    void DiamondSquareAlgo_Abbruch_col_ungerade(int row, int col, int size_y, int size_x, float offset)
    {
        int top_vert;
        int bot_vert;

        top_vert = row * (mCols + 1) + col;
        bot_vert = (top_vert + (3 * (mCols + 1)));

        Debug.Log("topVert = " + top_vert);
        Debug.Log("botVert = " + bot_vert);

        int mid_1 = (top_vert + (mCols + 1));
        int mid_2 = (bot_vert - (mCols + 1));

        Debug.Log("mid_1 = " + mid_1);
        Debug.Log("mid_2 = " + mid_2);

        mVerts[mid_1].y = (mVerts[top_vert].y + mVerts[bot_vert].y) * 0.5f + Random.Range(-offset, offset);
        mVerts[mid_2].y = (mVerts[top_vert].y + mVerts[bot_vert].y) * 0.5f + Random.Range(-offset, offset);

    }

}
