using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 颜色收集器
/// </summary>
public class ColorCollecter
{
    public Color TotalValue = new Color(0f, 0f, 0f);
    public List<Color> ColorList = new List<Color>();

    public void AddColor(Color color)
    {
        ColorList.Add(color);
        Update();
    }

    public void RemoveColor(Color color)
    {
        ColorList.Remove(color);
        Update();
    }


    /// <summary>
    /// 计算出这些颜色叠加出来的效果色
    /// </summary>
    public void Update()
    {
        if(ColorList.Count <= 0)
        {
            TotalValue = new Color(0f, 0f, 0f);
        }
        else
        {
            TotalValue = ColorList[ColorList.Count - 1];
        }
    }
}
