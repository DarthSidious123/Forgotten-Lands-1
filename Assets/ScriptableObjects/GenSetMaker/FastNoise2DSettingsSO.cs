using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "FastNoise Settings/2D")]
public class FastNoise2DSettingsSO : FastNoiseSettingsSO
{
    public int offset = 0;

    [Range(0f, 0.5f)]
    public float difference = 0;
}
