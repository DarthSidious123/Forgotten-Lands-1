using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using UnityEngine;

public class World : MonoBehaviour
{
    //public Vector2Int size;

    public const int MaxWorldHeight = 128;
    public int minDelay = 1, maxDelay = 10;

    [Header("Biome Maps settings")]

    public float biomeFrequency = 0.01f;
    public float biomeAmplitude = 10f;
    public FastNoiseLite.NoiseType biomeNoiseType = FastNoiseLite.NoiseType.Perlin;

    public int biomeOctaves = 1;
    public FastNoiseLite.FractalType biomeFractalType = FastNoiseLite.FractalType.None;




    public FastNoiseLite landscapeMap, temperatureMap, wetMap;


    [Space(25)]

    [Header("Terrain settings")]

    public FastNoise2DSettingsSO planeSettings;
    public FastNoise2DSettingsSO sharpMountainSettings;
    public FastNoise2DSettingsSO smoothMountainSettings;
    public FastNoise2DSettingsSO plateauMountainSettings;

    public FastNoiseLite planeNoise, sharpMountainNoise, smoothMountainNoise, plateauMountainNoise, terrainBlenderNoise, mountainBlenderNoise;

    [Range(0f, 1f)]
    public float terrainBlenderFrequency = 0.5f, mountainBlenderFrequency = 0.5f, mountainThreshold = 0.6f;







    [Space(25)]

    [Header("Cave settings")]

    public List<FastNoise3DSettingsSO> caveSettingsList;
    public bool generateCaves = true;

    [Space(10)]


    public List<FastNoiseLite> noises2D = new List<FastNoiseLite>();
    public List<FastNoiseLite> noises3D = new List<FastNoiseLite>();


 


    [Header("World Gen - Noise")]
    public float noiseScale = 2.0f;
    public string worldSeed = "";
    public float seed = 0;


    [Header("World Gen")]
    public int seaLevel = 40;
    public int maximumLandHeight = 80;


    [Header("Misc")]
    public int renderDistance = 5;
    public int viewDistance = 10;
    public ItemTableScriptableObject itemTable;
    public BlockTableScriptableObject blockTable;

    public GameObject ChunkPrefab;

    public Dictionary<Vector3Int, Chunk> chunks = new Dictionary<Vector3Int, Chunk>();

    private Vector3Int lastPlayerChunk;





    void Awake()
    {
        CreateTerrainNoises();

        CreateFastNoiseLite3D();
        CreateTerrainMap2D();
    }

    void Start()
    {
        this.blockTable.GenerateTextureAtlas();

        this.lastPlayerChunk = this.WorldCoordinateToChunk(Player.main.transform.position);

        this.seed = this.GenerateSeed(this.worldSeed);
        StartCoroutine(this.GenerateChunksTask());
    }

    void Update()
    {
        var currentPlayerChunk = this.WorldCoordinateToChunk(Player.main.transform.position);

        if (this.lastPlayerChunk != currentPlayerChunk)
        {
            this.lastPlayerChunk = currentPlayerChunk;

            this.DestroyOutOfRangeChunks();
            StartCoroutine(this.GenerateChunksTask());
        }
    }







    //
    //
    //

    void CreateTerrainMap2D()
    {
        landscapeMap = new FastNoiseLite((int)seed);

        landscapeMap.SetNoiseType(biomeNoiseType);
        landscapeMap.SetFrequency(biomeFrequency);

        landscapeMap.SetFractalType(biomeFractalType);
        landscapeMap.SetFractalOctaves(biomeOctaves);
        landscapeMap.SetFractalLacunarity(2f);
        landscapeMap.SetFractalGain(0.5f);
    }

