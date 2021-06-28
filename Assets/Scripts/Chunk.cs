using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class Chunk
{
    public ChunkCoord coord;
    GameObject chunkObject;
    public MeshRenderer meshRenderer;
    public MeshFilter meshFilter;
    int vertexIndex = 0;
    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();
    List<int> transparentTriangles = new List<int>();
    Material[] materials = new Material[2];
    List<Vector2> uvs = new List<Vector2>();
    List<Color> colors = new List<Color>();
    World world;
    public VoxelState[,,] voxelMap = new VoxelState[VoxelData.ChunkWidth, VoxelData.ChunkHeight, VoxelData.ChunkWidth];
    private bool _isActive;
    private bool isVoxelMapPopulated = false;
    public Queue<VoxelMod> modifications = new Queue<VoxelMod>();
    private bool threadLocked = false;
    public Vector3 position;

    public Chunk(World w, ChunkCoord _coord, bool generateOnLoad)
    {
        world = w;
        coord = _coord;
        _isActive = true;
        if (generateOnLoad)
        {
            Init();
        }
    }

    public void Init()
    {
        chunkObject = new GameObject();
        meshFilter = chunkObject.AddComponent<MeshFilter>();
        meshRenderer = chunkObject.AddComponent<MeshRenderer>();
        materials[0] = world.mat;
        materials[1] = world.transparentMat;
        meshRenderer.materials = materials;
        chunkObject.transform.SetParent(world.transform);
        chunkObject.transform.position = new Vector3(coord.x * VoxelData.ChunkWidth, coord.y * VoxelData.ChunkHeight, coord.z * VoxelData.ChunkWidth);
        chunkObject.name = "Chunk " + coord.x + ", " + coord.y + ", " + coord.z;
        position = chunkObject.transform.position;
        if (world.enableThreading)
        {
            new Thread(new ThreadStart(PopulateVoxelMap)).Start();
        }
        else
        {
            PopulateVoxelMap();
            //UpdateChunkData();
        }
        //
    }

    void Start()
    {
        world = GameObject.Find("World").GetComponent<World>();

    }

    public void UpdateChunk()
    {
        if (world.enableThreading)
        {
            new Thread(new ThreadStart(UpdateChunkData)).Start();
        }
        else
            UpdateChunkData();
    }

    private void UpdateChunkData()
    {
        threadLocked = true;
        while (modifications.Count>0)
        {
            VoxelMod v = modifications.Dequeue();
            Vector3 pos = v.position -= position;
            voxelMap[(int)pos.x, (int)pos.y, (int)pos.z].id = v.id;
        }
        ClearChunkData();
        CalcLight();
        for (int x = 0; x < VoxelData.ChunkWidth; x++)
        {
            for (int y = 0; y < VoxelData.ChunkHeight; y++)
            {
                for (int z = 0; z < VoxelData.ChunkWidth; z++)
                {
                    if (world.blockTypes[voxelMap[x, y, z].id].isSolid)
                        UpdateDataToChunk(new Vector3(x, y, z));
                }
            }
        }
        lock (world.chunksToDraw)
        {
            world.chunksToDraw.Enqueue(this);
        }
        threadLocked = false;
        //CreateMesh();
    }

    void ClearChunkData()
    {
        vertexIndex = 0;
        vertices.Clear();
        triangles.Clear();
        transparentTriangles.Clear();
        uvs.Clear();
        colors.Clear();
    }

    public bool isActive
    {
        get { return _isActive; }
        set
        {
            _isActive = value;
            if (chunkObject != null)
            {
                chunkObject.SetActive(value);
            }
        }
    }

    public bool isEditable
    {
        get
        {
            if (!isVoxelMapPopulated || threadLocked)
                return false;
            else return true;
        }
    }

    bool IsVoxelInChunk(int x, int y, int z)
    {
        if (x < 0 || x > VoxelData.ChunkWidth - 1 || y < 0 || y > VoxelData.ChunkHeight - 1 || z < 0 || z > VoxelData.ChunkWidth - 1)
        {
            return false;
        }
        else return true;
    }

    VoxelState CheckVoxel(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        int z = Mathf.FloorToInt(pos.z);
        if (!IsVoxelInChunk(x, y, z))
        {
            return world.GetVoxelState(pos + position);
            //return world.blockTypes[world.GetVoxel(pos + position)].isSolid;
        }
        else return voxelMap[x, y, z];
    }

    public VoxelState GetVoxelFromGlobalVector3(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        int z = Mathf.FloorToInt(pos.z);
        x -= Mathf.FloorToInt(position.x);
        y -= Mathf.FloorToInt(position.y);
        z -= Mathf.FloorToInt(position.z);

        return voxelMap[x, y, z];
    }

    void PopulateVoxelMap()
    {
        for (int x = 0; x < VoxelData.ChunkWidth; x++)
        {
            for (int y = 0; y < VoxelData.ChunkHeight; y++)
            {
                for (int z = 0; z < VoxelData.ChunkWidth; z++)
                {
                    voxelMap[x, y, z] = new VoxelState(world.GetVoxel(new Vector3(x, y, z) + position));
                }
            }
        }
        UpdateChunkData();
        isVoxelMapPopulated = true;
        //
    }

    void UpdateDataToChunk(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        int z = Mathf.FloorToInt(pos.z);
        byte blockID = voxelMap[x, y, z].id;
        bool isTransparent = world.blockTypes[blockID].renderNeighborFaces;
        for (int p = 0; p < 6; p++)
        {
            VoxelState neighbor = CheckVoxel(pos + VoxelData.faceChecks[p]);
            if (neighbor!=null && world.blockTypes[neighbor.id].renderNeighborFaces)
            {
                vertices.Add(VoxelData.voxelVerts[VoxelData.voxelTris[p, 0]] + pos);
                vertices.Add(VoxelData.voxelVerts[VoxelData.voxelTris[p, 1]] + pos);
                vertices.Add(VoxelData.voxelVerts[VoxelData.voxelTris[p, 2]] + pos);
                vertices.Add(VoxelData.voxelVerts[VoxelData.voxelTris[p, 3]] + pos);
                AddTexture(world.blockTypes[blockID].GetTextureID(p));
                float lightlevel = neighbor.globalLightPercent;
                colors.Add(new Color(0, 0, 0, lightlevel));
                colors.Add(new Color(0, 0, 0, lightlevel));
                colors.Add(new Color(0, 0, 0, lightlevel));
                colors.Add(new Color(0, 0, 0, lightlevel));
                if (!isTransparent)
                {
                    triangles.Add(vertexIndex + 0);
                    triangles.Add(vertexIndex + 1);
                    triangles.Add(vertexIndex + 2);
                    triangles.Add(vertexIndex + 2);
                    triangles.Add(vertexIndex + 1);
                    triangles.Add(vertexIndex + 3);
                }
                else
                {
                    transparentTriangles.Add(vertexIndex + 0);
                    transparentTriangles.Add(vertexIndex + 1);
                    transparentTriangles.Add(vertexIndex + 2);
                    transparentTriangles.Add(vertexIndex + 2);
                    transparentTriangles.Add(vertexIndex + 1);
                    transparentTriangles.Add(vertexIndex + 3);
                }
                vertexIndex += 4;
                //(world.blockTypes[voxelMap[(int)pos.x, (int)pos.y, (int)pos.z]].GetTextureID(p));
            }
        }
    }

    public void EditVoxel(Vector3 pos, byte newID)
    {
        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        int z = Mathf.FloorToInt(pos.z);
        x -= Mathf.FloorToInt(chunkObject.transform.position.x);
        y -= Mathf.FloorToInt(chunkObject.transform.position.y);
        z -= Mathf.FloorToInt(chunkObject.transform.position.z);

        voxelMap[x, y, z].id = newID;

        UpdateSurroundingVoxels(x, y, z);
        UpdateChunkData();
    }

    void UpdateSurroundingVoxels (int x, int y, int z)
    {
        Vector3 thisVoxel = new Vector3(x, y, z);
        for (int p=0;p<6;p++)
        {
            Vector3 currentVoxel = thisVoxel + VoxelData.faceChecks[p];
            if(!IsVoxelInChunk((int)currentVoxel.x, (int)currentVoxel.y, (int)currentVoxel.z))
            {
                world.GetChunkFromVector3(currentVoxel + position).UpdateChunkData();
            }
        }
    }

    public void CreateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        //mesh.triangles = triangles.ToArray();
        mesh.subMeshCount = 2;
        mesh.SetTriangles(triangles.ToArray(), 0);
        mesh.SetTriangles(transparentTriangles.ToArray(), 1);
        mesh.uv = uvs.ToArray();
        mesh.colors = colors.ToArray();
        mesh.RecalculateNormals();
        meshFilter.mesh = mesh;
    }

    void AddTexture (int textureID)
    {
        float y = textureID / VoxelData.TextureAtlasSizeInBlocks;
        float x = textureID - (y * VoxelData.TextureAtlasSizeInBlocks);
        x *= VoxelData.NormalizedBlockTextureSize;
        y *= VoxelData.NormalizedBlockTextureSize;
        y = 1 - y - VoxelData.NormalizedBlockTextureSize;

        uvs.Add(new Vector2(x, y));
        uvs.Add(new Vector2(x, y + VoxelData.NormalizedBlockTextureSize ));
        uvs.Add(new Vector2(x + VoxelData.NormalizedBlockTextureSize, y));
        uvs.Add(new Vector2(x + VoxelData.NormalizedBlockTextureSize, y + VoxelData.NormalizedBlockTextureSize));
    }

    void CalcLight()
    {
        Queue<Vector3Int> litVoxels = new Queue<Vector3Int>();
        for (int x = 0; x < VoxelData.ChunkWidth; x++)
        {
            for (int z = 0; z < VoxelData.ChunkWidth; z++)
            {
                float lightray = 1.0f;
                for (int y = VoxelData.ChunkHeight-1; y >=0; y--) //*VoxelData.ViewDistanceInChunks*2-1
                {
                    VoxelState thisVoxel = voxelMap[x, y, z];
                    if (thisVoxel.id > 0 && world.blockTypes[thisVoxel.id].transparency<lightray)
                        lightray = world.blockTypes[thisVoxel.id].transparency;
                    thisVoxel.globalLightPercent = lightray;
                    voxelMap[x, y, z] = thisVoxel;
                    if (lightray > VoxelData.lightFalloff)
                        litVoxels.Enqueue(new Vector3Int(x, y, z));
                }
            }
        }
        while (litVoxels.Count>0)
        {
            Vector3Int v = litVoxels.Dequeue();
            for(int p=0;p<6;p++)
            {
                Vector3 currentVoxel = v + VoxelData.faceChecks[p];
                Vector3Int neighbor = new Vector3Int((int)currentVoxel.x, (int)currentVoxel.y, (int)currentVoxel.z);
                if (IsVoxelInChunk(neighbor.x, neighbor.y, neighbor.z))
                {
                    if(voxelMap[neighbor.x, neighbor.y, neighbor.z].globalLightPercent<voxelMap[v.x,v.y,v.z].globalLightPercent-VoxelData.lightFalloff)
                    {
                        voxelMap[neighbor.x, neighbor.y, neighbor.z].globalLightPercent = voxelMap[v.x, v.y, v.z].globalLightPercent - VoxelData.lightFalloff;
                        if(voxelMap[neighbor.x, neighbor.y, neighbor.z].globalLightPercent>VoxelData.lightFalloff)
                        {
                            litVoxels.Enqueue(neighbor);
                        }
                    }
                }
            }
        }
    }
}

public class ChunkCoord
{
    public int x, y, z;
    public ChunkCoord(int _x,int _y, int _z)
    {
        x = _x;
        y = _y;
        z = _z;
    }

    public ChunkCoord(Vector3 pos)
    {
        int xc = Mathf.FloorToInt(pos.x);
        int yc = Mathf.FloorToInt(pos.y);
        int zc = Mathf.FloorToInt(pos.z);
        x = xc / VoxelData.ChunkWidth;
        y = yc / VoxelData.ChunkHeight;
        z = zc / VoxelData.ChunkWidth;
    }

    public ChunkCoord()
    {
        x = 0;
        y = 0;
        z = 0;
    }

    public bool Equals (ChunkCoord other)
    {
        if (other == null)
            return false;
        else if (other.x == x && other.y == y && other.z == z)
            return true;
        else
            return false;
    }
}

public class VoxelState
{
    public byte id;
    public float globalLightPercent;

    public VoxelState()
    {
        id = 0;
        globalLightPercent = 0.0f;
    }

    public VoxelState (byte blockId)
    {
        id = blockId;
        globalLightPercent = 0.0f;
    }
}