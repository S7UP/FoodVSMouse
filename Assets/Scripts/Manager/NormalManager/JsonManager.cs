using System.IO;
using UnityEngine;
using Newtonsoft.Json;

/// <summary>
/// 负责本地Json存取的管理者
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
    /// 存储在玩家本地（存档目录处）
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

    // 读取JSON文件解析为对象
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
        Debug.Log("文件加载失败，加载路径是：" + filepath);
        return default(T);
    }


    /// <summary>
    /// 从本地读取
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
    /// 尝试从Resource里读取Json文件
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
    /// 删除某个JSON文件
    /// </summary>
    /// <param name="path"></param>
    public static void Delete(string path)
    {
        string filepath = Application.streamingAssetsPath + "/Json/" + path + ".json";
        Debug.Log("文件路径为：" + filepath);
        if (File.Exists(filepath))
        {
            //删除文件
            Debug.Log("删除文件："+filepath+" 成功！");
            File.Delete(filepath);
        }
    }
}