    void CreateTerrainNoises()
    {
        if (planeSettings != null)
        {
            planeNoise = new FastNoiseLite((int)(seed));

            planeNoise.SetNoiseType(planeSettings.noiseType);
            planeNoise.SetFrequency(planeSettings.frequency);

            planeNoise.SetFractalType(planeSettings.fractalType);
            planeNoise.SetFractalOctaves(planeSettings.octaves);
            planeNoise.SetFractalLacunarity(2);
            planeNoise.SetFractalGain(0.5f);
        }

        if (sharpMountainSettings != null)
        {
            sharpMountainNoise = new FastNoiseLite((int)(seed));

            sharpMountainNoise.SetNoiseType(sharpMountainSettings.noiseType);
            sharpMountainNoise.SetFrequency(sharpMountainSettings.frequency);

            sharpMountainNoise.SetFractalType(sharpMountainSettings.fractalType);
            sharpMountainNoise.SetFractalOctaves(sharpMountainSettings.octaves);
            sharpMountainNoise.SetFractalLacunarity(2);
            sharpMountainNoise.SetFractalGain(0.5f);
        }

        if (smoothMountainSettings != null)
        {
            smoothMountainNoise = new FastNoiseLite((int)(seed));

            smoothMountainNoise.SetNoiseType(smoothMountainSettings.noiseType);
            smoothMountainNoise.SetFrequency(smoothMountainSettings.frequency);

            smoothMountainNoise.SetFractalType(smoothMountainSettings.fractalType);
            smoothMountainNoise.SetFractalOctaves(smoothMountainSettings.octaves);
            smoothMountainNoise.SetFractalLacunarity(2);
            smoothMountainNoise.SetFractalGain(0.5f);
        }

        if (plateauMountainSettings != null)
        {
            plateauMountainNoise = new FastNoiseLite((int)(seed));
            plateauMountainNoise.SetNoiseType(plateauMountainSettings.noiseType);
            plateauMountainNoise.SetFrequency(plateauMountainSettings.frequency);

            plateauMountainNoise.SetFractalType(plateauMountainSettings.fractalType);
            plateauMountainNoise.SetFractalOctaves(plateauMountainSettings.octaves);
            plateauMountainNoise.SetFractalLacunarity(2);
            plateauMountainNoise.SetFractalGain(0.5f);
        }



        terrainBlenderNoise = new FastNoiseLite((int)(seed));

        terrainBlenderNoise.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
        terrainBlenderNoise.SetFrequency(terrainBlenderFrequency);



        mountainBlenderNoise = new FastNoiseLite((int)seed);

        mountainBlenderNoise.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
        mountainBlenderNoise.SetFrequency(mountainBlenderFrequency);

        /*
        terrainBlenderNoise.SetFractalType(terrainBlenderSettings.fractalType);
        terrainBlenderNoise.SetFractalOctaves(terrainBlenderSettings.octaves);
        terrainBlenderNoise.SetFractalLacunarity(2);
        terrainBlenderNoise.SetFractalGain(0.5f);
        */
    }








    
    void CreateFastNoiseLite3D()
    {
        foreach (var noise3D in caveSettingsList)
        {
            
            var noise = new FastNoiseLite((int)seed);
            noise.SetNoiseType(noise3D.noiseType);
            noise.SetFrequency(noise3D.frequency);
            noises3D.Add(noise);
            
        }
    }
    

    //
    //
    //




    private float GenerateSeed(string seed)
    {
        if (seed == String.Empty)
        {
            return UnityEngine.Random.Range((float)Int16.MinValue, (float)Int16.MaxValue);
        }

        System.Random random = new System.Random(seed.GetHashCode());
        return (float)(random.NextDouble() * (Int16.MaxValue - (double)Int16.MinValue)) + Int16.MinValue;
    }


    //
    //
    //







    private void DestroyOutOfRangeChunks()
    {
        var toUnloadChunks = new List<Vector3Int>();


        foreach (var chunk in chunks.Values)
        {
            if (this.IsOutOfRange(chunk.coordinates) && chunk.gameObject != null)
            {

                Destroy(chunk.gameObject);
            }
        }

    }

