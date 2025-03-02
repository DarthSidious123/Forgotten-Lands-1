using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "FastNoise Settings/3D")]
public class FastNoise3DSettingsSO : FastNoiseSettingsSO
{
    [Header("Cave Settings")]

    [Range(0f, 1f)]
    public float caveTolerancy;

    [Range(0f, 0.5f)]
    public float difference = 0;

    public CaveType caveType;

    public enum CaveType { Cheese, Spaghetti }

}
