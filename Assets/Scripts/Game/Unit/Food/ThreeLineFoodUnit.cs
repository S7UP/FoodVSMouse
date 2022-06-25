using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 三线酒架具体实现
/// </summary>
public class ThreeLineFoodUnit : FoodUnit
{
    public override void MInit()
    {
        base.MInit();
        SetLevel(12);
    }

    /// <summary>
    /// 根据等级表和等级来更新对应数据
    /// </summary>
    public override void UpdateAttributeByLevel()
    {
        NumericBox.Attack.SetBase((float)(attr.baseAttrbute.baseAttack + attr.valueList[mLevel]));
    }

    /// <summary>
    /// 判断是否有有效的攻击目标
    /// </summary>
    /// <returns></returns>
    protected override bool IsHasTarget()
    {
        List<BaseUnit>[] list = GameController.Instance.GetEnemyList();
        int start = Mathf.Max(0, GetRowIndex() - 1);
        int end = Mathf.Min(GetRowIndex() + 1, MapController.yRow - 1);
        for (int i = start; i <= end; i++)
        {
            if (list[i].Count > 0)
                return true;
        }
        return false;
    }

    /// <summary>
    /// 是否满足普通攻击的条件
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetGeneralAttackCondition()
    {
        // 发现目标即可
        return IsHasTarget();
    }

    /// <summary>
    /// 进入普通攻击动画状态
    /// </summary>
    public override void BeforeGeneralAttack()
    {
        // 切换为攻击动画贴图
        SetActionState(new AttackState(this));
    }

    /// <summary>
    /// 普通攻击期间
    /// </summary>
    public override void OnGeneralAttack()
    {
        // 切换时的第一帧直接不执行update()，因为下述的info.normalizedTime的值还停留在上一个状态，逻辑会出问题！
        if (currentStateTimer <= 0)
        {
            return;
        }
        // 伤害判定帧应当执行判定
        if (IsDamageJudgment())
        {
            mAttackFlag = false;
            ExecuteDamage();
        }
    }

    /// <summary>
    /// 退出普通攻击的条件
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetEndGeneralAttackCondition()
    {
        return animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f; // 攻击动画播放完整一次后视为技能结束
        //return AnimatorManager.GetCurrentFrame(animator) == AnimatorManager.GetTotalFrame(animator);
    }

    /// <summary>
    /// 退出普通攻击后要做的事
    /// </summary>
    public override void AfterGeneralAttack()
    {
        mAttackFlag = true;
        SetActionState(new IdleState(this));
    }

    /// <summary>
    /// 是否为伤害判定时刻（近战攻击为打出实际伤害，远程攻击为确定发射弹体）
    /// </summary>
    /// <returns></returns>
    public override bool IsDamageJudgment()
    {
        AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
        return (info.normalizedTime - Mathf.FloorToInt(info.normalizedTime) >= attackPercent && mAttackFlag);
    }

    /// <summary>
    /// 执行具体的攻击，位于伤害判定为真之后
    /// </summary>
    public override void ExecuteDamage()
    {
        for (int i = -1; i <= 1; i++)
        {
            for (int j = 0; j < 2; j++)
            {
                BaseBullet b = GameController.Instance.CreateBullet(this, transform.position, Vector2.right, BulletStyle.Wine);
                b.SetDamage(mCurrentAttack);
                b.SetStandardVelocity(24.0f);
                // 添加一个纵向位移的任务
                GameController.Instance.AddTasker(new StraightMovePresetTasker(b, MapManager.gridHeight / 30, 0, Vector3.up * i, MapManager.gridHeight));
                // 横向位移
                GameController.Instance.AddTasker(new StraightMovePresetTasker(b, MapManager.gridWidth / 30 * j, 0, Vector3.right, 60));
            }
        }
    }
}
