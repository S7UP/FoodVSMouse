using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 关卡数值管理器
/// </summary>
public class NumberManager
{
    private Dictionary<string, float> dict = new Dictionary<string, float>();

    public NumberManager()
    {

    }

    /// <summary>
    /// 获取数值
    /// </summary>
    /// <returns></returns>
    public float GetValue(string key)
    {
        if (dict.ContainsKey(key))
        {
            return dict[key];
        }
        else
        {
            return 0;
        }
    }
}
