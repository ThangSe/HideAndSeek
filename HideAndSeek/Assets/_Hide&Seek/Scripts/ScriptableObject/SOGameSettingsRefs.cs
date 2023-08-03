using UnityEngine;

[CreateAssetMenu(fileName = "Game Settings SO", menuName = "ScriptableObjects/GameSettingsRefsSO")]
public class SOGameSettingsRefs : ScriptableObject
{
    public float gamePlayingTimerMax;
    public float countdownToStartTimerMax;
    public LayerMask unWalkableLayerMask = 64;
    public LayerMask itemLayerMask;
    public LayerMask winningLayerMask;
    public float enemySpawnRate = .3f;
    public float itemSpawnRate = .05f;
    public float cameraZoomFactor = 1f;
    public float cameraZoomSpeed = 5f;
    public float winningScore = 1200;
    public float cameraOriginalSize = 20f;
    public float playerSpeed = 7f;
    public float enemySpeed = 6f;
    public float enemySpeedMultiplerDefault = 1f;
}
