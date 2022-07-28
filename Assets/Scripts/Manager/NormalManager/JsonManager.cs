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
