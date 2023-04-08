using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class MeshSlice : MonoBehaviour
{
    private class MeshPart
    {
        // List to store the vertices, normals, triangles, and UVs of a mesh
        private List<Vector3> _vertices = new List<Vector3>();
        private List<Vector3> _normals = new List<Vector3>();
        private List<List<int>> _triangles = new List<List<int>>();
        private List<Vector2> _UVs = new List<Vector2>();
        // Arrays to store the vertices, normals, triangles, and UVs of a mesh
        public Vector3[] Vertices;
        public Vector3[] Normals;
        public int[][] Triangles;
        public Vector2[] UV;
        // GameObject to store the newly created mesh
        public GameObject GeneratedObject;
        // Bounds of the mesh
        public Bounds Bounds = new Bounds();
        
        // Adds a triangle to the mesh, along with its vertices, normals, and UVs
        public void AddTriangle(int submesh, Vector3[] vertices, Vector3[] normals, Vector2[] uvs)
        {
            // Create a new list of triangles if necessary
            if (_triangles.Count - 1 < submesh)
                _triangles.Add(new List<int>());

            // Add the vertices and triangle indices to their respective lists
            for (int i = 0; i < vertices.Length; i++)
            {
                _triangles[submesh].Add(_vertices.Count);
                _vertices.Add(vertices[i]);
            }

            // Add the normals and UVs to their respective lists
            _normals.AddRange(normals);
            _UVs.AddRange(uvs);

            // Update the bounds of the mesh
            Bounds.Encapsulate(vertices[0]);
            Bounds.Encapsulate(vertices[1]);
            Bounds.Encapsulate(vertices[2]);
        }
        // Fills the arrays with the vertices, normals, triangles, and UVs
        public void FillArrays()
        {
            Vertices = _vertices.ToArray();
            Normals = _normals.ToArray();
            UV = _UVs.ToArray();
            Triangles = new int[_triangles.Count][];
            for (var i = 0; i < _triangles.Count; i++)
                Triangles[i] = _triangles[i].ToArray();
        }

        // Creates a new GameObject with the mesh
        public void MakeNewGeneratedObject(MeshSlice original)
        {
            // Create a new GameObject
            GeneratedObject = new GameObject(original.name);
            // Set the position, rotation, and scale of the GameObject
            var originalTransform = original.transform;
            var generatedTransform = GeneratedObject.transform;
            generatedTransform.position = originalTransform.position;
            generatedTransform.rotation = originalTransform.rotation;
            generatedTransform.localScale = originalTransform.localScale;

            // Create a new Mesh and set its vertices, normals, UVs, and triangles
            var mesh = new Mesh
            {
                name = original.GetComponent<MeshFilter>().mesh.name,
                vertices = Vertices,
                normals = Normals,
                uv = UV
            };

            for (var i = 0; i < Triangles.Length; i++)
            {
                mesh.SetTriangles(Triangles[i], i, true);
            }

            Bounds = mesh.bounds;
            
            var renderer = GeneratedObject.AddComponent<MeshRenderer>();
            renderer.materials = original.GetComponent<MeshRenderer>().materials;
            var filter = GeneratedObject.AddComponent<MeshFilter>();
            var collider = GeneratedObject.AddComponent<MeshCollider>();
            var rigidbody = GeneratedObject.AddComponent<Rigidbody>();
            var meshDestroy = GeneratedObject.AddComponent<MeshSlice>();
            filter.mesh = mesh;
            collider.convex = true;
            meshDestroy.SliceForce = original.SliceForce;
            collider.sharedMesh = mesh;
        }
    }
    private static readonly int _howManyIterations = 512;
    private static int _currentIterations = 0;
    public float SliceForce = 0; // The force with which the mesh parts will explode
    private void Update()
    {
        if(_currentIterations++ < _howManyIterations) {
            StartCoroutine(DestroyMeshAsync());
        }
    }

    private IEnumerator DestroyMeshAsync()
    {
        var originalMesh = GetComponent<MeshFilter>().mesh; // Get the original mesh of the gameobject
        var meshParts = new List<MeshPart>(); // Create a list to hold the mesh parts
        var subMeshParts = new List<MeshPart>(); // Create a list to hold the submesh parts

        originalMesh.RecalculateBounds(); // Recalculate the bounds of the original mesh

        var mainMeshPart = new MeshPart() // Create the main mesh part
        {
            Vertices = originalMesh.vertices,
            UV = originalMesh.uv,
            Normals = originalMesh.normals,
            Triangles = new int[originalMesh.subMeshCount][],
            Bounds = originalMesh.bounds
        };
        for (int i = 0; i < originalMesh.subMeshCount; i++)
            mainMeshPart.Triangles[i] = originalMesh.GetTriangles(i);

        meshParts.Add(mainMeshPart); // Add the main part to the list of mesh parts

        foreach (var t in meshParts)
        {
            var bounds = t.Bounds;
            bounds.Expand(0.5f);

            var plane = new Plane(
                UnityEngine.Random.onUnitSphere, 
                new Vector3(
                    Random.Range(bounds.min.x, bounds.max.x),
                    Random.Range(bounds.min.y, bounds.max.y),
                    Random.Range(bounds.min.z, bounds.max.z)));

            // Generate two submeshes by cutting the current mesh with the cutting plane
            subMeshParts.Add(MeshGeneration(t, plane, true));
            subMeshParts.Add(MeshGeneration(t, plane, false));
        }
        meshParts = new List<MeshPart>(subMeshParts); // Update the mesh parts list with the submesh parts
        subMeshParts.Clear(); // Clear the submesh parts list

        for (var index = 0; index < meshParts.Count; index++)
        {
            var t = meshParts[index];
            t.MakeNewGeneratedObject(this); // Create a gameobject from the mesh part
            t.GeneratedObject.GetComponent<Rigidbody>()
                .AddForceAtPosition(t.Bounds.center * SliceForce,
                    transform.position); // Add explosion force to the mesh part
        }

        Destroy(gameObject);// Destroy the original gameobject
        yield return null;
    }

    Vector2 _edgeUV;
    Vector3 _edgeVertex;
    // Define a method to add an edge to a mesh part
    void EdgeCreator(int subMesh, MeshPart meshPart, Vector3 normal, Vector3 aVertex, Vector3 bVertex, Vector2 aUv, Vector2 bUv, ref bool isEdgeSet)
    {
        // If edge is not yet set, set it to vertex a
        if (!isEdgeSet)
        {
            _edgeVertex = aVertex;
            _edgeUV = aUv;
            isEdgeSet = true;
        }
        else // Otherwise, add a triangle using vertex a and vertex b
        {
            // Create a plane from the edge and the two vertices
            Plane edgePlane = new Plane(_edgeVertex, aVertex, bVertex);

            // Determine which side of the plane the edge is on
            bool side = edgePlane.GetSide(_edgeVertex + normal);

            // Define the vertices for the triangle based on the side of the edge
            Vector3[] verts = {_edgeVertex, side ? aVertex : bVertex, side ? bVertex : aVertex};

            // Define the normals for the triangle (all the same)
            Vector3[] normals = {normal, normal, normal};

            // Define the UVs for the triangle
            Vector2[] uvs = {_edgeUV, aUv, bUv};

            // Add the triangle to the mesh part
            meshPart.AddTriangle(subMesh, verts, normals, uvs);
        }
    }
    
    // This method generates a new mesh part by cutting the original mesh part with a plane.
    // The 'original' parameter contains the original mesh part to be cut.
    // The 'plane' parameter defines the plane used to cut the mesh part.
    // The 'left' parameter specifies whether to keep the triangles on the left or the right side of the plane.
    private MeshPart MeshGeneration(MeshPart original, Plane plane, bool left)
    {
        var partMesh = new MeshPart() { };
        var ray1 = new Ray();
        var ray2 = new Ray();
        var tasks = new List<Task>();

        // Loop through all the triangles in the original mesh part.
        for (var i = 0; i < original.Triangles.Length; i++)
        {
            var triangles = original.Triangles[i];
            bool isEdgeSet = false;

            // Create a task to handle the current triangle
            tasks.Add(Task.Run(() =>
            {

                // Loop through all the vertices in the current triangle.
                for (var j = 0; j < triangles.Length; j = j + 3)
                {
                    // Determine which side of the plane each vertex is on.
                    var a = plane.GetSide(original.Vertices[triangles[j]]) == left;
                    var b = plane.GetSide(original.Vertices[triangles[j + 1]]) == left;
                    var c = plane.GetSide(original.Vertices[triangles[j + 2]]) == left;

                    var sideCount = (a ? 1 : 0) + (b ? 1 : 0) + (c ? 1 : 0);

                    // If all the vertices are on the opposite side of the plane, skip this triangle.
                    if (sideCount == 0)
                    {
                        continue;
                    }

                    // If all the vertices are on the same side of the plane, add the triangle to the new mesh part.
                    if (sideCount == 3)
                    {
                        Vector3[] verts = new[]
                        {
                            original.Vertices[triangles[j]],
                            original.Vertices[triangles[j + 1]],
                            original.Vertices[triangles[j + 2]]
                        };
                        Vector3[] normals = new[]
                        {
                            original.Normals[triangles[j]],
                            original.Normals[triangles[j + 1]],
                            original.Normals[triangles[j + 2]]
                        };
                        Vector2[] uvs = new[]
                        {
                            original.UV[triangles[j]], original.UV[triangles[j + 1]], original.UV[triangles[j + 2]]
                        };
                        partMesh.AddTriangle(i,verts, normals, uvs);
                        continue;
                    }

                    // If only one vertex is on the opposite side of the plane, calculate the intersection points
                    // and create two new triangles by connecting the intersection points with the original triangle's vertices.
                    // find the index of the vertex on the opposite side of the plane and create a new triangle by connecting the two intersection points with the two vertices that are on the same side of the plane
                    var singleIndex = b == c ? 0 : a == c ? 1 : 2;

                    // create the first ray to find the intersection point with the first edge of the triangle
                    ray1.origin = original.Vertices[triangles[j + singleIndex]];
                    var dir1 = original.Vertices[triangles[j + ((singleIndex + 1) % 3)]] -
                               original.Vertices[triangles[j + singleIndex]];
                    ray1.direction = dir1;
                    plane.Raycast(ray1, out var enter1);
                    var lerp1 = enter1 / dir1.magnitude;

                    ray2.origin = original.Vertices[triangles[j + singleIndex]];
                    var dir2 = original.Vertices[triangles[j + ((singleIndex + 2) % 3)]] -
                               original.Vertices[triangles[j + singleIndex]];
                    ray2.direction = dir2;
                    plane.Raycast(ray2, out var enter2);
                    var lerp2 = enter2 / dir2.magnitude;

                    //first vertex = ancor
                    EdgeCreator(i,
                        partMesh,
                        left ? plane.normal * -1f : plane.normal,
                        ray1.origin + ray1.direction.normalized * enter1,
                        ray2.origin + ray2.direction.normalized * enter2,
                        Vector2.Lerp(original.UV[triangles[j + singleIndex]],
                            original.UV[triangles[j + ((singleIndex + 1) % 3)]], lerp1),
                        Vector2.Lerp(original.UV[triangles[j + singleIndex]],
                            original.UV[triangles[j + ((singleIndex + 2) % 3)]], lerp2), ref isEdgeSet);

                    if (sideCount == 1)
                    {
                        Vector3[] verts = new[]
                        {
                            original.Vertices[triangles[j + singleIndex]],
                            ray1.origin + ray1.direction.normalized * enter1,
                            ray2.origin + ray2.direction.normalized * enter2
                        };
                        Vector3[] normals = new[]
                        {
                            original.Normals[triangles[j + singleIndex]],
                            Vector3.Lerp(original.Normals[triangles[j + singleIndex]],
                                original.Normals[triangles[j + ((singleIndex + 1) % 3)]], lerp1),
                            Vector3.Lerp(original.Normals[triangles[j + singleIndex]],
                                original.Normals[triangles[j + ((singleIndex + 2) % 3)]], lerp2)
                        };
                        Vector2[] uvs = new[]
                        {
                            original.UV[triangles[j + singleIndex]],
                            Vector2.Lerp(original.UV[triangles[j + singleIndex]],
                                original.UV[triangles[j + ((singleIndex + 1) % 3)]], lerp1),
                            Vector2.Lerp(original.UV[triangles[j + singleIndex]],
                                original.UV[triangles[j + ((singleIndex + 2) % 3)]], lerp2)
                        };
                        partMesh.AddTriangle(i,verts,normals,uvs);
                        continue;
                    }

                    // If there are only two sides cut through the triangle and add the resulting triangles to the MeshPart
                    if (sideCount == 2)
                    {
                        {
                            Vector3[] verts = new[]
                            {
                                ray1.origin + ray1.direction.normalized * enter1,
                                original.Vertices[triangles[j + ((singleIndex + 1) % 3)]],
                                original.Vertices[triangles[j + ((singleIndex + 2) % 3)]]
                            };
                            Vector3[] normals = new[]
                            {
                                Vector3.Lerp(original.Normals[triangles[j + singleIndex]],
                                    original.Normals[triangles[j + ((singleIndex + 1) % 3)]], lerp1),
                                original.Normals[triangles[j + ((singleIndex + 1) % 3)]],
                                original.Normals[triangles[j + ((singleIndex + 2) % 3)]]
                            };
                            Vector2[] uvs = new[]
                            {
                                Vector2.Lerp(original.UV[triangles[j + singleIndex]],
                                    original.UV[triangles[j + ((singleIndex + 1) % 3)]], lerp1),
                                original.UV[triangles[j + ((singleIndex + 1) % 3)]],
                                original.UV[triangles[j + ((singleIndex + 2) % 3)]]
                            };
                            // Add the first triangle resulting from the cut to the MeshPart
                            partMesh.AddTriangle(i, verts, normals, uvs);
                        }
                        {
                            Vector3[] verts = new[]
                            {
                                ray1.origin + ray1.direction.normalized * enter1,
                                original.Vertices[triangles[j + ((singleIndex + 2) % 3)]],
                                ray2.origin + ray2.direction.normalized * enter2,
                            };
                            Vector3[] normals = new[]
                            {
                                Vector3.Lerp(original.Normals[triangles[j + singleIndex]],
                                    original.Normals[triangles[j + ((singleIndex + 1) % 3)]], lerp1),
                                original.Normals[triangles[j + ((singleIndex + 2) % 3)]],
                                Vector3.Lerp(original.Normals[triangles[j + singleIndex]],
                                    original.Normals[triangles[j + ((singleIndex + 2) % 3)]], lerp2),
                            };
                            Vector2[] uvs = new[]
                            {
                                Vector2.Lerp(original.UV[triangles[j + singleIndex]],
                                    original.UV[triangles[j + ((singleIndex + 1) % 3)]], lerp1),
                                original.UV[triangles[j + ((singleIndex + 2) % 3)]],
                                Vector2.Lerp(original.UV[triangles[j + singleIndex]],
                                    original.UV[triangles[j + ((singleIndex + 2) % 3)]], lerp2)
                            };
                            // Add the second triangle resulting from the cut to the MeshPart
                            partMesh.AddTriangle(i,verts,normals,uvs);
                        }
                        // Continue to the next triangle
                        continue;
                    }


                }
            }));
            Task.WaitAll(tasks.ToArray());
        }

        partMesh.FillArrays();

        return partMesh;
    }

}