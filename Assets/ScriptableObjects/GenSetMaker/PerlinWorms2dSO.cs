using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "FastNoise Settings/NoiseWorms")]
public class NoiseWormsSO : FastNoiseSettingsSO
{




    [Space(10)]

    [Range(0f, 0.5f)]
    public float differenceHo = 0f, differenceVe = 0f;
    public float differenceModifier = 0f;
}


