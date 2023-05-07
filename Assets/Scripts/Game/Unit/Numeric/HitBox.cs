using UnityEngine;
/// <summary>
/// �ܻ�ʱ������ͼ��˸�����ĺ���
/// </summary>
public class HitBox
{
    private int maxValue;
    private int value;
    private int add;
    private int lastHitTime; // ������һ�α����м������֡

    public HitBox()
    {
        maxValue = 30;
        value = 0;
        add = -1;
        lastHitTime = 0;
    }

    public void Initialize()
    {
        maxValue = 30;
        value = 0;
        add = -1;
    }

    public void Update()
    {
        lastHitTime++;
        value += add;
        if (value >= maxValue)
        {
            add = -1;
        }
        value = Mathf.Min(maxValue, Mathf.Max(value, 0));
    }

    public void OnHit()
    {
        lastHitTime = 0;
        add = 5;
    }

    public float GetPercent()
    {
        return (float)value / maxValue;
    }

    public float GetLastHitTime()
    {
        return lastHitTime;
    }
}
