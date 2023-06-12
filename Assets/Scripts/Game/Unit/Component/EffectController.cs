using System.Collections.Generic;

using UnityEngine;

public class EffectController : IGameControllerMember
{
    private struct Effect
    {
        public BaseEffect _instance;
        public int level;
    }

    private BaseUnit master;
    private Dictionary<string, BaseEffect> effectDict = new Dictionary<string, BaseEffect>(); // �������Ψһ����Ч����������string��Ϊkey�ģ�
    private Dictionary<string, List<Effect>> EffectGroupDict = new Dictionary<string, List<Effect>>(); // ��Ч���ֵ䣬ͬ����Ч��ͨ��͸���Ȼ�ϵķ�ʽ����
    
    private bool isHideEffect = false; // �Ƿ�������Ч

    public EffectController(BaseUnit master)
    {
        this.master = master;
    }

    public void MInit()
    {
        effectDict.Clear();
        EffectGroupDict.Clear();
    }
    public void MUpdate()
    {
        {
            List<string> delList = new List<string>();
            foreach (var keyValuePair in effectDict)
            {
                if (!keyValuePair.Value.IsValid())
                    delList.Add(keyValuePair.Key);
            }
            foreach (var t in delList)
            {
                effectDict.Remove(t);
            }
        }

        {
            foreach (var keyValuePair in EffectGroupDict)
            {
                List<Effect> delList = new List<Effect>();
                foreach (var eff in EffectGroupDict[keyValuePair.Key])
                {
                    if(!eff._instance.IsValid())
                        delList.Add(eff);
                }
                foreach (var eff in delList)
                {
                    EffectGroupDict[keyValuePair.Key].Remove(eff);
                }
                if(delList.Count > 0)
                    UpdateEffectGroup(keyValuePair.Key);
            }
        }


    }
    public void MPause()
    {
        
    }

    public void MPauseUpdate()
    {
        
    }

    public void MResume()
    {
        
    }
    public void MDestory()
    {
        {
            foreach (var keyValuePair in effectDict)
            {
                keyValuePair.Value.ExecuteDeath();
            }
            effectDict.Clear();
        }

        {
            foreach (var keyValuePair in EffectGroupDict)
            {
                foreach (var eff in EffectGroupDict[keyValuePair.Key])
                {
                    eff._instance.ExecuteDeath();
                }
            }
            EffectGroupDict.Clear();
        }

    }

    #region ��ӡ��Ƴ���Ч��ķ���
    
    public void AddEffectToGroup(string key, int level, BaseEffect e)
    {
        if (!EffectGroupDict.ContainsKey(key))
            EffectGroupDict.Add(key, new List<Effect>());
        Effect eff = new Effect() { level = level, _instance = e };
        EffectGroupDict[key].Add(eff);
        UpdateEffectGroup(key);
    }

    public void RemoveEffectFromGroup(string key, BaseEffect e)
    {
        Effect target = new Effect();
        bool flag = false;
        if (EffectGroupDict.ContainsKey(key))
        {
            foreach (var eff in EffectGroupDict[key])
            {
                if (e == eff._instance)
                {
                    target = eff;
                    flag = true;
                }
            }
            if (flag)
            {
                EffectGroupDict[key].Remove(target);
                target._instance.ExecuteDeath();
                UpdateEffectGroup(key);
            }
        }
    }
    #endregion

    #region ��ȡ��Ч�ķ���
    public bool IsContainEffect(string t)
    {
        if (effectDict.ContainsKey(t))
        {
            if (!effectDict[t].IsValid())
            {
                effectDict.Remove(t);
                return false;
            }
            else
            {
                return true;
            }
        }
        return false;
    }

    public BaseEffect GetEffect(string t)
    {
        if (IsContainEffect(t))
            return effectDict[t];
        return null;
    }
    #endregion

    #region ��ӡ��Ƴ���Ч�ķ���
    public void AddEffectToDict(string t, BaseEffect eff, Vector2 localPosition)
    {
        effectDict.Add(t, eff);
        eff.transform.SetParent(master.GetSpriteRenderer().transform);
        eff.transform.localPosition = localPosition;
        if (isHideEffect)
            eff.Hide(true);
        else
            eff.Hide(false);
    }

    public void RemoveEffectFromDict(string t)
    {
        if (IsContainEffect(t))
        {
            BaseEffect eff = effectDict[t];
            effectDict.Remove(t);
            eff.ExecuteDeath();
        }
    }
    #endregion

    #region ����˽�з���
    private void UpdateEffectGroup(string key)
    {
        EffectGroupDict[key].Sort((e1, e2) => {
            return e1.level.CompareTo(e2.level);
        });
        float alpha = 1;
        if (EffectGroupDict[key].Count > 1)
            alpha = 0.5f;
        foreach (var eff in EffectGroupDict[key])
        {
            eff._instance.transform.SetParent(master.GetSpriteRenderer().transform);
            eff._instance.SetAlpha(alpha);
        }
    }
    #endregion
    /// <summary>
    /// ����ȫ����Ч
    /// </summary>
    public void HideEffect(bool enable)
    {
        isHideEffect = enable;
        if (enable)
        {
            foreach (var keyValuePair in effectDict)
            {
                keyValuePair.Value.Hide(true);
            }
        }
        else
        {
            foreach (var keyValuePair in effectDict)
            {
                keyValuePair.Value.Hide(false);
            }
        }
    }
}
