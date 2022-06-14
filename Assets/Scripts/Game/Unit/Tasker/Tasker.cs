using System;
using System.Collections;
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
    /// 每帧要做的事
    /// </summary>
    public void MUpdate()
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

    public void EndTasker()
    {
        if (EndEvent != null)
            EndEvent();
        GameManager.Instance.PushGameObjectToFactory(FactoryType.GameFactory, "Tasker/Tasker", this.gameObject);
    }
}
