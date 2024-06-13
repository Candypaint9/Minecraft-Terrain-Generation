using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.IO;
using System;
using System.Linq;
using System.Diagnostics;

public class World : MonoBehaviour
{
    public Settings settings;

    public Material material;
    public Material transparentMaterial;
    public Material waterMaterial;

    public BlockType[] blockTypes;
    public Transform player;
    public GameObject debugScreen;

    public BiomeAttributes[] biomes;

    [HideInInspector]
    public static int seed;

    List<Vector2Int> activeChunks = new List<Vector2Int>();
    public List<Vector2Int> chunksToCreate = new List<Vector2Int>();

    public Dictionary<Vector3, byte> extraPlantsToDraw = new Dictionary<Vector3, byte>();
    public bool arePlantsBeingAdded;

    [HideInInspector]
    public List<Chunk> chunksToUpdate_1 = new List<Chunk>();
    [HideInInspector]
    public List<Chunk> chunksToUpdate_2 = new List<Chunk>();
    [HideInInspector]
    public List<Chunk> chunksToUpdate_3 = new List<Chunk>();

    [HideInInspector]
    public Queue<Chunk> chunkMeshesToCreate = new Queue<Chunk>();

    [HideInInspector]
    public Queue<Chunk> chunkVoxelsToCreate_1 = new Queue<Chunk>();    
    [HideInInspector]
    public Queue<Chunk> chunkVoxelsToCreate_2 = new Queue<Chunk>();
    [HideInInspector]
    public Queue<Chunk> chunkVoxelsToCreate_3 = new Queue<Chunk>();
    [HideInInspector]
    public int chunkCount = 0;
    public int chunkUpdateCount = 0;

    [HideInInspector]
    public Dictionary<Vector2Int, Chunk> chunkObjectsList = new Dictionary<Vector2Int, Chunk>();

    [HideInInspector]
    public Vector2Int playerChunkCoord;
    Vector2Int playerPrevChunkCoord;

    Thread updateChunksThread_1;
    [HideInInspector]
    public object chunkUpdateThreadLock_1 = new object();
    bool isUpdateChunksThreadRunning_1;
    
    Thread updateChunksThread_2;
    [HideInInspector]
    public object chunkUpdateThreadLock_2 = new object();
    bool isUpdateChunksThreadRunning_2;
        
    Thread updateChunksThread_3;
    [HideInInspector]
    public object chunkUpdateThreadLock_3 = new object();
    bool isUpdateChunksThreadRunning_3;

    Thread chunkVoxelsThread_1;
    [HideInInspector]
    public object chunkVoxelThreadLock_1 = new object();
    bool isChunkVoxelThreadRunning_1;
    
    Thread chunkVoxelsThread_2;
    [HideInInspector]
    public object chunkVoxelThreadLock_2 = new object();
    bool isChunkVoxelThreadRunning_2;  
    
    Thread chunkVoxelsThread_3;
    [HideInInspector]
    public object chunkVoxelThreadLock_3 = new object();
    bool isChunkVoxelThreadRunning_3;

    ManualResetEvent _pauseEventChunkUpdate_1 = new ManualResetEvent(true);
    ManualResetEvent _pauseEventChunkUpdate_2 = new ManualResetEvent(true);
    ManualResetEvent _pauseEventChunkUpdate_3 = new ManualResetEvent(true);

    ManualResetEvent _pauseEventChunkVoxel_1 = new ManualResetEvent(true);
    ManualResetEvent _pauseEventChunkVoxel_2 = new ManualResetEvent(true);
    ManualResetEvent _pauseEventChunkVoxel_3 = new ManualResetEvent(true);


    private void Awake()
    {
        updateChunksThread_1 = new Thread(new ThreadStart(ThreadedChunkUpdate_1));
        updateChunksThread_1.Start();
        isUpdateChunksThreadRunning_1 = true;

        updateChunksThread_2 = new Thread(new ThreadStart(ThreadedChunkUpdate_2));
        updateChunksThread_2.Start();
        isUpdateChunksThreadRunning_2 = true;

        updateChunksThread_3 = new Thread(new ThreadStart(ThreadedChunkUpdate_3));
        updateChunksThread_3.Start();
        isUpdateChunksThreadRunning_3 = true;
        
        chunkVoxelsThread_1 = new Thread(new ThreadStart(ThreadedCreateChunkVoxels_1));
        chunkVoxelsThread_1.Start();
        isChunkVoxelThreadRunning_1 = true;        

        chunkVoxelsThread_2 = new Thread(new ThreadStart(ThreadedCreateChunkVoxels_2));
        chunkVoxelsThread_2.Start();
        isChunkVoxelThreadRunning_2 = true;

        chunkVoxelsThread_3 = new Thread(new ThreadStart(ThreadedCreateChunkVoxels_3));
        chunkVoxelsThread_3.Start();
        isChunkVoxelThreadRunning_3 = true;

        seed = UnityEngine.Random.Range(-10000, 10000);
        UnityEngine.Debug.Log(seed);

        CheckChunksInRenderDistance();
    }


