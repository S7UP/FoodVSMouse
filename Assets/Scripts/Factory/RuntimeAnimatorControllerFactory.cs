using System.Collections.Generic;

using UnityEngine;

public class RuntimeAnimatorControllerFactory : IBaseResourceFactory<RuntimeAnimatorController>
{
    protected Dictionary<string, RuntimeAnimatorController> factoryDict = new Dictionary<string, RuntimeAnimatorController>();
    protected string loadPath;

    public RuntimeAnimatorControllerFactory()
    {
        loadPath = "Animator/AnimatorController/";
    }
    public RuntimeAnimatorController GetSingleResources(string resourcePath)
    {
        RuntimeAnimatorController itemGo = null;
        string itemLoadPath = loadPath + resourcePath;
        if (factoryDict.ContainsKey(resourcePath))
        {
            itemGo = factoryDict[resourcePath];
        }
        else
        {
            itemGo = Resources.Load<RuntimeAnimatorController>(itemLoadPath);
            factoryDict.Add(resourcePath, itemGo);
        }
        // 安全检测机制
        if (itemGo == null)
            Debug.Log(resourcePath + "的资源获取失败,失败路径为：" + itemLoadPath);
        return itemGo;
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
