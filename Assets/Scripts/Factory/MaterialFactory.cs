using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// ר������ȡ��Ϸ���ʵ�
/// </summary>
public class MaterialFactory
{
    // ����·��
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
