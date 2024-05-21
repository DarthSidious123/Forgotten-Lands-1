using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "FastNoise Settings/3D")]
public class FastNoise3DSettingsSO : FastNoiseSettingsSO
{
    [Range(0f, 100f)]
    public float caveTolerancy;
}
