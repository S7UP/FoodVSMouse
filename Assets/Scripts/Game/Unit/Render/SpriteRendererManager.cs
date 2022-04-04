using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 管理特定SpriteRenderer属性的收集器
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
    /// 将效果色应用到具体贴图之上
    /// </summary>
    public void Update()
    {
        spriteRenderer.color = colorCollecter.TotalValue;
    }
}
