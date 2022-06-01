using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �ؿ���ֵ������
/// </summary>
public class NumberManager
{
    private Dictionary<string, float> dict = new Dictionary<string, float>();

    public NumberManager()
    {

    }

    /// <summary>
    /// ��ȡ��ֵ
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
