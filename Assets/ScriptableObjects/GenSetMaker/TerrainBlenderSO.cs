using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "FastNoise Settings/Terrain Blender")]
public class BlenderSO : FastNoiseSettingsSO
{
    [Header("Blender Settings")]

    [Range(0f, 1f)]
    public float threshold = 0.6f;
}
