using S7P.Component;
using System.Collections.Generic;
using S7P.Numeric;
/// <summary>
/// 挂在目标身上存储一些变量
/// </summary>
public class FloatCollectorComponent : IComponent
{
    public const string ComponentKey = "FloatCollectorComponent";
    private Dictionary<string, FloatModifierCollector> CollectorDict = new Dictionary<string, FloatModifierCollector>();

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

    #region 供外界使用的方法
    public bool HasCollector(string key)
    {
        return CollectorDict.ContainsKey(key);
    }
    
    public FloatModifierCollector GetCollector(string key)
    {
        if (CollectorDict.ContainsKey(key))
            return CollectorDict[key];
        else
            return null;
    }

    public List<FloatModifierCollector> GetAllCollector()
    {
        List<FloatModifierCollector> list = new List<FloatModifierCollector>();
        foreach (var keyValuePair in CollectorDict)
        {
            list.Add(keyValuePair.Value);
        }
        return list;
    }

    public void AddCollector(string key, FloatModifierCollector collector)
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

    public void AddModifier(string key, FloatModifier mod)
    {
        if (!HasCollector(key))
            AddCollector(key, new FloatModifierCollector());
        GetCollector(key).AddModifier(mod);
    }

    public void RemoveModifier(string key, FloatModifier mod)
    {
        if (HasCollector(key))
        {
            GetCollector(key).RemoveModifier(mod);
        }
    }
    #endregion
}
