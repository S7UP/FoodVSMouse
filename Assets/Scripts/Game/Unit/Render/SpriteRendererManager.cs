using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �����ض�SpriteRenderer���Ե��ռ���
/// </summary>
public class SpriteRendererManager
{
    public SpriteRenderer spriteRenderer;
    public ColorCollecter colorCollecter = new ColorCollecter();

    private SpriteRendererManager()
    {

    }

    public SpriteRendererManager(SpriteRenderer spriteRenderer)
    {
        this.spriteRenderer = spriteRenderer;
    }

    public void AddColor(Color color)
    {
        colorCollecter.AddColor(color);
        Update();
    }

    public void RemoveColor(Color color)
    {
        colorCollecter.RemoveColor(color);
        Update();
    }


    /// <summary>
    /// ��Ч��ɫӦ�õ�������ͼ֮��
    /// </summary>
    public void Update()
    {
        spriteRenderer.color = colorCollecter.TotalValue;
    }
}
