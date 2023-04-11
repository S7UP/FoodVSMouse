using UnityEngine;
using S7P.Numeric;
/// <summary>
/// 高空地块
/// </summary>
public class SkyGridType : BaseGridType
{
    private const string TaskName = "SkyTask";
    public const string NoAffect = "NoAffectSky";

    /// <summary>
    /// 为某个单位添加完全不受天空影响的效果
    /// </summary>
    /// <param name="unit"></param>
    public static void AddNoAffectBySky(BaseUnit unit, BoolModifier boolModifier)
    {
        unit.NumericBox.AddDecideModifierToBoolDict(NoAffect, boolModifier); 
        unit.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreDropFromSky, boolModifier); 
    }

    /// <summary>
    /// 为某个单位移除完全不受天空影响的效果
    /// </summary>
    /// <param name="unit"></param>
    public static void RemoveNoAffectBySky(BaseUnit unit, BoolModifier boolModifier)
    {
        unit.NumericBox.RemoveDecideModifierToBoolDict(NoAffect, boolModifier); 
        unit.NumericBox.RemoveDecideModifierToBoolDict(StringManager.IgnoreDropFromSky, boolModifier); 
    }

    public static bool IsIgnoreDrop(BaseUnit unit)
    {
        return unit.NumericBox.GetBoolNumericValue(StringManager.IgnoreDropFromSky);
    }

    public override void OnCollision(Collider2D collision)
    {
        if (collision.tag.Equals("Barrier"))
        {
            BaseUnit u = collision.GetComponent<BaseUnit>();
            if (!unitList.Contains(u) && IsMeetingEnterCondition(u))
            {
                unitList.Add(u);
                OnUnitEnter(u);
            }
        }else
            base.OnCollision(collision);
    }

    public override void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag.Equals("Barrier"))
        {
            BaseUnit u = collision.GetComponent<BaseUnit>();
            if (unitList.Contains(u))
            {
                unitList.Remove(u);
                OnUnitExit(u);
            }
        } else
            base.OnTriggerExit2D(collision);
    }

    /// <summary>
    /// 是否满足进入条件
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetingEnterCondition(BaseUnit unit)
    {
        // BOSS单位无视地形效果
        if(unit is MouseUnit)
        {
            MouseUnit m = unit as MouseUnit;
            if (m.IsBoss() || !MouseManager.IsGeneralMouse(unit))
                return false;
        }

        // 人物单位也不受高影响
        if(unit is CharacterUnit)
        {
            return false;
        }

        // 空军和地鼠也不受影响
        return unit.GetHeight()==0;
    }

    /// <summary>
    /// 当有单位进入地形时施加给单位的效果
    /// </summary>
    /// <param name="unit"></param>
    public override void OnUnitEnter(BaseUnit unit)
    {
        SkyTask t;
        if (unit.GetTask(TaskName) == null)
        {
            t = new SkyTask(unit);
            unit.AddUniqueTask(TaskName, t);
        }
        else
        {
            t = unit.GetTask(TaskName) as SkyTask;
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
            SkyTask t = unit.GetTask(TaskName) as SkyTask;
            t.DecCount();
        }
        else
        {
            Debug.LogWarning("为什么有东西可以没带高空任务出高空？");
        }
    }


    /// <summary>
    /// 高空任务
    /// </summary>
    private class SkyTask : ITask
    {
        private int count; // 进入的高空格子数
        private BaseUnit unit;

        public SkyTask(BaseUnit unit)
        {
            this.unit = unit;
        }

        public void OnEnter()
        {
            count = 1;
        }

        public void OnUpdate()
        {
            // 如果目标 没有被空载 且 不免疫摔落 那么直接摔死吧
            if((!unit.NumericBox.IntDict.ContainsKey(StringManager.BearInSky) || unit.NumericBox.IntDict[StringManager.BearInSky].Value <= 0)
                && !IsIgnoreDrop(unit) && !CottonCandy.IsBearing(unit) && !UnitManager.IsFlying(unit))
            {
                unit.ExecuteDrop();
            }
        }

        public bool IsMeetingExitCondition()
        {
            return count <= 0;
        }

        public void OnExit()
        {

        }

        // 自定义方法
        public void AddCount()
        {
            count++;
        }

        public void DecCount()
        {
            count--;
        }
    }
}
