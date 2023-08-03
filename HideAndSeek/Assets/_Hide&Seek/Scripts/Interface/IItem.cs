using UnityEngine;

public interface IItem
{
    void Activate(Vector3 pos, SOItemRefs itemRefsSO);
    void Deactivate();
    bool GetActivateState();
    void SettingSprite(Sprite newSprite);
    int GetIntTypeEffect();

    float GetTimeEffect();
    string GetItemName();
    GameObject GetGameObject();
}
