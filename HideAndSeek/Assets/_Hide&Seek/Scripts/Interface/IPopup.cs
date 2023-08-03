using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPopup 
{
    public void Act();
    public void Active(Vector3 pos, Vector3 dir, string text, float timerExisted, float popupSpeed);
    public void Deactive();
    public bool GetActiveState();
}
