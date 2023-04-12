using UnityEngine;
using System.Collections.Generic;
/// <summary>
/// 管理Execel文件的管理者
/// </summary>
public class ExcelManager
{
    /// <summary>
    /// 读取Excel
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static CSV ReadCSV(string path, int column)
    {
        string resPath = "Excel/";
        CSV csv = new CSV();
        csv.LoadFile(resPath + path, column);
        return csv;
    }

    /// <summary>
    /// 内部类，CSV
    /// </summary>
    public class CSV
    {
        //利用二维数组存储对应行和列中的字符串
        public string[][] m_ArrayData;
        public Dictionary<string, float[]>[][] m_ParamData; // 参数字典

        public void LoadFile(string path, int column)
        {
            TextAsset text = Resources.Load<TextAsset>(path);
            // 每读取到column个逗号作为一行，且第一行是要被忽略掉的
            string[] ss = text.text.Split(',');
            string[] strs = new string[ss.Length - column];
            for (int i = 0; i < ss.Length - column; i++)
            {
                strs[i] = ss[i + column].Replace("\"", "");
            }
            // 每次求余等于0时等于另起一行，去检测\n并将\n之前的内容删掉
            int row = strs.Length / column; // 可以通过逗号数除以列数来计算行数
            m_ArrayData = new string[row][];
            m_ParamData = new Dictionary<string, float[]>[row][];
            for (int i = 0; i < m_ArrayData.Length; i++)
            {
                m_ArrayData[i] = new string[column];
                m_ParamData[i] = new Dictionary<string, float[]>[column];
                for (int j = 0; j < m_ArrayData[i].Length; j++)
                {
                    m_ArrayData[i][j] = strs[i*column + j];
                    ParamManager.GetStringParamArray(m_ArrayData[i][j], out m_ParamData[i][j]); // 提取出某一格中所有参数并转化到字典里去
                    if (j == 0)
                    {
                        // 新起一行时，去检测\n并将\n之前的内容删掉
                        int index = 0;
                        foreach (var c in m_ArrayData[i][j].ToCharArray())
                        {
                            index++;
                            if (c.Equals('\n'))
                            {
                                break;
                            }
                        }
                        m_ArrayData[i][j] = m_ArrayData[i][j].Substring(index);
                    }
                    StandardizationParamStandardization(i, j); // 把某一格中所有参数标识符转化为实际的数值
                }
            }
        }

        /// <summary>
        /// 把参数标准化 如把${a=(1/2/3)}这种全转化成${a}
        /// </summary>
        /// <param name="row"></param>
        /// <param name="col"></param>
        public void StandardizationParamStandardization(int row, int col)
        {
            string s = GetValue(row, col);
            // 先作一次分割，以"${"的组合作为识别标志，把可能出现参数的位置都分出来
            string[] strs1 = s.Split(new string[] { "${" }, System.StringSplitOptions.RemoveEmptyEntries);
            for (int i = 1; i < strs1.Length; i++)
            {
                string str = strs1[i];
                int paramNameEndIndex = str.IndexOf('=');
                if (paramNameEndIndex == -1)
                    paramNameEndIndex = str.IndexOf('}');
                string paramName = str.Substring(0, paramNameEndIndex);

                int endIndex = str.IndexOf('}');
                strs1[i] = str.Substring(endIndex + 1, str.Length - endIndex - 1); // 裁剪掉原来的参数部分
                // 替换成只有标识符部分
                strs1[i] = "${" + paramName + "}"+strs1[i];
            }
            s = "";
            foreach (var str in strs1)
            {
                s += str;
            }
            SetValue(row, col, s);
        }

        /// <summary>
        /// 取代某个特定的参数后返回处理过的字符串（不改变原字符串）
        /// </summary>
        public string GetValueByReplaceParam(int row, int col, string paramName, string replaceContent)
        {
            return ParamManager.GetStringByReplaceParam(GetValue(row, col), paramName, replaceContent);
        }

        /// <summary>
        /// 通过默认的方法取代掉全部的参数部分来获取处理过的字符串（不改变原字符串）
        /// </summary>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <returns></returns>
        public string GetValueByReplaceAllParam(int row, int col)
        {
            string s = GetValue(row, col);
            foreach (var keyValuePair in m_ParamData[row][col])
            {
                s = ParamManager.GetStringByReplaceParam(s, keyValuePair.Key, ParamArrayToString(row, col, keyValuePair.Key));
            }
            return s;
        }


        public Dictionary<string, float[]> GetParamDict(int row, int col)
        {
            return m_ParamData[row][col];
        }

        public string ParamArrayToString(int row, int col, string paramName)
        {
            if (m_ParamData[row][col].ContainsKey(paramName))
            {
                string s = "("+ m_ParamData[row][col][paramName][0];
                for (int i = 1; i < m_ParamData[row][col][paramName].Length; i++)
                {
                    s = s + "/" + m_ParamData[row][col][paramName][i];
                }
                s = s + ")";
                return s;
            }
            else
            {
                return "(未知)";
            }
        }

        public float[] GetParamArray(int row, int col, string paramName)
        {
            if (m_ParamData[row][col].ContainsKey(paramName))
            {
                return m_ParamData[row][col][paramName];
            }
            else
            {
                return new float[] { 0 };
            }
        }

        public int GetColumn()
        {
            return m_ArrayData[0].Length;
        }

        public int GetRow()
        {
            return m_ArrayData.Length;
        }

        //外部调用的取值接口，需要先LoadFile()后才能返回正确的数据
        public string GetValue(int row, int col)
        {
            return m_ArrayData[row][col];
        }

        public void SetValue(int row, int col, string s)
        {
            m_ArrayData[row][col] = s;
        }
    }
}
