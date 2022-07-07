using System.Collections.Generic;

using UnityEngine;
// 精灵资源工厂
public class SpriteFactory : IBaseResourceFactory<Sprite>
{
    protected Dictionary<string, Sprite> factoryDict = new Dictionary<string, Sprite>();
    protected string loadPath;

    public SpriteFactory()
    {
        loadPath = "Pictures/";
    }

    public Sprite GetSingleResources(string resourcePath)
    {
        Sprite itemGo = null;
        string itemLoadPath = loadPath + resourcePath;
        if (factoryDict.ContainsKey(resourcePath))
        {
            itemGo = factoryDict[resourcePath];
        }
        else
        {
            itemGo = Resources.Load<Sprite>(itemLoadPath);
            factoryDict.Add(resourcePath, itemGo);
        }
        // 安全检测机制
        if (itemGo == null)
        {
            Debug.Log(resourcePath + "的资源获取失败,失败路径为：" + itemLoadPath);
        }
        return itemGo;
    }
}