    private bool IsOutOfRange(Vector3Int chunkCoordinates)
    {
        return Vector3Int.Distance(this.lastPlayerChunk, chunkCoordinates) > this.viewDistance;
    }





    public Vector3Int WorldCoordinateToChunk(Vector3 coordinates)
    {
        coordinates /= Chunk.Size;

        return new Vector3Int(
            Mathf.FloorToInt(coordinates.x),
            Mathf.FloorToInt(coordinates.y),
            Mathf.FloorToInt(coordinates.z)
        );
    }

    public Vector3Int WorldCoordinateToBlock(Vector3 coordinates)
    {
        return new Vector3Int(
            Chunk.CorrectBlockCoordinate(Mathf.RoundToInt(coordinates.x % Chunk.Size)),
            Chunk.CorrectBlockCoordinate(Mathf.RoundToInt(coordinates.y % Chunk.Size)),
            Chunk.CorrectBlockCoordinate(Mathf.RoundToInt(coordinates.z % Chunk.Size))
        );
    }

    private IEnumerator GenerateChunksTask()
    {
        StartCoroutine(this.GenerateChunks());

        yield return null;
    }





    async void AsyncGenerateChunk()
    {
        var radius = renderDistance;
        var center = lastPlayerChunk;


        await Task.Run(() =>
        {
            for (var x = -renderDistance + center.x; x <= renderDistance + center.x; x++)
            {
                for (var y = -renderDistance + center.y; y <= renderDistance + center.y; y++)
                {
                    for (var z = -renderDistance + center.z; z <= renderDistance + center.z;)
                    {
                        Vector3Int chunkPos = new Vector3Int(x, y, z);

                        CreateChunk(chunkPos);
                    }
                }
            }
        });

    }




