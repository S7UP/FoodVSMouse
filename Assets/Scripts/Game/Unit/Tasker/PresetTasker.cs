using System;

/// <summary>
/// Ԥ������
/// </summary>
public class PresetTasker
{
    public Action InitAction;
    public Action UpdateAciton;
    public Func<bool> EndCondition;
    public Action EndEvent;

    /// <summary>
    /// ���췽��
    /// </summary>
    //public PresetTasker(Action InitAction, Action UpdateAciton, Func<bool> EndCondition, Action EndEvent)
    //{
    //    this.InitAction = InitAction;
    //    this.UpdateAciton = UpdateAciton;
    //    this.EndCondition = EndCondition;
    //    this.EndEvent = EndEvent;
    //}
}
