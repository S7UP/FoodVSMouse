using System;

using UnityEngine;
/// <summary>
/// 魔术师鼠
/// </summary>
public class MagicianMouse : MouseUnit
{
    private static RuntimeAnimatorController Retinue_RuntimeAnimatorController; // 随从的
    private float distance; // 投掷的水平距离
    private float minCanAttackPosX; // 可以攻击的x最小坐标（即小于某个X后就不会在攻击了）


    public override void Awake()
    {
        if (Retinue_RuntimeAnimatorController == null)
            Retinue_RuntimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Mouse/44/Retinue");
        base.Awake();
    }

    public override void MInit()
    {
        distance = 4 * MapManager.gridWidth;
        minCanAttackPosX = MapManager.GetColumnX(1) + distance; // 这个算法表明最远可以打到左二列
        base.MInit();
    }

    public override void OnIdleState()
    {
        // 没目标了就走了
        if (!IsMeetGeneralAttackCondition())
            SetActionState(new MoveState(this));
    }

    /// <summary>
    /// 是否满足普通攻击的条件
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetGeneralAttackCondition()
    {
        // 没小于某个x坐标就能攻击了
        return transform.position.x > minCanAttackPosX;
    }

    /// <summary>
    /// 执行具体的攻击，位于伤害判定为真之后
    /// </summary>
    public override void ExecuteDamage()
    {
        // 生成一个小老鼠
        MouseModel m = MouseModel.GetInstance(Retinue_RuntimeAnimatorController);
        m.SetBaseAttribute(100, 10, 2.0f, 1.0f, 0, 0.5f, 0);
        m.transform.position = transform.position;
        m.currentYIndex = MapManager.GetYIndex(m.transform.position.y);
        m.SetActionState(new MoveState(m));
        GameController.Instance.AddMouseUnit(m);

        // 然后把它扔出去
        // 添加一个弹起的任务
        CustomizationTask t = TaskManager.AddParabolaTask(m, TransManager.TranToVelocity(18), 1.2f, m.transform.position, m.transform.position + distance * (Vector3)moveRotate, false);
        m.SetActionState(new TransitionState(m));
        m.animatorController.Play("Fly", true);
        Action oldExitFunc = t.OnExitFunc;
        t.OnExitFunc = delegate
        {
            oldExitFunc();
            m.SetActionState(new MoveState(m));
            // 但是落地后晕眩2s
            m.AddNoCountUniqueStatusAbility(StringManager.Stun, new StunStatusAbility(m, 120, false));
        };
    }


    public override void AfterGeneralAttack()
    {
        mAttackFlag = true;
        SetActionState(new MoveState(this));
    }
}
