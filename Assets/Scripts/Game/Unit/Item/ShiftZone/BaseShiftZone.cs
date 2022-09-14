using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 基础变速地带
/// </summary>
public class BaseShiftZone : BaseItem
{
    private float changePercent; // 基础移速改变值
    private List<BaseUnit> unitList = new List<BaseUnit>(); // 位于该变速带生效范围内的单位表
    public int skinType; // 皮肤种类

    public override void MInit()
    {
        skinType = 0;
        unitList.Clear();
        base.MInit();
        changePercent = 0;
        SetSkin(0); // 默认为蜗牛粘液
        animatorController.Play("Appear");
    }

    /// <summary>
    /// 设置皮肤外观
    /// </summary>
    public void SetSkin(int skinType)
    {
        this.skinType = skinType;
        animator.runtimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Item/"+mType+"/"+mShape+"/"+ skinType);
    }

    /// <summary>
    /// 设置速度改变比例
    /// </summary>
    public void SetChangePercent(float per)
    {
        changePercent = per;
    }

    private void OnCollision(Collider2D collision)
    {
        // 死亡动画时不接受任何碰撞事件
        if (isDeathState)
        {
            return;
        }
        if (collision.tag.Equals("Mouse"))
        {
            MouseUnit m = collision.GetComponent<MouseUnit>();
            if (unitList.Contains(m) || m.GetRowIndex() != GetRowIndex() || m.GetHeight() != GetHeight())
                return;
            // 添加变速buff
            m.AddUniqueStatusAbility(GetUniqueStatusName(), new ChangeVelocityStatusAbility(m, changePercent));
            unitList.Add(m);
        }
    }

    /// <summary>
    /// 碰撞事件
    /// </summary>
    /// <param name="collision"></param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        OnCollision(collision);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        OnCollision(collision);
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
            unitList.Remove(m);
            m.RemoveUniqueStatusAbility(GetUniqueStatusName());
        }
    }

    public override void OnDieStateEnter()
    {
        base.OnDieStateEnter();
        // 移除自己身上所有单位的由自身给予的效果
        foreach (var m in unitList)
        {
            m.RemoveUniqueStatusAbility(GetUniqueStatusName());
        }
        unitList.Clear();
    }

    public override void AfterDeath()
    {
        base.AfterDeath();
        // 移除自己身上所有单位的由自身给予的效果
        foreach (var m in unitList)
        {
            m.RemoveUniqueStatusAbility(GetUniqueStatusName());
        }
        unitList.Clear();
    }

    /// <summary>
    /// 根据shape获取唯一性状态名
    /// </summary>
    private string GetUniqueStatusName()
    {
        return "变速效果"+mShape+ "-"+ skinType;
    }

    public override void UpdateRenderLayer(int arrayIndex)
    {
        spriteRenderer.sortingOrder = LayerManager.CalculateSortingLayer(LayerManager.UnitType.Ally, GetRowIndex(), -5, arrayIndex);
    }
}
