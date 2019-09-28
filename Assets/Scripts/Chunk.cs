using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk
{
    public const int CHUNK_WIDTH = 8;
    public const int CHUNK_HEIGHT = 8;

    List<Vector3> meshVertices = new List<Vector3>();
    List<int> meshTriangles = new List<int>();
    List<Vector2> meshUvs = new List<Vector2>();

    int vertexIndex = 0;

    MeshFilter meshFilter;

    MeshRenderer meshRenderer;

    GameObject chunkGameObject;

    byte[,,] voxelMap = new byte[CHUNK_WIDTH, CHUNK_HEIGHT, CHUNK_WIDTH];

    World world;

    // Start is called before the first frame update
    public Chunk(World worldReference, ChunkCoord positionChunk)
    {
        world = worldReference;
        chunkGameObject = new GameObject();
        meshFilter = chunkGameObject.AddComponent<MeshFilter>();
        meshRenderer = chunkGameObject.AddComponent<MeshRenderer>();

        meshRenderer.material = world.material;

        chunkGameObject.transform.SetParent(world.transform);
        chunkGameObject.transform.position = new Vector3(positionChunk.x * CHUNK_WIDTH, 0, positionChunk.y * CHUNK_WIDTH);
        chunkGameObject.name = "Chunk " + positionChunk.x + ", " + positionChunk.y;

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
                    if (world.blockTypes[voxelMap[i,j,k]].isSolid)
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
                    voxelMap[i, j, k] = 1;
                }
            }
        }
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

        if (!IsVoxelInChunk(voxelPosition))
            return world.CheckForVoxel(voxelPosition + position);

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

    bool IsVoxelInChunk(Vector3 voxelPosition)
    {
        return (voxelPosition.x >= 0 && voxelPosition.y >= 0 && voxelPosition.z >= 0) &&
               (voxelPosition.x < CHUNK_WIDTH - 1 && voxelPosition.y < CHUNK_HEIGHT - 1 && voxelPosition.z < CHUNK_WIDTH - 1);
    }

    public Vector3 position
    {
        get { return chunkGameObject.transform.position; }
    }
}

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
