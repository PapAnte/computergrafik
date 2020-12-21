using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirLightMovement : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.RotateAround(new Vector3(0.0f, 1.0f, 0.0f), Vector3.up, 9 * Time.deltaTime);
    }
}
