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

    public void Initialize()
    {
        statusAbilityList.Clear();
        removeList.Clear();
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
