using UnityEngine;

public interface IEffect
{
    void Act();
    void Activate(Vector3 pos);
    void Deactivate();
    bool GetActivateState();
    void SettingSprite(Sprite newSprite, int index);
}
