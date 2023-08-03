using UnityEngine;

public interface IWall
{
    void Activate(Vector3 pos, WallRefsSO wallRefsSO);
    void Deactivate();
    bool GetActivateState();
    void SettingSprite(Sprite newSprite, int index);

    bool IsFakeWall();
    bool IsFinishWall();
}
