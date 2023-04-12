using UnityEngine;
using System.Collections.Generic;
/// <summary>
/// ����Execel�ļ��Ĺ�����
/// </summary>
public class ExcelManager
{
    /// <summary>
    /// ��ȡExcel
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
    /// �ڲ��࣬CSV
    /// </summary>
    public class CSV
    {
        //���ö�ά����洢��Ӧ�к����е��ַ���
        public string[][] m_ArrayData;
        public Dictionary<string, float[]>[][] m_ParamData; // �����ֵ�

        public void LoadFile(string path, int column)
        {
            TextAsset text = Resources.Load<TextAsset>(path);
            // ÿ��ȡ��column��������Ϊһ�У��ҵ�һ����Ҫ�����Ե���
            string[] ss = text.text.Split(',');
            string[] strs = new string[ss.Length - column];
            for (int i = 0; i < ss.Length - column; i++)
            {
                strs[i] = ss[i + column].Replace("\"", "");
            }
            // ÿ���������0ʱ��������һ�У�ȥ���\n����\n֮ǰ������ɾ��
            int row = strs.Length / column; // ����ͨ��������������������������
            m_ArrayData = new string[row][];
            m_ParamData = new Dictionary<string, float[]>[row][];
            for (int i = 0; i < m_ArrayData.Length; i++)
            {
                m_ArrayData[i] = new string[column];
                m_ParamData[i] = new Dictionary<string, float[]>[column];
                for (int j = 0; j < m_ArrayData[i].Length; j++)
                {
                    m_ArrayData[i][j] = strs[i*column + j];
                    ParamManager.GetStringParamArray(m_ArrayData[i][j], out m_ParamData[i][j]); // ��ȡ��ĳһ�������в�����ת�����ֵ���ȥ
                    if (j == 0)
                    {
                        // ����һ��ʱ��ȥ���\n����\n֮ǰ������ɾ��
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
                    StandardizationParamStandardization(i, j); // ��ĳһ�������в�����ʶ��ת��Ϊʵ�ʵ���ֵ
                }
            }
        }

        /// <summary>
        /// �Ѳ�����׼�� ���${a=(1/2/3)}����ȫת����${a}
        /// </summary>
        /// <param name="row"></param>
        /// <param name="col"></param>
        public void StandardizationParamStandardization(int row, int col)
        {
            string s = GetValue(row, col);
            // ����һ�ηָ��"${"�������Ϊʶ���־���ѿ��ܳ��ֲ�����λ�ö��ֳ���
            string[] strs1 = s.Split(new string[] { "${" }, System.StringSplitOptions.RemoveEmptyEntries);
            for (int i = 1; i < strs1.Length; i++)
            {
                string str = strs1[i];
                int paramNameEndIndex = str.IndexOf('=');
                if (paramNameEndIndex == -1)
                    paramNameEndIndex = str.IndexOf('}');
                string paramName = str.Substring(0, paramNameEndIndex);

                int endIndex = str.IndexOf('}');
                strs1[i] = str.Substring(endIndex + 1, str.Length - endIndex - 1); // �ü���ԭ���Ĳ�������
                // �滻��ֻ�б�ʶ������
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
        /// ȡ��ĳ���ض��Ĳ����󷵻ش�������ַ��������ı�ԭ�ַ�����
        /// </summary>
        public string GetValueByReplaceParam(int row, int col, string paramName, string replaceContent)
        {
            return ParamManager.GetStringByReplaceParam(GetValue(row, col), paramName, replaceContent);
        }

        /// <summary>
        /// ͨ��Ĭ�ϵķ���ȡ����ȫ���Ĳ�����������ȡ��������ַ��������ı�ԭ�ַ�����
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
                return "(δ֪)";
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

        //�ⲿ���õ�ȡֵ�ӿڣ���Ҫ��LoadFile()����ܷ�����ȷ������
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
