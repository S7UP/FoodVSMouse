using System.Collections.Generic;

using S7P.Numeric;

using UnityEngine;
/// <summary>
/// 木盘子
/// </summary>
public class WoodenDisk : FoodUnit
{
    private BoolModifier IgnoreWaterModifier = new BoolModifier(true); // 免疫水蚀修饰器

    // 对以下表中的老鼠类型木盘子作用不生效
    private List<MouseNameTypeMap> noEffectMouseList = new List<MouseNameTypeMap>() 
    { 
        MouseNameTypeMap.SubmarineMouse,
        MouseNameTypeMap.RowboatMouse,
    };


    public override void MInit()
    {
        base.MInit();
        // 免疫水蚀效果
        WaterGridType.AddNoAffectByWater(this, IgnoreWaterModifier);
        typeAndShapeToLayer = -1;
        // 创建承载检测区域
        CreateCheckArea();
        // 设置仅能被部分老鼠阻挡并且伤害
        AddCanBlockFunc((self, other) =>
        {
            if(other is MouseUnit)
            {
                if (noEffectMouseList.Contains((MouseNameTypeMap)other.mType))
                    return true;
            }
            return false;
        });
        AddCanHitFunc((self, bullet) => {
            return false;
        });
        AddCanBeSelectedAsTargetFunc((self, other) => {
            if (other is MouseUnit)
            {
                if (noEffectMouseList.Contains((MouseNameTypeMap)other.mType))
                    return true;
            }
            return false;
        });
    }

    /// <summary>
    /// 创建检测区域
    /// </summary>
    private void CreateCheckArea()
    {
        RetangleAreaEffectExecution r = Environment.WaterManager.GetVehicleArea(transform.position, new Vector2(MapManager.gridWidth, MapManager.gridHeight), new S7P.Numeric.FloatModifier(0.2f * MapManager.gridHeight));
        // 额外添加一个敌人进入的条件：木盘子不能承载特定老鼠
        r.AddEnemyEnterConditionFunc((u) => {
            return !noEffectMouseList.Contains((MouseNameTypeMap)u.mType);
        });
        GameController.Instance.AddAreaEffectExecution(r);

        // 跟随逻辑
        CustomizationTask t = new CustomizationTask();
        t.AddTaskFunc(delegate {
            if (IsAlive())
            {
                r.transform.position = transform.position;
                return false;
            }
            else
                return true;
        });
        r.AddTask(t);

        AddOnDestoryAction(delegate {
            r.MDestory();
        });
    }

    public override void UpdateAttributeByLevel()
    {
        SetMaxHpAndCurrentHp((float)(attr.baseAttrbute.baseHP + attr.valueList[mLevel]));
    }

    public override void LoadSkillAbility()
    {
        
    }
}
