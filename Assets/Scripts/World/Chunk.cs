using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using System.Reflection;

[RequireComponent(typeof(MeshFilter), typeof(MeshCollider))]
public class Chunk : MonoBehaviour
{
    public const int Size = 16;

    public World world;

    [HideInInspector]
    public Vector3Int coordinates;

    [HideInInspector]
    public BlockScriptableObject[,,] blocks = null;

    public ChunkNeighbours neighbours;

    public List<Vector3> vertices;
    public List<int> triangles;
    public List<Vector2> uv;
    public int verticesIndex = 0;

    private MeshFilter filter;
    private new MeshCollider collider;
    private new MeshRenderer renderer;

    public GenerateBlock generateBlock;

    private Bounds bounds;

    public bool loaded = false;


    enum LandscapeType
    {
        Plains, Hills, Mountains
    }






    void Awake()
    {
        generateBlock = new GenerateBlock(this);


        this.blocks = new BlockScriptableObject[Chunk.Size, Chunk.Size, Chunk.Size];

        this.world = this.transform.parent.GetComponent<World>();

        this.filter = this.GetComponent<MeshFilter>();
        this.collider = this.GetComponent<MeshCollider>();
        this.renderer = this.GetComponent<MeshRenderer>();

        this.CreateBoundsBox();

        this.filter.mesh = new Mesh();
        this.collider.sharedMesh = this.filter.mesh;
    }

    void Start()
    {
        this.renderer.material.mainTexture = this.world.blockTable.GetBlockAtlasTexture();

        this.neighbours = new ChunkNeighbours(this.world, this.coordinates);

        this.Init();
    }

    void Update()
    {
        // this.FrustrumCulling();
    }


    public void Init()
    {
        Vector3Int worldCoordinates = this.coordinates * Chunk.Size;

        for (int x = 0; x < Chunk.Size; x++)
        {
            int worldX = x + worldCoordinates.x;

            for (int z = 0; z < Chunk.Size; z++)
            {
                int worldZ = z + worldCoordinates.z;


                //2D Noises

                int landscapeHeight = 0;
                List<int> heights2D = new List<int>();

                /*
                float landscapeValue = GetLandscapeNoise(worldX, worldZ);

                
                
                if (landscapeValue < 0.5f)
                {
                    landscapeHeight = GetHeight2D(0, worldX, worldZ);
                }
                if (landscapeValue > 0.5f)
                {
                    landscapeHeight = GetHeight2D(1, worldX, worldZ);
                }
                


                */
                landscapeHeight = GetTerrain(worldX, worldZ);


                for (int y = 0; y < Chunk.Size; y++)
                {
                    int worldY = y + worldCoordinates.y;



                    //3D Noises

                    bool checkCaves = CheckCaves(worldY, worldX, worldZ);
                    
                    //



                    if (worldY == landscapeHeight && checkCaves)
                    {
                        var block = this.world.blockTable.GetBlock("grass");
                        this.SetBlock(new Vector3Int(x, y, z), block);
                    }

                    else if ((worldY < landscapeHeight) && worldY >= (landscapeHeight - 4) && checkCaves)
                    {
                        var block = this.world.blockTable.GetBlock("dirt");
                        this.SetBlock(new Vector3Int(x, y, z), block);
                    }

                    else if (worldY <= (landscapeHeight - 4) && (worldY > -World.MaxWorldHeight) && checkCaves)
                    {
                        var block = this.world.blockTable.GetBlock("stone");
                        this.SetBlock(new Vector3Int(x, y, z), block);
                    }

                    else if (worldY == -World.MaxWorldHeight)
                    {
                        var block = this.world.blockTable.GetBlock("bedrock");
                        this.SetBlock(new Vector3Int(x, y, z), block);
                    }
                }
            }
        }

        this.loaded = true;
        this.StartFirstRender();
    }





    int GetHeight2D(int count, int worldX, int worldZ)
    {
        float height2D = 0;

        float noiseValue = world.noises2D[count].GetNoise(
            worldX * world.landscapeSettingsList[count].scale,
            worldZ * world.landscapeSettingsList[count].scale);

        height2D = (noiseValue + 1) / 2 * world.landscapeSettingsList[count].amplitude + world.minimumSeeLevel;

        return Mathf.FloorToInt(height2D);
    }


