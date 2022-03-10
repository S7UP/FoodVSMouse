using System.Collections;
using System.Collections.Generic;
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
        string filePath = Application.dataPath + "/Resources/Json/" + path + ".json";
        string saveJsonStr = JsonConvert.SerializeObject(obj);
        StreamWriter sw = new StreamWriter(filePath);
        sw.Write(saveJsonStr);
        sw.Close();
    }

    // 读取关卡文件解析JSON转化为LevelInfo对象
    public static T Load<T>(string path)
    {
        string filepath = Application.dataPath + "/Resources/Json/" + path + ".json";
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
}
