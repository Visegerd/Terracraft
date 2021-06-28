using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugScreen : MonoBehaviour
{
    World world;
    Text text;
    float frameRate;
    float timer;
    int halfWorldSizeInVoxels, halfWorldSizeInChunks;

    private void Start()
    {
        world = GameObject.Find("World").GetComponent<World>();
        text = GetComponent<Text>();
        halfWorldSizeInVoxels = VoxelData.WorldSizeInVoxels / 2;
        halfWorldSizeInChunks = VoxelData.WorldSizeInChunks / 2;
    }

    private void Update()
    {
        string debugText = "Debug Info:";
        debugText += "\n" + "fps " + frameRate + "\n\n";
        debugText += "XYZ: " + (Mathf.FloorToInt(world.player.transform.position.x) - halfWorldSizeInVoxels) + " / " + (Mathf.FloorToInt(world.player.transform.position.y) - halfWorldSizeInVoxels) + " / " + (Mathf.FloorToInt(world.player.transform.position.z) - halfWorldSizeInVoxels);
        debugText += "\n\n";
        debugText += "Chunk: " + world.playerLastChunkCoord.x + " / " + world.playerLastChunkCoord.y + " / " + world.playerLastChunkCoord.z;
        text.text = debugText;
        if(timer>1.0f)
        {
            frameRate = (int)(1.0f / Time.unscaledDeltaTime);
            timer = 0;
        }
        else
        {
            timer += Time.deltaTime;
        }
    }
}
