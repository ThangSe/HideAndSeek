using UnityEngine;

public interface IEnemy
{
    void Action();
    void Activate(Vector3 pos, Vector3 dir);
    void Deactivate();
    bool GetActiveState();
    void SettingSprite(Sprite newSprite, int index);
}
