using UnityEngine;
using UnityEngine.UI;

public class NewPopup : IPopup
{
    private GameObject _object;
    private float _speed = 50f;
    private float _disappearTimer;
    private float _disappearSpeed = .25f;
    private Color _defaultTextColor = Color.black;
    private Color _textColor;

    public NewPopup(GameObject newObject)
    {
        _object = newObject;
    } 
    public void Act()
    {
        _disappearTimer -= Time.deltaTime;
        if(_disappearTimer < 0)
        {
            Deactive();
        }
        else
        {
            _object.transform.position += Vector3.up * _speed * Time.deltaTime;
            _textColor.a -= _disappearSpeed * Time.deltaTime;
            _object.transform.GetComponent<Text>().color = _textColor;
        }
    }

    public void Active(Vector3 pos, Vector3 dir, string text, float timerExisted, float popupSpeed)
    {
        _object.transform.GetComponent<Text>().color = _defaultTextColor;
        _object.transform.GetComponent <Text>().text = text;
        _textColor = _defaultTextColor;
        _disappearTimer = timerExisted;
        _speed = popupSpeed;
        _object.transform.position = pos;
        _object.SetActive(true);
    }

    public void Deactive()
    {
        _object.SetActive(false);
    }

    public bool GetActiveState()
    {
        return _object.activeSelf;
    }
}
