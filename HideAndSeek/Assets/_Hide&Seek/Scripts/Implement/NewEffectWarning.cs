using UnityEngine;

public class NewEffectWarning : IState
{

    private GameObject _object;

    public NewEffectWarning(GameObject newObject, Vector3 pos)
    {
        _object = newObject;
        _object.transform.position = pos;
    }

    public void Act()
    {
        throw new System.NotImplementedException();
    }

    public void Activate(Vector3 pos,Vector3 direction)
    {
        _object.transform.position = pos;
        _object.SetActive(true);
    }

    public void Deactivate()
    {
        _object.SetActive(false);
    }

    public bool GetState()
    {
        return _object.activeSelf;
    }

    public void SettingSprite(Sprite newSprite, int index)
    {
        throw new System.NotImplementedException();
    }
}
