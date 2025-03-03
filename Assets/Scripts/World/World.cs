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

    public bool generateBiomes = true;

    public FastNoise2DSettingsSO temperatureMapSO, wetMapSO;

    public FastNoiseLite temperatureMap, wetMap;


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

    [Header("Rivers settings")]

    public FastNoise2DSettingsSO riverSettings;

    public bool generateRivers = true;
    [Range(0f, 25f)]
    public float riverThreshold = 0, riverWidth = 0;
    public FastNoiseLite riverNoise;

    [Space(25)]

    [Header("Cave settings")]

    public bool generateCaves = true;

    public FastNoise3DSettingsSO smallCrackCavesSO, smallCavityCavesSO, smallCrackLimiterSO, mediumCavesSO, bigCavesSO;

    public FastNoiseLite smallCrackCaves, smallCavityCaves, smallCrackLimiter, mediumCaves, bigCaves;


    public FastNoiseWormsSO smallWormsSO;
    public FastNoiseLite smallWorms;

    public FastNoise2DSettingsSO caveBordersSO;
    public FastNoiseLite cavesBorders;


    [Space(10)]

    public bool generateRocks = true;
    public FastNoise3DSettingsSO BaseRockSO;
    public FastNoiseLite BaseRock;



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
    //public Dictionary<Vector3Int, Chunk> activeChunks = new Dictionary<Vector3Int, Chunk>();

    private Vector3Int lastPlayerChunk;





    void Awake()
    {
        CreateTerrainNoises();

        CreateCaveNoises();
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

        //DestroyFarChunks();

        Vector3 playerPos = Player.main.transform.position;
        //DestroyOutOfRangeChunks(playerPos);
        StartCoroutine(this.GenerateChunksTask());
    }


    //
    //
    //

    void CreateTerrainNoises()
    {
        if (planeSettings != null)
        {
            planeNoise = FastCreate2dNoise(planeSettings);
        }
        if (sharpMountainSettings != null)
        {
            sharpMountainNoise = FastCreate2dNoise(sharpMountainSettings);
        }
        if (smoothMountainSettings != null)
        {
            smoothMountainNoise = FastCreate2dNoise(smoothMountainSettings);
        }
        if (plateauMountainSettings != null)
        {
            plateauMountainNoise = new FastNoiseLite((int)(seed));
        }


        if (riverSettings != null)
        {
            riverNoise = FastCreate2dNoise(riverSettings);
        }


        if (temperatureMapSO != null)
        {
            temperatureMap = FastCreate2dNoise(temperatureMapSO);
        }



        terrainBlenderNoise = new FastNoiseLite((int)(seed));

        terrainBlenderNoise.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
        terrainBlenderNoise.SetFrequency(terrainBlenderFrequency);



        mountainBlenderNoise = new FastNoiseLite((int)seed);

        mountainBlenderNoise.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
        mountainBlenderNoise.SetFrequency(mountainBlenderFrequency);


    }






    FastNoiseLite FastCreate2dNoise(FastNoise2DSettingsSO noise2dSO)
    {
        FastNoiseLite newNoise = new FastNoiseLite((int)seed);
        newNoise.SetNoiseType(noise2dSO.noiseType);
        newNoise.SetFrequency(noise2dSO.frequency);

        newNoise.SetFractalType(noise2dSO.fractalType);
        newNoise.SetFractalOctaves(noise2dSO.octaves);
        newNoise.SetFractalLacunarity(2);
        newNoise.SetFractalGain(0.5f);

        return newNoise;
    }

    
    void CreateCaveNoises()
    {
        if (caveBordersSO != null)
        {
            cavesBorders = new FastNoiseLite((int)seed);
            cavesBorders.SetNoiseType(caveBordersSO.noiseType);
            cavesBorders.SetFrequency(caveBordersSO.frequency);

            cavesBorders.SetFractalType(caveBordersSO.fractalType);
            cavesBorders.SetFractalOctaves(caveBordersSO.octaves);
            cavesBorders.SetFractalLacunarity(2f);
            cavesBorders.SetFractalGain(0.5f);
        }
        if (smallCrackCavesSO != null)
        {
            smallCrackCaves = FastCreate3dNoise(smallCrackCavesSO);  
        }
        if (smallCavityCavesSO != null)
        {
            smallCavityCaves = FastCreate3dNoise(smallCavityCavesSO);
        }
        if (smallCrackLimiterSO != null)
        {
            smallCrackLimiter = FastCreate3dNoise(smallCrackLimiterSO);
        }

        if (smallWormsSO != null)
        {
            smallWorms = FastCreate3dWorm(smallWormsSO);
        }

        if (BaseRockSO != null)
        {
            BaseRock = FastCreate3dNoise(BaseRockSO);
        }





        if (mediumCavesSO != null)
        {
            mediumCaves = new FastNoiseLite((int)seed);
            mediumCaves.SetNoiseType(mediumCavesSO.noiseType);
            mediumCaves.SetFrequency(mediumCavesSO.frequency);
        }


    }
    FastNoiseLite FastCreate3dNoise(FastNoise3DSettingsSO noise3dSO)
    {
        FastNoiseLite newNoise = new FastNoiseLite((int)seed);
        newNoise.SetNoiseType(noise3dSO.noiseType);
        newNoise.SetFrequency(noise3dSO.frequency);

        newNoise.SetFractalType(noise3dSO.fractalType);
        newNoise.SetFractalOctaves(noise3dSO.octaves);
        newNoise.SetFractalLacunarity(2f);
        newNoise.SetFractalGain(0.5f);

        return newNoise;
    }

    FastNoiseLite FastCreate3dWorm(FastNoiseWormsSO worm3dSO)
    {
        FastNoiseLite newWorm = new FastNoiseLite((int)seed);
        newWorm.SetNoiseType(worm3dSO.noiseType);
        newWorm.SetFrequency(worm3dSO.frequency);

        newWorm.SetFractalType(worm3dSO.fractalType);
        newWorm.SetFractalOctaves(worm3dSO.octaves);
        newWorm.SetFractalLacunarity(2f);
        newWorm.SetFractalGain(0.5f);

        return newWorm;
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



    
    private void DestroyFarChunks()
    {
        /*
        var toRemove = new List<Vector3Int>();

        foreach (var activeChunk in activeChunks.Keys)
        {
            if (activeChunk.x + lastPlayerChunk.x >= renderDistance || -activeChunk.x + lastPlayerChunk.x <= -renderDistance)
            {
                toRemove.Add(activeChunk);
            }
            else if (activeChunk.y + lastPlayerChunk.y >= renderDistance || -activeChunk.y + lastPlayerChunk.y <= -renderDistance)
            {
                toRemove.Add(activeChunk);
            }
            else if (activeChunk.z + lastPlayerChunk.z >= renderDistance || -activeChunk.z + lastPlayerChunk.z <= -renderDistance)
            {
                toRemove.Add(activeChunk);
            }
        }


        foreach (var chunkPos in toRemove)
        {
            activeChunks.TryGetValue(chunkPos, out var chunk);
            Destroy(chunk.gameObject);
            activeChunks.Remove(chunkPos);
        }

        toRemove.Clear();
        */
        
    }


    private void DestroyOutOfRangeChunks()
    {
        var toRemove = new List<Vector3Int>();

        foreach (var chunk in chunks.Values)
        {
            var coordinates = chunk.coordinates;
            if (this.IsOutOfRange(coordinates))
            {
                Destroy(chunk.gameObject);
                toRemove.Add(coordinates);
            }
        }

        foreach (var i in toRemove)
        {
            this.chunks.Remove(i);
        }

    }

    private void DestroyOutOfRangeChunks(Vector3 playerPosition)
    {
        foreach (var T in chunks)
        {
            Vector3Int chunkCoordinates = T.Key;
            Chunk chunk = T.Value;

            float distance = Vector3.Distance(playerPosition, chunk.transform.position);

            if (distance > viewDistance)
            {
                if (chunk.created)
                {
                    chunk.OnDisableChunk();
                }
            }
            else
            {
                if (!chunk.created)
                {
                    chunk.OnEnableChunk();
                }
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
        Vector3Int center = lastPlayerChunk;
        List<Vector3Int> chunksToGenerate = new List<Vector3Int>();

        for (int x = -renderDistance; x <= renderDistance; x++)
        {
            for (int y = -renderDistance; y <= renderDistance; y++)
            {
                for (int z = -renderDistance; z <= renderDistance; z++)
                {
                    Vector3Int chunkPos = center + new Vector3Int(x, y, z);

                    if (!chunks.ContainsKey(chunkPos) && Vector3Int.Distance(chunkPos, center) <= renderDistance)
                    {
                        chunksToGenerate.Add(chunkPos);
                    }
                }
            }
        }

        chunksToGenerate.Sort((a, b) => Vector3Int.Distance(center, a).CompareTo(Vector3Int.Distance(center, b)));

        foreach (var chunkPos in chunksToGenerate)
        {
            if (!chunks.ContainsKey(chunkPos))
            {
                CreateChunk(chunkPos);
            }
            yield return null;
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

    //
    public Chunk GetOrCreateChunk(Vector3Int coordinates)
    {
        if (chunks.TryGetValue(coordinates, out Chunk chunk))
        {
            return chunk;
        }

        GameObject chunkObj = Instantiate(ChunkPrefab, coordinates * Chunk.Size, Quaternion.identity, transform);
        chunk = chunkObj.GetComponent<Chunk>();
        chunk.coordinates = coordinates;
        chunk.Init();


        chunks[coordinates] = chunk;

        return chunk;
    }



    public void EnableChunk(Vector3Int coordinates)
    {
        if (chunks.ContainsKey(coordinates))
        {
            chunks[coordinates].OnEnableChunk();
        }
    }
    public void DisableChunk(Vector3Int coordinates)
    {
        if (chunks.ContainsKey(coordinates))
        {
            chunks[coordinates].OnDisableChunk();
        }
    }
    //

    public Chunk GetChunkNoCheck(Vector3Int coordinates)
    {
        return this.chunks.TryGetValue(coordinates, out var chunk) ? chunk : null;
    }

    public Chunk CreateChunk(Vector3Int coordinates)
    {
        if(coordinates.y >= -(MaxWorldHeight / Chunk.Size) && coordinates.y <= (MaxWorldHeight / Chunk.Size)) 
        {
            var chunkCoordinates = coordinates * Chunk.Size;

            var obj = Instantiate(ChunkPrefab, chunkCoordinates, Quaternion.identity, this.transform);

            var chunk = obj.GetComponent<Chunk>();
            chunk.coordinates = coordinates;

            this.chunks.Add(coordinates, chunk);
            //activeChunks.Add(coordinates, chunk);
        }
        return null;
    }
}
