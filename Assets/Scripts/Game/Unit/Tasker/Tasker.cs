using System;
using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// 任务挂载器，用于执行任务
/// </summary>
public class Tasker : MonoBehaviour
{
    public Action UpdateAciton;
    public Func<bool> EndCondition;
    public Action EndEvent;
    public List<Action> OtherEndListener = new List<Action>();

    /// <summary>
    /// 分配好任务内容就可以开始了
    /// </summary>
    public void StartTask(Action InitAction, Action UpdateAciton, Func<bool> EndCondition, Action EndEvent)
    {
        InitAction();
        this.UpdateAciton = UpdateAciton;
        this.EndCondition = EndCondition;
        this.EndEvent = EndEvent;
    }

    /// <summary>
    /// 通过一个预设任务直接分配
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
    /// 每帧要做的事
    /// </summary>
    public virtual void MUpdate()
    {
        // 判断结束条件
        if (EndCondition == null || EndCondition())
        {
            EndTasker();
            return;
        }
            
        // 执行主体逻辑
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
