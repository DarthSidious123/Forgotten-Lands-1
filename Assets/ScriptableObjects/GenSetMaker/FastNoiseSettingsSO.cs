using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FastNoiseSettingsSO : ScriptableObject
{
    public FastNoiseLite.NoiseType noiseType = FastNoiseLite.NoiseType.Perlin;

    [Header("Main")]

    public bool generate = true;


    [Range(0f, 1f)]
    public float frequency;
    public float amplitude;

    public float scaleXZ = 1, scaleY = 1;

    [Header("Octaves")]

    public FastNoiseLite.FractalType fractalType = FastNoiseLite.FractalType.None;

    public int octaves = 1;


}
