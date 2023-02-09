using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// ������ʯ����
/// </summary>
public class BaseJewelSkill
{
    public float maxEnergy; // ������
    public float startEnergy; // ��ʼ����
    public float currentEnergy; // ��ǰ����
    public float deltaEnergy; // ÿ֡�����仯ֵ
    public List<Action<BaseJewelSkill>> BeforeExecuteActionList = new List<Action<BaseJewelSkill>>(); // ��ִ�м���ǰ���¼�
    public List<Action<BaseJewelSkill>> AfterExecuteActionList = new List<Action<BaseJewelSkill>>(); // ��ִ�м��ܺ���¼�
    protected Dictionary<string, float[]> ParamArrayDict = new Dictionary<string, float[]>(); // �����ֵ�

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
    /// �����Ƿ����㴥������
    /// </summary>
    /// <returns></returns>
    public bool IsEnoughEnergy()
    {
        return currentEnergy >= maxEnergy;
    }

    /// <summary>
    /// �������б�ʯ����Ч�������������ı�ʯ����ʩ���߼������ⲿ����
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
    /// ǿ��ִ�б�ʯ����Ч�������ⲿ����
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
    /// ��������д������Ϊ����ı�ʯ����ʵ������
    /// </summary>
    protected virtual void OnExecute()
    {

    }

    /// <summary>
    /// ��������д����ʼ�������ⲿ�Ĳ���
    /// </summary>
    protected virtual void InitParam()
    {

    }

    /// <summary>
    /// ���һ������
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
    /// ��ȡ����ֵ
    /// </summary>
    /// <param name="key"></param>
    /// <param name="index"></param>
    /// <returns>���û��key�������е�����Ϊ0���򷵻�0�ұ�warning���������û��Խ�磬���������أ����򷵻��������һλ</returns>
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
    /// �Ƿ񲻴��ڻ���Խ��
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
