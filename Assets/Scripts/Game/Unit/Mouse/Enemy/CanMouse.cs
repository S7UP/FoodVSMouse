using System;

using UnityEngine;
/// <summary>
/// 罐头鼠
/// </summary>
public class CanMouse : MouseUnit
{
    private FloatModifier defenseModifier = new FloatModifier(0.5f); // 减伤修饰
    private FloatModifier moveSpeedModifier1 = new FloatModifier(-50); // 减速修饰
    private FloatModifier moveSpeedModifier2 = new FloatModifier(300); // 加速修饰
    private FloatModifier attackSpeedModifier = new FloatModifier(100); // 加攻速修饰
    private BoolModifier IgnoreBombInstantKill = new BoolModifier(true);
    private BoolModifier IgnoreSlowDown = new BoolModifier(true);
    private BoolModifier IgnoreStun = new BoolModifier(true);

    private bool isReinstallState; // 是否为重装态

    private bool isDroping; // 是否在扔装备然后晕眩的过程中

    public override void MInit()
    {
        isDroping = false;
        base.MInit();
        isReinstallState = false; // 只是初始化，不用担心，而且不这么做后面的方法会失效
        // 初始态为重装态
        SetReinstallState();
    }

    /// <summary>
    /// 受到灰烬伤害时
    /// </summary>
    /// <param name="dmg"></param>
    public override void OnBombBurnDamage(float dmg)
    {
        // 原先的伤害执行
        base.OnBombBurnDamage(dmg);
        // 处于重装状态但不能是飞行动画期间
        if (isReinstallState && !(mCurrentActionState is TransitionState))
        {
            // 起飞咯！！！！
            SetActionState(new TransitionState(this));
            //Tasker t = GameController.Instance.AddTasker(new ParabolaMovePresetTasker(this, 24.0f, 0.75f, transform.position, transform.position + (Vector3)moveRotate*3*MapManager.gridWidth, false));
            //DisableMove(true);
            //// 飞行结束时切换成丢装备且晕眩状态
            //t.AddOtherEndEvent(delegate { SetActionState(new CastState(this)); DisableMove(false); });

            float dist = 3 * MapManager.gridWidth;
            CustomizationTask t = TaskManager.AddParabolaTask(this, dist / 60, dist / 2, transform.position, transform.position + (Vector3)moveRotate * dist, false);
            DisableMove(true);
            Action oldExit = t.OnExitFunc;
            t.OnExitFunc = delegate
            {
                if (oldExit != null)
                    oldExit();
                SetActionState(new CastState(this));
                DisableMove(false);
            };
        }
    }

    /// <summary>
    /// 设置为重装状态
    /// </summary>
    private void SetReinstallState()
    {
        if (isReinstallState)
            return;
        isReinstallState = true;
        // 轻装状态效果移除
        NumericBox.MoveSpeed.RemovePctAddModifier(moveSpeedModifier2);
        NumericBox.AttackSpeed.RemovePctAddModifier(attackSpeedModifier);

        // 获取减伤
        NumericBox.Defense.AddAddModifier(defenseModifier);
        // 总移速降低效果
        NumericBox.MoveSpeed.AddFinalPctAddModifier(moveSpeedModifier1);
        // 免疫灰烬减速与定身
        NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreBombInstantKill, IgnoreBombInstantKill);
        NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreSlowDown, IgnoreSlowDown);
        NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreStun, IgnoreStun);

        mHertIndex = 0;
        UpdateRuntimeAnimatorController();
    }

    /// <summary>
    /// 设置为轻装状态
    /// </summary>
    private void SetLightState()
    {
        if (!isReinstallState)
            return;
        isReinstallState = false;
        // 重装效果移除
        NumericBox.Defense.RemoveAddModifier(defenseModifier);
        NumericBox.MoveSpeed.RemoveFinalPctAddModifier(moveSpeedModifier1);
        NumericBox.RemoveDecideModifierToBoolDict(StringManager.IgnoreBombInstantKill, IgnoreBombInstantKill);
        NumericBox.RemoveDecideModifierToBoolDict(StringManager.IgnoreSlowDown, IgnoreSlowDown);
        NumericBox.RemoveDecideModifierToBoolDict(StringManager.IgnoreStun, IgnoreStun);


        // 获取加速效果
        NumericBox.MoveSpeed.AddPctAddModifier(moveSpeedModifier2);
        // 获取攻速加成效果
        NumericBox.AttackSpeed.AddPctAddModifier(attackSpeedModifier);

        mHertIndex = 1;
        UpdateRuntimeAnimatorController();
    }


    public override void OnTransitionStateEnter()
    {
        animatorController.Play("Fly");
        // 进入不可选取状态
        CloseCollision();
    }

    public override void OnTransitionStateExit()
    {
        // 转为轻装状态
        SetLightState();
        OpenCollision();
    }

    public override bool CanBlock(BaseUnit unit)
    {
        return !isDroping && base.CanBlock(unit);
    }

    public override void OnCastStateEnter()
    {
        animatorController.Play("Drop");
        isDroping = true;
    }

    public override void OnCastState()
    {
        if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
            SetActionState(new MoveState(this));
    }

    public override void OnCastStateExit()
    {
        isDroping = false;
    }

    /// <summary>
    /// 这个受伤贴图逻辑比较特殊，直接重写置空，不走常规逻辑否则可能报错
    /// </summary>
    public override void UpdateHertMap()
    {

    }
}
