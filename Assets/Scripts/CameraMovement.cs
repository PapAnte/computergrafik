using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {   
        // Kamera rotiert um y-Achse 
        // (rotiert umd den Ursprung, nicht im Bezug auf den Planeten, ist hier nicht notwendig)
        transform.RotateAround(new Vector3(0.0f, 1.0f, 0.0f), Vector3.up, 2 * Time.deltaTime);
    }
}
