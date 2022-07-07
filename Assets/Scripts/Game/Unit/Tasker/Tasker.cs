using System;
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
    public List<Action> OtherEndListener = new List<Action>();

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
    /// ͨ��һ��Ԥ������ֱ�ӷ���
    /// </summary>
    /// <param name="presetTasker"></param>
    public void StartTask(PresetTasker presetTasker)
    {
        if(presetTasker.InitAction!=null)
            presetTasker.InitAction();
        this.UpdateAciton = presetTasker.UpdateAciton;
        this.EndCondition = presetTasker.EndCondition;
        this.EndEvent = presetTasker.EndEvent;
    }

    /// <summary>
    /// ÿ֡Ҫ������
    /// </summary>
    public virtual void MUpdate()
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

    public virtual void EndTasker()
    {
        if (EndEvent != null)
            EndEvent();
        foreach (var item in OtherEndListener)
        {
            item();
        }
        OtherEndListener.Clear();
        ExecuteRecycle();
    }

    public virtual void ExecuteRecycle()
    {
        GameManager.Instance.PushGameObjectToFactory(FactoryType.GameFactory, "Tasker/Tasker", this.gameObject);
    }

    public void AddOtherEndEvent(Action action)
    {
        OtherEndListener.Add(action);
    }
}
