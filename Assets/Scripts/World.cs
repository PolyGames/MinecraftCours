using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
    public static float GRAVITY = -9.8f;

    public const int WORLD_WIDTH_IN_CHUNKS = 200;
    public const int VIEW_DISTANCE_IN_CHUNKS = 15;

    public Material material;

    public BlockType[] blockTypes;

    Chunk[,] chunks = new Chunk[WORLD_WIDTH_IN_CHUNKS, WORLD_WIDTH_IN_CHUNKS];

    public bool bypassRemovingInnerFacesChunks = true;

    [SerializeField]
    Transform playerTransform;

    ChunkCoord playerChunkCoord;
    ChunkCoord playerLastChunkCoord;

    List<ChunkCoord> activeChunks = new List<ChunkCoord>();
    List<ChunkCoord> chunksToCreate = new List<ChunkCoord>();

    bool isCreatingChunks;

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
        playerLastChunkCoord = new ChunkCoord(playerTransform.position);
    }

    public Chunk GetChunkFromVector3(Vector3 worldPosition)
    {
        int x = Mathf.FloorToInt(worldPosition.x / Chunk.CHUNK_WIDTH);
        int y = Mathf.FloorToInt(worldPosition.z / Chunk.CHUNK_WIDTH);

        return chunks[x, y];
    }

    void GenerateWorld()
    {
        for (int i = (WORLD_WIDTH_IN_CHUNKS / 2) - VIEW_DISTANCE_IN_CHUNKS; i < (WORLD_WIDTH_IN_CHUNKS / 2) + VIEW_DISTANCE_IN_CHUNKS; i++)
        {
            for (int j = (WORLD_WIDTH_IN_CHUNKS / 2) - VIEW_DISTANCE_IN_CHUNKS; j < (WORLD_WIDTH_IN_CHUNKS / 2) + VIEW_DISTANCE_IN_CHUNKS; j++)
            {
                chunks[i, j] = new Chunk(this, new ChunkCoord(i, j), true);
                activeChunks.Add(new ChunkCoord(i, j));
            }
        }
    }

    public bool CheckForVoxel(Vector3 voxelPosition)
    {
        ChunkCoord chunkCoord = new ChunkCoord(voxelPosition);

        if (!IsVoxelInWorld(voxelPosition) || !IsChunkInWorld(chunkCoord) || voxelPosition.y < 0 || voxelPosition.y > Chunk.CHUNK_HEIGHT)
            return false;

        if (chunks[chunkCoord.x, chunkCoord.y] != null && chunks[chunkCoord.x, chunkCoord.y].isVoxelMapPopulated)
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

    void Update()
    {
        playerChunkCoord = new ChunkCoord(playerTransform.position);
        if (!playerChunkCoord.Equals(playerLastChunkCoord))
        {
            CheckViewDistance();
        }

        if (chunksToCreate.Count > 0 && !isCreatingChunks)
        {
            StartCoroutine("CreateChunks");
        }
    }

    IEnumerator CreateChunks()
    {
        isCreatingChunks = true;

        while (chunksToCreate.Count > 0)
        {
            chunks[chunksToCreate[0].x, chunksToCreate[0].y].InitChunk();
            chunksToCreate.RemoveAt(0);

            yield return null;
        }

        isCreatingChunks = false;
    }

    void CheckViewDistance()
    {
        playerLastChunkCoord = playerChunkCoord;

        List<ChunkCoord> previouslyActiveChunks = new List<ChunkCoord>(activeChunks);

        for (int i = playerChunkCoord.x - VIEW_DISTANCE_IN_CHUNKS; i < playerChunkCoord.x + VIEW_DISTANCE_IN_CHUNKS; i++)
        {
            for (int j = playerChunkCoord.y - VIEW_DISTANCE_IN_CHUNKS; j < playerChunkCoord.y + VIEW_DISTANCE_IN_CHUNKS; j++)
            {
                ChunkCoord currentCoord = new ChunkCoord(i, j);
                if (IsChunkInWorld(currentCoord))
                {
                    if (chunks[i, j] == null)
                    {
                        chunks[i, j] = new Chunk(this, currentCoord, false);
                        chunksToCreate.Add(currentCoord);
                    }
                    else if (!chunks[i, j].isActive)
                    {
                        chunks[i, j].isActive = true;
                    }
                    activeChunks.Add(currentCoord);

                    for (int k = 0; k < previouslyActiveChunks.Count; k++)
                    {
                        if (previouslyActiveChunks[k].Equals(currentCoord))
                        {
                            previouslyActiveChunks.RemoveAt(k);
                        }
                    }
                }
            }
        }

        foreach (ChunkCoord chunk in previouslyActiveChunks)
        {
            chunks[chunk.x, chunk.y].isActive = false;
        }
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
