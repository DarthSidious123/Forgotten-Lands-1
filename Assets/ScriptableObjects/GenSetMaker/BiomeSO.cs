using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Biome Settings/Surface Biome")]
public class BiomeSO : ScriptableObject
{
    [Header("Main")]
    public bool generate = true;

    public bool generateSnow = false;
    public BlockScriptableObject topBlock, bottomBlock;
    [Min(0)]
    public int topBlockThickness, bottomBlockThickness;

    [Header("Noise Value")]
    [Range(0f, 1f)]
    public float forestMin;
    [Range(0f, 1f)]
    public float forestMax, temperatureMin, temperatureMax, wetnessMin, wetnessMax, heightMin, heightMax;
}
