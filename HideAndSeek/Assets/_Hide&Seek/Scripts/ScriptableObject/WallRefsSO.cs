using UnityEngine;

[CreateAssetMenu(fileName = "Wall Settings SO", menuName = "ScriptableObjects/WallRefsSO")]
public class WallRefsSO : ScriptableObject
{
    public float defaultSize = .75f;
    public enum WallType
    {
        Vertical,
        Horizontal,
        Square,
    }
    public WallType type;    
    [Range(1, 30)]
    public int multipler;

}
