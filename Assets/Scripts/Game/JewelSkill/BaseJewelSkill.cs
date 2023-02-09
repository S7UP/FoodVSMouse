using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// 基础宝石技能
/// </summary>
public class BaseJewelSkill
{
    public float maxEnergy; // 总能量
    public float startEnergy; // 初始能量
    public float currentEnergy; // 当前能量
    public float deltaEnergy; // 每帧能量变化值
    public List<Action<BaseJewelSkill>> BeforeExecuteActionList = new List<Action<BaseJewelSkill>>(); // 在执行技能前的事件
    public List<Action<BaseJewelSkill>> AfterExecuteActionList = new List<Action<BaseJewelSkill>>(); // 在执行技能后的事件
    protected Dictionary<string, float[]> ParamArrayDict = new Dictionary<string, float[]>(); // 参数字典

    public BaseJewelSkill(float maxEnergy, float startEnergy, float deltaEnergy, Dictionary<string, float[]> paramDict)
    {
        this.startEnergy = startEnergy;
        //this.startEnergy = maxEnergy;
        this.maxEnergy = maxEnergy;
        this.currentEnergy = startEnergy;
        this.deltaEnergy = deltaEnergy;
        if(paramDict!=null)
            foreach (var keyValuePair in paramDict)
            {
                ParamArrayDict.Add(keyValuePair.Key, keyValuePair.Value);
            }
    }

    public void Initial()
    {
        InitParam();
        currentEnergy = startEnergy;
    }

    public void Update()
    {
        if (currentEnergy < maxEnergy)
            currentEnergy = Mathf.Max(0, Mathf.Min(currentEnergy + deltaEnergy, maxEnergy));
    }
    
    /// <summary>
    /// 能量是否满足触发技能
    /// </summary>
    /// <returns></returns>
    public bool IsEnoughEnergy()
    {
        return currentEnergy >= maxEnergy;
    }

    /// <summary>
    /// 尝试试行宝石技能效果，属于正常的宝石技能施放逻辑，由外部调用
    /// </summary>
    public bool TryExecute()
    {
        if (IsEnoughEnergy())
        {
            Execute();
            currentEnergy = 0;
            return true;
        }
        else
            return false;
    }

    /// <summary>
    /// 强制执行宝石技能效果，由外部调用
    /// </summary>
    public void Execute()
    {
        foreach (var action in BeforeExecuteActionList)
        {
            action(this);
        }
        OnExecute();
        foreach (var action in AfterExecuteActionList)
        {
            action(this);
        }
    }

    /// <summary>
    /// 由子类重写，里面为具体的宝石技能实现内容
    /// </summary>
    protected virtual void OnExecute()
    {

    }

    /// <summary>
    /// 由子类重写，初始化来自外部的参数
    /// </summary>
    protected virtual void InitParam()
    {

    }

    /// <summary>
    /// 添加一个参数
    /// </summary>
    public void AddParamArray(string key, float[] arr)
    {
        if (!ParamArrayDict.ContainsKey(key))
            ParamArrayDict.Add(key, arr);
        else
        {
            ParamArrayDict[key] = arr;
        }
    }

    public void AddParamArray(string key, List<float> list)
    {
        AddParamArray(key, list.ToArray());
    }

    /// <summary>
    /// 获取参数值
    /// </summary>
    /// <param name="key"></param>
    /// <param name="index"></param>
    /// <returns>如果没有key或者其中的数组为0，则返回0且报warning，如果有且没有越界，则正常返回，否则返回数组最后一位</returns>
    public float GetParamValue(string key, int index)
    {
        if (ParamArrayDict.ContainsKey(key) && ParamArrayDict[key].Length > 0)
        {
            float[] arr = ParamArrayDict[key];
            if (index < arr.Length)
                return arr[index];
            else
                return arr[arr.Length - 1];
        }
        else
        {
            return 0;
        }
    }

    /// <summary>
    /// 是否不存在或者越界
    /// </summary>
    /// <param name="key"></param>
    /// <param name="index"></param>
    /// <returns></returns>
    public bool IsParamValueInValidOrOutOfBound(string key, int index)
    {
        if (ParamArrayDict.ContainsKey(key) && ParamArrayDict[key].Length > 0)
        {
            float[] arr = ParamArrayDict[key];
            if (index < arr.Length)
                return false;
            else
                return true;
        }
        return true;
    }
}
