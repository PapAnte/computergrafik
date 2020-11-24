using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MashGenerator : MonoBehaviour
{

    Vector3[] mVerts;
    Vector2[] mUVs;
    int[] mTris;



    // Start is called before the first frame update
    void Start()
    {
        mVerts = new Vector3[4];
        mUVs = new Vector2[4];
        mTris = new int[6];

        Mesh mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        mVerts[0] = new Vector3(-1.0f, 0.0f, 1.0f);
        mVerts[1] = new Vector3(1.0f, 0.0f, 1.0f);
        mVerts[2] = new Vector3(-1.0f, 0.0f, -1.0f);
        mVerts[3] = new Vector3(1.0f, 0.0f, -1.0f);

        mUVs[0] = new Vector2(0.0f, 0.0f);
        mUVs[1] = new Vector2(1.0f, 0.0f);
        mUVs[2] = new Vector2(0.0f, 1.0f);
        mUVs[3] = new Vector2(1.0f, 1.0f);

        mTris[0] = 0;
        mTris[1] = 1;
        mTris[2] = 3;

        mTris[3] = 0;
        mTris[4] = 3;
        mTris[5] = 2;

        mesh.vertices = mVerts;
        mesh.uv = mUVs;
        mesh.triangles = mTris;

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
