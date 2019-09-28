using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VoxelData
{
    public static readonly byte TextureAtlasSizeInBlocks = 4;

    public static float NormalizedBlockTextureSize
    {
        get { return 1f / (float)TextureAtlasSizeInBlocks; }
    }

    public static readonly Vector3[] voxelVertices = new Vector3[8]
    {
        new Vector3(0, 0, 0), // 0
        new Vector3(1, 0, 0), // 1
        new Vector3(1, 1, 0), // 2
        new Vector3(0, 1, 0), // 3
        new Vector3(0, 0, 1), // 4
        new Vector3(1, 0, 1), // 5
        new Vector3(1, 1, 1), // 6
        new Vector3(0, 1, 1), // 7
    };

    public static readonly int[,] voxelTriangles = new int[6, 4]
    {
        { 0, 3, 1, 2}, // back
        { 5, 6, 4, 7}, // front
        { 3, 7, 2, 6}, // top
        { 1, 5, 0, 4}, // bottom
        { 4, 7, 0, 3}, // left
        { 1, 2, 5, 6}, // right
    };

    public static readonly Vector2[] voxelUvs = new Vector2[6]
    {
        new Vector2(0, 0),
        new Vector2(0, 1),
        new Vector2(1, 0),
        new Vector2(1, 0),
        new Vector2(0, 1),
        new Vector2(1, 1),
    };

    public static readonly Vector3[] voxelFaceChecks = new Vector3[6]
    {
        new Vector3(0, 0, -1),
        new Vector3(0, 0, 1),
        new Vector3(0, 1, 0),
        new Vector3(0, -1, 0),
        new Vector3(1, 0, 0),
        new Vector3(-1, 0, 0),
    };
}
