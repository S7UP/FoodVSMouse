using UnityEngine;
using S7P.Numeric;
/// <summary>
/// 潜水艇类
/// </summary>
public class SubmarineMouse : MouseUnit, IInWater
{
    private bool isEnterWater; // 于TransitionState时使用，判断是播放下水动画还是出水动画
    private FloatModifier SpeedModifier = new FloatModifier(50f); // 加速修饰（当非受伤态时处于水中时获得50%移速加成）

    public override void MInit()
    {
        isEnterWater = false;
        base.MInit();
        // 免疫灰烬秒杀效果、水蚀效果
        NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreBombInstantKill, new BoolModifier(true));
        NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreWaterGridState, new BoolModifier(true));
    }


    public override void SetCollider2DParam()
    {
        mBoxCollider2D.offset = new Vector2(1f/3 * MapManager.gridWidth, 0);
        mBoxCollider2D.size = new Vector2(1.0f * MapManager.gridWidth, 0.49f * MapManager.gridHeight);
    }

    public void OnEnterWater()
    {
        isEnterWater = true;
        // 仅第一个受伤阶段有移速加成
        if (mHertIndex == 0)
        {
            NumericBox.MoveSpeed.RemovePctAddModifier(SpeedModifier);
            NumericBox.MoveSpeed.AddPctAddModifier(SpeedModifier);
        }
        SetActionState(new TransitionState(this));
    }

    public void OnStayWater()
    {

    }

    public void OnExitWater()
    {
        SetNoCollideAllyUnit();
        isEnterWater = false;
        NumericBox.MoveSpeed.RemovePctAddModifier(SpeedModifier);
        SetActionState(new TransitionState(this));
    }

    public override void UpdateRuntimeAnimatorController()
    {
        base.UpdateRuntimeAnimatorController();
        // 当切换成受伤阶段时取消移速加成，但如果从受伤态切换为普通态则判定一次是否在水中，如果在的话则把移速加成回调
        if(mHertIndex == 0 && isEnterWater)
        {
            NumericBox.MoveSpeed.RemovePctAddModifier(SpeedModifier);
            NumericBox.MoveSpeed.AddPctAddModifier(SpeedModifier);
        }else if(mHertIndex > 0)
            NumericBox.MoveSpeed.RemovePctAddModifier(SpeedModifier);
    }

    public override void OnTransitionStateEnter()
    {
        if (isEnterWater)
            animatorController.Play("Enter");
        else
            animatorController.Play("Exit");
    }

    public override void OnTransitionState()
    {
        if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
        {
            SetActionState(new MoveState(this));
        }
    }

    public override void OnMoveStateEnter()
    {
        if (isEnterWater)
            animatorController.Play("Move1", true);
        else
            animatorController.Play("Move0", true);
    }

    /// <summary>
    /// 只有在水下时才可以被阻挡（判断依据是是否含有水BUFF）
    /// </summary>
    /// <param name="unit"></param>
    /// <returns></returns>
    public override bool CanBlock(BaseUnit unit)
    {
        return WaterGridType.IsInWater(this) && base.CanBlock(unit);
    }

    public override void OnDieStateEnter()
    {
        // 如果自己在水域中且没有被承载就播放特有的淹死动画
        if (WaterGridType.IsInWater(this) && !WoodenDisk.IsBearing(this))
            animatorController.Play("Die1");
        else
            animatorController.Play("Die0");
    }

    /// <summary>
    /// 当与友方单位（美食、人物）发生刚体碰撞判定时（增加一个水中载具）
    /// </summary>
    public override void OnAllyCollision(BaseUnit unit)
    {
        // 检测本格美食最高受击优先级单位（包括水中载具）
        BaseUnit target = unit.GetGrid().GetHighestAttackPriorityUnitIncludeWaterVehicle(this);
        if (!isBlock && UnitManager.CanBlock(this, target)) // 检测双方能否互相阻挡
        {
            isBlock = true;
            mBlockUnit = target;
        }
    }

    /// <summary>
    /// 索敌方式增加一个水中载具
    /// </summary>
    /// <returns></returns>
    public override bool IsHasTarget()
    {
        if (isBlock && mBlockUnit.IsAlive())
        {
            // 若目标依附于格子，则将目标切换为目标所在格的最高攻击优先级目标
            BaseGrid g = mBlockUnit.GetGrid();
            if (g != null)
            {
                mBlockUnit = g.GetHighestAttackPriorityUnitIncludeWaterVehicle(this);
            }
            return true;
        }
        return false;
    }

    /// <summary>
    /// 拳皇拳击鼠攻击附带单格AOE伤害效果
    /// </summary>
    public override void ExecuteDamage()
    {
        if(mShape==2 || mShape == 5)
        {
            if (IsHasTarget())
            {
                BaseGrid g = GetCurrentTarget().GetGrid();
                if (g != null)
                {
                    foreach (var item in g.GetAttackableFoodUnitListIncludeWaterVehicle())
                    {
                        TakeDamage(item);
                    }
                }
                else
                    base.ExecuteDamage();
            } 
        }
        else
        {
            base.ExecuteDamage();
        }
    }

    /// <summary>
    /// 潜水艇更倾向于水地形，而非陆地
    /// </summary>
    public override void SetGridDangerousWeightDict()
    {
        GridDangerousWeightDict[GridType.Water] = GridDangerousWeightDict[GridType.Default] - 1;
    }
}
