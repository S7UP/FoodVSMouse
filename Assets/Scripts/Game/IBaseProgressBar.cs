using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// �������ӿ�
/// </summary>
public interface IBaseProgressBar
{
    public bool IsFinish();
    public void PInit();
    public void PUpdate();
    public void Show();
    public void Hide();
}
