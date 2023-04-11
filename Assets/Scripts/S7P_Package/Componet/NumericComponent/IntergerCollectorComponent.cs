using S7P.Component;
using System.Collections.Generic;
using S7P.Numeric;
/// <summary>
/// ����Ŀ�����ϴ洢һЩ����
/// </summary>
public class IntergerCollectorComponent : IComponent
{
    private Dictionary<string, IntModifierCollector> CollectorDict = new Dictionary<string, IntModifierCollector>();

    protected override void OnInitial()
    {
        
    }

    protected override void OnUpdate()
    {
        
    }

    protected override void OnPauseUpdate()
    {
       
    }

    protected override void OnExit()
    {
        
    }

    protected override void OnDestory()
    {
        
    }

    #region �����ʹ�õķ���
    public bool HasCollector(string key)
    {
        return CollectorDict.ContainsKey(key);
    }
    
    public IntModifierCollector GetCollector(string key)
    {
        if (CollectorDict.ContainsKey(key))
            return CollectorDict[key];
        else
            return null;
    }

    public void AddCollector(string key, IntModifierCollector collector)
    {
        if (CollectorDict.ContainsKey(key))
            CollectorDict[key] = collector;
        else
            CollectorDict.Add(key, collector);
    }

    public void RemoveCollector(string key)
    {
        if (CollectorDict.ContainsKey(key))
            CollectorDict.Remove(key);
    }
    #endregion
}
