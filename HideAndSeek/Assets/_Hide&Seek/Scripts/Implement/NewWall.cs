using UnityEngine;

public class NewWall : IWall
{
    private GameObject _object;
    private Vector3 wallSize;

    private bool _isFake;
    private bool _isFinish;
    public NewWall(GameObject newObject, Vector3 pos, WallRefsSO wallRefsSO, bool isFinish, bool isFake)
    {
        _object = newObject;
        _object.transform.position = pos;
        SetWallSize(wallRefsSO);
        _isFinish = isFinish;
        _isFake = isFake;
        if(_isFinish)
        {
            _object.GetComponent<SpriteRenderer>().color = Color.black;
        }
    }
    public void Activate(Vector3 pos, WallRefsSO wallRefsSO)
    {
        _object.SetActive(true);
        _object.transform.position = pos;
        SetWallSize(wallRefsSO);
    }

    private void SetWallSize(WallRefsSO wallRefsSO)
    {
        if (wallRefsSO.type == WallRefsSO.WallType.Horizontal)
        {
            wallSize = new Vector3(wallRefsSO.defaultSize * wallRefsSO.multipler, wallRefsSO.defaultSize, 1);
            _object.transform.localScale = wallSize;
        }
        if (wallRefsSO.type == WallRefsSO.WallType.Vertical)
        {
            wallSize = new Vector3(wallRefsSO.defaultSize, wallRefsSO.defaultSize * wallRefsSO.multipler, 1);
            _object.transform.localScale = wallSize;
        }
        if (wallRefsSO.type == WallRefsSO.WallType.Square)
        {
            wallSize = new Vector3(wallRefsSO.defaultSize * wallRefsSO.multipler, wallRefsSO.defaultSize * wallRefsSO.multipler, 1);
            _object.transform.localScale = wallSize;
        }
    }



    public void SettingSprite(Sprite newSprite, int index)
    {
        throw new System.NotImplementedException();
    }

    public bool IsFakeWall()
    {
        return _isFake;
    }

    public bool IsFinishWall()
    {
        return _isFinish;
    }

    public void Deactivate()
    {
        _object.SetActive(false);
    }

    public bool GetActivateState()
    {
        return _object.activeSelf;
    }
}
