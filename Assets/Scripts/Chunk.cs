using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    const int CHUNK_WIDTH = 8;
    const int CHUNK_HEIGHT = 8;

    List<Vector3> meshVertices = new List<Vector3>();
    List<int> meshTriangles = new List<int>();
    List<Vector2> uvs = new List<Vector2>();

    int vertexIndex = 0;

    [SerializeField]
    MeshFilter meshFilter;

    [SerializeField]
    MeshRenderer meshRenderer;

    bool[,,] voxelMap = new bool[CHUNK_WIDTH, CHUNK_HEIGHT, CHUNK_WIDTH];

    // Start is called before the first frame update
    void Start()
    {
        InitializeVoxelMap();
        AddVoxelDataForAllCubes();
        CreateChunkMesh();
    }

    void AddVoxelDataForAllCubes()
    {
        for (int i = 0; i < CHUNK_WIDTH; i++)
        {
            for (int j = 0; j < CHUNK_HEIGHT; j++)
            {
                for (int k = 0; k < CHUNK_WIDTH; k++)
                {
                    if (voxelMap[i, j, k])
                    {
                        AddCubeToChunk(new Vector3(i, j, k));
                    }
                }
            }
        }
    }

    void InitializeVoxelMap()
    {
        for (int i = 0; i < CHUNK_WIDTH; i++)
        {
            for (int j = 0; j < CHUNK_HEIGHT; j++)
            {
                for (int k = 0; k < CHUNK_WIDTH; k++)
                {
                    voxelMap[i, j, k] = true;
                }
            }
        }
    }

    void AddCubeToChunk(Vector3 voxelPosition)
    {
        for (int i = 0; i < 6; i++)
        {
            for (int j = 0; j < 6; j++)
            {
                int triangleIndex = VoxelData.voxelTriangles[i, j];
                meshVertices.Add(voxelPosition + VoxelData.voxelVertices[triangleIndex]);
                meshTriangles.Add(vertexIndex);
                uvs.Add(VoxelData.voxelUvs[j]);
                
                vertexIndex++;
            }
        }
    }

    void CreateChunkMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = meshVertices.ToArray();
        mesh.triangles = meshTriangles.ToArray();
        mesh.uv = uvs.ToArray();

        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
