using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateBlock
{
    public Chunk chunk;

    public GenerateBlock(Chunk chunk)
    { 
        this.chunk = chunk;
    }

    public bool[] CheckBlock(int x, int y, int z)
    {
        var max = Chunk.Size - 1;

        var hasBack = (z > 0) ? (chunk.blocks[x, y, z - 1] == null) : (chunk.neighbours.front?.created == true && chunk.neighbours.front.blocks[x, y, max] == null);
        var hasFront = (z < max) ? (chunk.blocks[x, y, z + 1] == null) : (chunk.neighbours.back?.created == true && chunk.neighbours.back.blocks[x, y, 0] == null);
        var hasTop = (y < max) ? (chunk.blocks[x, y + 1, z] == null) : (chunk.neighbours.top?.created == true && chunk.neighbours.top.blocks[x, 0, z] == null);
        var hasBottom = (y > 0) ? (chunk.blocks[x, y - 1, z] == null) : (chunk.neighbours.bottom?.created == true && chunk.neighbours.bottom.blocks[x, max, z] == null);
        var hasLeft = (x > 0) ? (chunk.blocks[x - 1, y, z] == null) : (chunk.neighbours.right?.created == true && chunk.neighbours.right.blocks[max, y, z] == null);
        var hasRight = (x < max) ? (chunk.blocks[x + 1, y, z] == null) : (chunk.neighbours.left?.created == true && chunk.neighbours.left.blocks[0, y, z] == null);


        bool myHasBack;
        if (z > 0)
        {
            myHasBack = (chunk.blocks[x, y, z - 1] == null || chunk.blocks[x, y, z - 1] == !chunk.world.blockTable.GetBlock("air"));
        }
        else
        {
            myHasBack = (chunk.neighbours.front?.created == true && chunk.neighbours.front.blocks[x, y, max] == null);
        }


        bool myHasFront;
        if (z < max)
        {
            myHasFront = (chunk.blocks[x, y, z + 1] == null || chunk.blocks[x, y, z + 1] == !chunk.world.blockTable.GetBlock("air"));
        }
        else
        {
            myHasFront = (chunk.neighbours.back?.created == true && chunk.neighbours.back.blocks[x, y, 0] == null);
        }


        bool myHasTop;
        if (y < max)
        {
            myHasTop = (chunk.blocks[x, y + 1, z] == null || chunk.blocks[x, y + 1, z] == !chunk.world.blockTable.GetBlock("air"));
        }
        else
        {
            myHasTop = (chunk.neighbours.top?.created == true && chunk.neighbours.top.blocks[x, 0, z] == null);
        }


        bool myHasBottom;
        if (y > 0)
        {
            myHasBottom = (chunk.blocks[x, y - 1, z] == null || chunk.blocks[x, y - 1, z] == !chunk.world.blockTable.GetBlock("air"));
        }
        else
        {
            myHasBottom = (chunk.neighbours.bottom?.created == true && chunk.neighbours.bottom.blocks[x, max, z] == null);
        }

        bool myHasLeft;
        if (x > 0)
        {
            myHasLeft = (chunk.blocks[x - 1, y, z] == null || chunk.blocks[x - 1, y, z] == !chunk.world.blockTable.GetBlock("air"));
        }
        else
        {
            myHasLeft = (chunk.neighbours.right?.created == true && chunk.neighbours.right.blocks[max, y, z] == null);
        }

        bool myHasRight;
        if (x < max)
        {
            myHasRight = (chunk.blocks[x + 1, y, z] == null || chunk.blocks[x + 1, y, z] == !chunk.world.blockTable.GetBlock("air"));
        }
        else
        {
            myHasRight = (chunk.neighbours.left?.created == true && chunk.neighbours.left.blocks[0, y, z] == null);
        }

        return new bool[]
        {
            myHasBack,
            myHasFront,
            myHasTop,
            myHasBottom,
            myHasLeft,
            myHasRight,
        };





        return new bool[]
        {
            hasBack,
            hasFront,
            hasTop,
            hasBottom,
            hasLeft,
            hasRight,
        };
    }

    public void GenerateBlockMethod(BlockScriptableObject block, Vector3 position, bool[] checks)
    {
        for (int i = 0; i < 6; i++)
        {
            if (checks[i])
            {
                chunk.vertices.Add(position + Cube.vertices[Cube.triangles[i, 0]]);
                chunk.vertices.Add(position + Cube.vertices[Cube.triangles[i, 1]]);
                chunk.vertices.Add(position + Cube.vertices[Cube.triangles[i, 2]]);
                chunk.vertices.Add(position + Cube.vertices[Cube.triangles[i, 3]]);

                chunk.triangles.Add(chunk.verticesIndex + 0);
                chunk.triangles.Add(chunk.verticesIndex + 1);
                chunk.triangles.Add(chunk.verticesIndex + 2);
                chunk.triangles.Add(chunk.verticesIndex + 2);
                chunk.triangles.Add(chunk.verticesIndex + 1);
                chunk.triangles.Add(chunk.verticesIndex + 3);

                var texture = block.rects[i];

                chunk.uv.Add(new Vector2(texture.xMax, texture.yMin));
                chunk.uv.Add(new Vector2(texture.xMax, texture.yMax));
                chunk.uv.Add(new Vector2(texture.xMin, texture.yMin));
                chunk.uv.Add(new Vector2(texture.xMin, texture.yMax));

                chunk.verticesIndex += 4;


            }
        }
    }
}
