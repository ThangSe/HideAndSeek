using UnityEngine;

public interface IState
{
    void Act();

    void Activate(Vector3 position, Vector3 direction);

    void Deactivate();

    bool GetState();

    void SettingSprite(Sprite newSprite, int index);
}