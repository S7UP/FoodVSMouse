using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// ���������������ִ������
/// </summary>
public class Tasker : MonoBehaviour
{
    public Action UpdateAciton;
    public Func<bool> EndCondition;
    public Action EndEvent;

    /// <summary>
    /// ������������ݾͿ��Կ�ʼ��
    /// </summary>
    public void StartTask(Action InitAction, Action UpdateAciton, Func<bool> EndCondition, Action EndEvent)
    {
        InitAction();
        this.UpdateAciton = UpdateAciton;
        this.EndCondition = EndCondition;
        this.EndEvent = EndEvent;
    }

    /// <summary>
    /// ÿ֡Ҫ������
    /// </summary>
    public void MUpdate()
    {
        // �жϽ�������
        if (EndCondition == null || EndCondition())
        {
            EndTasker();
            return;
        }
            
        // ִ�������߼�
        if (UpdateAciton != null)
            UpdateAciton();
        
    }

    public void EndTasker()
    {
        if (EndEvent != null)
            EndEvent();
        GameManager.Instance.PushGameObjectToFactory(FactoryType.GameFactory, "Tasker/Tasker", this.gameObject);
    }
}
