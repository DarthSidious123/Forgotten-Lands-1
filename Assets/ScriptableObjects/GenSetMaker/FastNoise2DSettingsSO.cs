using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "FastNoise Settings/2D")]
public class FastNoise2DSettingsSO : FastNoiseSettingsSO
{
    [Range(0, 100)]
    public int maximumHeight = 20, scale = 50;
}
