using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Droplet : MonoBehaviour
{
    private static int Count = 0;
    private int Index = 0;
    private static List<Vector4> _list = new List<Vector4>();
    private Transform _transform;
    void Start()
    {
        _transform = transform;
        _list.Add(transform.position);
        Index = Count++;
    }
    void Update()
    {
        _list[Index] = transform.position;
        Shader.SetGlobalVectorArray( "DropletArray", _list);
    }
}
