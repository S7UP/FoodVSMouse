using System.Collections.Generic;

using S7P.Numeric;

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
        // 免疫摔落
        SkyGridType.AddNoAffectBySky(this, new BoolModifier(true));
        typeAndShapeToLayer = -1;
        // 添加岩浆灼烧特效
        // EffectManager.AddLavaEffectToUnit(this);
        // 创建承载检测区域
        CreateCheckArea();
        // 设置仅能被部分老鼠阻挡并且伤害
        AddCanBlockFunc((self, other) =>
        {
            return false;
        });
        AddCanHitFunc((self, bullet) => {
            return false;
        });
        AddCanBeSelectedAsTargetFunc((self, other) => {
            return false;
        });
    }

    //private void OnUnitEnter(BaseUnit unit)
    //{
    //    // 为目标添加一层<高空承载>标签
    //    if (!unit.NumericBox.IntDict.ContainsKey(StringManager.BearInSky))
    //        unit.NumericBox.IntDict.Add(StringManager.BearInSky, new IntNumeric());
    //    unit.NumericBox.IntDict[StringManager.BearInSky].AddAddModifier(BearInSkyModifier);
    //    // 超过承受数直接隐藏（破裂）
    //    if (unitList.Count >= maxBearCount)
    //    {
    //        isBreak = true;
    //        recoverTimeLeft = 60 * 24;
    //        Hide();
    //    }
    //}

    //private void OnUnitExit(BaseUnit unit)
    //{
    //    // 移除目标一层<高空承载>标签
    //    if (unit.NumericBox.IntDict.ContainsKey(StringManager.BearInSky))
    //        unit.NumericBox.IntDict[StringManager.BearInSky].RemoveAddModifier(BearInSkyModifier);
    //}

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
        t.AddOnExitAction(delegate {
            r.ExecuteRecycle();
        });
        r.AddTask(t);
    }

    private void OnCollision(BaseUnit unit)
    {
        // 只有高度为0的单位能被棉花承载
        if (!unitList.Contains(unit) && unit.GetHeight() == 0)
        {
            // 使目标的棉花糖承载数+1
            if (!unit.NumericBox.IntDict.ContainsKey(Tag))
                unit.NumericBox.IntDict.Add(Tag, new IntNumeric());
            unit.NumericBox.IntDict[Tag].AddAddModifier(VehicleModifier);
            // 把目标对象加入自己承载的表中
            unitList.Add(unit);
        }
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

    public override void AfterDeath()
    {
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
