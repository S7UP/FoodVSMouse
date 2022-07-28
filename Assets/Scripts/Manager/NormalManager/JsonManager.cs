using System.IO;
using UnityEngine;
using Newtonsoft.Json;

/// <summary>
/// ���𱾵�Json��ȡ�Ĺ�����
/// </summary>
public class JsonManager
{
    public static void Save<T>(T obj, string path)
    {
        string filePath = Application.streamingAssetsPath + "/Json/" + path + ".json";
        string saveJsonStr = JsonConvert.SerializeObject(obj);
        StreamWriter sw = new StreamWriter(filePath);
        sw.Write(saveJsonStr);
        sw.Close();
    }

    // ��ȡJSON�ļ�����Ϊ����
    public static T Load<T>(string path)
    {
        string filepath = Application.streamingAssetsPath + "/Json/" + path + ".json";
        if (File.Exists(filepath))
        {
            StreamReader sr = new StreamReader(filepath);
            string jsonStr = sr.ReadToEnd();
            sr.Close();
            return JsonConvert.DeserializeObject<T>(jsonStr);
        }
        Debug.Log("�ļ�����ʧ�ܣ�����·���ǣ�" + filepath);
        return default(T);
    }

    /// <summary>
    /// ɾ��ĳ��JSON�ļ�
    /// </summary>
    /// <param name="path"></param>
    public static void Delete(string path)
    {
        string filepath = Application.streamingAssetsPath + "/Json/" + path + ".json";
        Debug.Log("�ļ�·��Ϊ��" + filepath);
        if (File.Exists(filepath))
        {
            //ɾ���ļ�
            Debug.Log("ɾ���ļ���"+filepath+" �ɹ���");
            File.Delete(filepath);
        }
    }
}
