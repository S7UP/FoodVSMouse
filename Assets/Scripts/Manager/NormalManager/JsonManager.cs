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
        //string filePath = Application.dataPath + "/Resources/Json/" + path + ".json";
        string filePath = Application.streamingAssetsPath + "/Json/" + path + ".json";
        string saveJsonStr = JsonConvert.SerializeObject(obj);
        StreamWriter sw = new StreamWriter(filePath);
        sw.Write(saveJsonStr);
        sw.Close();
    }

    // ��ȡJSON�ļ�����Ϊ����
    public static T Load<T>(string path)
    {
        //string filepath = Application.dataPath + "/Resources/Json/" + path + ".json";
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
}
