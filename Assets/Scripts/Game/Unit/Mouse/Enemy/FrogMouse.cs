using UnityEngine;
/// <summary>
/// 青蛙王子鼠
/// </summary>
public class FrogMouse : MouseUnit, IInWater
{
    private bool isJumped; // 是否跳过一次
    private bool isInWater; // 是否位于水中
    private bool isDieInWater;
    private FloatModifier FrogSpeedModifier = new FloatModifier(200); // 由青蛙带来的移动速度加成
    private FloatModifier FrogInWaterSpeedModifier = new FloatModifier(500); // 由青蛙在水里再带来的移动速度加成（可与上述相加算）

    public override void MInit()
    {
        isJumped = false;
        isInWater = false;
        isDieInWater = false;
        base.MInit();
        // 初始获得携带青蛙的移动速度
        NumericBox.MoveSpeed.AddPctAddModifier(FrogSpeedModifier);
        // 免疫水蚀
        NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreWaterGridState, new BoolModifier(true));
    }

    public override bool IsDamageJudgment()
    {
        if (!isJumped)
            return true;
        else
            return base.IsDamageJudgment();
    }

    /// <summary>
    /// 执行具体的攻击，位于伤害判定为真之后
    /// </summary>
    public override void ExecuteDamage()
    {
        // 在未进行第一次跳跃时，普通攻击被替换为跳跃
        if (!isJumped)
        {
            // 添加一个弹起任务
            isJumped = true;
            // 进入不可选取状态
            CloseCollision();
            // 跳跃格子数等于 0.5*当前移动速度标准值
            float dist = 0.5f * TransManager.TranToStandardVelocity(GetMoveSpeed());
            Tasker t = GameController.Instance.AddTasker(new ParabolaMovePresetTasker(this, 12.0f, 0.8f, transform.position, transform.position + (Vector3)moveRotate * dist * MapManager.gridWidth, false));
            t.AddOtherEndEvent(delegate 
            { 
                OpenCollision();
                // 结束后青蛙消失，失去青蛙带来的所有移动速度加成
                NumericBox.MoveSpeed.RemovePctAddModifier(FrogSpeedModifier);
                NumericBox.MoveSpeed.RemovePctAddModifier(FrogInWaterSpeedModifier);
            });
        }
        else
        {
            base.ExecuteDamage();
        }
    }

    public void OnEnterWater()
    {
        isInWater = true;
        // 下水后如果有带青蛙则获得移速加成
        if (!isJumped)
        {
            NumericBox.MoveSpeed.RemovePctAddModifier(FrogSpeedModifier);
            NumericBox.MoveSpeed.RemovePctAddModifier(FrogInWaterSpeedModifier);
            NumericBox.MoveSpeed.AddPctAddModifier(FrogSpeedModifier);
            NumericBox.MoveSpeed.AddPctAddModifier(FrogInWaterSpeedModifier);
        }
        SetActionState(new TransitionState(this));
        // 添加水波特效
        EffectManager.AddWaterWaveEffectToUnit(this);
    }

    public void OnStayWater()
    {

    }

    public void OnExitWater()
    {
        // 出水也要分情况，是有可能死亡后清除DEBUFF效果然后强制出水的
        if (isDeathState)
        {
            isInWater = false;
            isDieInWater = true;
        }
        else
        {
            SetNoCollideAllyUnit();
            isInWater = false;
            // 移除青蛙在水里的那部分移速加成
            NumericBox.MoveSpeed.RemovePctAddModifier(FrogInWaterSpeedModifier);
            SetActionState(new TransitionState(this));
        }
        // 移除水波特效
        EffectManager.RemoveWaterWaveEffectFromUnit(this);
    }

    public override void OnTransitionStateEnter()
    {
        if (isInWater)
        {
            if (isJumped)
                animatorController.Play("Enter1");
            else
                animatorController.Play("Enter0");
        }
        else
        {
            if (isJumped)
                animatorController.Play("Exit1");
            else
                animatorController.Play("Exit0");
        }
    }

    public override void OnTransitionState()
    {
        if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
        {
            SetActionState(new MoveState(this));
        }
    }

    public override void OnAttackStateEnter()
    {
        if (isJumped)
        {
            if(isInWater)
                animatorController.Play("Attack1");
            else
                animatorController.Play("Attack0");
        }
        else
        {
            if (isInWater)
                animatorController.Play("Jump1");
            else
                animatorController.Play("Jump0");
        }
    }

    public override void OnMoveStateEnter()
    {
        if (isJumped)
        {
            if(isInWater)
                animatorController.Play("Move3", true); // 水上走路
            else
                animatorController.Play("Move1", true); // 陆地走路
        }
        else
        {
            if (isInWater)
                animatorController.Play("Move2", true); // 水上蛙泳
            else
                animatorController.Play("Move0", true); // 陆地蛙跳
        }
    }

    public override void OnIdleStateEnter()
    {
        if (isInWater)
            animatorController.Play("Idle1", true);
        else
            animatorController.Play("Idle0", true);
    }

    public override void OnIdleState()
    {
        if (!IsHasTarget())
            SetActionState(new MoveState(this));
    }

    public override void OnDieStateEnter()
    {
        if (isDieInWater)
            animatorController.Play("Die1", true);
        else
            animatorController.Play("Die0", true);
    }

    /// <summary>
    /// 青蛙王子鼠更倾向于水地形，而非陆地
    /// </summary>
    public override void SetGridDangerousWeightDict()
    {
        GridDangerousWeightDict[GridType.Water] = GridDangerousWeightDict[GridType.Default] - 1;
    }
}
