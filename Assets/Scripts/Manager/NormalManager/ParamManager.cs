using System.Collections.Generic;
/// <summary>
/// �������������ṩ��̬������
/// </summary>
public class ParamManager
{
    /// <summary>
    /// ��ȡĳ���ַ����еĲ�����ʾ���������ȡת�����ֵ���ȥ
    /// </summary>
    /// <param name="s">�������ģ�"������${a=(1/2/3)}����${b=(1/2/3)}������"����Ҫ��Ҫʶ��a��b�Լ�����Ӧ������</param>
    /// <param name="ParamDict">������Ķ���ʶ�������Ȼ����Ϊ�����浽�ֵ���ȥ���磺{"a", {1,2,3}}, {"b", {1,2,3}}</param>
    /// <returns></returns>
    public static void GetStringParamArray(string s, out Dictionary<string, float[]> ParamDict)
    {
        // ����һ�ηָ��"${"�������Ϊʶ���־���ѿ��ܳ��ֲ�����λ�ö��ֳ���
        string[] strs1 = s.Split(new string[] { "${" }, System.StringSplitOptions.RemoveEmptyEntries);
        ParamDict = new Dictionary<string, float[]>();
        // ����ȥ��һ�κ󣬶�ÿ�δ���ȥ��"}"����ȷ��������
        for (int i = 1; i < strs1.Length; i++)
        {
            string str = strs1[i];
            int endIndex = str.IndexOf('}');
            str = str.Substring(0, endIndex);
            // ����Ӧ�ð���ֻʣa=(1/2/3)�����ˣ�Ȼ�����ǾͿ���ȥ��ParamManager�е�Parse�������Խ���
            string paramName;
            float[] array;
            if (Parse(str, out paramName, out array))
            {
                if (ParamDict.ContainsKey(paramName))
                {
                    if (array != null)
                        ParamDict[paramName] = array;
                }
                else if (array != null)
                {
                    ParamDict.Add(paramName, array);
                }
            }
        }
    }

    /// <summary>
    /// ���ַ����н�������
    /// </summary>
    /// <param name="s">������ַ����ĸ�ʽ�磺"t=(0/1/2)"</param>
    /// <param name="paramName">���ؽ��������Ĳ�����������������Ϊ"t"</param>
    /// <param name="array">���ؽ������������飬����������Ϊfloat[]{0,1,2}</param>
    /// <returns></returns>
    public static bool Parse(string s, out string paramName, out float[] array)
    {
        s = s.Replace(" ", ""); // ȥ�����пո�
        string[] str1 = s.Split('='); // �ԵȺ�Ϊ�绮��Ϊ������
        paramName = str1[0]; // �󲿷�Ϊ����:"t"
        // ��������һ���жϣ���Ϊ��������п��ܳ���û�и�ֵ�������������Ĳ���"t=(0/1/2)"��ֻ��һ��"t"
        if (str1.Length < 2)
        { 
            array = null; // ���������ֵȻ�󷵻�
            return true;
        }
        // ��ʼ�����Ҳ���str1[1]="(0/1/2)", �ӵ�һ�������ſ�ʼ������һ�������Ž���
        int startIndex = str1[1].IndexOf('(') + 1;
        int endIndex = str1[1].IndexOf(')');
        // ��ȡ�м�Ĳ���str2="0/1/2"
        string str2 = str1[1].Substring(startIndex, endIndex - startIndex);
        // ��'/'�ŷָȻ���ÿ����ת��Ϊfloat�����뵽�������ok��
        string[] str3 = str2.Split('/');
        array = new float[str3.Length];
        for (int i = 0; i < array.Length; i++)
        {
            if (!float.TryParse(str3[i], out array[i]))
                return false;
        }
        return true;
    }

    /// <summary>
    /// ȡ��ĳ���ض��Ĳ����󷵻ش�������ַ��������ı�ԭ�ַ�����
    /// </summary>
    public static string GetStringByReplaceParam(string s, string paramName, string replaceContent)
    {
        // ����һ�ηָ��"${"�������Ϊʶ���־���ѿ��ܳ��ֲ�����λ�ö��ֳ���
        string[] strs1 = s.Split(new string[] { "${" }, System.StringSplitOptions.RemoveEmptyEntries);
        for (int i = 1; i < strs1.Length; i++)
        {
            string str = strs1[i];
            int endIndex = str.IndexOf('}');
            // ͨ���ȶ���ȷ���ǲ���Ҫ�滻�Ĳ���
            if (paramName.Equals(str.Substring(0, endIndex)))
            {
                strs1[i] = str.Substring(endIndex + 1, str.Length - endIndex - 1); // �ü���ԭ���Ĳ�������
                // �滻
                strs1[i] = replaceContent + strs1[i];
            }
            else
            {
                // ������ǣ��Ͱ�"${"����ȥ
                strs1[i] = "${" + strs1[i];
            }
        }
        string new_s = "";
        foreach (var str in strs1)
        {
            new_s += str;
        }
        return new_s;
    }
}
