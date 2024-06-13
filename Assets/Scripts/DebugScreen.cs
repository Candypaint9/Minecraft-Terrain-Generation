using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugScreen : MonoBehaviour
{
    public GameObject world;
    public GameObject player;
    ChunkModification chunkModification;
    World worldScript;
    Text textObject;

    float fps;
    float timer;


    private void Start()
    {
        worldScript = world.GetComponent<World>();
        textObject = GetComponent<Text>();
        chunkModification = player.GetComponent<ChunkModification>();
    }

    private void Update()
    {
        string debugText = "DEBUG MENU";
        debugText += "\n\n";
        debugText += "fps: " + fps;
        debugText += "\n\n";
        debugText += "XYZ: " + Mathf.FloorToInt(worldScript.player.transform.position.x) + " / "
                      + Mathf.FloorToInt(worldScript.player.transform.position.y) + " / "
                      + Mathf.FloorToInt(worldScript.player.transform.position.z);
        debugText += "\n\n";
        debugText += "Chunk-XZ: " + worldScript.playerChunkCoord.x + " / " + worldScript.playerChunkCoord.y;
        debugText += "\n\n";
        debugText += "Looking At XYZ: " + Mathf.FloorToInt(chunkModification.blockHighlightPos.x) + " / "
                      + Mathf.FloorToInt(chunkModification.blockHighlightPos.y) + " / "
                      + Mathf.FloorToInt(chunkModification.blockHighlightPos.z);

        textObject.text = debugText;

        if (timer >= 1f)
        {
            fps = (int)(1f / Time.unscaledDeltaTime);
            timer = 0;
        }
        timer += Time.deltaTime;
    }
}
