using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class Chunk
{
    World world;

    MeshFilter meshFilter;
    MeshRenderer meshRenderer;

    GameObject chunkObject;

    Material[] materials = new Material[3];

    public Dictionary<Vector3, byte> plants = new Dictionary<Vector3, byte>();

    List<Vector3> vertices = new List<Vector3>();
    List<Vector2> uvs = new List<Vector2>();
    List<Vector3> normals = new List<Vector3>();

    List<int> triangles = new List<int>();
    List<int> transparentTriangles = new List<int>();
    List<int> waterTriangles = new List<int>();

    byte[,,] voxelMap = new byte[VoxelData.chunkWidth, VoxelData.chunkHeight, VoxelData.chunkWidth];

    //to help add triangles to the triangles list
    int vertexIndex = 0;

    public Vector3 chunkPositionInWorld;

    bool isChunkActive;
    bool voxelGenDone = false;
    public bool meshGenDone = false;
    public bool plantGenDone = false;

    public Chunk(Vector2Int coords, World _world)
    {
        world = _world;
    }


    //CHUNKS WILL BE UPDATED FROM WORLD SCRIPT DUE TO THREADING
    public void CreateChunk(Vector2 coords)
    {
        chunkPositionInWorld = new Vector3(coords.x * VoxelData.chunkWidth, 0, coords.y * VoxelData.chunkWidth);

        chunkObject = new GameObject();

        meshRenderer = chunkObject.AddComponent<MeshRenderer>();
        meshFilter = chunkObject.AddComponent<MeshFilter>();

        chunkObject.transform.SetParent(world.transform);
        chunkObject.transform.position = chunkPositionInWorld;
        chunkObject.name = "Chunk (" + coords.x + ", " + coords.y + ")";

        materials[0] = world.material;
        materials[1] = world.transparentMaterial;
        materials[2] = world.waterMaterial;
        meshRenderer.materials = materials;


        Math.DivRem(world.chunkCount, 2, out int r);

        if (r == 0)
        {
            lock (world.chunkVoxelThreadLock_1)
            {
                world.chunkVoxelsToCreate_1.Enqueue(this);

                world.chunkCount++;
            }
        }
        if (r == 1)
        {
            lock (world.chunkVoxelThreadLock_2)
            {
                world.chunkVoxelsToCreate_2.Enqueue(this);

                world.chunkCount++;
            }
        }
        if (r == 2)
        {
            lock (world.chunkVoxelThreadLock_3)
            {
                world.chunkVoxelsToCreate_3.Enqueue(this);

                world.chunkCount++;
            }
        }
    }



    public bool isActive
    {
        get { return isChunkActive; }
        set
        {
            isChunkActive = value;
            if (chunkObject != null)
            {
                chunkObject.SetActive(value);
            }
        }
    }


    public bool canBeEdited()
    {
        if (!voxelGenDone)
        {
            return false;
        }

        return true;
    }


    //returns true if air
    public bool[] CheckBlockFromGlobalPos(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x) - Mathf.FloorToInt(chunkPositionInWorld.x);
        int y = Mathf.FloorToInt(pos.y);
        int z = Mathf.FloorToInt(pos.z) - Mathf.FloorToInt(chunkPositionInWorld.z);

        //check if block is out of index, if it is return air
        //no need to check x and z as we know the block is in this chunk
        if (y < 0 || y > 255)
        {
            return new bool[]
            {
                false, false, false
            };
        }

        return new bool[]
        {
            world.blockTypes[voxelMap[x, y, z]].isAir,
            world.blockTypes[voxelMap[x, y, z]].isTransparent,
            world.blockTypes[voxelMap[x, y, z]].isWater
        };
    }


    //returns true is there is air on pos
    //2nd element is true if transparent
    bool[] CheckVoxel(Vector3 pos, BiomeAttributes biome, BiomeAttributes[] _biomeArray)
    {
        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        int z = Mathf.FloorToInt(pos.z);

        if (y < 0 || y > 255)
        {
            return new bool[]
            {
                false, false, false
            };
        }
        if (x < 0 || z < 0 || x >= VoxelData.chunkWidth || z >= VoxelData.chunkWidth)
        {
            return world.isAirPresent(pos + chunkPositionInWorld, biome, _biomeArray);
        }

        return new bool[]
        {
            world.blockTypes[voxelMap[x, y, z]].isAir,
            world.blockTypes[voxelMap[x, y, z]].isTransparent,
            world.blockTypes[voxelMap[x, y, z]].isWater
        };
    }


    public void BreakOrPlaceBlocks(Vector3Int pos, byte newBlock)
    {
        //to prevent breaking bedrock
        if (pos.y > 0 && pos.y < VoxelData.chunkHeight)
        {
            voxelMap[pos.x, pos.y, pos.z] = newBlock;
        }

        //higher priority to update chunk rather than gen chunks far away
        world.SplitBetweenChunksToUpdate(0, this);

        UpdateSurroundingBlocks(pos);
    }


    void UpdateSurroundingBlocks(Vector3Int pos)
    {
        //checking only 4 blocks around not top and bottom
        for (int i = 0; i < 4; i++)
        {
            Vector3 checkingPos = pos + VoxelData.facesCheck[i];

            if (checkingPos.x < 0 || checkingPos.z < 0 || checkingPos.x >= VoxelData.chunkWidth || checkingPos.z >= VoxelData.chunkWidth)
            {
                Vector2Int chunkCoords = world.GetChunkCoordsFromWorldPos(checkingPos + chunkPositionInWorld);

                //higher priority to update chunk rather than gen chunks far away
                world.SplitBetweenChunksToUpdate(0, world.chunkObjectsList[chunkCoords]);
            }
        }
    }


    public void CreateVoxelData()
    {
        for (int z = 0; z < VoxelData.chunkWidth; z++)
        {
            for (int x = 0; x < VoxelData.chunkWidth; x++)
            {
                Vector2 posVector2 = new Vector2(x + chunkPositionInWorld.x, z + chunkPositionInWorld.z);
                BiomeAttributes[] _biomeArray = world.GetBiomesOfSurroundingBlocks(posVector2);
                BiomeAttributes biome = world.OptimiszedGetBiomeOfPos(posVector2, _biomeArray);

                for (int y = 0; y < VoxelData.maxNaturalTerrainHeight; y++)
                {
                    voxelMap[x, y, z] = world.GetVoxelType(new Vector3(x, y, z) + chunkPositionInWorld, biome, _biomeArray);
                }
            }
        }

        while (world.arePlantsBeingAdded)
        {

        }
        MakePlants();

        plantGenDone = true;
        voxelGenDone = true;

        world.SplitBetweenChunksToUpdate(1, this);
    }


    public void MakePlants()
    {
        for (int i = 0; i < plants.Count; i++)
        {
            Vector3 pos = plants.Keys.ElementAt(i);

            byte block = plants[pos];

            pos -= chunkPositionInWorld;

            int x = Mathf.FloorToInt(pos.x);
            int y = Mathf.FloorToInt(pos.y);
            int z = Mathf.FloorToInt(pos.z);

            voxelMap[x, y, z] = block;
        }
    }


    //call this function when creating or updating mesh
    public void CreateOrUpdateMeshData()
    {
        ClearMeshData();

        for (int z = 0; z < VoxelData.chunkWidth; z++)
        {
            for (int x = 0; x < VoxelData.chunkWidth; x++)
            {
                BiomeAttributes[] _biomeArray = new BiomeAttributes[4];
                _biomeArray = null;
                BiomeAttributes biome = null;


                for (int y = 0; y < VoxelData.chunkHeight; y++)
                {
                    Vector3 pos = new Vector3(x, y, z);
                    bool[] posCheck = CheckVoxel(pos, null, null);
                    bool isPosAir = posCheck[0];
                    bool isPosTransparent = posCheck[1];
                    bool isPosWater = posCheck[2];

                    //checks if the current block is air or not
                    if (!isPosAir)
                    {
                        for (int face = 0; face < 6; face++)
                        {
                            Vector3 facePos = pos + VoxelData.facesCheck[face];

                            bool[] facePosCheck = CheckVoxel(facePos, null, null);
                            bool isFacePosAir = facePosCheck[0];
                            bool isFacePosTransparent = facePosCheck[1];
                            bool isFacePosWater = facePosCheck[2];

                            //giving null as we dont know biome of face
                            if (isFacePosAir || isFacePosTransparent ||
                                isFacePosWater && !isPosWater)
                            {
                                //giving the function transparecy of the block whose faces are being drawn
                                AddData(pos, face, isPosTransparent, isPosWater);
                            }
                        }
                    }
                }
            }
        }

        //for threading
        lock (world.chunkMeshesToCreate)
        {
            world.chunkMeshesToCreate.Enqueue(this);
        }
    }


    //used to clear mesh before updating when blocks broken or placed
    void ClearMeshData()
    {
        vertexIndex = 0;
        vertices.Clear();
        uvs.Clear();
        normals.Clear();

        triangles.Clear();
        transparentTriangles.Clear();
        waterTriangles.Clear();
    }


    Vector2 GetTextureCoords(int textureID)
    {
        //each texture has a texture ID starting from 0 to whatever 
        //0 is the bottom left most texture and 1 is the next one...
        //after we reach the end of the row the next one is the one above the first texture

        //coordinates of texture(in 0, 1, 2...)
        int x = textureID % VoxelData.textureAtlasSizeInBlocks;
        int y = textureID / VoxelData.textureAtlasSizeInBlocks;

        //real chunkPositionInWorld of texture
        float X = x * VoxelData.normalizedBlockTextureSize;
        float Y = y * VoxelData.normalizedBlockTextureSize;

        return new Vector2(X, Y);
    }


    void AddData(Vector3 pos, int faceIndex, bool isTransparent, bool isWater)
    {
        int textureID = world.blockTypes[voxelMap[Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z)]].GetTextureID(faceIndex);
        Vector2 textureCoords = GetTextureCoords(textureID);

        for (int i = 0; i < 4; i++)
        {
            //reduce size of water block
            if (isWater)
            {
                vertices.Add(pos + VoxelData.voxelVerts[faceIndex][i] + new Vector3(0, -0.1f, 0));
            }
            else
            {
                vertices.Add(pos + VoxelData.voxelVerts[faceIndex][i]);
            }

            uvs.Add(textureCoords + VoxelData.voxelUvs[i]);

            //each normal is in same direction as facechecks
            normals.Add(VoxelData.facesCheck[faceIndex]);
        }


        if (isWater)
        {
            waterTriangles.Add(vertexIndex);
            waterTriangles.Add(vertexIndex + 1);
            waterTriangles.Add(vertexIndex + 2);
            waterTriangles.Add(vertexIndex + 2);
            waterTriangles.Add(vertexIndex + 1);
            waterTriangles.Add(vertexIndex + 3);
        }
        else if (isTransparent)
        {
            transparentTriangles.Add(vertexIndex);
            transparentTriangles.Add(vertexIndex + 1);
            transparentTriangles.Add(vertexIndex + 2);
            transparentTriangles.Add(vertexIndex + 2);
            transparentTriangles.Add(vertexIndex + 1);
            transparentTriangles.Add(vertexIndex + 3);
        }
        else
        {
            triangles.Add(vertexIndex);
            triangles.Add(vertexIndex + 1);
            triangles.Add(vertexIndex + 2);
            triangles.Add(vertexIndex + 2);
            triangles.Add(vertexIndex + 1);
            triangles.Add(vertexIndex + 3);
        }

        vertexIndex += 4;
    }


    public void CreateMeshObject()
    {
        //dont need to use mesh.clear as we are replacing old mesh anyways
        Mesh mesh = new Mesh
        {
            vertices = vertices.ToArray(),
            uv = uvs.ToArray(),
            normals = normals.ToArray(),
            subMeshCount = 3
        };

        mesh.SetTriangles(triangles, 0);
        mesh.SetTriangles(transparentTriangles, 1);
        mesh.SetTriangles(waterTriangles, 2);

        //mesh optimize breaks when more than one submesh used as different types of blocks are present
        if (transparentTriangles.Count == 0 && waterTriangles.Count == 0)
        {
            mesh.Optimize();
        }

        meshFilter.mesh = mesh;

        meshGenDone = true;
    }
}