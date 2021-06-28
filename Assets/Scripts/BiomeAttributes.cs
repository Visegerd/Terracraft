using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Scripts/BiomeAttributes", menuName = "Terracraft/Biome Attributes")]
public class BiomeAttributes : ScriptableObject
{
    public string biomeName;
    public int solidGroundHeight;
    public int terrainHeight;
    public float terrainScale;
    [Header("Trees")]
    public float treeZoneScale=1.3f;
    [Range(0.001f,1.0f)]
    public float treeZoneThreshold=0.6f;
    public float treePlacementScale = 15.0f;
    [Range(0.001f, 1.0f)]
    public float treePlacementThreshold = 0.8f;
    public int maxTreeHeight = 26;
    public int minTreeHeight = 13;
    public Lode[] lodes;

}

[System.Serializable]
public class Lode
{
    public string nodeName;
    public byte blockID;
    public int minHeight;
    public int maxHeight;
    public float scale;
    public float threshold;
    public float noiseOffset;
}
