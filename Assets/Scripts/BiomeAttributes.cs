using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BiomeAttributes", menuName = "Minecraft/Biome Attribute")]
public class BiomeAttributes : ScriptableObject
{
    public string biomeName;

    [Header("Specific Biome Values")]
    [Range(-1f, 1f)]
    public float minHumidity;
    [Range(-1f, 1f)]
    public float maxHumidity;
    [Range(-1f, 1f)]
    public float minHeight;
    [Range(-1f, 1f)]
    public float maxHeight;
    [Range(-1f, 1f)]
    public float minTemp;
    [Range(-1f, 1f)]
    public float maxTemp;

    [Header("Biome Blocks")]
    public string surfaceBlock;
    public string subSurfaceBlock;

    [Header("Trees and Plants")]
    [Range(1, 15)]
    public float plantAreaFrequency;
    [Range(0, 1)]
    public float plantThreshold;
    [Range(1, 7)]
    public int plantTrunkHeight;

    [Header("Noise Values")]
    [Range(1, 20)]
    public int octaves;
    [Range(1f, 50f)]
    public float amplitude;
    [Range(0.01f, 7.50f)]
    public float frequency;
    [Range(1f,  5f)]
    public float lacunarity;
    [Range(0f, 1f)]
    public float persistance;
}



[System.Serializable]
public class Ore
{
    public string oreName;
    public byte blockID;

    public int minHeight;
    public int maxHeight;

    public int seedOffset;
}



public static class BiomeMapValues
{
    public static readonly int minGroundHeight = 62;

    //to get similar heights to actual world height
    public static readonly int heightSeedOffset = 0;
    public static readonly int humiditySeedOffset = 1;
    public static readonly int tempSeedOffset = 2;
    public static readonly int treeAreaSeedOffset = 3;
    public static readonly int treeSeedOffset = 4;

    //dec to get larger biomes
    public static readonly float frequency = 0.1f;

    //inc to get more blending(kepp between 1-5)
    public static readonly int biomeBlending = 3;

    public static readonly float plantAreaThreshold = 0.6f;
    public static readonly float plantFrequency = 100f;
}
