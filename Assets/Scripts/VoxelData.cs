using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VoxelData
{
    public static readonly int ChunkWidth = 16;
    public static readonly int ChunkHeight = 16;
    public static readonly int WorldSizeInChunks = 80;
    public static readonly int WorldHeightInChunks = 80;
    public static readonly int ViewDistanceInChunks = 5;

    public static float minLightLevel = 0.1f;
    public static float maxLightLevel = 0.95f;
    public static float lightFalloff = 0.08f;

    public static int WorldSizeInVoxels
    {
        get { return WorldSizeInChunks * ChunkWidth; }
    }

    public static readonly int TextureAtlasSizeInBlocks = 4;
    public static float NormalizedBlockTextureSize
    {
        get { return 1.0f / (float)TextureAtlasSizeInBlocks; }
    }

    public static readonly Vector3[] voxelVerts = new Vector3[8]
    {
        new Vector3(0.0f,0.0f,0.0f),
        new Vector3(1.0f,0.0f,0.0f),
        new Vector3(0.0f,1.0f,0.0f),
        new Vector3(0.0f,0.0f,1.0f),
        new Vector3(1.0f,1.0f,0.0f),
        new Vector3(1.0f,0.0f,1.0f),
        new Vector3(0.0f,1.0f,1.0f),
        new Vector3(1.0f,1.0f,1.0f)
    };

    public static readonly Vector3[] faceChecks = new Vector3[6]
    {
        new Vector3(0.0f,1.0f,0.0f),
        new Vector3(0.0f,-1.0f,0.0f),
        new Vector3(0.0f,0.0f,-1.0f),
        new Vector3(0.0f,0.0f,1.0f),
        new Vector3(-1.0f,0.0f,0.0f),
        new Vector3(1.0f,0.0f,0.0f)
    };

    public static readonly int[,] voxelTris = new int[6, 4]
    {
        //top down front back left right
        {2,6,4,7}, //top
        {1,5,0,3}, //down
        {0,2,1,4}, //front
        {5,7,3,6}, //back
        {3,6,0,2}, //left
        {1,4,5,7}, //right
    };

    public static readonly Vector2[] voxelUvs = new Vector2[4]
    {
        new Vector2(0.0f,0.0f),
        new Vector2(0.0f,1.0f),
        new Vector2(1.0f,0.0f),
        new Vector2(1.0f,1.0f)
    };
}
