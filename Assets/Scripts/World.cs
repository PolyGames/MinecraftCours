using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
    public const int WORLD_WIDTH_IN_CHUNKS = 5;

    public Material material;

    public BlockType[] blockTypes;

    Chunk[,] chunks = new Chunk[WORLD_WIDTH_IN_CHUNKS, WORLD_WIDTH_IN_CHUNKS];

    public bool bypassRemovingInnerFacesChunks = true;

    public static int WORLD_WIDTH_IN_VOXELS
    {
        get { return WORLD_WIDTH_IN_CHUNKS * Chunk.CHUNK_WIDTH; }

    }

    // Start is called before the first frame update
    void Start()
    {
        GenerateWorld();
    }

    void GenerateWorld()
    {
        for (int i = 0; i < WORLD_WIDTH_IN_CHUNKS; i++)
        {
            for (int j = 0; j < WORLD_WIDTH_IN_CHUNKS; j++)
            {
                chunks[i, j] = new Chunk(this, new ChunkCoord(i, j));
            }
        }
    }

    public bool CheckForVoxel(Vector3 voxelPosition)
    {
        ChunkCoord chunkCoord = new ChunkCoord(voxelPosition);

        if (!IsChunkInWorld(chunkCoord) || voxelPosition.y < 0 || voxelPosition.y > Chunk.CHUNK_HEIGHT)
            return false;

        if (chunks[chunkCoord.x, chunkCoord.y] != null)
            return blockTypes[chunks[chunkCoord.x, chunkCoord.y].GetBlockTypeFromWorldVector3(voxelPosition)].isSolid;

        return blockTypes[GetVoxel(voxelPosition)].isSolid;
    }

    bool IsChunkInWorld(ChunkCoord chunkPosition)
    {
        return (chunkPosition.x >= 0 && chunkPosition.y >= 0) &&
               (chunkPosition.x < WORLD_WIDTH_IN_CHUNKS && chunkPosition.y < WORLD_WIDTH_IN_CHUNKS);
    }

    bool IsVoxelInWorld(Vector3 pos)
    {
        return (pos.x >= 0 && pos.y >= 0 && pos.z >= 0) &&
               (pos.x < WORLD_WIDTH_IN_VOXELS && pos.y < Chunk.CHUNK_HEIGHT && pos.z < WORLD_WIDTH_IN_VOXELS);
    }

    public byte GetVoxel(Vector3 voxelPosition)
    {
        if (!IsVoxelInWorld(voxelPosition))
            return 0;

        if ((voxelPosition.x + voxelPosition.y + voxelPosition.z) % 2 == 0)
            return 1;
        else
            return 1;
    }
}

[System.Serializable]
public class BlockType
{
    public string blockName;
    public bool isSolid;

    [Header("Texture Values")]
    public byte backFaceTextureID;
    public byte frontFaceTextureID;
    public byte topFaceTextureID;
    public byte bottomFaceTextureID;
    public byte leftFaceTextureID;
    public byte rightFaceTextureID;

    public byte GetTextureID(byte faceIndex)
    {
        switch (faceIndex)
        {
            case 0:
                return backFaceTextureID;
            case 1:
                return frontFaceTextureID;
            case 2:
                return topFaceTextureID;
            case 3:
                return bottomFaceTextureID;
            case 4:
                return leftFaceTextureID;
            case 5:
                return rightFaceTextureID;
            default:
                Debug.Log("Invalid face index for the get texture of the blocktype");
                return 0;
        }
    }
}
