using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Block Table")]
public class BlockTableScriptableObject : ScriptableObject
{
    public List<Block> blocks = new List<Block>();
}
