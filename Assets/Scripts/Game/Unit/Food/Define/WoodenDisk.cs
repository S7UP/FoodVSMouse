using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// 木盘子
/// </summary>
public class WoodenDisk : FoodUnit
{
    private const string TaskKey = "木盘子提供的悬浮效果";
    private BoolModifier IgnoreWaterModifier = new BoolModifier(true); // 免疫水蚀修饰器
    private List<BaseUnit> unitList = new List<BaseUnit>(); // 受影响的单位表 

    private const string Tag = "WoodenDiskCandyVehicle";
    private BoolModifier IgnoreLavaDeBuff = new BoolModifier(true); // 免疫岩浆修饰器
    private IntModifier VehicleModifier = new IntModifier(1); // 棉花载具数修饰器



    // 对以下表中的老鼠类型木盘子作用不生效
    private List<MouseNameTypeMap> noEffectMouseList = new List<MouseNameTypeMap>() 
    { 
        MouseNameTypeMap.SubmarineMouse,
        MouseNameTypeMap.RowboatMouse,
    };


    public override void MInit()
    {
        unitList.Clear();
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
        RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(transform.position, 1, 1, "BothCollide");
        r.isAffectFood = true;
        r.isAffectMouse = true;
        r.isAffectCharacter = true;
        r.SetOnFoodEnterAction(OnCollision);
        r.SetOnEnemyEnterAction(OnCollision);
        r.SetOnCharacterEnterAction(OnCollision);
        r.SetOnFoodStayAction(OnCollision);
        r.SetOnEnemyStayAction(OnCollision);
        r.SetOnCharacterStayAction(OnCollision);
        r.SetOnFoodExitAction(OnUnitExit);
        r.SetOnEnemyExitAction(OnUnitExit);
        r.SetOnCharacterExitAction(OnUnitExit);
        GameController.Instance.AddAreaEffectExecution(r);

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
        t.OnExitFunc = delegate {
            r.ExecuteRecycle();
        };
        r.AddTask(t);
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

    private void OnCollision(BaseUnit u)
    {
        // 检查目标是否不能生效
        if ((u is MouseUnit && noEffectMouseList.Contains((MouseNameTypeMap)u.mType)) || u.GetHeight() != 0)
            return;

        if (!unitList.Contains(u))
        {
            // 使目标的木盘子承载数+1
            if (!u.NumericBox.IntDict.ContainsKey(Tag))
                u.NumericBox.IntDict.Add(Tag, new IntNumeric());
            u.NumericBox.IntDict[Tag].AddAddModifier(VehicleModifier);
            // 把目标对象加入自己承载的表中
            unitList.Add(u);
        }
    }

    private void OnUnitExit(BaseUnit unit)
    {
        if (unitList.Contains(unit))
        {
            // 使目标的木盘子承载数-1
            if (!unit.NumericBox.IntDict.ContainsKey(Tag))
                unit.NumericBox.IntDict.Add(Tag, new IntNumeric());
            unit.NumericBox.IntDict[Tag].RemoveAddModifier(VehicleModifier);
            // 把目标对象移出自己承载的表中
            unitList.Remove(unit);
        }
    }

    public override void AfterDeath()
    {
        // 移除水波纹效果
        // EffectManager.RemoveWaterWaveEffectFromUnit(this);

        // 移除表中还存在的单位身上的由该盘子产生的免疫水蚀标签
        for (int i = 0; i < unitList.Count; i++)
        {
            OnUnitExit(unitList[0]);
        }
        base.AfterDeath();
    }

    /// <summary>
    /// 获取目标身上所在的悬浮任务
    /// </summary>
    /// <param name="unit"></param>
    /// <returns></returns>
    public static ITask GetFloatTask(BaseUnit unit)
    {
        return unit.GetTask(TaskKey);
    }

    /// <summary>
    /// 目标是否被木盘子承载
    /// </summary>
    /// <param name="unit"></param>
    /// <returns></returns>
    public static bool IsBearing(BaseUnit unit)
    {
        // 是否含有承载tag且承载数必须大于0
        return unit.NumericBox.IntDict.ContainsKey(Tag) && unit.NumericBox.IntDict[Tag].Value > 0;
    }
}
