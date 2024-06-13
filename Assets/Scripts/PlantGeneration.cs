using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PlantGeneration
{
    public static void MakePlant(Vector3 pos, BiomeAttributes biome, World world)
    {
        #region trunk
        byte block;

        if (biome == world.biomes[2])
        {
            //CHANGE THIS TO CACTUS
            block = world.GetBlockIndexFromName("Log");
        }
        else
        { 
            block = world.GetBlockIndexFromName("Log");

            for (int i = 0; i < biome.plantTrunkHeight; i++)
            {
                Vector3 plantPos = new Vector3(pos.x, pos.y + i, pos.z);
                Vector2Int chunkCoords = world.GetChunkCoordsFromWorldPos(plantPos);

                if (world.chunkObjectsList.ContainsKey(chunkCoords))
                {
                    world.chunkObjectsList[chunkCoords].plants[plantPos] = block;
                }
                else
                {
                    world.extraPlantsToDraw[plantPos] = block;
                }
            }
        }

        #endregion


        #region Leaves
        if (biome == world.biomes[2])
        {
            return;
        }

        Vector3 startingPos = new Vector3(pos.x, pos.y + biome.plantTrunkHeight - 2, pos.z);

        List<Vector3> treeArray = new List<Vector3>();

        //bottom 2 layers
        for (int x = -2; x < 3; x++)
        {
            for (int z = -2; z < 3; z++)
            {
                for (int y = 0; y < 2; y++)
                {
                    Vector3 leafPosition = startingPos + new Vector3(x, y, z);

                    if (x == 0 && z == 0 && leafPosition.y <= startingPos.y + biome.plantTrunkHeight)
                    {
                        continue;
                    }
                    
                    treeArray.Add(leafPosition);
                }
            }
        }

        //3rd layer
        for (int x = -1; x < 2; x++)
        {
            for (int y = 2, z = -1; z < 2; z++)
            {
                Vector3 leafPosition = startingPos + new Vector3(x, y, z);

                treeArray.Add(leafPosition);
            }
        }

        //last layer
        treeArray.Add(startingPos + new Vector3(0, 3, 0));
        treeArray.Add(startingPos + new Vector3(1, 3, 0));
        treeArray.Add(startingPos + new Vector3(0, 3, 1));
        treeArray.Add(startingPos + new Vector3(-1, 3, 0));
        treeArray.Add(startingPos + new Vector3(0, 3, -1));

        AddLeavesToDict(world, treeArray, world.GetBlockIndexFromName("Leaves"));
        #endregion
    }


    static void AddLeavesToDict(World world, List<Vector3> list, byte block)
    {
        for (int i = 0; i < list.Count; i++)
        {
            Vector3 plantPos = list[i];
            Vector2Int chunkCoords = world.GetChunkCoordsFromWorldPos(plantPos);

            if (world.chunkObjectsList.ContainsKey(chunkCoords))
            {
                world.chunkObjectsList[chunkCoords].plants[plantPos] = block;
            }
            else
            {
                while (world.arePlantsBeingAdded)
                {

                }
                world.extraPlantsToDraw[plantPos] = block;
            }
        }
    }
}
