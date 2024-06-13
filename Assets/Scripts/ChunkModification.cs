using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkModification : MonoBehaviour
{
    public GameObject blockHightlight;
    public Transform world;
    World worldScript;
    Transform camera;

    //for breaking block using manual raycast
    public float increase;

    Vector3 placeBlockPos;
    public Vector3 blockHighlightPos;

    byte selectedBlock;


    private void Start()
    {
        worldScript = world.GetComponent<World>();
        camera = GetComponentInChildren<Camera>().transform;

        selectedBlock = worldScript.GetBlockIndexFromName("Leaves");
    }


    private void Update()
    {
        blockHighlightPos = blockHightlight.transform.position;

        SelectAndHighlightBlock();

        if (Input.GetMouseButtonDown(0) && blockHightlight.activeSelf)
        {
            BreakBlock();
        }
        if (Input.GetMouseButtonDown(1) && blockHightlight.activeSelf)
        {
            PlaceBlocks();
        }
    }



    void BreakBlock()
    {
        int x = Mathf.FloorToInt(blockHighlightPos.x);
        int y = Mathf.FloorToInt(blockHighlightPos.y);
        int z = Mathf.FloorToInt(blockHighlightPos.z);
        Vector3Int pos = new Vector3Int(x, y, z);

        EditAndUpdatetVoxelData(pos, 0);
    }


    void EditAndUpdatetVoxelData(Vector3Int pos, byte replacementBlock)
    {
        Vector2Int chunkPos = worldScript.GetChunkCoordsFromWorldPos(pos);

        if (worldScript.chunkObjectsList[chunkPos].canBeEdited())
        {
            Vector3Int blockPos = pos - new Vector3Int(chunkPos.x * VoxelData.chunkWidth, 0, chunkPos.y * VoxelData.chunkWidth);
            worldScript.chunkObjectsList[chunkPos].BreakOrPlaceBlocks(blockPos, replacementBlock);
        }
    }


    void PlaceBlocks()
    {
        int x = Mathf.FloorToInt(placeBlockPos.x);
        int y = Mathf.FloorToInt(placeBlockPos.y);
        int z = Mathf.FloorToInt(placeBlockPos.z);
        Vector3Int pos = new Vector3Int(x, y, z);

        EditAndUpdatetVoxelData(pos, selectedBlock);
    }


    void SelectAndHighlightBlock()
    {
        float currentDist = increase;

        while(currentDist < VoxelData.blockReach)
        {
            Vector3 currentRayPos = camera.position + camera.forward * currentDist;

            bool[] posCheck = worldScript.isAirPresent(currentRayPos, null, null);
            bool isPosAir = posCheck[0];
            bool isPosWater = posCheck[2];

            if (!isPosAir && !isPosWater)
            {
                blockHightlight.transform.position = new Vector3(Mathf.FloorToInt(currentRayPos.x),
                                                        Mathf.FloorToInt(currentRayPos.y),
                                                        Mathf.FloorToInt(currentRayPos.z));

                blockHightlight.SetActive(true);

                //return so the highlight block isActive stays true
                return;
            }
            else
            {
                placeBlockPos = new Vector3(Mathf.FloorToInt(currentRayPos.x),
                                    Mathf.FloorToInt(currentRayPos.y),
                                    Mathf.FloorToInt(currentRayPos.z));
            }

            currentDist += increase;
        }

        blockHightlight.SetActive(false);
    }
}
