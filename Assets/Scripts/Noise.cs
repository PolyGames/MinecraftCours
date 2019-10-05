using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Noise
{
    public static float Get2DNoise(Vector2 position, float offset, float scale)
    {
        return Mathf.PerlinNoise((position.x + 0.01f) / Chunk.CHUNK_WIDTH * scale + offset,
                                 (position.y + 0.01f) / Chunk.CHUNK_WIDTH * scale + offset);
    }
}
