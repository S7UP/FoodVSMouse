using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// 菠萝爆炸面包
/// </summary>
public class PineappleBreadBoom : FoodUnit
{
    private int mHertIndex; // 受伤阶段 0：正常 1：小伤 2：重伤
    private List<float> mHertRateList = new List<float>()
    {
        0.67f, 0.33f
    };
    private float pctAddValue; // 增加伤害百分比

    public override void MInit()
    {
        mHertIndex = 0;
        pctAddValue = 0;
        base.MInit();

        // 在受到伤害结算之后，更新受伤贴图状态
        AddActionPointListener(ActionPointType.PostReceiveDamage, delegate { UpdateHertMap(); AddPctAttackWhenHited(); });
        // 在接收治疗结算之后，更新受伤贴图状态
        AddActionPointListener(ActionPointType.PostReceiveCure, delegate { UpdateHertMap(); });
    }



    public override void OnIdleStateEnter()
    {
        animatorController.Play("Idle" + mHertIndex);
    }

    public override void OnDieStateEnter()
    {
        animatorController.Play("Die");
    }

    /// <summary>
    /// 你游第一个有死亡动画的美食（泪目）
    /// </summary>
    public override void DuringDeath()
    {
        if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
            DeathEvent();
    }

    /// <summary>
    /// 产生爆炸效果
    /// </summary>
    public override void AfterDeath()
    {
        ExecuteBoom();
    }

    ////////////////////////////////////////////////////////////////////////以下是私有方法/////////////////////////////////////////////////////////////


    /// <summary>
    /// 当受伤或者被治疗时，更新单位贴图状态
    /// </summary>
    private void UpdateHertMap()
    {
        // 要是死了的话就免了吧
        if (isDeathState)
            return;

        // 是否要切换控制器的flag
        bool flag = false;
        // 恢复到上一个受伤贴图检测
        while (mHertIndex > 0 && GetHeathPercent() > mHertRateList[mHertIndex - 1])
        {
            mHertIndex--;
            flag = true;
        }
        // 下一个受伤贴图的检测
        while (mHertIndex < mHertRateList.Count && GetHeathPercent() <= mHertRateList[mHertIndex])
        {
            mHertIndex++;
            flag = true;
        }
        // 有切换通知时才切换
        if (flag)
        {
            animatorController.Play("Idle" + mHertIndex);
        }
    }

    /// <summary>
    /// 挨打会增加最终爆破伤害加成
    /// </summary>
    private void AddPctAttackWhenHited()
    {
        pctAddValue += 10;
    }

    /// <summary>
    /// 爆炸咯！
    /// </summary>
    private void ExecuteBoom()
    {
        // 如果不是被打死的（比如铲子移除），则不会有爆破伤害加成
        // 而二转则会把爆破伤害加成降低至原来的50%而非没有
        if (mCurrentHp > 0)
        {
            if (mShape < 2)
                pctAddValue = 0;
            else
                pctAddValue /= 2;
        }

        // 原地产生一个爆炸特效
        {
            BaseEffect e = BaseEffect.GetInstance("BoomEffect");
            e.animator.runtimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Food/" + mType + "/BoomEffect");
            e.transform.position = transform.position;
            e.MInit();
            GameController.Instance.AddEffect(e);
        }
        // 原地产生一个爆炸伤害判定效果
        {
            BombAreaEffectExecution bombEffect = BombAreaEffectExecution.GetInstance();
            Debug.Log("当前爆炸可造成伤害：" + mCurrentAttack * (1 + pctAddValue / 100));
            bombEffect.Init(this, mCurrentAttack*(1 + pctAddValue/100), GetRowIndex(), 4, 3, -0.5f, 0, false, true);
            bombEffect.transform.position = this.GetPosition();
            GameController.Instance.AddAreaEffectExecution(bombEffect);
        }
    }
}
