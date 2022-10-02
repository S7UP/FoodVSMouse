using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// 棉花糖
/// </summary>
public class CottonCandy : FoodUnit
{
    private const string Tag = "CottonCandyVehicle";
    private BoolModifier IgnoreLavaDeBuff = new BoolModifier(true); // 免疫岩浆修饰器
    private List<BaseUnit> unitList = new List<BaseUnit>(); // 受影响的单位表
    private IntModifier VehicleModifier = new IntModifier(1); // 棉花载具数修饰器

    public override void MInit()
    {
        unitList.Clear();
        base.MInit();
        // 免疫岩浆负面效果
        NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreLavaDeBuff, IgnoreLavaDeBuff);
        typeAndShapeToLayer = -1;
        // 添加岩浆灼烧特效
        EffectManager.AddLavaEffectToUnit(this);
    }

    /// <summary>
    /// 棉花的接触判定更大一些
    /// </summary>
    public override void SetCollider2DParam()
    {
        mBoxCollider2D.offset = new Vector2(0, 0);
        mBoxCollider2D.size = new Vector2(0.55f * MapManager.gridWidth, 0.55f * MapManager.gridHeight);
    }

    public override void UpdateAttributeByLevel()
    {
        SetMaxHpAndCurrentHp((float)(attr.baseAttrbute.baseHP + attr.valueList[mLevel]));
    }

    public override void LoadSkillAbility()
    {
        
    }

    public override void MUpdate()
    {
        // 剔除已经无效的单位
        List<BaseUnit> delList = new List<BaseUnit>();
        foreach (var item in unitList)
        {
            if (!item.IsValid())
                delList.Add(item);
        }
        foreach (var item in delList)
        {
            unitList.Remove(item);
        }
        base.MUpdate();
    }

    private void OnCollision(Collider2D collision)
    {
        if(collision.tag.Equals("Food") || collision.tag.Equals("Mouse") || collision.tag.Equals("Item") || collision.tag.Equals("Character"))
        {
            BaseUnit unit = collision.GetComponent<BaseUnit>();
           
            // 只有高度为0的单位能被棉花承载
            if (!unitList.Contains(unit) && unit.GetHeight() == 0)
            {
                // 使目标的棉花糖承载数+1
                if(!unit.NumericBox.IntDict.ContainsKey(Tag))
                    unit.NumericBox.IntDict.Add(Tag, new IntNumeric());
                unit.NumericBox.IntDict[Tag].AddAddModifier(VehicleModifier);
                // 把目标对象加入自己承载的表中
                unitList.Add(unit);
            } 
        }
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        OnCollision(collision);
    }

    public void OnTriggerStay2D(Collider2D collision)
    {
        OnCollision(collision);
    }

    private void OnUnitExit(BaseUnit unit)
    {
        if (unitList.Contains(unit))
        {
            // 使目标的棉花糖承载数-1
            if (!unit.NumericBox.IntDict.ContainsKey(Tag))
                unit.NumericBox.IntDict.Add(Tag, new IntNumeric());
            unit.NumericBox.IntDict[Tag].RemoveAddModifier(VehicleModifier);
            // 把目标对象移出自己承载的表中
            unitList.Remove(unit);
        }
    }

    public void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag.Equals("Food") || collision.tag.Equals("Mouse") || collision.tag.Equals("Item") || collision.tag.Equals("Character"))
        {
            BaseUnit unit = collision.GetComponent<BaseUnit>();
            OnUnitExit(unit);
        }
    }

    public override void AfterDeath()
    {
        // 移除特效
        EffectManager.RemoveLavaEffectFromUnit(this);
        // 使表中还存在的单位执行一次离开自身的事件
        for (int i = 0; i < unitList.Count; i++)
        {
            OnUnitExit(unitList[0]);
        }
        base.AfterDeath();
    }

    /// <summary>
    /// 目标是否被棉花糖承载
    /// </summary>
    /// <param name="unit"></param>
    /// <returns></returns>
    public static bool IsBearing(BaseUnit unit)
    {
        // 是否含有承载tag且承载数必须大于0
        return unit.NumericBox.IntDict.ContainsKey(Tag) && unit.NumericBox.IntDict[Tag].Value > 0;
    }
}