    private void Start()
    {
        player.transform.position = new Vector3(0, 80, 0);

        playerPrevChunkCoord = GetChunkCoordsFromWorldPos(player.transform.position);
    }


    private void Update()
    {
        //for chunk generating
        playerChunkCoord = GetChunkCoordsFromWorldPos(player.transform.position);
        if (playerChunkCoord != playerPrevChunkCoord)
        {
            CheckChunksInRenderDistance();
            playerPrevChunkCoord = playerChunkCoord;
        }


        if (chunksToCreate.Count > 0)
        {
            CreateChunksFromList();
        }
        if (chunkMeshesToCreate.Count > 0)
        {
            if (chunkMeshesToCreate.Peek().canBeEdited())
            {
                chunkMeshesToCreate.Dequeue().CreateMeshObject();
            }
        }

        if (!isUpdateChunksThreadRunning_1 && chunksToUpdate_1.Count > 0)
        {
            isUpdateChunksThreadRunning_1 = true;
            Resume(_pauseEventChunkUpdate_1);
        } 
        if (!isUpdateChunksThreadRunning_2 && chunksToUpdate_2.Count > 0)
        {
            isUpdateChunksThreadRunning_2 = true;
            Resume(_pauseEventChunkUpdate_2);
        } 
        if (!isUpdateChunksThreadRunning_3 && chunksToUpdate_3.Count > 0)
        {
            isUpdateChunksThreadRunning_3 = true;
            Resume(_pauseEventChunkUpdate_3);
        } 


        if (!isChunkVoxelThreadRunning_1 && chunkVoxelsToCreate_1.Count > 0)
        {
            isChunkVoxelThreadRunning_1 = true;
            Resume(_pauseEventChunkVoxel_1);
        }
        if (!isChunkVoxelThreadRunning_2 && chunkVoxelsToCreate_2.Count > 0)
        {
            isChunkVoxelThreadRunning_2 = true;
            Resume(_pauseEventChunkVoxel_2);
        }
        if (!isChunkVoxelThreadRunning_3 && chunkVoxelsToCreate_3.Count > 0)
        {
            isChunkVoxelThreadRunning_3 = true;
            Resume(_pauseEventChunkVoxel_3);
        }


        //for the debug screen
        if (Input.GetKeyDown(KeyCode.F3)) 
        {
            debugScreen.SetActive(!debugScreen.activeSelf);
        }
    }


    private void OnApplicationQuit()
    {
        updateChunksThread_1.Abort();
        updateChunksThread_2.Abort();
        chunkVoxelsThread_1.Abort();
        chunkVoxelsThread_2.Abort();
        chunkVoxelsThread_3.Abort();
    }


    // FUNCTIONS FOR THREADING
    public void Pause(ManualResetEvent _event)
    {
        _event.Reset();
    }
    public void Resume(ManualResetEvent _event)
    {
        _event.Set();
    }



    //CHUNK GEN FUNCTIONS: STARTS

    void ThreadedCreateChunkVoxels_1()
    {
        while (true)
        {
            _pauseEventChunkVoxel_1.WaitOne(Timeout.Infinite);

            if (chunkVoxelsToCreate_1.Count > 0)
            {
                _createChunkVoxels_1();
            }

            else if (chunkVoxelsToCreate_1.Count == 0 && isChunkVoxelThreadRunning_1)
            {
                isChunkVoxelThreadRunning_1 = false;
                Pause(_pauseEventChunkVoxel_1);
            }
        }
    }
    void _createChunkVoxels_1()
    {
        lock (chunkVoxelThreadLock_1)
        {
            chunkVoxelsToCreate_1.Dequeue().CreateVoxelData();
        }
    }
    
