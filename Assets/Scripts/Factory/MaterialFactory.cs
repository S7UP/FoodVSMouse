using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 专门用来取游戏材质的
/// </summary>
public class MaterialFactory
{
    // 加载路径
    protected string loadPath;

    public MaterialFactory()
    {
        loadPath = "Material/";
    }

    public Material GetMaterial(string name)
    {
        return Resources.Load<Material>(loadPath+name);
    }
}
