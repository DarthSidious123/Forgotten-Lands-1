using UnityEngine;

[CreateAssetMenu(menuName = "SO/Item")]
public class ItemScriptableObject : ScriptableObject
{
    public float miningPower = 1.0f;
    public float attackPower = 1.0f;
    public float durability = -1.0f;

    public Texture2D sprite;

    public Rect[] rects = null;
}
