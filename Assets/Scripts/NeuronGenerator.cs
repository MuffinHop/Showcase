using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class NeuronGenerator : MonoBehaviour
{
    public float radius = 0.5f;
    public float height = 1f;
    public int numSegments = 16;
    public int numHeightSegments = 16;
    [SerializeField] private Material _material;
    [SerializeField] private Material _sphereMaterial;
    [SerializeField] private int _howManyPoints = 4;
    [SerializeField] private GameObject[] _points;
    private HashSet<(Vector3, Vector3)> _connections = new HashSet<(Vector3, Vector3)>();
    
    private void Start()
    {
        // Create a sphere GameObject for each point
        foreach (GameObject point in _points)
        {
            GameObject sphereObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphereObj.transform.localScale = Vector3.one * radius * 4f;
            sphereObj.transform.position = point.transform.position;
            sphereObj.GetComponent<MeshRenderer>().material = _sphereMaterial;
            sphereObj.transform.parent = transform;
        }
        for (int j = 0; j < _points.Length; j++)
        {
            for (int i = 0; i < _points.Length; i++)
            {
                if (i == j) continue;
                ConnectPoints(_points[i].transform.position, _points[j].transform.position);
            }
        }
    }

    private void ConnectPoints(Vector3 positionA, Vector3 positionB)
    {
        // Check if the connection already exists
        if (_connections.Contains((positionA, positionB)) || _connections.Contains((positionB, positionA)))
        {
            return;
        }

        // Add the connection to the HashSet
        _connections.Add((positionA, positionB));
        DrawCylinder(positionA, positionB);
        
    }
    private void DrawCylinder(Vector3 positionA, Vector3 positionB)
    {
        // Calculate the direction and distance between the two GameObjects
        Vector3 direction = positionB - positionA;
        float distance = direction.magnitude;

        // Create a new GameObject to hold the cylinder
        GameObject cylinderObj = new GameObject("Cylinder");

        // Set the position of the cylinder to the midpoint between the two GameObjects
        cylinderObj.transform.position = (positionA + positionB) / 2f;

        // Set the rotation of the cylinder to align with the direction between the two GameObjects
        cylinderObj.transform.rotation = Quaternion.LookRotation(direction);

        // Create a new Mesh to hold the cylinder's geometry
        Mesh mesh = new Mesh();

        // Create arrays to hold the cylinder's vertices, normals, UVs, and triangles
        Vector3[] vertices = new Vector3[numSegments * (numHeightSegments+1)];
        Vector3[] normals = new Vector3[numSegments * (numHeightSegments+1)];
        Vector2[] uvs = new Vector2[numSegments * (numHeightSegments+1)];
        int[] triangles = new int[numSegments * numHeightSegments * 6];

        // Calculate the angle between each segment of the cylinder
        float angleStep = 2f * Mathf.PI / numSegments;

        // Calculate the height between each segment of the cylinder
        float heightStep = distance * height / numHeightSegments;

        // Calculate the position, normal, and UV of each vertex of the cylinder
        for (int j = 0; j <= numHeightSegments; j++)
        {
            float yPos = (j - numHeightSegments/2) * heightStep - height / 2f;
            float v = (float)j / numHeightSegments;

            for (int i = 0; i < numSegments; i++)
            {
                float angle = i * angleStep;
                Vector3 position = new Vector3(radius * Mathf.Cos(angle), radius * Mathf.Sin(angle), yPos);
                Vector3 normal = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
                Vector2 uv = new Vector2((float)i / numSegments, v);

                int index = j * numSegments + i;
                vertices[index] = position;
                normals[index] = -normal;
                uvs[index] = uv;
            }
        }

        // Create triangles for each segment of the cylinder
        for (int j = 0; j < numHeightSegments; j++)
        {
            for (int i = 0; i < numSegments; i++)
            {
                int triangleIndex = (j * numSegments + i) * 6;

                int index0 = j * numSegments + i;
                int index1 = j * numSegments + (i + 1) % numSegments;
                int index2 = (j + 1) * numSegments + i;
                int index3 = (j + 1) * numSegments + (i + 1) % numSegments;

                triangles[triangleIndex] = index0;
                triangles[triangleIndex + 1] = index2;
                triangles[triangleIndex + 2] = index1;

                triangles[triangleIndex + 3] = index2;
                triangles[triangleIndex + 4] = index3;
                triangles[triangleIndex + 5] = index1;
            }
        }

        // Assign the arrays to the Mesh
        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.uv = uvs;
        mesh.triangles = triangles;

        // Set the Mesh on the cylinder's MeshFilter component
        cylinderObj.AddComponent<MeshFilter>().mesh = mesh;

        // Set the Material on the cylinder's MeshRenderer component
        cylinderObj.AddComponent<MeshRenderer>().material = _material;
        cylinderObj.transform.parent = transform;
    }
}