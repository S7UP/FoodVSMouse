using UnityEngine;
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

        public void LoadFile(string path, int column)
        {
            TextAsset text = Resources.Load<TextAsset>(path);
            // ÿ��ȡ��column��������Ϊһ�У��ҵ�һ����Ҫ�����Ե���
            string[] ss = text.text.Split(',');
            string[] strs = new string[ss.Length - column];
            for (int i = 0; i < ss.Length - column; i++)
            {
                strs[i] = ss[i + column].Replace('"', ' ');
                //Debug.Log(strs[i]);
            }
            // int index = 0; // ÿ���������0ʱ��������һ�У�ȥ���\n����\n֮ǰ������ɾ��
            int row = strs.Length / column - 1; // ����ͨ��������������������������
            m_ArrayData = new string[row][];
            for (int i = 0; i < m_ArrayData.Length; i++)
            {
                m_ArrayData[i] = new string[column];
                for (int j = 0; j < m_ArrayData[i].Length; j++)
                {
                    m_ArrayData[i][j] = strs[i*column + j];
                    if (j == 0)
                    {
                        // ����һ��ʱ��ȥ���\n����\n֮ǰ������ɾ��
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

        //�ⲿ���õ�ȡֵ�ӿڣ���Ҫ��LoadFile()����ܷ�����ȷ������
        public string GetVaule(int row, int col)
        {
            return m_ArrayData[row][col];
        }
    }
}
