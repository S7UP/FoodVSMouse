using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static UnityEngine.GraphicsBuffer;
/// <summary>
/// 划艇鼠
/// </summary>
public class RowboatMouse : MouseUnit, IInWater
{
    private List<BaseUnit> blockedUnitList = new List<BaseUnit>(); // 被阻挡的单位表
    private FloatModifier SpeedModifier = new FloatModifier(50); // 移速加成标签
    private bool isAddSpeed;
    private const int DamageInterval = 30; // 伤害间隔
    private int timeLeft; // 触发伤害判定剩余时间
    private const int AdvanceTime = 720; // 前进时间
    private const int BackTime = 360; // 后退时间
    private int moveTimeLeft; // 移动剩余时间（时间到了切到下一阶段)
    private bool isAdvance; // 是否在前进阶段

    public override void MInit()
    {
        blockedUnitList.Clear();
        isAddSpeed = false;
        timeLeft = 0;
        moveTimeLeft = 0;
        isAdvance = false;
        base.MInit();
        // 免疫灰烬秒杀效果、冰冻减速效果、冰冻效果、水蚀效果
        NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreBombInstantKill, new BoolModifier(true));
        NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreFrozen, new BoolModifier(true));
        NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreFrozenSlowDown, new BoolModifier(true));
        NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreWaterGridState, new BoolModifier(true));
        // 初始为出水状态
        SetActionState(new TransitionState(this));
    }

    public override void SetCollider2DParam()
    {
        mBoxCollider2D.offset = new Vector2(1.24f * MapManager.gridWidth, 0);
        mBoxCollider2D.size = new Vector2(2.99f * MapManager.gridWidth, 0.49f * MapManager.gridHeight);
    }

    public override void LoadSkillAbility()
    {
        // 不加载技能，即没有平A
    }

    /// <summary>
    /// 当与友方单位（美食、人物）发生刚体碰撞判定时
    /// </summary>
    public override void OnAllyCollision(BaseUnit target)
    {
        if (UnitManager.CanBlock(this, target)) // 检测双方能否互相阻挡
        {
            if (!blockedUnitList.Contains(target))
                blockedUnitList.Add(target);
        }
    }

    /// <summary>
    /// 当友方单位离开时
    /// </summary>
    /// <param name="collision"></param>
    public override void OnAllyTriggerExit(Collider2D collision)
    {
        if (collision.tag.Equals("Food") || collision.tag.Equals("Character"))
        {
            BaseUnit unit = collision.GetComponent<BaseUnit>();
            blockedUnitList.Remove(unit);
        }
    }

    public override void MUpdate()
    {
        // 剔除无效的单位
        List<BaseUnit> delList = new List<BaseUnit>();
        foreach (var item in blockedUnitList)
        {
            if (!item.IsAlive())
                delList.Add(item);
        }
        foreach (var item in delList)
        {
            blockedUnitList.Remove(item);
        }
        base.MUpdate();
    }

    public override void OnTransitionStateEnter()
    {
        CloseCollision();
        animatorController.Play("Appear");
    }

    public override void OnTransitionState()
    {
        if (animatorController.GetCurrentAnimatorStateRecorder().timer == 1)
        {
            // 出现第一帧时强制把坐标同步为当前所在行右二列
            transform.position = MapManager.GetGridLocalPosition(8, currentYIndex);
        }else if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
        {
            OpenCollision();
            SetActionState(new MoveState(this));
        }
    }

    /// <summary>
    /// 在移动的过程中，如果有接触的目标则持续伤害接触目标 （每半秒10伤害）
    /// 如果没有接触目标则获得100%移动速度加成
    /// </summary>
    public override void OnMoveState()
    {
        // 前进后退切换逻辑
        if (moveTimeLeft > 0)
            moveTimeLeft--;
        else
        {
            if (isAdvance)
            {
                // 前进切为后退
                isAdvance = false;
                moveTimeLeft = BackTime;
                SetMoveRoate(Vector2.right);
            }
            else
            {
                // 后退切为前进
                isAdvance = true;
                moveTimeLeft = AdvanceTime;
                SetMoveRoate(Vector2.left);
            }
        }

        base.OnMoveState();
        if (timeLeft > 0)
            timeLeft--;
        if (blockedUnitList.Count > 0)
        {
            if (isAddSpeed)
            {
                NumericBox.MoveSpeed.RemovePctAddModifier(SpeedModifier);
                isAddSpeed = false;
            }

            // 伤害接触目标
            if(timeLeft <= 0)
            {
                timeLeft = DamageInterval;
                foreach (var item in blockedUnitList)
                {
                    TakeDamage(item);
                }
            }
        }
        else
        {
            if (!isAddSpeed)
            {
                NumericBox.MoveSpeed.AddPctAddModifier(SpeedModifier);
                isAddSpeed = true;
            }
        }
    }

    /// <summary>
    /// 不可被阻挡
    /// </summary>
    /// <param name="unit"></param>
    /// <returns></returns>
    //public override bool CanBlock(BaseUnit unit)
    //{
    //    return false;
    //}

    public void OnEnterWater()
    {
        
    }

    public void OnStayWater()
    {
        
    }

    public void OnExitWater()
    {
        
    }
}
