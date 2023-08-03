using UnityEngine;

[CreateAssetMenu(fileName = "Item SO", menuName = "ScriptableObjects/ItemRefsSO")]
public class SOItemRefs : ScriptableObject
{
    public enum ItemType
    {
        Buff,
        Debuff,
    }

    public enum ItemEffect
    {
        Speed = 0,
        Score = 1,
    }
    public ItemEffect effect;
    public ItemType type;
    public string effectName;
    public float timeEffect;
}
