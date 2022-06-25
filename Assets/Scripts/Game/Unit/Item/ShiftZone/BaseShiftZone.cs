using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 基础变速地带
/// </summary>
public class BaseShiftZone : BaseItem
{
    private float changePercent; // 基础移速改变值

    public override void MInit()
    {
        base.MInit();
        changePercent = 0;
    }

    /// <summary>
    /// 设置速度改变比例
    /// </summary>
    public void SetChangePercent(float per)
    {
        changePercent = per;
    }

    /// <summary>
    /// 碰撞事件
    /// </summary>
    /// <param name="collision"></param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 死亡动画时不接受任何碰撞事件
        if (isDeathState)
        {
            return;
        }
        if (collision.tag.Equals("Mouse"))
        {
            MouseUnit m = collision.GetComponent<MouseUnit>();
            if (m.GetRowIndex() != GetRowIndex() || m.GetHeight() != GetHeight())
                return;
            // 添加变速buff
            m.AddUniqueStatusAbility(GetUniqueStatusName(), new ChangeVelocityStatusAbility(m, changePercent));
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // 死亡动画时不接受任何碰撞事件
        if (isDeathState)
        {
            return;
        }
        if (collision.tag.Equals("Mouse"))
        {
            MouseUnit m = collision.GetComponent<MouseUnit>();
            if (m.GetRowIndex() != GetRowIndex() || m.GetHeight() != GetHeight())
                return;
            // 移除变速buff
            m.RemoveUniqueStatusAbility(GetUniqueStatusName());
        }
    }

    /// <summary>
    /// 根据shape获取唯一性状态名
    /// </summary>
    private string GetUniqueStatusName()
    {
        return "变速效果"+mShape;
    }
}
