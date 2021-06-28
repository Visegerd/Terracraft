using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
    [Header("World Gen Values")]
    public int seed;
    public BiomeAttributes biome;

    [Header("Performance")]
    public bool enableThreading;

    public Transform player;
    public Vector3 spawnPosition;
    public Material mat;
    public Material transparentMat;
    public BlockType[] blockTypes;
    public static World world;
    Queue<Queue<VoxelMod>> modifications = new Queue<Queue<VoxelMod>>();
    List<Chunk> chunksToUpdate = new List<Chunk>();
    public Queue<Chunk> chunksToDraw = new Queue<Chunk>();
    private bool inUI = false;
    public GameObject creativeInventoryWindow;
    public GameObject cursorSlot;
    [Range(1.0f,0.0f)]
    public float globalLightLevel;
    public Color day;
    public Color night;
    public bool isInUI
    {
        get
        {
            return inUI;
        }
        set
        {
            inUI = value;
            if (isInUI)
            {
                Cursor.lockState = CursorLockMode.None;
                creativeInventoryWindow.SetActive(true);
                cursorSlot.SetActive(true);
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                creativeInventoryWindow.SetActive(false);
                cursorSlot.SetActive(false);
            }
        }
    }

    ChunkCoord GetChunkCoordFromVector3(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x / VoxelData.ChunkWidth);
        int y = Mathf.FloorToInt(pos.y / VoxelData.ChunkHeight);
        int z = Mathf.FloorToInt(pos.z / VoxelData.ChunkWidth);
        return new ChunkCoord(x, y, z);
    }

    public Chunk GetChunkFromVector3(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x / VoxelData.ChunkWidth);
        int y = Mathf.FloorToInt(pos.y / VoxelData.ChunkHeight);
        int z = Mathf.FloorToInt(pos.z / VoxelData.ChunkWidth);
        return chunks[x, y, z];
    }

    Chunk[,,] chunks = new Chunk[VoxelData.WorldSizeInChunks, VoxelData.WorldHeightInChunks, VoxelData.WorldSizeInChunks];
    List<ChunkCoord> activeChunks = new List<ChunkCoord>();
    List<ChunkCoord> chunksToCreate = new List<ChunkCoord>();
    public ChunkCoord playerLastChunkCoord;
    bool applyModifications = false;

    public GameObject debugScreen;

    private void Awake()
    {
        world = GetComponent<World>();
        Random.InitState(seed);
    }

    private void Start()
    {
        //Chunk newChunk = new Chunk(this,new ChunkCoord(0,0,0));
        //Chunk newChunk2 = new Chunk(this, new ChunkCoord(1, 0, 0));
        Shader.SetGlobalFloat("minGlobalLightLevel", VoxelData.minLightLevel);
        Shader.SetGlobalFloat("maxGlobalLightLevel", VoxelData.maxLightLevel);
        Shader.SetGlobalFloat("GlobalLightLevel", globalLightLevel);
        Camera.main.backgroundColor = Color.Lerp(night, day, globalLightLevel);
        spawnPosition = new Vector3(VoxelData.WorldSizeInChunks * VoxelData.ChunkWidth / 2.0f, VoxelData.WorldHeightInChunks * VoxelData.ChunkHeight / 2.0f, VoxelData.WorldSizeInChunks * VoxelData.ChunkWidth / 2.0f);
        GenerateWorld();
        playerLastChunkCoord = GetChunkCoordFromVector3(player.position);
    }

    private void Update()
    {
        if (!GetChunkCoordFromVector3(player.position).Equals(playerLastChunkCoord))
        { 
            CheckDistancePlayer();
            playerLastChunkCoord = GetChunkCoordFromVector3(player.position);
        }
        if(!applyModifications)
        {
            ApplyModifications();
        }
        if (chunksToCreate.Count > 0)
        {
            CreateChunk();
        }
        if (chunksToUpdate.Count > 0)
        {
            UpdateChunks();
        }
        if(chunksToDraw.Count>0)
        {
            lock(chunksToDraw)
            {
                if(chunksToDraw.Peek().isEditable)
                {
                    chunksToDraw.Dequeue().CreateMesh();
                }
            }
        }
        if (Input.GetKeyDown(KeyCode.F3))
        {
            debugScreen.SetActive(!debugScreen.activeSelf);
        }
    }

    void GenerateWorld()
    {
        for(int x=(VoxelData.WorldSizeInChunks/2)-VoxelData.ViewDistanceInChunks;x< (VoxelData.WorldSizeInChunks / 2) + VoxelData.ViewDistanceInChunks; x++)
        {
            for (int y = VoxelData.WorldHeightInChunks/2 - VoxelData.ViewDistanceInChunks; y < VoxelData.WorldHeightInChunks/2; y++)
            {
                for (int z = (VoxelData.WorldSizeInChunks / 2) - VoxelData.ViewDistanceInChunks; z < (VoxelData.WorldSizeInChunks / 2) + VoxelData.ViewDistanceInChunks; z++)
                {
                    chunks[x, y, z] = new Chunk(this, new ChunkCoord(x, y, z), true);
                    activeChunks.Add(new ChunkCoord(x, y, z));
                }
            }
        }
        //while (modifications.Count>0)
        //{
        //    VoxelMod v = modifications.Dequeue();
        //    ChunkCoord c = GetChunkCoordFromVector3(v.position);
        //    if(chunks[c.x,c.y,c.z]==null)
        //    {
        //        chunks[c.x, c.y, c.z] = new Chunk(this, c, true);
        //        activeChunks.Add(c);
        //    }
        //    chunks[c.x, c.y, c.z].modifications.Enqueue(v);
        //    if(!chunksToUpdate.Contains(chunks[c.x,c.y,c.z]))
        //    {
        //        chunksToUpdate.Add(chunks[c.x, c.y, c.z]);
        //    }
        //}
        //for(int i=0;i<chunksToUpdate.Count;i++)
        //{
        //    chunksToUpdate[0].UpdateChunkData();
        //    chunksToUpdate.RemoveAt(0);
        //}
        player.position = spawnPosition;
    }

    void CreateChunk()
    {
        ChunkCoord c = chunksToCreate[0];
        chunksToCreate.RemoveAt(0);
        activeChunks.Add(c);
        chunks[c.x, c.y, c.z].Init();
    }

    void UpdateChunks()
    {
        bool update = false;
        int index = 0;
        while(!update&&index<chunksToUpdate.Count-1)
        {
            if(chunksToUpdate[index].isEditable)
            {
                chunksToUpdate[index].UpdateChunk(); //Data();
                chunksToUpdate.RemoveAt(index);
                update = true;
            }
            else
            {
                index++;
            }
        }
    }

    void ApplyModifications()
    {
        applyModifications = true;
        while(modifications.Count>0)
        {
            Queue<VoxelMod> queue = modifications.Dequeue();

            while (queue.Count > 0)
            {
                VoxelMod v = queue.Dequeue();
                ChunkCoord c = GetChunkCoordFromVector3(v.position);
                if (chunks[c.x, c.y, c.z] == null)
                {
                    chunks[c.x, c.y, c.z] = new Chunk(this, c, true);
                    activeChunks.Add(c);
                }
                chunks[c.x, c.y, c.z].modifications.Enqueue(v);
                if (!chunksToUpdate.Contains(chunks[c.x, c.y, c.z]))
                {
                    chunksToUpdate.Add(chunks[c.x, c.y, c.z]);
                }
            }
        }
        applyModifications = false;
    }

    bool IsChunkInWorld(ChunkCoord coord)
    {
        if (coord.x > 0 && coord.x < VoxelData.WorldSizeInChunks && coord.y > 0 && coord.y < VoxelData.WorldSizeInChunks && coord.z > 0 && coord.z < VoxelData.WorldSizeInChunks)
            return true;
        else return false;
    }

    bool IsVoxelInWorld(Vector3 pos)
    {
        if (pos.x > 0 && pos.x < VoxelData.WorldSizeInVoxels && pos.y > 0 && pos.y < VoxelData.WorldSizeInVoxels && pos.z > 0 && pos.z < VoxelData.WorldSizeInVoxels)
            return true;
        else return false;
    }

    public byte GetVoxel(Vector3 pos)
    {
        int yPos = (Mathf.FloorToInt(pos.y));
        if (!IsVoxelInWorld(pos))
            return 0;
        if (yPos == 0)
            return 4;

        int terrainHeight = Mathf.FloorToInt(biome.terrainHeight * Noise.Get2DPerlin(new Vector2(pos.x, pos.z), 0.0f, biome.terrainScale)) + biome.solidGroundHeight; //VoxelData.WorldHeightInChunks/2.0f+VoxelData.WorldHeightInChunks/2.0f*VoxelData.ChunkHeight
        byte voxelValue = 0;
        if (yPos == terrainHeight)
            voxelValue = 3;
        else if (yPos < terrainHeight && yPos > terrainHeight - 4)
            voxelValue = 2;
        else if (yPos <= terrainHeight-4)
            voxelValue= 1;
        else voxelValue = 0;

        if (voxelValue == 1)// && voxelValue == 2)
        {
            foreach (Lode lode in biome.lodes)
            {
                if (yPos > lode.minHeight && yPos < lode.maxHeight)
                {
                    if (Noise.Get3DPerlin(pos, lode.noiseOffset, lode.scale, lode.threshold))
                    {
                        voxelValue = lode.blockID;
                    }
                }
            }
            foreach (Lode lode in biome.lodes)
            {
                if(yPos > lode.minHeight && yPos<lode.maxHeight && lode.nodeName=="Air")
                {
                    if(Noise.Get3DPerlin(pos,lode.noiseOffset,lode.scale,lode.threshold-((float)lode.maxHeight- (float)yPos*2 + (float)lode.minHeight)/((float)lode.maxHeight*4)))
                    {
                        voxelValue = lode.blockID;
                    }
                }
            }
        }
        else if (voxelValue == 2 || voxelValue == 3)// && voxelValue == 2)
        {
            foreach (Lode lode in biome.lodes)
            {
                if (yPos > lode.minHeight && yPos < lode.maxHeight && lode.nodeName == "Air")
                {
                    if (Noise.Get3DPerlin(pos, lode.noiseOffset, lode.scale, lode.threshold - ((float)lode.maxHeight - (float)yPos * 2 + (float)lode.minHeight) / ((float)lode.maxHeight * 4)))
                    {
                        voxelValue = lode.blockID;
                    }
                }
            }
        }
        if(yPos==terrainHeight && voxelValue!=0)
        {
            if(Noise.Get2DPerlin(new Vector2(pos.x,pos.z),0.32534f,biome.treeZoneScale)>biome.treeZoneThreshold)
            {
                if (Noise.Get2DPerlin(new Vector2(pos.x, pos.z), 0.72345f, biome.treePlacementScale) > biome.treePlacementThreshold)
                {
                    voxelValue = 9;
                    modifications.Enqueue(Structure.MakeTree(pos, biome.minTreeHeight, biome.maxTreeHeight));
                    //modifications.Enqueue(new VoxelMod(new Vector3(pos.x, pos.y + 1, pos.z), 9));
                }
            }
        }
        return voxelValue;
    }

    void CheckDistancePlayer()
    {
        ChunkCoord coord = GetChunkCoordFromVector3(player.position);
        List<ChunkCoord> lastActiveChunks = new List<ChunkCoord>(activeChunks);
        for (int x = coord.x - VoxelData.ViewDistanceInChunks; x < coord.x + VoxelData.ViewDistanceInChunks; x++)
        {
            for (int y = coord.y - VoxelData.ViewDistanceInChunks; y < coord.y + VoxelData.ViewDistanceInChunks; y++)
            {
                for (int z = coord.z - VoxelData.ViewDistanceInChunks; z < coord.z + VoxelData.ViewDistanceInChunks; z++)
                {
                    if (IsChunkInWorld(new ChunkCoord(x, y, z)))
                    {
                        if (chunks[x, y, z] == null)
                        {
                            chunks[x, y, z] = new Chunk(this, new ChunkCoord(x, y, z), false);
                            chunksToCreate.Add(new ChunkCoord(x, y, z));
                        }
                        else if (!chunks[x, y, z].isActive)
                        {
                            chunks[x, y, z].isActive = true;
                            //activeChunks.Add(new ChunkCoord(x, y, z));
                        }
                        activeChunks.Add(new ChunkCoord(x, y, z));
                    }
                    for (int i=0;i<lastActiveChunks.Count;i++)
                    {
                        if (lastActiveChunks[i].Equals(new ChunkCoord(x, y, z)))
                            lastActiveChunks.RemoveAt(i);
                    }
                }
            }
        }
        foreach(ChunkCoord c in lastActiveChunks)
        {
            chunks[c.x, c.y, c.z].isActive = false;
        }
    }

    public bool CheckForVoxel(Vector3 pos)
    {
        ChunkCoord thisChunk = new ChunkCoord(pos);
        if (!IsChunkInWorld(thisChunk) || pos.y < 0 || pos.y > VoxelData.WorldHeightInChunks * VoxelData.ChunkHeight)
        {
            return false;
        }
        if (chunks[thisChunk.x, thisChunk.y, thisChunk.z] != null && chunks[thisChunk.x, thisChunk.y, thisChunk.z].isEditable)
        {
            return blockTypes[chunks[thisChunk.x, thisChunk.y, thisChunk.z].GetVoxelFromGlobalVector3(pos).id].isSolid;
        }
        return blockTypes[GetVoxel(pos)].isSolid;
    }

    public VoxelState GetVoxelState (Vector3 pos)
    {
        ChunkCoord thisChunk = new ChunkCoord(pos);
        if (!IsChunkInWorld(thisChunk) || pos.y < 0 || pos.y > VoxelData.WorldHeightInChunks * VoxelData.ChunkHeight)
        {
            return null;
        }
        if (chunks[thisChunk.x, thisChunk.y, thisChunk.z] != null && chunks[thisChunk.x, thisChunk.y, thisChunk.z].isEditable)
        {
            return chunks[thisChunk.x, thisChunk.y, thisChunk.z].GetVoxelFromGlobalVector3(pos);
        }
        return new VoxelState (GetVoxel(pos));
    }
}

[System.Serializable]
public class BlockType
{
    public string blockName;
    public bool isSolid;
    public Sprite icon;
    public bool renderNeighborFaces;
    public float transparency;

    [Header("Texture Values")]
    public int topFaceTexture;
    public int downFaceTexture;
    public int frontFaceTexture;
    public int backFaceTexture;
    public int leftFaceTexture;
    public int rightFaceTexture;
    //top down front back left right

    public int GetTextureID(int faceIndex)
    {
        switch(faceIndex)
        {
            case 0:
            {
                return topFaceTexture;
            }
            case 1:
                {
                    return downFaceTexture;
                }
            case 2:
                {
                    return frontFaceTexture;
                }
            case 3:
                {
                    return backFaceTexture;
                }
            case 4:
                {
                    return leftFaceTexture;
                }
            case 5:
                {
                    return rightFaceTexture;
                }
            default:
                {
                    Debug.LogError("Error in GetTextureID; Invalid face index");
                    return 0;
                }
        }
    }
}
public class VoxelMod
{
    public Vector3 position;
    public byte id;
    public VoxelMod()
    {
        position = new Vector3();
        id = 0;
    }

    public VoxelMod(Vector3 pos,byte ajdi)
    {
        id = ajdi;
        position = pos;
    }
}
