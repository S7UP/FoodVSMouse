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

    /// <summary>
    /// �洢����ұ��أ��浵Ŀ¼����
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="obj"></param>
    /// <param name="path"></param>
    public static void SaveOnLocal<T>(T obj, string path)
    {
        string filePath = Application.persistentDataPath + "/" + path + ".json";
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
    /// �ӱ��ض�ȡ
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="path"></param>
    /// <param name="result"></param>
    /// <returns></returns>
    public static bool TryLoadFromLocal<T>(string path, out T result)
    {
        string filepath = Application.persistentDataPath + "/" + path + ".json";
        if (File.Exists(filepath))
        {
            StreamReader sr = new StreamReader(filepath);
            string jsonStr = sr.ReadToEnd();
            sr.Close();
            result = JsonConvert.DeserializeObject<T>(jsonStr);
            return true;
        }
        result = default(T);
        return false;
    }

    /// <summary>
    /// ���Դ�Resource���ȡJson�ļ�
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="path"></param>
    /// <param name="result"></param>
    /// <returns></returns>
    public static bool TryLoadFromResource<T>(string path, out T result)
    {
        var jsonTextFile = Resources.Load<TextAsset>("Json/"+path);
        if(jsonTextFile != null)
        {
            //result = JsonUtility.FromJson<T>(jsonTextFile.text);
            result = JsonConvert.DeserializeObject<T>(jsonTextFile.text);
            return true;
        }
        else
        {
            result = default(T);
            return false;
        }
        
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