    void ThreadedCreateChunkVoxels_2()
    {
        while (true)
        {
            _pauseEventChunkVoxel_2.WaitOne(Timeout.Infinite);

            if (chunkVoxelsToCreate_2.Count > 0)
            {
                _createChunkVoxels_2();
            }

            else if (chunkVoxelsToCreate_2.Count == 0 && isChunkVoxelThreadRunning_2)
            {
                isChunkVoxelThreadRunning_2 = false;
                Pause(_pauseEventChunkVoxel_2);
            }
        }
    }
    void _createChunkVoxels_2()
    {
        lock (chunkVoxelThreadLock_2)
        {
            chunkVoxelsToCreate_2.Dequeue().CreateVoxelData();
        }
    }    

    void ThreadedCreateChunkVoxels_3()
    {
        while (true)
        {
            _pauseEventChunkVoxel_3.WaitOne(Timeout.Infinite);

            if (chunkVoxelsToCreate_3.Count > 0)
            {
                _createChunkVoxels_3();
            }

            else if (chunkVoxelsToCreate_3.Count == 0 && isChunkVoxelThreadRunning_3)
            {
                isChunkVoxelThreadRunning_3 = false;
                Pause(_pauseEventChunkVoxel_3);
            }
        }
    }
    void _createChunkVoxels_3()
    {
        lock (chunkVoxelThreadLock_3)
        {
            chunkVoxelsToCreate_3.Dequeue().CreateVoxelData();
        }
    }

    void ThreadedChunkUpdate_1()
    {
        while (true)
        {
            _pauseEventChunkUpdate_1.WaitOne(Timeout.Infinite);

            if (chunksToUpdate_1.Count > 0)
            {
                _updateChunks_1();
            }

            else if (chunksToUpdate_1.Count == 0 && isUpdateChunksThreadRunning_1)
            {
                isUpdateChunksThreadRunning_1 = false;
                Pause(_pauseEventChunkUpdate_1);
            }
        }
    }
    void _updateChunks_1()
    {
        lock (chunkUpdateThreadLock_1)
        {
            if (chunksToUpdate_1[0].canBeEdited())
            {
                chunksToUpdate_1[0].CreateOrUpdateMeshData();
                
                chunksToUpdate_1.RemoveAt(0);
            }
        }
    }
    
    void ThreadedChunkUpdate_2()
    {
        while (true)
        {
            _pauseEventChunkUpdate_2.WaitOne(Timeout.Infinite);

            if (chunksToUpdate_2.Count > 0)
            {                
                _updateChunks_2();
            }

            else if (chunksToUpdate_2.Count == 0 && isUpdateChunksThreadRunning_2)
            {
                isUpdateChunksThreadRunning_2 = false;
                Pause(_pauseEventChunkUpdate_2);
            }
        }
    }
    void _updateChunks_2()
    {
        lock (chunkUpdateThreadLock_2)
        {
            if (chunksToUpdate_2[0].canBeEdited())
            {
                chunksToUpdate_2[0].CreateOrUpdateMeshData();
                
                chunksToUpdate_2.RemoveAt(0);
            }
        }
    }
    
    void ThreadedChunkUpdate_3()
    {
        while (true)
        {
            _pauseEventChunkUpdate_3.WaitOne(Timeout.Infinite);

            if (chunksToUpdate_3.Count > 0)
            {                
                _updateChunks_3();
            }

            else if (chunksToUpdate_3.Count == 0 && isUpdateChunksThreadRunning_3)
            {
                isUpdateChunksThreadRunning_3 = false;
                Pause(_pauseEventChunkUpdate_3);
            }
        }
    }
    void _updateChunks_3()
    {
        lock (chunkUpdateThreadLock_3)
        {
            if (chunksToUpdate_3[0].canBeEdited())
            {
                chunksToUpdate_3[0].CreateOrUpdateMeshData();
                
                chunksToUpdate_3.RemoveAt(0);
            }
        }
    }

    public void SplitBetweenChunksToUpdate(int priority, Chunk chunk)
    {
        Math.DivRem(chunkUpdateCount, 3, out int rem);

        if (rem == 0)
        {
            if (priority == 1)
            {
                lock (chunkUpdateThreadLock_1)
                {
                    chunksToUpdate_1.Add(chunk);
                }
            }
            if (priority == 0)
            {
                lock (chunkUpdateThreadLock_1)
                {
                    chunksToUpdate_1.Insert(0, chunk);
                }
            }
        }
        else if (rem == 1)
        {
            if (priority == 1)
            {
                lock (chunkUpdateThreadLock_2)
                {
                    chunksToUpdate_2.Add(chunk);
                }
            }
            if (priority == 0)
            {
                lock (chunkUpdateThreadLock_2)
                {
                    chunksToUpdate_2.Insert(0, chunk);
                }
            }
        } 
        else if (rem == 2)
        {
            if (priority == 1)
            {
                lock (chunkUpdateThreadLock_3)
                {
                    chunksToUpdate_3.Add(chunk);
                }
            }
            if (priority == 0)
            {
                lock (chunkUpdateThreadLock_3)
                {
                    chunksToUpdate_3.Insert(0, chunk);
                }
            }
        }

        chunkUpdateCount++;
    }