    int GetTerrain(int worldX, int worldZ)
    {
        float planeValue = world.planeNoise.GetNoise(
           worldX * world.planeSettings.scale,
           worldZ * world.planeSettings.scale);

        planeValue = (planeValue + 1) / 2 * world.planeSettings.amplitude + world.minimumSeeLevel;



        float mountainValue = world.mountainNoise.GetNoise(
            worldX * world.mountainSettings.scale,
            worldZ * world.mountainSettings.scale);

        mountainValue = (mountainValue + 1) /2 * world.mountainSettings.amplitude + world.minimumSeeLevel;



        float terrainBlenderValue = world.terrainBlenderNoise.GetNoise(
            worldX * world.terrainBlenderScale,
            worldZ * world.terrainBlenderScale);

        terrainBlenderValue = (terrainBlenderValue + 1) / 2;





        float blendedvalue = Mathf.Lerp(planeValue, mountainValue, terrainBlenderValue - world.mountainThreshold);


        return Mathf.FloorToInt(blendedvalue);
    }



    float GetLandscapeNoise(int worldX, int worldZ)
    {
        float landscapeValue = world.landscapeMap.GetNoise(worldX * 10, worldZ * 10);

        return (landscapeValue + 1) / 2;
    }


    bool CheckCaves(int worldX, int worldY, int worldZ)
    {
        if (world.generateCaves)
        {
            List<int> heights3D = new List<int>();


            for (int noise3d = 0; noise3d < world.noises3D.Count; noise3d++)
            {
                int height3D = Mathf.FloorToInt(
                    ((world.noises3D[noise3d].GetNoise(worldX, worldY, worldZ) + 1) / 2) * world.caveSettingsList[noise3d].amplitude);

                heights3D.Add(height3D);
            }



            if (heights3D[0] <= world.caveSettingsList[0].caveTolerancy &&
                heights3D[1] <= world.caveSettingsList[1].caveTolerancy &&
                heights3D[2] <= world.caveSettingsList[2].caveTolerancy)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            return true;
        }
    }




    private void CreateBoundsBox()
    {
        int chunkSize = Chunk.Size;
        var axis = chunkSize / 2.0f - 0.5f;

        var center = new Vector3(axis, axis, axis) + this.transform.position;
        var size = new Vector3(chunkSize, chunkSize, chunkSize);

        this.bounds = new Bounds(center, size);
    }

    public void FrustrumCulling()
    {
        var visible = Vector3.Distance(this.bounds.center, Player.main.transform.position) <= (this.world.viewDistance * Chunk.Size);

        if (visible)
        {
            Plane[] planes = GeometryUtility.CalculateFrustumPlanes(Camera.main);
            visible = GeometryUtility.TestPlanesAABB(planes, this.bounds);
        }

        this.renderer.enabled = visible;
    }

    public void BreakBlock(Vector3Int coordinates)
    {
        int x = Chunk.CorrectBlockCoordinate(coordinates.x);
        int y = Chunk.CorrectBlockCoordinate(coordinates.y);
        int z = Chunk.CorrectBlockCoordinate(coordinates.z);

        this.blocks[x, y, z] = null;

        int max = Chunk.Size - 1;

        if (x == 0)
        {
            var neighbour = this.world.GetChunkNoCheck(this.coordinates + new Vector3Int(-1, 0, 0));
            neighbour?.NeighbourRenderChunk();
        }
        else if (x == max)
        {
            var neighbour = this.world.GetChunkNoCheck(this.coordinates + new Vector3Int(1, 0, 0));
            neighbour?.NeighbourRenderChunk();
        }

        if (y == 0)
        {
            var neighbour = this.world.GetChunkNoCheck(this.coordinates + new Vector3Int(0, -1, 0));
            neighbour?.NeighbourRenderChunk();
        }
        else if (y == max)
        {
            var neighbour = this.world.GetChunkNoCheck(this.coordinates + new Vector3Int(0, 1, 0));
            neighbour?.NeighbourRenderChunk();
        }

        if (z == 0)
        {
            var neighbour = this.world.GetChunkNoCheck(this.coordinates + new Vector3Int(0, 0, -1));
            neighbour?.NeighbourRenderChunk();
        }
        else if (z == max)
        {
            var neighbour = this.world.GetChunkNoCheck(this.coordinates + new Vector3Int(0, 0, 1));
            neighbour?.NeighbourRenderChunk();
        }

        StartCoroutine(this.GenerateMesh());
    }