    private IEnumerator GenerateChunks()
    {
        var radius = this.renderDistance;
        var center = this.lastPlayerChunk;

        this.GenerateChunkIfInRange(center);
        for (int i = 1; i <= radius; i++)
        {
            this.GenerateChunkIfInRange(new Vector3Int(i, 0, 0) + center);
            this.GenerateChunkIfInRange(new Vector3Int(-i, 0, 0) + center);
            this.GenerateChunkIfInRange(new Vector3Int(0, i, 0) + center);
            this.GenerateChunkIfInRange(new Vector3Int(0, -i, 0) + center);
            this.GenerateChunkIfInRange(new Vector3Int(0, 0, i) + center);
            this.GenerateChunkIfInRange(new Vector3Int(0, 0, -i) + center);

            yield return UnityEngine.Random.Range(minDelay, minDelay);

            for (int j = 1; j <= i; j++)
            {
                // +x
                this.GenerateChunkIfInRange(new Vector3Int(i, j, 0) + center);
                this.GenerateChunkIfInRange(new Vector3Int(i, -j, 0) + center);
                this.GenerateChunkIfInRange(new Vector3Int(i, 0, j) + center);
                this.GenerateChunkIfInRange(new Vector3Int(i, 0, -j) + center);
                this.GenerateChunkIfInRange(new Vector3Int(i, j, j) + center);
                this.GenerateChunkIfInRange(new Vector3Int(i, -j, j) + center);
                this.GenerateChunkIfInRange(new Vector3Int(i, j, -j) + center);
                this.GenerateChunkIfInRange(new Vector3Int(i, -j, -j) + center);

                yield return UnityEngine.Random.Range(minDelay, minDelay);

                // -x
                this.GenerateChunkIfInRange(new Vector3Int(-i, j, 0) + center);
                this.GenerateChunkIfInRange(new Vector3Int(-i, -j, 0) + center);
                this.GenerateChunkIfInRange(new Vector3Int(-i, 0, j) + center);
                this.GenerateChunkIfInRange(new Vector3Int(-i, 0, -j) + center);
                this.GenerateChunkIfInRange(new Vector3Int(-i, j, j) + center);
                this.GenerateChunkIfInRange(new Vector3Int(-i, -j, j) + center);
                this.GenerateChunkIfInRange(new Vector3Int(-i, j, -j) + center);
                this.GenerateChunkIfInRange(new Vector3Int(-i, -j, -j) + center);

                yield return UnityEngine.Random.Range(minDelay, minDelay);

                // +z
                this.GenerateChunkIfInRange(new Vector3Int(0, j, i) + center);
                this.GenerateChunkIfInRange(new Vector3Int(0, -j, i) + center);
                this.GenerateChunkIfInRange(new Vector3Int(j, 0, i) + center);
                this.GenerateChunkIfInRange(new Vector3Int(-j, 0, i) + center);
                this.GenerateChunkIfInRange(new Vector3Int(j, j, i) + center);
                this.GenerateChunkIfInRange(new Vector3Int(j, -j, i) + center);
                this.GenerateChunkIfInRange(new Vector3Int(-j, j, i) + center);
                this.GenerateChunkIfInRange(new Vector3Int(-j, -j, i) + center);

                yield return UnityEngine.Random.Range(minDelay, minDelay);

                // -z
                this.GenerateChunkIfInRange(new Vector3Int(0, j, -i) + center);
                this.GenerateChunkIfInRange(new Vector3Int(0, -j, -i) + center);
                this.GenerateChunkIfInRange(new Vector3Int(j, 0, -i) + center);
                this.GenerateChunkIfInRange(new Vector3Int(-j, 0, -i) + center);
                this.GenerateChunkIfInRange(new Vector3Int(j, j, -i) + center);
                this.GenerateChunkIfInRange(new Vector3Int(j, -j, -i) + center);
                this.GenerateChunkIfInRange(new Vector3Int(-j, j, -i) + center);
                this.GenerateChunkIfInRange(new Vector3Int(-j, -j, -i) + center);

                yield return UnityEngine.Random.Range(minDelay, minDelay);

                
                // +y
                this.GenerateChunkIfInRange(new Vector3Int(0, i, j) + center);
                this.GenerateChunkIfInRange(new Vector3Int(0, i, -j) + center);
                this.GenerateChunkIfInRange(new Vector3Int(j, i, 0) + center);
                this.GenerateChunkIfInRange(new Vector3Int(-j, i, 0) + center);
                this.GenerateChunkIfInRange(new Vector3Int(j, i, j) + center);
                this.GenerateChunkIfInRange(new Vector3Int(j, i, -j) + center);
                this.GenerateChunkIfInRange(new Vector3Int(-j, i, j) + center);
                this.GenerateChunkIfInRange(new Vector3Int(-j, i, -j) + center);

                yield return UnityEngine.Random.Range(minDelay, minDelay);

                // -y
                this.GenerateChunkIfInRange(new Vector3Int(0, -i, j) + center);
                this.GenerateChunkIfInRange(new Vector3Int(0, -i, -j) + center);
                this.GenerateChunkIfInRange(new Vector3Int(j, -i, 0) + center);
                this.GenerateChunkIfInRange(new Vector3Int(-j, -i, 0) + center);
                this.GenerateChunkIfInRange(new Vector3Int(j, -i, j) + center);
                this.GenerateChunkIfInRange(new Vector3Int(j, -i, -j) + center);
                this.GenerateChunkIfInRange(new Vector3Int(-j, -i, j) + center);
                this.GenerateChunkIfInRange(new Vector3Int(-j, -i, -j) + center);

                yield return UnityEngine.Random.Range(minDelay, minDelay);
                
                
                // +x
                for (int k = 1; k < j; k++)
                {
                    this.GenerateChunkIfInRange(new Vector3Int(i, j, k) + center);
                    this.GenerateChunkIfInRange(new Vector3Int(i, -j, k) + center);
                    this.GenerateChunkIfInRange(new Vector3Int(i, j, -k) + center);
                    this.GenerateChunkIfInRange(new Vector3Int(i, -j, -k) + center);
                    this.GenerateChunkIfInRange(new Vector3Int(i, k, j) + center);
                    this.GenerateChunkIfInRange(new Vector3Int(i, -k, j) + center);
                    this.GenerateChunkIfInRange(new Vector3Int(i, k, -j) + center);
                    this.GenerateChunkIfInRange(new Vector3Int(i, -k, -j) + center);

                    yield return UnityEngine.Random.Range(minDelay, minDelay);
                }

                // -x
                for (int k = 1; k < j; k++)
                {
                    this.GenerateChunkIfInRange(new Vector3Int(-i, j, k) + center);
                    this.GenerateChunkIfInRange(new Vector3Int(-i, -j, k) + center);
                    this.GenerateChunkIfInRange(new Vector3Int(-i, j, -k) + center);
                    this.GenerateChunkIfInRange(new Vector3Int(-i, -j, -k) + center);
                    this.GenerateChunkIfInRange(new Vector3Int(-i, k, j) + center);
                    this.GenerateChunkIfInRange(new Vector3Int(-i, -k, j) + center);
                    this.GenerateChunkIfInRange(new Vector3Int(-i, k, -j) + center);
                    this.GenerateChunkIfInRange(new Vector3Int(-i, -k, -j) + center);

                    yield return UnityEngine.Random.Range(minDelay, minDelay);
                }

                // +z
                for (int k = 1; k < j; k++)
                {
                    this.GenerateChunkIfInRange(new Vector3Int(k, j, i) + center);
                    this.GenerateChunkIfInRange(new Vector3Int(k, -j, i) + center);
                    this.GenerateChunkIfInRange(new Vector3Int(-k, j, i) + center);
                    this.GenerateChunkIfInRange(new Vector3Int(-k, -j, i) + center);
                    this.GenerateChunkIfInRange(new Vector3Int(j, k, i) + center);
                    this.GenerateChunkIfInRange(new Vector3Int(j, -k, i) + center);
                    this.GenerateChunkIfInRange(new Vector3Int(-j, k, i) + center);
                    this.GenerateChunkIfInRange(new Vector3Int(-j, -k, i) + center);

                    yield return UnityEngine.Random.Range(minDelay, minDelay);
                }

                // -z
                for (int k = 1; k < j; k++)
                {
                    this.GenerateChunkIfInRange(new Vector3Int(k, j, -i) + center);
                    this.GenerateChunkIfInRange(new Vector3Int(k, -j, -i) + center);
                    this.GenerateChunkIfInRange(new Vector3Int(-k, j, -i) + center);
                    this.GenerateChunkIfInRange(new Vector3Int(-k, -j, -i) + center);
                    this.GenerateChunkIfInRange(new Vector3Int(j, k, -i) + center);
                    this.GenerateChunkIfInRange(new Vector3Int(j, -k, -i) + center);
                    this.GenerateChunkIfInRange(new Vector3Int(-j, k, -i) + center);
                    this.GenerateChunkIfInRange(new Vector3Int(-j, -k, -i) + center);

                    yield return UnityEngine.Random.Range(minDelay, minDelay);
                }

                
                // +y
                for (int k = 1; k < j; k++)
                {
                    this.GenerateChunkIfInRange(new Vector3Int(k, i, j) + center);
                    this.GenerateChunkIfInRange(new Vector3Int(k, i, -j) + center);
                    this.GenerateChunkIfInRange(new Vector3Int(-k, i, j) + center);
                    this.GenerateChunkIfInRange(new Vector3Int(-k, i, -j) + center);
                    this.GenerateChunkIfInRange(new Vector3Int(j, i, k) + center);
                    this.GenerateChunkIfInRange(new Vector3Int(j, i, -k) + center);
                    this.GenerateChunkIfInRange(new Vector3Int(-j, i, k) + center);
                    this.GenerateChunkIfInRange(new Vector3Int(-j, i, -k) + center);

                    yield return UnityEngine.Random.Range(minDelay, minDelay);
                }

                // -y
                for (int k = 1; k < j; k++)
                {
                    this.GenerateChunkIfInRange(new Vector3Int(k, -i, j) + center);
                    this.GenerateChunkIfInRange(new Vector3Int(k, -i, -j) + center);
                    this.GenerateChunkIfInRange(new Vector3Int(-k, -i, j) + center);
                    this.GenerateChunkIfInRange(new Vector3Int(-k, -i, -j) + center);
                    this.GenerateChunkIfInRange(new Vector3Int(j, -i, k) + center);
                    this.GenerateChunkIfInRange(new Vector3Int(j, -i, -k) + center);
                    this.GenerateChunkIfInRange(new Vector3Int(-j, -i, k) + center);
                    this.GenerateChunkIfInRange(new Vector3Int(-j, -i, -k) + center);

                    yield return UnityEngine.Random.Range(minDelay, minDelay);

                }
                
                
            }

            yield return UnityEngine.Random.Range(minDelay, minDelay);
        }
    }

