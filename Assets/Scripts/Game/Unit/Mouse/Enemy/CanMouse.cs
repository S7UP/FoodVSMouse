using S7P.Numeric;

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
    private FloatModifier burnRateMod = new FloatModifier(0);
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
    public override float OnBombBurnDamage(float dmg)
    {
        float maxDist = transform.position.x - MapManager.GetColumnX(0);

        // 处于重装状态但不能是飞行动画期间
        if (isReinstallState && (!NumericBox.IntDict.ContainsKey(StringManager.Flying) || NumericBox.IntDict[StringManager.Flying].Value <= 0) && maxDist > 0)
        {
            float dist = Mathf.Min(maxDist, 3 * MapManager.gridWidth);
            CustomizationTask t = TaskManager.GetParabolaTask(this, dist / 60, dist / 2, transform.position, transform.position + (Vector3)moveRotate * dist, false);
            t.AddOnEnterAction(delegate {
                // 起飞咯！！！！
                DisableMove(true);
                // SetActionState(new TransitionState(this));
                animatorController.Play("Fly");
            });
            t.AddOnExitAction(delegate
            {
                DisableMove(false);
                // SetActionState(new CastState(this));
                // 转为轻装状态
                SetLightState();
                // 仅仅是晕眩动画
                taskController.AddTask(GetStunTask());
            });
            //AddUniqueTask("CanMouseFly", t);
            taskController.AddTask(t);
        }
        // 原先的伤害执行
        return base.OnBombBurnDamage(dmg);
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
        // 免疫减速与定身
        NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreSlowDown, IgnoreSlowDown);
        NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreStun, IgnoreStun);
        // 100%灰烬抗性
        NumericBox.BurnRate.AddModifier(burnRateMod);
        mHertIndex = 0;
        OnHertStageChanged();
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
        NumericBox.RemoveDecideModifierToBoolDict(StringManager.IgnoreSlowDown, IgnoreSlowDown);
        NumericBox.RemoveDecideModifierToBoolDict(StringManager.IgnoreStun, IgnoreStun);
        NumericBox.BurnRate.RemoveModifier(burnRateMod);

        // 获取加速效果
        NumericBox.MoveSpeed.AddPctAddModifier(moveSpeedModifier2);
        // 获取攻速加成效果
        NumericBox.AttackSpeed.AddPctAddModifier(attackSpeedModifier);

        mHertIndex = 1;
        OnHertStageChanged();
    }

    private CustomizationTask GetStunTask()
    {
        CustomizationTask t = new CustomizationTask();
        Func<BaseUnit, BaseUnit, bool> canBlockFunc = delegate { return false; };
        
        t.AddOnEnterAction(delegate
        {
            animatorController.Play("Drop");
            AddCanBlockFunc(canBlockFunc);
            DisableMove(true);
        });
        t.AddTaskFunc(delegate
        {
            return animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce();
        });
        t.AddOnExitAction(delegate
        {
            RemoveCanBlockFunc(canBlockFunc);
            DisableMove(false);
            animatorController.Play("Move");
        });
        return t;
    }

    /// <summary>
    /// 这个受伤贴图逻辑比较特殊，直接重写置空，不走常规逻辑否则可能报错
    /// </summary>
    public override void UpdateHertMap()
    {

    }
}
