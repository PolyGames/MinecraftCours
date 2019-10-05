using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk
{
    public const int CHUNK_WIDTH = 16;
    public const int CHUNK_HEIGHT = 16;

    List<Vector3> meshVertices = new List<Vector3>();
    List<int> meshTriangles = new List<int>();
    List<Vector2> meshUvs = new List<Vector2>();

    int vertexIndex = 0;

    MeshFilter meshFilter;

    MeshRenderer meshRenderer;

    GameObject chunkGameObject;

    byte[,,] voxelMap = new byte[CHUNK_WIDTH, CHUNK_HEIGHT, CHUNK_WIDTH];

    World world;

    ChunkCoord coord;

    bool isChunkActive;

    public bool isVoxelMapPopulated = false;
    
    public Chunk(World worldReference, ChunkCoord positionChunk, bool generateOnLoad)
    {
        world = worldReference;
        coord = positionChunk;
        isActive = true;

        if (generateOnLoad)
        {
            InitChunk();
        }
    }

    public void InitChunk()
    {
        chunkGameObject = new GameObject();
        meshFilter = chunkGameObject.AddComponent<MeshFilter>();
        meshRenderer = chunkGameObject.AddComponent<MeshRenderer>();

        meshRenderer.material = world.material;

        chunkGameObject.transform.SetParent(world.transform);
        chunkGameObject.transform.position = new Vector3(coord.x * CHUNK_WIDTH, 0, coord.y * CHUNK_WIDTH);
        chunkGameObject.name = "Chunk " + coord.x + ", " + coord.y;

        InitializeVoxelMap();
        AddVoxelDataForAllCubes();
    }

    void AddVoxelDataForAllCubes()
    {
        for (int i = 0; i < CHUNK_WIDTH; i++)
        {
            for (int j = 0; j < CHUNK_HEIGHT; j++)
            {
                for (int k = 0; k < CHUNK_WIDTH; k++)
                {
                    if (world.blockTypes[voxelMap[i,j,k]].isSolid)
                    {
                        AddCubeToChunk(new Vector3(i, j, k));
                    }
                }
            }
        }

        CreateChunkMesh();
    }

    void InitializeVoxelMap() // Pour être optimal, doit être fait pour tous les chunks avant même de créer les meshes
    {
        for (int i = 0; i < CHUNK_WIDTH; i++)
        {
            for (int j = 0; j < CHUNK_HEIGHT; j++)
            {
                for (int k = 0; k < CHUNK_WIDTH; k++)
                {
                    voxelMap[i, j, k] = world.GetVoxel(new Vector3(i, j, k) + position);
                }
            }
        }

        isVoxelMapPopulated = true;
    }

    void AddCubeToChunk(Vector3 voxelPosition)
    {
        for (byte i = 0; i < 6; i++) // chaque faces
        {
            if (!CheckVoxel(voxelPosition + VoxelData.voxelFaceChecks[i])) // si le voxel adjacent est pas solide
            {
                for (int j = 0; j < 4; j++) // chaque sommets
                {
                    meshVertices.Add(voxelPosition + VoxelData.voxelVertices[VoxelData.voxelTriangles[i, j]]);
                }
                byte blockID = voxelMap[(int)voxelPosition.x, (int)voxelPosition.y, (int)voxelPosition.z];
                AddTexture(world.blockTypes[blockID].GetTextureID(i));

                meshTriangles.Add(vertexIndex);
                meshTriangles.Add(vertexIndex + 1);
                meshTriangles.Add(vertexIndex + 2);
                meshTriangles.Add(vertexIndex + 2);
                meshTriangles.Add(vertexIndex + 1);
                meshTriangles.Add(vertexIndex + 3);
                
                vertexIndex += 4;
            }
        }
    }

    void CreateChunkMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = meshVertices.ToArray();
        mesh.triangles = meshTriangles.ToArray();
        mesh.uv = meshUvs.ToArray();

        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
    }

    void AddTexture(byte textureID)
    {
        float y = textureID / VoxelData.TextureAtlasSizeInBlocks;
        float x = textureID - (y * VoxelData.TextureAtlasSizeInBlocks);

        y *= VoxelData.NormalizedBlockTextureSize;
        x *= VoxelData.NormalizedBlockTextureSize;

        y = 1f - y - VoxelData.NormalizedBlockTextureSize;

        meshUvs.Add(new Vector2(x, y));
        meshUvs.Add(new Vector2(x, y + VoxelData.NormalizedBlockTextureSize));
        meshUvs.Add(new Vector2(x + VoxelData.NormalizedBlockTextureSize, y));
        meshUvs.Add(new Vector2(x + VoxelData.NormalizedBlockTextureSize, y + VoxelData.NormalizedBlockTextureSize));
    }

    bool CheckVoxel(Vector3 voxelPosition)
    {
        int x = Mathf.FloorToInt(voxelPosition.x);
        int y = Mathf.FloorToInt(voxelPosition.y);
        int z = Mathf.FloorToInt(voxelPosition.z);

        if (!IsVoxelInChunk(x, y, z))
        {
            return world.CheckForVoxel(voxelPosition + position);
        }

        return world.blockTypes[voxelMap[x, y, z]].isSolid;
    }

    public byte GetBlockTypeFromWorldVector3(Vector3 worldPosition)
    {
        int x = Mathf.FloorToInt(worldPosition.x);
        int y = Mathf.FloorToInt(worldPosition.y);
        int z = Mathf.FloorToInt(worldPosition.z);

        x -= Mathf.FloorToInt(position.x);
        z -= Mathf.FloorToInt(position.z);

        return voxelMap[x, y, z];
    }

    bool IsVoxelInChunk(int x, int y, int z)
    {
        return (x >= 0 && y >= 0 && z >= 0) &&
               (x < CHUNK_WIDTH && y < CHUNK_HEIGHT && z < CHUNK_WIDTH);
    }

    public Vector3 position
    {
        get { return chunkGameObject.transform.position; }
    }

    public bool isActive
    {
        get { return isChunkActive; }
        set
        {
            isChunkActive = value;
            if (chunkGameObject != null)
            {
                chunkGameObject.SetActive(value);
            }
        }
    }

    public void EditVoxel(Vector3 voxelPosition, byte newBlockID)
    {
        int x = Mathf.FloorToInt(voxelPosition.x);
        int y = Mathf.FloorToInt(voxelPosition.y);
        int z = Mathf.FloorToInt(voxelPosition.z);

        x -= Mathf.FloorToInt(position.x);
        z -= Mathf.FloorToInt(position.z);

        voxelMap[x, y, z] = newBlockID;

        UpdateSurroundingVoxels(x, y, z);

        UpdateChunkData();
    }

    void UpdateSurroundingVoxels(int x, int y, int z)
    {
        Vector3 voxelPosition = new Vector3(x, y, z);

        for (int i = 0; i < 6; i++)
        {
            Vector3 currentVoxel = voxelPosition + VoxelData.voxelFaceChecks[i];
            if (!IsVoxelInChunk((int)currentVoxel.x, (int)currentVoxel.y, (int)currentVoxel.z))
            {
                world.GetChunkFromVector3(currentVoxel + position).UpdateChunkData();
            }
        }
    }

    void UpdateChunkData()
    {
        vertexIndex = 0;
        meshTriangles.Clear();
        meshVertices.Clear();
        meshUvs.Clear();

        AddVoxelDataForAllCubes();
    }
}

/// <summary>
/// Classe qui représente la position des chunks selon le tableau de chunks du world
/// </summary>
public class ChunkCoord
{
    public int x;
    public int y;

    public ChunkCoord(int _x, int _y)
    {
        x = _x;
        y = _y;
    }

    public ChunkCoord(Vector3 position)
    {
        int _x = Mathf.FloorToInt(position.x);
        int _y = Mathf.FloorToInt(position.z);

        x = _x / Chunk.CHUNK_WIDTH;
        y = _y / Chunk.CHUNK_WIDTH;
    }

    public bool Equals(ChunkCoord other)
    {
        if (other == null)
            return false;
        return other.x == x && other.y == y;
    }
}