    public void SetBlock(Vector3Int coordinates, BlockScriptableObject block)
    {

        int x = Chunk.CorrectBlockCoordinate(coordinates.x);
        int y = Chunk.CorrectBlockCoordinate(coordinates.y);
        int z = Chunk.CorrectBlockCoordinate(coordinates.z);

        blocks[x, y, z] = world.blockTable.GetBlock("air");
        this.blocks[x, y, z] = block;
    }

    public void ResetBlock(Vector3Int coordinates, BlockScriptableObject block)
    {
        int x = Chunk.CorrectBlockCoordinate(coordinates.x);
        int y = Chunk.CorrectBlockCoordinate(coordinates.y);
        int z = Chunk.CorrectBlockCoordinate(coordinates.z);

        this.blocks[x, y, z] = block;

        int max = Chunk.Size - 1;

        if (x == 0)
        {
            var neighbour = this.world.GetChunkNoCheck(this.coordinates + new Vector3Int(-1, 0, 0));
            neighbour?.NeighbourRenderChunk();
        }
        else if (x == max)
        {
            var neighbour = this.world.GetChunkNoCheck(this.coordinates + new Vector3Int(1, 0, 0));
            neighbour?.NeighbourRenderChunk();
        }

        if (y == 0)
        {
            var neighbour = this.world.GetChunkNoCheck(this.coordinates + new Vector3Int(0, -1, 0));
            neighbour?.NeighbourRenderChunk();
        }
        else if (y == max)
        {
            var neighbour = this.world.GetChunkNoCheck(this.coordinates + new Vector3Int(0, 1, 0));
            neighbour?.NeighbourRenderChunk();
        }

        if (z == 0)
        {
            var neighbour = this.world.GetChunkNoCheck(this.coordinates + new Vector3Int(0, 0, -1));
            neighbour?.NeighbourRenderChunk();
        }
        else if (z == max)
        {
            var neighbour = this.world.GetChunkNoCheck(this.coordinates + new Vector3Int(0, 0, 1));
            neighbour?.NeighbourRenderChunk();
        }

        StartCoroutine(this.GenerateMesh());
    }
    






    public static int CorrectBlockCoordinate(int axis)
    {
        return axis >= 0 ? axis : (axis + Chunk.Size);
    }

    private void StartFirstRender()
    {
        StartCoroutine(this.GenerateMesh());

        this.world.GetChunkNoCheck(this.coordinates + new Vector3Int(0, 0, -1))?.NeighbourRenderChunk();
        this.world.GetChunkNoCheck(this.coordinates + new Vector3Int(0, 0, 1))?.NeighbourRenderChunk();
        this.world.GetChunkNoCheck(this.coordinates + new Vector3Int(0, 1, 0))?.NeighbourRenderChunk();
        this.world.GetChunkNoCheck(this.coordinates + new Vector3Int(0, -1, 0))?.NeighbourRenderChunk();
        this.world.GetChunkNoCheck(this.coordinates + new Vector3Int(1, 0, 0))?.NeighbourRenderChunk();
        this.world.GetChunkNoCheck(this.coordinates + new Vector3Int(-1, 0, 0))?.NeighbourRenderChunk();
    }

    public void NeighbourRenderChunk()
    {
        if (this.loaded)
        {
            StartCoroutine(this.GenerateMesh());
        }
    }

    public IEnumerator GenerateMesh()
    {
        this.neighbours.Update();

        yield return null;

        this.vertices = new List<Vector3>();
        this.triangles = new List<int>();
        this.uv = new List<Vector2>();

        this.verticesIndex = 0;

        for (int x = 0; x < Chunk.Size; x++)
        {
            for (int y = 0; y < Chunk.Size; y++)
            {
                for (int z = 0; z < Chunk.Size; z++)
                {
                    var block = this.blocks[x, y, z];
                    if (block != null)
                    {
                        var position = new Vector3(x, y, z);
                        generateBlock.GenerateBlockMethod(block, position, generateBlock.CheckBlock(x, y, z));
                    }
                }
            }
        }

        yield return null;

        var mesh = new Mesh();
        mesh.vertices = this.vertices.ToArray();
        mesh.triangles = this.triangles.ToArray();
        mesh.uv = this.uv.ToArray();
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mesh.Optimize();
        // mesh.uv = this.mesh.uv;

        this.filter.mesh = mesh;
        this.collider.sharedMesh = this.filter.mesh;
    }

}