    void AddPlantsToChunks()
    {
        arePlantsBeingAdded = true;

        for (int i = 0; i < extraPlantsToDraw.Count; i++)
        {
            Vector3 pos = extraPlantsToDraw.Keys.ElementAt(i);
            byte block = extraPlantsToDraw[pos];
            Vector2Int chunkCoords = GetChunkCoordsFromWorldPos(pos);

            if (chunkObjectsList.ContainsKey(chunkCoords))
            {
                Chunk chunk = chunkObjectsList[chunkCoords];
                
                chunk.plants[pos] = block;
            }
        }

        arePlantsBeingAdded = false;
    }


    void CreateChunksFromList()
    {
        Vector2Int coords = chunksToCreate[0];

        AddPlantsToChunks();

        chunkObjectsList[coords].CreateChunk(coords);

        chunksToCreate.RemoveAt(0);
    }


    void CheckChunksInRenderDistance()
    {
        List<Vector2Int> previouslyActiveCreatedChunks = new List<Vector2Int>(activeChunks);

        //loop through all chunks in render distance
        for (int x = playerChunkCoord.x - settings.renderDist; x <= playerChunkCoord.x + settings.renderDist; x++)
        {
            for (int z = playerChunkCoord.y - settings.renderDist; z <= playerChunkCoord.y + settings.renderDist; z++)
            {
                Vector2Int pos = new Vector2Int(x, z);

                if (!activeChunks.Contains(pos))
                {
                    //if there is no gameobject in the position in the list
                    if (!chunkObjectsList.Keys.Contains(pos))
                    {
                        chunksToCreate.Add(pos);

                        chunkObjectsList.Add(pos, new Chunk(pos, this));
                    }
                    if (chunkObjectsList.Keys.Contains(pos) && !chunkObjectsList[pos].isActive)
                    {
                        chunkObjectsList[pos].isActive = true;
                    }

                    activeChunks.Add(pos);
                }


                if (previouslyActiveCreatedChunks.Contains(pos))
                {
                    previouslyActiveCreatedChunks.Remove(pos);
                }
            }
        }

        //disabling chunks not in render distance
        for (int i = 0; i < previouslyActiveCreatedChunks.Count; i++)
        {
            Vector2Int pos = previouslyActiveCreatedChunks[i];

            chunkObjectsList[pos].isActive = false;
            activeChunks.Remove(pos);
        }
    }


