using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CokeBoom : FoodUnit
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
        //NumericBox.Attack.SetBase((float)(attr.baseAttrbute.baseAttack + attr.valueList[mLevel]));
    }

    /// <summary>
    /// 判断是否有有效的攻击目标
    /// </summary>
    /// <returns></returns>
    protected override bool IsHasTarget()
    {
        // 即时型炸弹不需要
        return false;
    }

    /// <summary>
    /// 是否满足普通攻击的条件
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetGeneralAttackCondition()
    {
        // 即时型炸弹不需要
        return true;
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
        //return animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f; // 攻击动画播放完整一次后视为技能结束
        return AnimatorManager.GetCurrentFrame(animator) == AnimatorManager.GetTotalFrame(animator)-1;
    }

    /// <summary>
    /// 退出普通攻击后要做的事
    /// </summary>
    public override void AfterGeneralAttack()
    {
        // 灰烬型卡片直接销毁自身
        ExecuteDeath();
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
        // 原地产生一个爆炸效果
        GameObject instance = GameManager.Instance.GetGameObjectResource(FactoryType.GameFactory, "AreaEffect/BombAreaEffect");
        BombAreaEffectExecution bombEffect = instance.GetComponent<BombAreaEffectExecution>();
        bombEffect.Init(this, 900, GetRowIndex(), 4, 3, -0.5f, 0, false, true);
        bombEffect.transform.position = this.GetPosition();
        GameController.Instance.AddAreaEffectExecution(bombEffect);
    }
}
