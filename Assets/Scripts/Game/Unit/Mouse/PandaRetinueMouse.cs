using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ��è�����
/// </summary>
public class PandaRetinueMouse : MouseUnit
{
    /// <summary>
    /// ���ŷ��ж���
    /// </summary>
    public void PlayFlyClip()
    {
        animator.Play("Fly");
    }
}
