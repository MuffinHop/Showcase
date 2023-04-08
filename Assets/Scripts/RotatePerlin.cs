using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotatePerlin : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float t = Time.time/3f;
        transform.rotation = Quaternion.Euler(Mathf.PerlinNoise(t, 1f)*64f,Time.time*30.0f,Mathf.PerlinNoise(t, 0.0f)*24f);
    }
}
