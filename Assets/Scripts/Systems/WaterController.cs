using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using UnityEngine.SceneManagement;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class WaterController : MonoBehaviour
{
    public Material waterMaterial;
    public float width;
    public float length;
    public int resolution;

    private void Start()
    {
        RenderMesh2();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    private void RenderMesh()
    {
        MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = waterMaterial;
        MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
        Mesh mesh = new Mesh();
        
        List<Vector3> vertices = new();
        List<Vector2> uvs = new();
        List<Vector3> normals = new();
        List<int> indices = new();
        for (int x = 0; x < resolution-1; x++)
        {
            for (int z = 0; z < resolution-1; z++)
            {
                float tx = (float)x / (float)(resolution-1);
                float tz = (float)z / (float)(resolution-1);
                Vector3 position = new Vector3(-0.5f + tx, 0, -0.5f + tz);
                vertices.Add(position);
                // Debug.Log(position);
                normals.Add( Vector3.up );
                uvs.Add( new Vector2(tx,tz) );
                
                if (x < resolution * resolution && z < resolution * resolution)
                {
                    Debug.Log(position);
                    int quad = x*resolution + z;
                    
                    indices.Add(quad);
                    indices.Add(quad+1);
                    indices.Add(quad+resolution);
                    
                    indices.Add(quad + 1);
                    indices.Add(quad+resolution+1);
                    indices.Add(quad+resolution);
                    // // lower left triangle
                    // indices.Add(x + z);
                    // indices.Add(x + z + 1);
                    // indices.Add(x + z + resolution);
                    //
                    // // upper right triangle
                    // indices.Add(x + z + 1);
                    // indices.Add(x + z + resolution + 1);
                    // indices.Add(x + z + resolution);
                }
            }
        }
        
        // List<int> indices = new();
        // for (int x = 0; x < resolution-1; x++)
        // {
        //     for (int z = 0; z < resolution-1; z++)
        //     {
        //         int quad = z*resolution + x;
	       //
        //         indices.Add(quad);
        //         indices.Add(quad+resolution);
        //         indices.Add(quad+resolution+1);
	       //
        //         indices.Add(quad);
        //         indices.Add(quad+resolution+1);
        //         indices.Add(quad+1);
        //     }
        // }
        
        mesh.name = $"Terrain {mesh.GetHashCode()}";
        mesh.vertices = vertices.ToArray();
        mesh.triangles = indices.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.normals = normals.ToArray();
	  
        meshFilter.sharedMesh = mesh;
    }

    private void RenderMesh2()
    {
        MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = waterMaterial;

        MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();

        Mesh mesh = new Mesh();

        List<Vector3> vertices = new();
        List<int> tris = new();
        List<Vector3> normals = new();
        List<Vector2> uv = new();
        
        for (int x = 0; x < resolution - 1; x ++)
        for (int z = 0; z < resolution - 1; z++)
        {
            float tx = (float)x / (float)(resolution-1);
            float tz = (float)z / (float)(resolution-1);
            vertices.Add( new Vector3(-0.5f+tx,0,-0.5f+tz) );
            // normals.Add( Vector3.up );
            // uv.Add( new Vector2(tx,tz) );
            
            // Vector3 position = new Vector3((float)(resolution - 1) / x + 1, 0, (float)(resolution - 1) / z + 1);
            // Vector3 position = new Vector3((float)x / (resolution - 1), (float)z / (resolution - 1));
            // vertices.Add(position);

            if (x < resolution && z < resolution)
            {
                // lower left triangle
                tris.Add(x + z);
                tris.Add(x + z + 1);
                tris.Add(x + z + (resolution - 1));
                
                // upper right triangle
                tris.Add(x + z + 1);
                tris.Add(x + z + (resolution - 1) + 1);
                tris.Add(x + z + (resolution - 1));
            }

            
            normals.Add(Vector3.up);
            
            uv.Add(new Vector2((float)x/resolution, (float)z/resolution));
        }
        mesh.vertices = vertices.ToArray();
        mesh.triangles = tris.ToArray();
        mesh.normals = normals.ToArray();
        mesh.uv = uv.ToArray();

        meshFilter.mesh = mesh;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
    }
}
