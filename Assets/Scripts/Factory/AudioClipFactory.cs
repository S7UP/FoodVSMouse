using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class AudioClipFactory : IBaseResourceFactory<AudioClip>
{
    protected Dictionary<string, AudioClip> factoryDict = new Dictionary<string, AudioClip>();
    protected string loadPath;

    public AudioClipFactory()
    {
        loadPath = "AudioClips/";
    }
    public AudioClip GetSingleResources(string resourcePath)
    {
        AudioClip itemGo = null;
        string itemLoadPath = loadPath + resourcePath;
        if (factoryDict.ContainsKey(resourcePath))
        {
            itemGo = factoryDict[resourcePath];
        }
        else
        {
            itemGo = Resources.Load<AudioClip>(itemLoadPath);
            factoryDict.Add(resourcePath, itemGo);
        }
        // 安全检测机制
        if (itemGo == null)
        {
            Debug.Log(resourcePath+"的资源获取失败,失败路径为："+itemLoadPath);
        }
        return itemGo;
    }

    public IEnumerator AsyncLoadSingleResources(string resourcePath)
    {
        string itemLoadPath = loadPath + resourcePath;
        if (!factoryDict.ContainsKey(resourcePath))
        {
            ResourceRequest req = Resources.LoadAsync<AudioClip>(itemLoadPath);
            if(req != null)
            {
                while (!req.isDone)
                {
                    yield return null;
                }
                Debug.Log("异步加载" + resourcePath + "的资源完成！");
                factoryDict.Add(resourcePath, req.asset as AudioClip);
            }
            else
            {
                Debug.Log("异步加载" + resourcePath + "的资源获取失败,失败路径为：" + itemLoadPath);
            }
        }
    }

    public void UnLoad(string resourcePath)
    {
        if (factoryDict.ContainsKey(resourcePath))
        {
            Resources.UnloadAsset(factoryDict[resourcePath]);
            factoryDict.Remove(resourcePath);
        }
    }
}
