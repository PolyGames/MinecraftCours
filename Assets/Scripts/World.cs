using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
    public static float GRAVITY = -9.8f;

    public const int WORLD_WIDTH_IN_CHUNKS = 6;

    public Material material;

    public BlockType[] blockTypes;

    Chunk[,] chunks = new Chunk[WORLD_WIDTH_IN_CHUNKS, WORLD_WIDTH_IN_CHUNKS];

    public bool bypassRemovingInnerFacesChunks = true;

    [SerializeField]
    Transform playerTransform;

    public static int WORLD_WIDTH_IN_VOXELS
    {
        get { return WORLD_WIDTH_IN_CHUNKS * Chunk.CHUNK_WIDTH; }

    }

    // Start is called before the first frame update
    void Start()
    {
        playerTransform.position = new Vector3(WORLD_WIDTH_IN_CHUNKS * Chunk.CHUNK_WIDTH /2f,
                                               Chunk.CHUNK_HEIGHT - 2,
                                               WORLD_WIDTH_IN_CHUNKS * Chunk.CHUNK_WIDTH / 2f);
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

    bool IsVoxelInWorld(Vector3 voxelPositionWorld)
    {
        return (voxelPositionWorld.x >= 0 && voxelPositionWorld.y >= 0 && voxelPositionWorld.z >= 0) &&
               (voxelPositionWorld.x < WORLD_WIDTH_IN_VOXELS && voxelPositionWorld.y < Chunk.CHUNK_HEIGHT && voxelPositionWorld.z < WORLD_WIDTH_IN_VOXELS);
    }

    public byte GetVoxel(Vector3 voxelPosition)
    {
        if (!IsVoxelInWorld(voxelPosition))
            return 0;

        int y = Mathf.FloorToInt(voxelPosition.y);

        if (y == 0)
        {
            return 4;
        }

        int terrainHeight = Mathf.FloorToInt(Chunk.CHUNK_HEIGHT * 
                                Noise.Get2DNoise(new Vector2(voxelPosition.x, voxelPosition.z), 0, 0.25f));
        byte blockType = 0;

        if (y == terrainHeight)
        {
            blockType = 1;
        }
        if (y < terrainHeight)
        {
            if (y < terrainHeight - 5)
            {
                blockType = 3;
            }
            else
            {
                blockType = 2;
            }
        }

        return blockType;
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
