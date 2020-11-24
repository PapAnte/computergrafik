using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiamondSquare : MonoBehaviour
{
    //Anzahl der Felder/Triangles die wir haben (2,4,8,16 ... 128)
    public int mDivisions;

    //Größe des Terrains
    public float mSize;

    //Max. Höhe des Terrains
    public float mHeight;

    //speichert vertices des Terrains
    Vector3[] mVerts;
    int mVertCount;

    // Start is called before the first frame update
    void Start()
    {
        CreateTerrain();
    }

    void CreateTerrain()
    {

        //creating simple flat terrain with own mesh filter
        mVertCount = (mDivisions + 1) * (mDivisions + 1);
        mVerts = new Vector3[mVertCount];
        Vector2[] uvs = new Vector2[mVertCount];
        int[] tris = new int[mDivisions * mDivisions * 6];

        float halfSize = mSize * 0.5f;
        float divisionSize = mSize / mDivisions;

        Mesh mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        int triOffset = 0;

        for(int i = 0; i <= mDivisions; i++)
        {

            for(int j = 0; j <= mDivisions; j++)
            {

                mVerts[i * (mDivisions + 1) + j] = new Vector3(-halfSize + j * divisionSize, 0.0f, halfSize - i * divisionSize);
                uvs[i * (mDivisions + 1) + j] = new Vector2((float)i / mDivisions, (float)j / mDivisions);

                //build up triangles
                if(i < mDivisions && j < mDivisions)
                {
                    int topLeft = i * (mDivisions + 1) + j;
                    int botLeft = (i + 1) * (mDivisions + 1) + j;

                    tris[triOffset] = topLeft;
                    tris[triOffset + 1] = topLeft + 1;
                    tris[triOffset + 2] = botLeft + 1;

                    tris[triOffset+ 3] = topLeft;
                    tris[triOffset + 4] = botLeft + 1;
                    tris[triOffset + 5] = botLeft;

                    //fill up tris[]
                    triOffset += 6;
                }
            }
        }

        //random calculate the height
        mVerts[0].y = Random.Range(-mHeight, mHeight);
        mVerts[mDivisions].y = Random.Range(-mHeight, mHeight);
        mVerts[mVerts.Length - 1].y = Random.Range(-mHeight, mHeight);
        mVerts[mVerts.Length - 1 - mDivisions].y = Random.Range(-mHeight, mHeight);

        //start diamond square algo

        int iterations = (int)Mathf.Log(mDivisions, 2);
        int numSqueres = 1;
        int squereSize = mDivisions;

        for(int i = 0; i < iterations; i++)
        {
            int row = 0;

            for(int j = 0; j < numSqueres; j++)
            {
                int col = 0;

                for(int k = 0; k < numSqueres; k++)
                {
                    DiamondSquareAlgo(row, col, squereSize, mHeight);
                    col += squereSize;
                }

                row += squereSize;
            }

            numSqueres *= 2;
            squereSize /= 2;

            //can be editable
            mHeight *= 0.5f;
        }

        mesh.vertices = mVerts;
        mesh.uv = uvs;
        mesh.triangles = tris;

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

    }

    void DiamondSquareAlgo(int row, int col, int size, float offset)
    {
        //Diamond Step
        int halfSize = (int)(size * 0.5f);
        int topLeft = row * (mDivisions + 1) + col;
        int botLeft = (row + size) * (mDivisions + 1) + col;

        int mid = (int)(row + halfSize) * (mDivisions + 1) + (int)(col + halfSize);
        mVerts[mid].y = (mVerts[topLeft].y + mVerts[topLeft + size].y + mVerts[botLeft].y + mVerts[botLeft + size].y) * 0.25f + Random.Range(-offset, offset);        //performance boost 

        //Square Step

        mVerts[topLeft + halfSize].y = (mVerts[topLeft].y + mVerts[topLeft + size].y + mVerts[mid].y) / 3 + Random.Range(-offset, offset);
        mVerts[mid - halfSize].y = (mVerts[topLeft].y + mVerts[botLeft].y + mVerts[mid].y) / 3 + Random.Range(-offset, offset);
        mVerts[mid + halfSize].y = (mVerts[topLeft+size].y + mVerts[botLeft+size].y + mVerts[mid].y)/3 + Random.Range(-offset, offset);
        mVerts[botLeft + halfSize].y = (mVerts[botLeft].y + mVerts[botLeft + size].y + mVerts[mid].y)/3 + Random.Range(-offset, offset);
    }
}