    public Vector2Int GetChunkCoordsFromWorldPos(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x / VoxelData.chunkWidth);
        int z = Mathf.FloorToInt(pos.z / VoxelData.chunkWidth);
        return new Vector2Int(x, z);
    }
    
    //CHUNK GEN FUNCTIONS: END


    public byte GetBlockIndexFromName(string name)
    {
        return name switch
        {
            "Air" => 0,
            "Water" => 1,
            "Bedrock" => 2,
            "Grass" => 3,
            "Dirt" => 4,
            "Stone" => 5,
            "Sand" => 6,
            "Snow" => 7,
            "ForestGrass" => 8,
            "Log" => 9,
            "Leaves" => 10,
            _ => 0
        };
    }



    public BiomeAttributes[] GetBiomesOfSurroundingBlocks(Vector2 pos)
    {
        BiomeAttributes[] _biomeArray = new BiomeAttributes[4];

        for (int i = 0; i < 4; i++)
        {
            _biomeArray[i] = Noise.GetBiome(VoxelData._posArray[i] + pos, this);
        }

        return _biomeArray;
    }

    public BiomeAttributes OptimiszedGetBiomeOfPos(Vector2 pos, BiomeAttributes[] _biomeArray)
    {
        BiomeAttributes biome = null;

        for (int count = 0, i = 0; i < 4; i++)
        {
            if (_biomeArray[i] == _biomeArray[0])
            {
                count++;
            }

            if (count == 4)
            {
                biome = _biomeArray[0];
            }
            else if (count != 4)
            {
                biome = Noise.GetBiome(pos, this);
            }
        }    
    
        return biome;
    }

    public byte GetVoxelType(Vector3 pos, BiomeAttributes biome, BiomeAttributes[] _biomeArray)
    {
        int yPos = Mathf.FloorToInt(pos.y);
        Vector2Int posVector2 = new Vector2Int(Mathf.FloorToInt(pos.x),
                                            Mathf.FloorToInt(pos.z));


        #region BasicReturns
        //If y is 0 set bedrock
        if (yPos == 0)
        {
            return GetBlockIndexFromName("Bedrock");
        }
        if (yPos < VoxelData.minNaturalTerrainHeight)
        {
            return GetBlockIndexFromName("Stone");
        }
        #endregion

        #region DecidingBiome
        if (biome == null)
        {
            biome = Noise.GetBiome(posVector2, this);
        }
        if (_biomeArray == null)
        {
            _biomeArray = GetBiomesOfSurroundingBlocks(posVector2);
        }
        #endregion

        #region GettingTerrainHeight
        int terrainHeight = 0;
        int count = 0;
        int sum = 0;

        for (int i = 0; i < 4; i++)
        {
            if (_biomeArray[i] != biome)
            {
                count += 1;
            }
        }
        if (count == 0)
        {
            terrainHeight = Noise.GetBiomeHeightNoise(posVector2, biome, FastNoiseLite.NoiseType.OpenSimplex2S, this);
        }
        else if (count != 0)
        {
            for (int i = 0; i < 4; i++)
            {
                sum += Noise.GetBiomeHeightNoise(VoxelData._posArray[i] + posVector2, _biomeArray[i], FastNoiseLite.NoiseType.OpenSimplex2S, this);
            }
            terrainHeight = sum / 4;
        }
        #endregion
        
        #region GettingTrees
        if (yPos == (terrainHeight - 1) + 1 && terrainHeight >= BiomeMapValues.minGroundHeight)    //as highest block is at (terrain height -1)
        {
            if (Noise.GetPlantNoise(posVector2, biome, this))
            {
                PlantGeneration.MakePlant(pos, biome, this);
            }
        }
        #endregion
        

        #region SettingBlocks
        //water
        if (yPos >= terrainHeight && yPos < BiomeMapValues.minGroundHeight)
        {
            return GetBlockIndexFromName("Water");
        }
        //default blocks
        if (yPos < terrainHeight - 3)
        {
            return GetBlockIndexFromName("Stone");
        }
        else if (yPos < terrainHeight - 1 && yPos >= terrainHeight - 4)
        {
            return GetBlockIndexFromName(biome.subSurfaceBlock);
        }
        else if (yPos == terrainHeight - 1)
        {
            return GetBlockIndexFromName(biome.surfaceBlock);
        }

        return GetBlockIndexFromName("Air");
        #endregion
    }



    //returns true if air present
    public bool[] isAirPresent(Vector3 pos, BiomeAttributes biome, BiomeAttributes[] _biomeArray)
    {
        Vector2Int chunkCoords = GetChunkCoordsFromWorldPos(pos);

        if (!chunkObjectsList.Keys.Contains(chunkCoords) || !chunkObjectsList[chunkCoords].canBeEdited())
        {
            //uses noise function to check if block present when chunks havent loaded
            byte returnedBlock = GetVoxelType(pos, biome, _biomeArray);


            return new bool[]
            {
                blockTypes[returnedBlock].isAir,
                blockTypes[returnedBlock].isTransparent,
                blockTypes[returnedBlock].isWater
            };
        }

        return chunkObjectsList[chunkCoords].CheckBlockFromGlobalPos(pos);
    }
}



[System.Serializable]
public class BlockType
{
    public string blockName;
    public bool isTransparent;
    public bool isAir;
    public bool isWater;

    [Header("Texture Values")]
    public int sideFaceTexture;
    public int topFaceTexture;
    public int bottomFaceTexture;

    //each texture has a texture ID starting from 0 to whatever 
    //0 is the bottom left most texture and 1 is the next one...
    //after we reach the end of the row the next one is the one above the first texture


    //gets the texutre for a particular face
    public int GetTextureID(int faceIndex)
    {
        return faceIndex switch
        {
            0 => sideFaceTexture,
            1 => sideFaceTexture,
            2 => sideFaceTexture,
            3 => sideFaceTexture,
            4 => topFaceTexture,
            5 => bottomFaceTexture,
            _ => 0,
        };
    }
}



[System.Serializable]
public class Settings
{
    [Range(0.5f, 10f)]
    public float mouseSens;
    [Range(1, 20)]
    public int renderDist;
}