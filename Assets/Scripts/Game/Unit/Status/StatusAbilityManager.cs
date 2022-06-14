using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using UnityEngine;

/// <summary>
/// 时效状态（BUFF）管理器（附加在单位上的），让每个单位都可以执行BUFF效果的东西
/// </summary>
public sealed class StatusAbilityManager
{
    public List<StatusAbility> statusAbilityList = new List<StatusAbility>();
    public List<StatusAbility> removeList = new List<StatusAbility>();
    // 唯一性buff（可以有多个施加源，但效果与一次相同，且所有施加源消失时buff才会失效）
    public class UniqueStatusAbility
    {
        public StatusAbility status;
        public int count; // 当前施加此状态buff数

        public void SetStatus(StatusAbility s) 
        {
            status = s;
        }
        public void AddCount()
        {
            count++;
        }
        public void DecCount()
        {
            count--;
        }
    }
    public Dictionary<string, UniqueStatusAbility> uniqueStatusAbilityDict = new Dictionary<string, UniqueStatusAbility>(); 
    // 不计数的唯一性buff
    public Dictionary<string, StatusAbility> noCountUniqueStatusAbilityDict = new Dictionary<string, StatusAbility>();

    public void Initialize()
    {
        statusAbilityList.Clear();
        removeList.Clear();
        noCountUniqueStatusAbilityDict.Clear();
    }


    /// <summary>
    /// 当BUFF加入时，就意味着激活了
    /// </summary>
    /// <param name="statusAbility"></param>
    public void AddStatusAbility(StatusAbility statusAbility)
    {
        statusAbility.statusAbilityManager = this;
        statusAbility.ActivateAbility();
        statusAbilityList.Add(statusAbility);
    }

    public void RemoveStatusAbility(StatusAbility statusAbility)
    {
        removeList.Add(statusAbility);
        //statusAbilityList.Remove(statusAbility);
    }

    public StatusAbility GetUniqueStatus(string statusName) 
    {
        if (!uniqueStatusAbilityDict.ContainsKey(statusName))
        {
            return null;
        }
        return uniqueStatusAbilityDict[statusName].status;
    }

    /// <summary>
    /// 添加一层唯一性buff
    /// </summary>
    /// <param name="statusName"></param>
    /// <param name="statusAbility"></param>
    public void AddUniqueStatusAbility(string statusName, StatusAbility statusAbility)
    {
        if (!uniqueStatusAbilityDict.ContainsKey(statusName))
        {
            // 如果第一次施加此效果
            uniqueStatusAbilityDict.Add(statusName, new UniqueStatusAbility() { status= statusAbility , count=0});
            statusAbility.statusAbilityManager = this;
            // 启动
            statusAbility.ActivateAbility();
            // 丢到这个表里正常执行
            statusAbilityList.Add(statusAbility); 
        }
        // 其他情况下层数+1，该干啥干啥
        uniqueStatusAbilityDict[statusName].AddCount();
        //Debug.Log("statusName="+ statusName+", count="+ uniqueStatusAbilityDict[statusName].count);
    }

    /// <summary>
    /// 移除一层唯一性buff
    /// </summary>
    /// <param name="statusName"></param>
    public void RemoveUniqueStatusAbility(string statusName)
    {
        if (uniqueStatusAbilityDict.ContainsKey(statusName))
        {
            // 层数-1
            uniqueStatusAbilityDict[statusName].DecCount();
            // 若减到0层，则直接移除此效果
            if (uniqueStatusAbilityDict[statusName].count <= 0)
            {
                StatusAbility statusAbility = uniqueStatusAbilityDict[statusName].status;
                //RemoveStatusAbility(statusAbility);
                statusAbility.EndActivate(); // 手动结束
                uniqueStatusAbilityDict.Remove(statusName);
            }
        }
    }

    /// <summary>
    /// 通过key来获取不计数的唯一性状态实体
    /// </summary>
    /// <param name="statusName"></param>
    /// <returns></returns>
    public StatusAbility GetNoCountUniqueStatus(string statusName)
    {
        if (!noCountUniqueStatusAbilityDict.ContainsKey(statusName))
        {
            return null;
        }
        return noCountUniqueStatusAbilityDict[statusName];
    }

    /// <summary>
    /// 添加不计数的唯一性buff
    /// </summary>
    /// <param name="statusName"></param>
    /// <param name="statusAbility"></param>
    public void AddNoCountUniqueStatusAbility(string statusName, StatusAbility statusAbility)
    {
        if (!noCountUniqueStatusAbilityDict.ContainsKey(statusName))
        {
            // 如果第一次施加此效果
            noCountUniqueStatusAbilityDict.Add(statusName, statusAbility);
            statusAbility.statusAbilityManager = this;
            // 启动
            statusAbility.ActivateAbility();
            // 丢到这个表里正常执行
            statusAbilityList.Add(statusAbility);
        }
        else
        {
            // 否则执行其覆盖方法
            statusAbility.OnCover();
        }
    }


    /// <summary>
    /// 直接从表中移除不计数的唯一性BUFF（是从表中移除，并不执行结束方法）
    /// </summary>
    /// <param name="statusName"></param>
    public void RemoveNoCountUniqueStatusAbility(string statusName)
    {
        if (noCountUniqueStatusAbilityDict.ContainsKey(statusName))
        {
            noCountUniqueStatusAbilityDict.Remove(statusName);
        }
    }

    /// <summary>
    /// 手动结束不计数的唯一性BUFF（会执行其结束的方法）
    /// </summary>
    public void EndNoCountUniqueStatusAbility(string statusName)
    {
        StatusAbility s = GetNoCountUniqueStatus(statusName);
        if(s!=null)
            s.EndActivate();
    }

    public void TryActivateStatusAbility(int index)
    {
        statusAbilityList[index].TryActivateAbility();
    }

    public void TryActivateStatusAbility(string name)
    {
        foreach (var item in statusAbilityList)
        {
            if (item.name.Equals(name))
            {
                item.TryActivateAbility();
                return;
            }
        }
    }

    public void TryEndStatusAbility(int index)
    {
        statusAbilityList[index].TryEndActivate();
    }

    public void TryEndStatusAbility(string name)
    {
        foreach (var item in statusAbilityList)
        {
            if (item.name.Equals(name))
            {
                item.TryEndActivate();
                return;
            }
        }
    }

    public void TryEndAllStatusAbility()
    {
        foreach (var item in statusAbilityList)
        {
            item.TryEndActivate();
        }
    }

    public void Update()
    {
        foreach (var item in statusAbilityList)
        {
            item.Update();
        }
        foreach (var item in removeList)
        {
            statusAbilityList.Remove(item);
        }
        removeList.Clear();
    }
}
