using UnityEngine;
using System;
/// <summary>
/// 用作模型的工具鼠
/// </summary>
public class MouseModel : MouseUnit
{
    public bool canTriggerCat;
    public bool canTriggerLoseWhenEnterLoseLine;

    public override void MInit()
    {
        mType = -1;
        mShape = -1;
        canTriggerCat = true;
        canTriggerLoseWhenEnterLoseLine = true;
        base.MInit();

        spriteRenderer.sprite = null;
        animator.runtimeAnimatorController = null;
    }

    /// <summary>
    /// 加载技能，此处仅加载普通攻击，具体技能加载实现请在子类中重写
    /// </summary>
    public override void LoadSkillAbility()
    {
        skillAbilityManager.AddSkillAbility(new GeneralAttackSkillAbility(this, new SkillAbility.SkillAbilityInfo()
        {
            name = "普通攻击",
            needEnergy = 30,
            startEnergy = 0,
            energyRegeneration = 1,
            skillType = SkillAbility.Type.GeneralAttack,
            isExclusive = true,
            canActiveInDeathState = false,
            priority = 0,
        }));
    }

    public override void UpdateRuntimeAnimatorController()
    {

    }

    public override void ExecuteRecycle()
    {
        GameManager.Instance.PushGameObjectToFactory(FactoryType.GameFactory, "Mouse/MouseModel", this.gameObject);
    }

    public static MouseModel GetInstance(RuntimeAnimatorController runtimeAnimatorController)
    {
        MouseModel m = GameManager.Instance.GetGameObjectResource(FactoryType.GameFactory, "Mouse/MouseModel").GetComponent<MouseModel>();
        m.animator.runtimeAnimatorController = runtimeAnimatorController;
        m.MInit();
        m.animator.runtimeAnimatorController = runtimeAnimatorController;
        return m;
    }

    /// <summary>
    /// 有多个状态贴图时用这个
    /// </summary>
    /// <param name="runtimeAnimatorControllerArray"></param>
    /// <param name="hertRateArray"></param>
    /// <returns></returns>
    public static MouseModel GetInstance(RuntimeAnimatorController[] runtimeAnimatorControllerArray, float[] hertRateArray)
    {
        MouseModel m = GameManager.Instance.GetGameObjectResource(FactoryType.GameFactory, "Mouse/MouseModel").GetComponent<MouseModel>();
        m.animator.runtimeAnimatorController = runtimeAnimatorControllerArray[0];
        m.MInit();
        foreach (var item in hertRateArray)
        {
            m.mHertRateList.Add(item);
        }
        m.animator.runtimeAnimatorController = runtimeAnimatorControllerArray[0];
        Action<CombatAction> action = delegate
        {
            // 要是死了的话就免了吧
            if (m.isDeathState)
                return;

            // 是否要切换控制器的flag
            bool flag = false;
            // 恢复到上一个受伤贴图检测
            while (m.mHertIndex > 0 && m.GetHeathPercent() > m.mHertRateList[m.mHertIndex - 1])
            {
                m.mHertIndex--;
                flag = true;
            }
            // 下一个受伤贴图的检测
            while (m.mHertIndex < m.mHertRateList.Count && m.GetHeathPercent() <= m.mHertRateList[m.mHertIndex])
            {
                m.mHertIndex++;
                flag = true;
            }
            // 有切换通知时才切换
            if (flag)
            {
                AnimatorStateRecorder a = m.animatorController.GetCurrentAnimatorStateRecorder(); // 获取当前在播放的动画
                m.animator.runtimeAnimatorController = runtimeAnimatorControllerArray[m.mHertIndex];
                m.animatorController.ChangeAnimator(m.animator);
                // 保持当前动画播放
                if (a != null)
                {
                    m.animatorController.Play(a.aniName, a.isCycle, a.GetNormalizedTime());
                }
                m.OnUpdateRuntimeAnimatorController();
            }
        };
        m.AddActionPointListener(ActionPointType.PostReceiveDamage, action);
        m.AddActionPointListener(ActionPointType.PostReceiveReboundDamage, action);
        return m;
    }

    public override bool CanTriggerCat()
    {
        return canTriggerCat;
    }

    public override bool CanTriggerLoseWhenEnterLoseLine()
    {
        return canTriggerLoseWhenEnterLoseLine;
    }
}
