using UnityEngine;
/// <summary>
/// 罐头鼠
/// </summary>
public class CanMouse : MouseUnit
{
    private FloatModifier defenseModifier = new FloatModifier(0.75f); // 减伤修饰
    private FloatModifier moveSpeedModifier1 = new FloatModifier(-50); // 减速修饰
    private FloatModifier moveSpeedModifier2 = new FloatModifier(300); // 加速修饰
    private FloatModifier attackSpeedModifier = new FloatModifier(100); // 加攻速修饰
    private BoolModifier IgnoreBombInstantKill = new BoolModifier(true);
    private BoolModifier IgnoreSlowDown = new BoolModifier(true);
    private BoolModifier IgnoreFrozen = new BoolModifier(true);

    private bool isReinstallState; // 是否为重装态

    public override void MInit()
    {
        base.MInit();
        isReinstallState = false; // 只是初始化，不用担心，而且不这么做后面的方法会失效
        // 初始态为重装态
        SetReinstallState();
    }

    /// <summary>
    /// 受到灰烬伤害时
    /// </summary>
    /// <param name="dmg"></param>
    public override void OnBurnDamage(float dmg)
    {
        // 原先的伤害执行
        base.OnBurnDamage(dmg);
        // 处于重装状态但不能是飞行动画期间
        if (isReinstallState && !(mCurrentActionState is TransitionState))
        {
            // 起飞咯！！！！
            SetActionState(new TransitionState(this));
            Tasker t = GameController.Instance.AddTasker(new ParabolaMovePresetTasker(this, 24.0f, 0.75f, transform.position, transform.position + (Vector3)moveRotate*3*MapManager.gridWidth, false));
            // 飞行结束时切换成丢装备且晕眩状态
            t.AddOtherEndEvent(delegate { SetActionState(new CastState(this)); });
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
        NumericBox.AttackSpeed.RemoveAddModifier(attackSpeedModifier);

        // 获取减伤
        NumericBox.Defense.AddAddModifier(defenseModifier);
        // 总移速降低效果
        NumericBox.MoveSpeed.AddFinalPctAddModifier(moveSpeedModifier1);
        // 免疫灰烬减速与定身
        NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreBombInstantKill, IgnoreBombInstantKill);
        NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreSlowDown, IgnoreSlowDown);
        NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreFrozen, IgnoreFrozen);

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
        NumericBox.RemoveDecideModifierToBoolDict(StringManager.IgnoreFrozen, IgnoreFrozen);


        // 获取加速效果
        NumericBox.MoveSpeed.AddPctAddModifier(moveSpeedModifier2);
        // 获取攻速加成效果
        NumericBox.AttackSpeed.AddAddModifier(attackSpeedModifier);

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

    public override void OnCastStateEnter()
    {
        animatorController.Play("Drop");
    }

    public override void OnCastState()
    {
        if (currentStateTimer == 0)
            return;
        if (AnimatorManager.GetNormalizedTime(animator) > 1.0)
            SetActionState(new MoveState(this));
    }

    /// <summary>
    /// 这个受伤贴图逻辑比较特殊，直接重写置空，不走常规逻辑否则可能报错
    /// </summary>
    protected override void UpdateHertMap()
    {

    }
}