    public void GenerateChunkIfInRange(Vector3Int coordinates)
    {
        this.GetChunk(coordinates);
    }

    public Chunk SetBlock(string id, Vector3Int coordinates)
    {
        var item = this.itemTable.GetItem(id);
        var block = item.item as BlockScriptableObject;

        var chunk = this.GetChunkByGlobalBlockCoordinates(coordinates);

        var x = coordinates.x % Chunk.Size;
        var y = coordinates.y % Chunk.Size;
        var z = coordinates.z % Chunk.Size;

        chunk.SetBlock(new Vector3Int(x, y, z), block);

        return chunk;
    }

    public Chunk GetChunkByGlobalBlockCoordinates(Vector3Int globalBlockCoordinates)
    {
        var coordinates = globalBlockCoordinates / Chunk.Size;
        return this.chunks[coordinates];
    }

    public Chunk GetChunk(Vector3Int coordinates)
    {
        if (this.chunks.ContainsKey(coordinates))
        {
            return this.chunks[coordinates];
        }

        return this.CreateChunk(coordinates);
    }

    public Chunk GetChunkNoCheck(Vector3Int coordinates)
    {
        return this.chunks.TryGetValue(coordinates, out var chunk) ? chunk : null;
    }

    public Chunk CreateChunk(Vector3Int coordinates)
    {
        if(coordinates.y >= -(MaxWorldHeight / Chunk.Size) && coordinates.y <= (MaxWorldHeight / Chunk.Size)) 
        {
            var chunkCoordinates = coordinates * Chunk.Size;

<<<<<<< HEAD
            BlockScriptableObject[,,] blocks = new BlockScriptableObject[Chunk.Size, Chunk.Size, Chunk.Size];

            Chunk chunk = null;
            GameObject obj = null;

            obj = Instantiate(ChunkPrefab, chunkCoordinates, Quaternion.identity, this.transform);



            if (chunks.TryGetValue(coordinates, out var existingChunk) && existingChunk != null)
            {
                obj = Instantiate(ChunkPrefab, chunkCoordinates, Quaternion.identity, this.transform);
                chunk.blocks = existingChunk.blocks;
                chunk.neighbours = new ChunkNeighbours(this, chunk.coordinates);
            }
            else
            {

            obj = Instantiate(ChunkPrefab, chunkCoordinates, Quaternion.identity, this.transform);
            chunk = obj.GetComponent<Chunk>();
                chunk.coordinates = coordinates;


            chunk.neighbours = new ChunkNeighbours(this, chunk.coordinates);
                chunk.Init();

                this.chunks.Add(coordinates, chunk);

                return chunk;
            }
=======
            var obj = Instantiate(ChunkPrefab, chunkCoordinates, Quaternion.identity, this.transform);
>>>>>>> parent of bef2c72 (18/14)

            var chunk = obj.GetComponent<Chunk>();
            chunk.coordinates = coordinates;


        }
        return null;
    }
}
