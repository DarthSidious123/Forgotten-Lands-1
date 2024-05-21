using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;

[RequireComponent(typeof(MeshFilter), typeof(MeshCollider))]
public class Chunk : MonoBehaviour
{
    public const int Size = 16;

    private World world;

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
            int worldX = x  + worldCoordinates.x;

            for (int z = 0; z < Chunk.Size; z++)
            {
                int worldZ = z + worldCoordinates.z;


                List<int> heights2D = new List<int>();

                for (int noise2d = 0; noise2d < world.noises2D.Count; noise2d++)
                {
                    int height2D = Mathf.FloorToInt(
                    ((world.noises2D[0].GetNoise(worldX, worldZ) + 1) / 2) * world.terrainSettingsList[0].amplitude * world.maximumLandHeight
                    + world.minimumSeeLevel);

                    heights2D.Add(height2D);
                }



                for (int y = 0; y < Chunk.Size; y++)
                {

                    int worldY = y + worldCoordinates.y;


                    List<int> heights3D = new List<int>();

                    for (int noise3d = 0; noise3d < world.noises3D.Count; noise3d++)
                    {
                        int height3D = Mathf.FloorToInt(
                            ((world.noises3D[noise3d].GetNoise(worldX, worldY, worldZ) + 1) / 2) * world.caveSettingsList[noise3d].amplitude);

                        heights3D.Add(height3D);
                    }

                    bool allHeights3D = false;
                    if (heights3D[0] <= world.caveSettingsList[0].caveTolerancy &&
                        heights3D[1] <= world.caveSettingsList[1].caveTolerancy &&
                        heights3D[2] <= world.caveSettingsList[2].caveTolerancy)
                    {
                        allHeights3D = true;
                    }



                    if (worldY == heights2D[0] && allHeights3D)
                    {
                        var block = this.world.blockTable.GetBlock("grass");
                        this.SetBlock(new Vector3Int(x, y, z), block);
                    }

                    else if ((worldY < heights2D[0]) && worldY >= (heights2D[0] - 4) && allHeights3D)
                    {
                        var block = this.world.blockTable.GetBlock("dirt");
                        this.SetBlock(new Vector3Int(x, y, z), block);
                    }

                    else if (worldY <= (heights2D[0] - 4) && (worldY > -World.MaxWorldHeight) && allHeights3D)
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




                /*
                int height = Mathf.FloorToInt(this.world.Noise(worldX, worldZ) * this.world.maximumLandHeight) + this.world.minimumSeeLevel;



                for (int y = 0; y < Chunk.Size; y++)
                {
                    var worldY = y + worldCoordinates.y;
                    
                    if (worldY >= 0 && worldY <= 128 && worldY <= height && this.world.Noise(worldX, worldZ, worldY) <= 0.3f)
                    {
                        if (worldY == height)
                        {
                            var block = this.world.blockTable.GetBlock("grass");
                            this.SetBlock(new Vector3Int(x, y, z), block);
                        }
                        else if (worldY >= (height - 4))
                        {
                            var block = this.world.blockTable.GetBlock("dirt");
                            this.SetBlock(new Vector3Int(x, y, z), block);
                        }
                        else if (worldY <= (height - 4) && worldY > -512)
                        {
                            var block = this.world.blockTable.GetBlock("stone");
                            this.SetBlock(new Vector3Int(x, y, z), block);
                        }
                    }

                    if (worldY < 0 && worldY >= -511 && worldY <= height && this.world.Noise(worldX, worldZ, worldY) <= world.CaveValue)
                    {
                        if (worldY <= (height - 4) && worldY > -512)
                        {
                            var block = this.world.blockTable.GetBlock("stone");
                            this.SetBlock(new Vector3Int(x, y, z), block);
                        }
                    }
                    

                    if (worldY == -512)
                    {
                        var block = this.world.blockTable.GetBlock("bedrock");
                        this.SetBlock(new Vector3Int(x, y, z), block);
                    }
                
                }
                */
            }
        }

        this.loaded = true;
        this.StartFirstRender();
    }

    private void CreateBoundsBox()
    {
        var chunkSize = Chunk.Size;
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
        var x = Chunk.CorrectBlockCoordinate(coordinates.x);
        var y = Chunk.CorrectBlockCoordinate(coordinates.y);
        var z = Chunk.CorrectBlockCoordinate(coordinates.z);

        this.blocks[x, y, z] = null;

        var max = Chunk.Size - 1;

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
        var x = Chunk.CorrectBlockCoordinate(coordinates.x);
        var y = Chunk.CorrectBlockCoordinate(coordinates.y);
        var z = Chunk.CorrectBlockCoordinate(coordinates.z);

        this.blocks[x, y, z] = block;
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
