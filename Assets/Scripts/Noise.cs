using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public class Noise
{
    public static int GetBiomeHeightNoise(Vector2 pos, BiomeAttributes biome, FastNoiseLite.NoiseType noiseType, World world)
    {
        int x = Mathf.FloorToInt(pos.x);
        int z = Mathf.FloorToInt(pos.y);

        FastNoiseLite noiseLib = new FastNoiseLite();
        noiseLib.SetNoiseType(noiseType);
        noiseLib.SetSeed(World.seed);

        int octaves = biome.octaves;
        float frequency = biome.frequency;
        float amplitude = biome.amplitude;
        float lacunarity = biome.lacunarity;
        float persistance = biome.persistance;

        float heightNoise = 0;

        for (int i = 0; i < octaves; i++)
        {
            float X = x * frequency;
            float Z = z * frequency;

            heightNoise += noiseLib.GetNoise(X, Z) * amplitude;

            frequency *= lacunarity;
            amplitude *= persistance;
        }

        int returnValues = 0;

        //always return -ve so that it is always underwater level
        if (biome == world.biomes[0])
        {
            returnValues = Mathf.RoundToInt(- Mathf.Abs(heightNoise) + BiomeMapValues.minGroundHeight);
        }
        //to return only +ve values for above ground
        else
        {
            returnValues = Mathf.RoundToInt(Mathf.Abs(heightNoise) + BiomeMapValues.minGroundHeight);
        }

        return returnValues;
    }



    public static float GetNoiseForBiomeGen(FastNoiseLite noiseLib, int x, int z)
    {
        float noise = noiseLib.GetNoise(x * BiomeMapValues.frequency, z * BiomeMapValues.frequency);

        return noise;
    }


    public static BiomeAttributes GetBiome(Vector2 pos, World world)
    {
        int x = Mathf.FloorToInt(pos.x);
        int z = Mathf.FloorToInt(pos.y);

        FastNoiseLite noiseLib = new FastNoiseLite();
        noiseLib.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2S);


        //GET HEIGHT NOISE
        noiseLib.SetSeed(World.seed + BiomeMapValues.heightSeedOffset);
        float heightNoise = GetNoiseForBiomeGen(noiseLib, x, z);

        //GET HUMIDITY NOISE
        noiseLib.SetSeed(World.seed + BiomeMapValues.humiditySeedOffset);
        float humidNoise = GetNoiseForBiomeGen(noiseLib, x, z);

        //GET TEMP NOISE
        noiseLib.SetSeed(World.seed + BiomeMapValues.tempSeedOffset);
        float tempNoise = GetNoiseForBiomeGen(noiseLib, x, z);

        //biome points list
        int[] biomePoints = new int[world.biomes.Length];


        for (int i = 0; i < world.biomes.Length; i++)
        {
            float minHumidity = world.biomes[i].minHumidity;
            float maxHumidity = world.biomes[i].maxHumidity;
            float minHeight = world.biomes[i].minHeight;
            float maxHeight = world.biomes[i].maxHeight;
            float minTemp = world.biomes[i].minTemp;
            float maxTemp = world.biomes[i].maxTemp;

            if (heightNoise > minHeight && heightNoise < maxHeight)
            {
                biomePoints[i] += 1;
            } 
            if (humidNoise > minHumidity && humidNoise < maxHumidity)
            {
                biomePoints[i] += 1;
            }            
            if (tempNoise > minTemp && tempNoise < maxTemp)
            {
                biomePoints[i] += 1;
            }
        }

        List<int> points = biomePoints.ToList<int>();

        int index = points.IndexOf(points.Max());

        BiomeAttributes biome = world.biomes[index];

        return biome;
    }


    public static bool GetPlantNoise(Vector2Int pos, BiomeAttributes biome, World world)
    {
        FastNoiseLite noiseLib = new FastNoiseLite();
        noiseLib.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2S);
        noiseLib.SetSeed(BiomeMapValues.treeAreaSeedOffset);


        //to get tree areas
        float areaNoise = Mathf.Abs(noiseLib.GetNoise(pos.x * biome.plantAreaFrequency, pos.y * biome.plantAreaFrequency));

        bool[] surroundingAreaPlants = new bool[6];


        if (areaNoise > BiomeMapValues.plantAreaThreshold)
        {
            float treeNoise = Mathf.Abs(noiseLib.GetNoise(pos.x * BiomeMapValues.plantFrequency,
                                                          pos.y * BiomeMapValues.plantFrequency));

            if (treeNoise > biome.plantThreshold)
            {
                //checking around for trees, if yes then no tree here
                for (int i = 0; i < 4; i++)
                {
                    float offsetX = (pos + VoxelData.surroundingTreesCheck[i]).x;
                    float offsetZ = (pos + VoxelData.surroundingTreesCheck[i]).y;

                    if (Mathf.Abs(noiseLib.GetNoise(offsetX * BiomeMapValues.plantFrequency,
                                                    offsetZ * BiomeMapValues.plantFrequency))
                                                    > biome.plantThreshold)
                    {
                        return false;
                    }
                }


                return true;
            }

            return false;
        }

        return false;
    }
}