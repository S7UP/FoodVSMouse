using UnityEngine;
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

        public void LoadFile(string path, int column)
        {
            TextAsset text = Resources.Load<TextAsset>(path);
            // 每读取到column个逗号作为一行，且第一行是要被忽略掉的
            string[] ss = text.text.Split(',');
            string[] strs = new string[ss.Length - column];
            for (int i = 0; i < ss.Length - column; i++)
            {
                strs[i] = ss[i + column].Replace('"', ' ');
                //Debug.Log(strs[i]);
            }
            // int index = 0; // 每次求余等于0时等于另起一行，去检测\n并将\n之前的内容删掉
            int row = strs.Length / column - 1; // 可以通过逗号数除以列数来计算行数
            m_ArrayData = new string[row][];
            for (int i = 0; i < m_ArrayData.Length; i++)
            {
                m_ArrayData[i] = new string[column];
                for (int j = 0; j < m_ArrayData[i].Length; j++)
                {
                    m_ArrayData[i][j] = strs[i*column + j];
                    if (j == 0)
                    {
                        // 新起一行时，去检测\n并将\n之前的内容删掉
                        int index = 0;
                        foreach (var c in m_ArrayData[i][j].ToCharArray())
                        {
                            if (c.Equals('\n'))
                            {
                                break;
                            }
                            index++;
                        }
                        m_ArrayData[i][j].Substring(index);
                    }
                }
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
        public string GetVaule(int row, int col)
        {
            return m_ArrayData[row][col];
        }
    }
}
