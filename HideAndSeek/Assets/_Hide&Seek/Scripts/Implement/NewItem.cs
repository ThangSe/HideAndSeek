using UnityEngine;

public class NewItem : IItem
{
    private GameObject _object;
    private float _timeEffect;
    private string _itemName;
    public enum EffectType
    {
        Speed = 0,
        Score = 1,
    }
    public EffectType _effectType;

    public NewItem(GameObject newObject, Vector3 pos, SOItemRefs itemRefsSO)
    {
        _object = newObject;
        _object.transform.position = pos;
        _timeEffect = itemRefsSO.timeEffect;
        _itemName = itemRefsSO.effectName;
        SetTypeItem(itemRefsSO);
    }

    private void SetTypeItem(SOItemRefs itemRefsSO)
    {
        if (itemRefsSO.effect == SOItemRefs.ItemEffect.Speed)
        {
            _effectType = EffectType.Speed;
        }
        if (itemRefsSO.effect == SOItemRefs.ItemEffect.Score)
        {
            _effectType = EffectType.Score;
        }
    }

    public void Activate(Vector3 pos, SOItemRefs itemRefsSO)
    {
        _object.transform.position = pos;
        _timeEffect = itemRefsSO.timeEffect;
        _itemName = itemRefsSO.effectName;
        SetTypeItem(itemRefsSO);
        _object.SetActive(true);
    }
    public void Deactivate()
    {
        _object.SetActive(false);
    }

    public bool GetActivateState()
    {
        return _object.activeSelf;
    }

    public void SettingSprite(Sprite newSprite)
    {
        _object.GetComponent<SpriteRenderer>().sprite = newSprite;
    }

    public GameObject GetGameObject()
    {
        return _object;
    }

    public int GetIntTypeEffect()
    {
        return (int)_effectType;
    }

    public float GetTimeEffect()
    {
        return _timeEffect;
    }

    public string GetItemName()
    {
        return _itemName;
    }
}
