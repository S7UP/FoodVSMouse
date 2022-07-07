using UnityEngine;
/// <summary>
/// 受击时控制贴图闪烁参数的盒子
/// </summary>
public class HitBox
{
    private int maxValue;
    private int value;
    private int add;

    public HitBox()
    {
        maxValue = 10;
        value = 0;
        add = -1;
    }

    public void Initialize()
    {
        maxValue = 10;
        value = 0;
        add = -1;
    }

    public void Update()
    {
        value += add;
        if (value >= maxValue)
        {
            add = -1;
        }
        value = Mathf.Min(maxValue, Mathf.Max(value, 0));
    }

    public void OnHit()
    {
        add = 1;
    }

    public float GetPercent()
    {
        return (float)value / maxValue;
    }
}
