/// <summary>
/// 参数管理器（提供静态方法）
/// </summary>
public class ParamManager
{
    /// <summary>
    /// 从字符串中解析参数
    /// </summary>
    /// <param name="s">传入的字符串的格式如："t=(0/1/2)"</param>
    /// <param name="paramName">返回解析出来的参数名，上述例子中为"t"</param>
    /// <param name="array">返回解析出来的数组，上述例子中为float[]{0,1,2}</param>
    /// <returns></returns>
    public static bool Parse(string s, out string paramName, out float[] array)
    {
        s = s.Replace(" ", ""); // 去掉所有空格
        string[] str1 = s.Split('='); // 以等号为界划分为两部分
        paramName = str1[0]; // 左部分为参数:"t"
        // 这里先作一次判断，因为参数表格中可能出现没有赋值的情况，即传入的不是"t=(0/1/2)"而只是一个"t"
        if (str1.Length < 2)
        { 
            array = null; // 给这个赋空值然后返回
            return true;
        }
        // 开始解析右部分str1[1]="(0/1/2)", 从第一个左括号开始，到第一个右括号结束
        int startIndex = str1[1].IndexOf('(') + 1;
        int endIndex = str1[1].IndexOf(')');
        // 截取中间的部分str2="0/1/2"
        string str2 = str1[1].Substring(startIndex, endIndex - startIndex);
        // 以'/'号分割，然后把每部分转化为float，加入到数组里，就ok了
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
    /// 取代某个特定的参数后返回处理过的字符串（不改变原字符串）
    /// </summary>
    public static string GetStringByReplaceParam(string s, string paramName, string replaceContent)
    {
        // 先作一次分割，以"${"的组合作为识别标志，把可能出现参数的位置都分出来
        string[] strs1 = s.Split(new string[] { "${" }, System.StringSplitOptions.RemoveEmptyEntries);
        for (int i = 1; i < strs1.Length; i++)
        {
            string str = strs1[i];
            int endIndex = str.IndexOf('}');
            // 通过比对来确定是不是要替换的参数
            if (paramName.Equals(str.Substring(0, endIndex)))
            {
                strs1[i] = str.Substring(endIndex + 1, str.Length - endIndex - 1); // 裁剪掉原来的参数部分
                // 替换
                strs1[i] = replaceContent + strs1[i];
            }
            else
            {
                // 如果不是，就把"${"还回去
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
