using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FastNoiseSettingsSO : ScriptableObject
{
    public FastNoiseLite.NoiseType noiseType = FastNoiseLite.NoiseType.Perlin;
    [Range(0f, 1f)]
    public float frequency, amplitude;
}
