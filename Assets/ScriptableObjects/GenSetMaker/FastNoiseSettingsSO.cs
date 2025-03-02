using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FastNoiseSettingsSO : ScriptableObject
{
    public FastNoiseLite.NoiseType noiseType = FastNoiseLite.NoiseType.Perlin;

    [Header("Main")]

    [Range(0f, 1f)]
    public float frequency;
    public float amplitude;

    public int scaleXZ = 1, scaleY = 1;

    [Header("Octaves")]

    public FastNoiseLite.FractalType fractalType = FastNoiseLite.FractalType.None;

    public int octaves = 1;


}
