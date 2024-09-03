using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FastNoiseSettingsSO : ScriptableObject
{
    public float probability = 0.5f;



    public FastNoiseLite.NoiseType noiseType = FastNoiseLite.NoiseType.Perlin;

    [Range(0f, 1f)]
    public float frequency;
    public float amplitude;


    [Header("Octaves")]

    public FastNoiseLite.FractalType fractalType = FastNoiseLite.FractalType.None;

    public int octaves = 1;


}
