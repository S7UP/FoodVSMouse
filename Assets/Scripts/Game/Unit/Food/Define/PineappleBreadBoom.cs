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
    private float dmgRecord; // 接收到的有效伤害
    private float low_burn_rate = 0; // 最低灰烬伤害
    private float hightest_burn_rate = 0; // 最高灰烬伤害

    public override void MInit()
    {
        mHertIndex = 0;
        dmgRecord = 0;
        base.MInit();

        switch (mShape)
        {
            case 1:
                low_burn_rate = 0.5f;
                hightest_burn_rate = 2.0f;
                break;
            case 2:
                low_burn_rate = 0.5f;
                hightest_burn_rate = 2.5f;
                break;
            default:
                low_burn_rate = 0;
                hightest_burn_rate = 1.5f;
                break;
        }

        // 在受到伤害结算之后，更新受伤贴图状态
        AddActionPointListener(ActionPointType.PostReceiveDamage, (action)=> { 
            UpdateHertMap(); 
            if(action is DamageAction)
            {
                DamageAction d = action as DamageAction;
                if(d.Creator != null)
                    dmgRecord += d.RealCauseValue;
            }
        });
        // 在接收治疗结算之后，更新受伤贴图状态
        AddActionPointListener(ActionPointType.PostReceiveCure, delegate { UpdateHertMap(); });
    }

    /// <summary>
    /// 炸弹掉落等效于正常死亡
    /// </summary>
    public override void BeforeDrop()
    {
        base.BeforeDeath();
    }

    public override void OnIdleStateEnter()
    {
        animatorController.Play("Idle" + mHertIndex);
    }

    public override void OnDieStateEnter()
    {
        base.OnDieStateEnter(); // 移除引用
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
    /// 爆炸咯！
    /// </summary>
    private void ExecuteBoom()
    {
        GameManager.Instance.audioSourceController.PlayEffectMusic("Boom");
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
            RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(transform.position + 0.5f*Vector3.left*MapManager.gridWidth, 4, 3, "ItemCollideEnemy");
            r.SetInstantaneous();
            r.isAffectMouse = true;
            r.SetOnEnemyEnterAction((u) => {
                BurnManager.BurnDamage(this, u, Mathf.Min(hightest_burn_rate, Mathf.Max(low_burn_rate, dmgRecord/500)));
            });
            GameController.Instance.AddAreaEffectExecution(r);
        }
    }
}
