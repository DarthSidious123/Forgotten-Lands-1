using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;
using System.Reflection;
using Unity.AI.Navigation;

[RequireComponent(typeof(MeshFilter), typeof(MeshCollider))]
public class Chunk : MonoBehaviour
{
    public const int Size = 16;



    [HideInInspector]
    public Vector3Int coordinates;

    [HideInInspector]
    public BlockScriptableObject[,,] blocks = null;

    public ChunkNeighbours neighbours;

    [HideInInspector]
    public List<Vector3> vertices;
    [HideInInspector]
    public List<int> triangles;
    [HideInInspector]
    public List<Vector2> uv;
    public int verticesIndex = 0;


    [Header("Links")]
    public NavMeshSurface surface;
    public World world;

    private MeshFilter filter;
    private new MeshCollider collider;
    private new MeshRenderer renderer;

    public GenerateBlock generateBlock;

    public bool created = false;
    private Bounds bounds;


    void Awake()
    {
        //surface = GetComponent<NavMeshSurface>();




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

        //ReloadNavMeshSurface();
    }

    void Update()
    {
        // this.FrustrumCulling();
    }


    void ReloadNavMeshSurface()
    {
        if (surface != null)
        {
            surface.BuildNavMesh();
        }
    }




    public void OnEnableChunk()
    {
        this.renderer.enabled = true;
        this.collider.enabled = true;

        this.created = true;

        this.StartFirstRender();
    }
    public void OnDisableChunk()
    {
        this.renderer.enabled = false;
        this.collider.enabled = false;

        this.world.chunks[this.coordinates] = this;

        this.created = false;
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


                landscapeHeight = GetTerrain(worldX, worldZ, out bool isSharpMountain);


                //Rivers 

                int riverDepth = 0;
                bool checkRivers = CheckRivers(worldX, worldZ, out riverDepth, landscapeHeight);



                for (int y = 0; y < Chunk.Size; y++)
                {
                    int worldY = y + worldCoordinates.y;

                    if (this.blocks[x, y, z] != null)
                    {
                        continue;
                    }

                    //3D Noises

                    bool checkCaves = CheckCaves(worldY, worldX, worldZ);


                    string rockTag = "stone";
                    var rock = CheckRock(worldX, worldY, worldZ);

                    if (rock == Rock.Stone)
                    {
                        rockTag = "stone";
                    }
                    if (rock == Rock.Marble)
                    {
                        rockTag = "marble";
                    }
                    if (rock == Rock.Granite)
                    {
                        rockTag = "granite";
                    }
                    //


                    //

                    string grassTag = "grass";
                    CheckBiome(worldX, worldZ);


                    if (worldY == landscapeHeight && checkCaves)
                    {
                        if (isSharpMountain)
                        {
                            var block = world.blockTable.GetBlock(rockTag);
                            SetBlock(new Vector3Int(x, y, z), block);
                        }
                        else
                        {
                            if (checkRivers)
                            {
                                var block = world.blockTable.GetBlock("sand");
                                SetBlock(new Vector3Int(x, y, z), block);
                            }
                            else
                            {
                                var block = this.world.blockTable.GetBlock(topBlockTeg);
                                this.SetBlock(new Vector3Int(x, y, z), block);
                            }
                        }
                    }

                    else if ((worldY < landscapeHeight) && worldY >= (landscapeHeight - 4) && checkCaves)
                    {
                        if (isSharpMountain)
                        {
                            var block = this.world.blockTable.GetBlock(rockTag);
                            this.SetBlock(new Vector3Int(x, y, z), block);
                        }
                        else
                        {
                            var block = this.world.blockTable.GetBlock(bottomBlockTeg);
                            this.SetBlock(new Vector3Int(x, y, z), block);
                        }
                    }

                    else if (worldY <= (landscapeHeight - 4) && (worldY > -World.MaxWorldHeight) && checkCaves)
                    {
                        var block = this.world.blockTable.GetBlock(rockTag);
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
        this.created = true;

        
 
        this.StartFirstRender();
    }





    int GetTerrain(int worldX, int worldZ, out bool isSharpMountain)
    {
        isSharpMountain = false;

        //

        float planeValue = world.planeNoise.GetNoise(
           worldX * world.planeSettings.scaleXZ,
           worldZ * world.planeSettings.scaleXZ);

        planeValue = (planeValue + 1) / 2 * world.planeSettings.amplitude + world.seaLevel;


        //Mountains

        float smoothMountainValue = world.smoothMountainNoise.GetNoise(
            worldX * world.smoothMountainSettings.scaleXZ,
            worldZ * world.smoothMountainSettings.scaleXZ);

        smoothMountainValue = (smoothMountainValue + 1) / 2 * world.smoothMountainSettings.amplitude + world.seaLevel;


        float sharpMountainValue = world.sharpMountainNoise.GetNoise(
            worldX * world.sharpMountainSettings.scaleXZ,
            worldZ * world.sharpMountainSettings.scaleXZ);

        sharpMountainValue = (sharpMountainValue + 1) / 2 * world.sharpMountainSettings.amplitude + world.sharpMountainSettings.offset + world.seaLevel;



        float sharpMountBlenderValue = (world.sharpMountAndTerrainBlender.GetNoise(worldX, worldZ) + 1) / 2;
        float smoothMountBlenderValue = (world.smoothMountAndTerrainBlender.GetNoise(worldX, worldZ) + 1) / 2;


        float blendedSmoothMountAndTerrain = Mathf.Lerp(planeValue, smoothMountainValue, smoothMountBlenderValue - world.smoothMountAndTerrainBlenderSO.threshold);
        float blendedSharpMountAndTerrain = Mathf.Lerp(planeValue, sharpMountainValue, sharpMountBlenderValue - world.sharpMountAndTerrainBlenderSO.threshold);

        float finalHeight = 0;

        if (blendedSharpMountAndTerrain > blendedSmoothMountAndTerrain)
        {
            isSharpMountain = true;
            finalHeight = blendedSharpMountAndTerrain;
        }
        else
        {
            finalHeight = blendedSmoothMountAndTerrain;
        }
        return Mathf.FloorToInt(finalHeight);

        /*
        float mountainBlenderValue = world.mountainBlenderNoise.GetNoise(worldX, worldZ);

        mountainBlenderValue = (mountainBlenderValue + 1) / 2;


        float mountainBlendedValue = Mathf.Lerp(smoothMountainValue, sharpMountainValue, mountainBlenderValue);




        float finalValue = smoothMountainValue;

        if (mountainBlenderValue > 0.5f)
        {
            finalValue = Mathf.Max(smoothMountainValue, mountainBlendedValue);
        }
        //
        */      
        /*
        float terrainBlenderValue = world.sharpMountAndTerrainBlender.GetNoise(worldX, worldZ);

        terrainBlenderValue = (terrainBlenderValue + 1) / 2;
        */
    }



    private enum Grass { Savanna, Normal, Taiga}


    private string topBlockTeg, bottomBlockTeg;
    private int topBlockThickness, bottomBlockThickness;


    void CheckBiome(int worldX, int worldZ)
    {
        BiomeSO biomeSO = null;

        if (world.generateBiomes)
        {
            float temperature = 0;
            float wetness = 0;

            if (world.temperatureMap != null)
            {
                temperature = ((world.temperatureMap.GetNoise(worldX * world.temperatureMapSO.scaleXZ, worldZ * world.temperatureMapSO.scaleXZ) + 1) / 2) * world.temperatureMapSO.amplitude;
            }
            if (world.wetnessMap != null)
            {
                wetness = ((world.wetnessMap.GetNoise(worldX * world.wetnessMapSO.scaleXZ, worldZ * world.wetnessMapSO.scaleXZ) + 1) / 2) * world.wetnessMapSO.amplitude;
            }

            biomeSO = world.GetBiomeSO(temperature, wetness);

            topBlockTeg = world.blockTable.GetBlock(biomeSO.topBlock);
            bottomBlockTeg = world.blockTable.GetBlock(biomeSO.bottomBlock);

            topBlockThickness = biomeSO.topBlockThickness;
            bottomBlockThickness = biomeSO.bottomBlockThickness;
        }
    }



    private enum BlockOrAir { Air, Block}

    bool CheckCaves(int worldY, int worldX, int worldZ)
    {
        if (world.generateCaves)
        {
            //FALSE IS AIR!!!
            if (world.cavesBorders != null)
            {
                int caveBorderValue = Mathf.FloorToInt(
                    ((world.cavesBorders.GetNoise(worldX, worldZ) + 1) / 2) * world.caveBordersSO.amplitude);

                if (worldY + world.caveBordersSO.offset >= caveBorderValue)
                {
                    if (CheckCrackCave() == BlockOrAir.Air)
                    {
                        return false;
                    }
                    if (CheckCavityCave() == BlockOrAir.Air)
                    {
                        return false;
                    }     
                    if (CheckNoiseWorm() == BlockOrAir.Air)
                    {
                        return false;
                    }  
                    else return true;
                }
                else return false;
            }
            else return true;
        }
        else return true;


        //
        BlockOrAir CheckCrackCave()
        {
            if (world.smallCrackCaves != null && world.smallCrackCavesSO.generate)
            {
                float smallCrackCaveValue = ((world.smallCrackCaves.GetNoise(
                    worldX * world.smallCrackCavesSO.scaleXZ,
                    worldY * world.smallCrackCavesSO.scaleY,
                    worldZ * world.smallCrackCavesSO.scaleXZ) + 1) / 2 * world.smallCrackCavesSO.amplitude);

                if (smallCrackCaveValue > 0.5f - world.smallCrackCavesSO.difference &&
                    smallCrackCaveValue < 0.5f + world.smallCrackCavesSO.difference &&
                    CheckCkackLimiter() == BlockOrAir.Air)
                {
                    return BlockOrAir.Air;
                }
                else return BlockOrAir.Block;
            }
            else return BlockOrAir.Block;
        }

        BlockOrAir CheckCkackLimiter()
        {
            if (world.smallCrackLimiter != null && world.smallCrackLimiterSO.generate)
            {
                int smallCrackLimiterValue = Mathf.FloorToInt(((world.smallCrackLimiter.GetNoise(
                    worldX * world.smallCrackLimiterSO.scaleXZ,
                    worldY * world.smallCrackLimiterSO.scaleY,
                    worldZ * world.smallCrackLimiterSO.scaleXZ) + 1) / 2) * world.smallCrackLimiterSO.amplitude);

                if (smallCrackLimiterValue >= world.smallCrackLimiterSO.caveTolerancy)
                {
                    return BlockOrAir.Air;
                }
                else return BlockOrAir.Block;
            }
            else return BlockOrAir.Block;
        }
        //

        BlockOrAir CheckCavityCave()
        {
            if (world.smallCavityCaves != null && world.smallCavityCavesSO.generate)
            {
                int smallCavityCaveValue = Mathf.FloorToInt(((world.smallCavityCaves.GetNoise(
                worldX * world.smallCavityCavesSO.scaleXZ,
                worldY * world.smallCavityCavesSO.scaleY,
                worldZ * world.smallCavityCavesSO.scaleXZ) + 1) / 2) * world.smallCavityCavesSO.amplitude);

                if (smallCavityCaveValue >= world.smallCavityCavesSO.caveTolerancy)
                {
                    return BlockOrAir.Air;
                }
                else return BlockOrAir.Block;
            }
            else return BlockOrAir.Block;
        }

        //
        BlockOrAir CheckNoiseWorm()
        {
            if (world.smallWorms != null && world.smallWormsSO.generate)
            {
                float wormHo = ((world.smallWorms.GetNoise(worldX * world.smallWormsSO.scaleXZ, worldZ * world.smallWormsSO.scaleXZ) + 1) / 2 * world.smallWormsSO.amplitude);

                float wormVe = ((world.smallWorms.GetNoise(worldX * world.smallWormsSO.scaleXZ, worldY* world.smallWormsSO.scaleY, worldZ * world.smallWormsSO.scaleXZ) + 1) / 2
                    * world.smallWormsSO.amplitude);



                if (wormHo > 0.5f - world.smallWormsSO.differenceHo * world.smallWormsSO.differenceModifier &&
                    wormHo < 0.5f + world.smallWormsSO.differenceHo * world.smallWormsSO.differenceModifier &&
                    wormVe > 0.5f - world.smallWormsSO.differenceVe * world.smallWormsSO.differenceModifier &&
                    wormVe < 0.5f + world.smallWormsSO.differenceVe * world.smallWormsSO.differenceModifier)
                {
                    return BlockOrAir.Air;
                }
                else return BlockOrAir.Block;

            }
            else return BlockOrAir.Block;
        }
        //
    }


    //
    private enum Rock { Granite, Marble, Stone }

    Rock CheckRock(int worldX, int worldY, int worldZ)
    {
        if (world.generateRocks && world.BaseRock != null)
        {
            return MarbleOrGranite();
        }
        else return Rock.Stone;


        Rock MarbleOrGranite()
        {
            float rockValue = (world.BaseRock.GetNoise(worldX + world.BaseRockSO.scaleXZ, worldY + world.BaseRockSO.scaleY, worldZ + world.BaseRockSO.scaleXZ) + 1) / 2 * world.BaseRockSO.amplitude;

            if (rockValue < 0.5f - world.BaseRockSO.difference)
            {
                return Rock.Marble;
            }
            else if (rockValue > 0.5f + world.BaseRockSO.difference)
            {
                return Rock.Granite;
            }
            else return Rock.Stone;
        }
    }
    //


    bool CheckRivers(int worldX, int worldZ, out int riverDepth, int landscapeHeight)
    {
        riverDepth = 0;

        if (world.generateRivers)
        {
            float riverValue = world.riverNoise.GetNoise(worldX * world.riverSettings.scaleXZ, worldZ * world.riverSettings.scaleXZ);
            riverValue = MathF.Abs(riverValue);

            if (landscapeHeight < world.riverThreshold + world.seaLevel)
            {
                return true;
            }


            /*
            float riverValue = world.riverNoise.GetNoise(worldX * world.riverSettings.scale, worldZ * world.riverSettings.scale);
            riverValue = MathF.Abs(riverValue);

            if (riverValue > world.riverThreshold)
            {
                riverDepth = Mathf.FloorToInt(riverValue) * 5;
                return true;
            }

            */
        }
        return false;
    }

    bool CheckRivers(int worldX, int worldZ)
    {
        if (world.generateRivers)
        {
            float riverValue = world.riverNoise.GetNoise(
                worldX * world.riverSettings.scaleXZ,
                worldZ * world.riverSettings.scaleXZ);

            //riverValue = (riverValue + 1) / 2;
            riverValue = MathF.Abs(riverValue);


            if (riverValue < world.riverThreshold)
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
            return false;
        }
    }

    //
    //
    //






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

        //blocks[x, y, z] = world.blockTable.GetBlock("air");
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
        if (world.chunks.TryGetValue(coordinates, out var chunk) && chunk != null && chunk.gameObject != null)
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
