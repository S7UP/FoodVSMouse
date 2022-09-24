using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterGridType : BaseGridType
{
    /// <summary>
    /// 当有单位进入地形时施加给单位的效果
    /// </summary>
    /// <param name="baseUnit"></param>
    public override void OnUnitEnter(BaseUnit baseUnit)
    {
        if (baseUnit.mHeight <= 0)
        {
            float descendGridCount;
            if (baseUnit is FoodUnit)
                descendGridCount = 0f;
            else if (baseUnit is MouseUnit)
                descendGridCount = 0.4f;
            else if (baseUnit is BaseItem)
                descendGridCount = 0f;
            else if (baseUnit is CharacterUnit)
                descendGridCount = 0f;
            else
                descendGridCount = 0;
            // 为目标无差别添加进水的唯一状态，之后相关判定逻辑转交给该状态处理
            baseUnit.AddUniqueStatusAbility(StringManager.WaterGridState, new WaterStatusAbility(baseUnit, descendGridCount));
        }
    }

    /// <summary>
    /// 当有单位处于地形时持续给单位的效果
    /// </summary>
    /// <param name="baseUnit"></param>
    public override void OnUnitStay(BaseUnit baseUnit)
    {

    }

    /// <summary>
    /// 当有单位离开地形时施加给单位的效果
    /// </summary>
    /// <param name="baseUnit"></param>
    public override void OnUnitExit(BaseUnit baseUnit)
    {
        baseUnit.RemoveUniqueStatusAbility(StringManager.WaterGridState);
    }
}
