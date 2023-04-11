using System.Collections.Generic;

using S7P.Numeric;

using UnityEngine;
/// <summary>
/// 机械投弹鼠
/// </summary>
public class AerialBombardmentMouse : MouseUnit, IFlyUnit
{
    private GeneralAttackSkillAbility generalAttackSkillAbility;
    private FlyThrowBombSkillAbility flyThrowBombSkillAbility;

    private bool isDrop; // 是否被击落

    public override void MInit()
    {
        base.MInit();
        isDrop = false;
        // 防爆
        NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreBombInstantKill, new BoolModifier(true));
    }

    public override void MUpdate()
    {
        base.MUpdate();
    }

    /// <summary>
    /// 加载技能
    /// </summary>
    public override void LoadSkillAbility()
    {
        List<SkillAbility.SkillAbilityInfo> infoList = AbilityManager.Instance.GetSkillAbilityInfoList(mUnitType, mType, mShape);
        // 普通攻击
        if (infoList.Count > 0)
        {
            generalAttackSkillAbility = new GeneralAttackSkillAbility(this, infoList[0]);
            skillAbilityManager.AddSkillAbility(generalAttackSkillAbility);
        }

        // 投炸弹技能
        if (infoList.Count > 1)
        {
            flyThrowBombSkillAbility = new FlyThrowBombSkillAbility(this, infoList[1]);
            skillAbilityManager.AddSkillAbility(flyThrowBombSkillAbility);
        }
    }

    /// <summary>
    /// 碰撞事件
    /// </summary>
    /// <param name="collision"></param>
    public void CheckUnit(Collider2D collision)
    {
        // 技能CD、技能持续间、掉落和死亡动画时不接受任何碰撞事件
        if (isDrop || isDeathState || flyThrowBombSkillAbility.isSpelling || !flyThrowBombSkillAbility.IsEnergyEnough())
        {
            return;
        }

        if(collision.tag.Equals("Food"))
        {
            // 检测到单位碰撞了！
            BaseUnit unit = collision.GetComponent<BaseUnit>();
            // 检测本格美食最高受击优先级单位
            BaseUnit target = unit.GetGrid().GetHighestAttackPriorityUnit(this);
            if (target != null)
            {
                // 标记技能发现目标了，可以执行投弹操作
                flyThrowBombSkillAbility.SetSkillConditionEnable(target.transform.position.x);
            }
        }
    }

    public override void OnTriggerEnter2D(Collider2D collision)
    {
        base.OnTriggerEnter2D(collision);
        CheckUnit(collision);
    }

    public override void OnTriggerStay2D(Collider2D collision)
    {
        base.OnTriggerStay2D(collision);
        CheckUnit(collision);
    }

    /// <summary>
    /// 执行降落，仅一次
    /// </summary>
    public void ExecuteDrop()
    {
        if (!isDrop)
        {
            isDrop = true;
            // 若当前生命值大于飞行状态临界点，则需要强制同步生命值至临界点
            if(mCurrentHp> mHertRateList[0] * mMaxHp)
            {
                mCurrentHp = (float)mHertRateList[0]*mMaxHp;
            }
            mHertRateList[0] = double.MaxValue;
            UpdateHertMap(); // 强制更新一次贴图
            // 如果此时在投弹，那么应当立即取消投弹状态
            if (flyThrowBombSkillAbility.isSpelling)
            {
                flyThrowBombSkillAbility.EndActivate();
            }
            // 设为转场状态，该状态下的具体实下如下几个方法
            SetActionState(new TransitionState(this));
        }
    }


    /// <summary>
    /// 当处于下落状态时，应当完全不被子弹击中
    /// </summary>
    /// <param name="bullet"></param>
    /// <returns></returns>
    public override bool CanHit(BaseBullet bullet)
    {
        return !(mCurrentActionState is TransitionState) && base.CanHit(bullet);
    }

    public override bool CanBeSelectedAsTarget(BaseUnit otherUnit)
    {
        return !(mCurrentActionState is TransitionState) && base.CanBeSelectedAsTarget(otherUnit);
    }

    /// <summary>
    /// 当贴图更新时要做的事
    /// </summary>
    public override void OnUpdateRuntimeAnimatorController()
    {
        // 当第一次切换到非第0阶段的贴图时（即退出飞行状态），将第0阶段的血量百分比设为超过1.0（即这之后永远达不到），然后播放坠落动画
        // 当前取值范围为1~3时触发
        // 0 飞行
        // 1 击落过程->正常移动
        // 2 受伤移动
        if (mHertIndex > 0 && mHertIndex <= 2 && !isDrop)
        {
            ExecuteDrop();
        }
    }

    public override void OnCastStateEnter()
    {
        animatorController.Play("Cast");
    }

    public override void OnCastState()
    {
        if (currentStateTimer <= 0)
        {
            return;
        }
        // 动画播放完一次后，通知技能效果结束
        //if (AnimatorManager.GetCurrentFrame(animator) == AnimatorManager.GetTotalFrame(animator))
        if(animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
        {
            flyThrowBombSkillAbility.CloseSkill();
        }
    }

    /// <summary>
    /// 进入转场状态时要做的事，这里特指进入刚被击落时要做的
    /// </summary>
    public override void OnTransitionStateEnter()
    {
        animatorController.Play("Drop");
    }

    public override void OnTransitionState()
    {
        // 动画播放完一次后，转为移动状态
        if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
        {
            SetActionState(new MoveState(this));
        }
    }

    public override void OnTransitionStateExit()
    {
        mHeight = 0; // 高度降低为地面高度
    }
}
