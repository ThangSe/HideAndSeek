using UnityEngine;

[CreateAssetMenu(fileName = "Map Settings SO", menuName = "ScriptableObjects/MapRefsSO")]
public class SOMapRefs : ScriptableObject
{
    [System.Serializable]
    public struct WallInfo
    {
        public enum SpecialWall
        {
            Normal,
            Fake,
            Finish
        }
        public SpecialWall specialWall;
        public WallRefsSO wallRefsSO;
        public float posX, posY;
    }
    public GameObject prefabWall;
    public Vector3[] enemySpawnPos;
    public Vector3[] itemSpawnPos;
    public Vector3[] playerSpawnPos;
    public SOItemRefs[] itemRefsSOs;
    public WallInfo[] wallsInfo;
    public Vector3 winningPos;
}
