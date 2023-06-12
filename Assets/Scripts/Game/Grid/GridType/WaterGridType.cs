
using Environment;

using S7P.Numeric;

using UnityEngine;

public class WaterGridType : BaseGridType
{
    private const string TaskName = "WaterTask";

    /// <summary>
    /// 为某个单位添加完全不受水影响的效果
    /// </summary>
    /// <param name="unit"></param>
    public static void AddNoAffectByWater(BaseUnit unit, BoolModifier boolModifier)
    {
        unit.NumericBox.AddDecideModifierToBoolDict(WaterTask.NoDrop, boolModifier); // 标记在水里不下降
        unit.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreWaterGridState, boolModifier); // 免疫水蚀
    }

    /// <summary>
    /// 为某个单位移除完全不受水影响的效果
    /// </summary>
    /// <param name="unit"></param>
    public static void RemoveNoAffectByWater(BaseUnit unit, BoolModifier boolModifier)
    {
        unit.NumericBox.RemoveDecideModifierToBoolDict(WaterTask.NoDrop, boolModifier); // 标记在水里不下降
        unit.NumericBox.RemoveDecideModifierToBoolDict(StringManager.IgnoreWaterGridState, boolModifier); // 免疫水蚀
    }

    /// <summary>
    /// 是否满足进入条件
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetingEnterCondition(BaseUnit unit)
    {
        if (unit.aliveTime <= 5)
            return false;
        // BOSS单位无视地形效果
        if (unit is MouseUnit)
        {
            MouseUnit m = unit as MouseUnit;
            if (m.IsBoss())
                return false;
        }
        return unit.GetHeight() <= 0;
    }

    /// <summary>
    /// 当有单位进入地形时施加给单位的效果
    /// </summary>
    /// <param name="baseUnit"></param>
    public override void OnUnitEnter(BaseUnit unit)
    {
        WaterTask t;
        if (unit.GetTask(TaskName) == null)
        {
            switch (unit.tag)
            {
                case "Food":
                    t = new WaterTask(UnitType.Food, unit); break;
                case "Mouse":
                    t = new WaterTask(UnitType.Mouse, unit); break;
                case "Character":
                    t = new WaterTask(UnitType.Character, unit); break;
                case "Item":
                    t = new WaterTask(UnitType.Item, unit); break;
                default:
                    Debug.LogWarning("水里进入了奇怪的东西");
                    t = new WaterTask(UnitType.Food, unit); break;
            }
            unit.AddUniqueTask(TaskName, t);
        }
        else
        {
            t = unit.GetTask(TaskName) as WaterTask;
            t.AddCount();
        }
    }

    /// <summary>
    /// 当有单位处于地形时持续给单位的效果
    /// </summary>
    /// <param name="unit"></param>
    public override void OnUnitStay(BaseUnit unit)
    {

    }

    /// <summary>
    /// 当有单位离开地形时施加给单位的效果
    /// </summary>
    /// <param name="unit"></param>
    public override void OnUnitExit(BaseUnit unit)
    {
        if (unit.GetTask(TaskName) != null)
        {
            WaterTask t = unit.GetTask(TaskName) as WaterTask;
            t.DecCount();
        }
    }

    /// <summary>
    /// 判断某个单位是否在水域范围内
    /// </summary>
    /// <returns></returns>
    public static bool IsInWater(BaseUnit unit)
    {
        return unit.GetTask(TaskName) != null;
    }
}
