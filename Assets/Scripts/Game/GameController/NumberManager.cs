using System.Collections.Generic;

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
