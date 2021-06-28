using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Structure
{
    public static Queue<VoxelMod> MakeTree (Vector3 position, int minTrunkHeight,int maxTrunkHeight)
    {
        Queue<VoxelMod> queue = new Queue<VoxelMod>();
        int height = (int)(maxTrunkHeight * Noise.Get2DPerlin(new Vector2(position.x, position.z), 162.34f, 5f));
        if (height < minTrunkHeight)
            height = minTrunkHeight;
        for(int i=1;i<height;i++)
        {
            queue.Enqueue(new VoxelMod(new Vector3(position.x, position.y + i, position.z), 9));
            if(i>=minTrunkHeight/2)
            {
                System.Random rand = new System.Random((int)(Noise.Get2DPerlin(new Vector2(position.x, position.z), 362.34f, 15f)*15.0f)*i);
                int range = rand.Next(0, 100);
                if(range>(((maxTrunkHeight-i*1.5f) / maxTrunkHeight ) * 100))
                {
                    int maxDistance = (maxTrunkHeight - i) / 2;
                    if (maxDistance < 1)
                        maxDistance = 1;
                    Vector3 direction = new Vector3(position.x + rand.Next(-maxDistance, maxDistance), position.y + rand.Next(-maxDistance / 2, maxDistance / 2) + 4 + i, position.z + rand.Next(-maxDistance, maxDistance));
                    Vector3 trunkPos = new Vector3(position.x, position.y + i, position.z);
                    Vector3 dir = direction - trunkPos;
                    dir = dir.normalized;
                    Vector3 currentPos = trunkPos;
                    while (maxDistance > 0)
                    {
                        currentPos += dir;
                        queue.Enqueue(new VoxelMod(new Vector3((int)currentPos.x, (int)currentPos.y, (int)currentPos.z), 9));
                        maxDistance--;
                    }
                    for (int x = -1; x < 2; x++)
                    {
                        for (int y = -1; y < 2; y++)
                        {
                            for (int z = -1; z < 2; z++)
                            {
                                queue.Enqueue(new VoxelMod(new Vector3((int)currentPos.x + x, (int)currentPos.y + y, (int)currentPos.z + z), 10));
                            }
                        }
                    }
                    //Queue<VoxelMod> branch = MakeTreeBranch(new Vector3(position.x, position.y+i, position.z), new Vector3(position.x + rand.Next(-maxDistance, maxDistance), position.y + rand.Next(-maxDistance/2, maxDistance/2)+i, position.z + rand.Next(-maxDistance, maxDistance)), maxDistance/2);
                    //while(branch.Count>0)
                    //    queue.Enqueue(branch.Dequeue());
                }
            }
        }
        for (int x = -2; x < 3; x++)
        {
            for (int y = -2; y < 3; y++)
            {
                for (int z = -2; z < 3; z++)
                {
                    queue.Enqueue(new VoxelMod(new Vector3(position.x+x, position.y + height + y, position.z+z), 10));
                }
            }
        }
        return queue;
    }

    public static Queue<VoxelMod> MakeTreeBranch(Vector3 position, Vector3 direction,int maxDistance)
    {
        Queue<VoxelMod> queue = new Queue<VoxelMod>();
        Vector3 dir = direction - position;
        dir = dir.normalized;
        Vector3 currentPos = position;
        while(maxDistance<=0)
        {
            currentPos += dir;
            queue.Enqueue(new VoxelMod(new Vector3((int)currentPos.x, (int)currentPos.y, (int)currentPos.z), 9));
            maxDistance--;
        }
        for (int x = -1; x < 2; x++)
        {
            for (int y = -1; y < 2; y++)
            {
                for (int z = -1; z < 2; z++)
                {
                    queue.Enqueue(new VoxelMod(new Vector3((int)currentPos.x + x, (int)currentPos.y + y, (int)currentPos.z + z), 10));
                }
            }
        }
        return queue;
    }
}
