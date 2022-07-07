using System.Collections.Generic;
/// <summary>
/// 魔镜鼠
/// </summary>
public class MagicMirrorMouse : MouseUnit
{
    private GeneralAttackSkillAbility generalAttackSkillAbility;
    private StealFireEnergySkillAbiliby stealFireEnergySkillAbiliby;
    private bool isFirstAttack;
    private int castState; // 施法状态： 0前摇 1施法中 2后摇

    public override void MInit()
    {
        base.MInit();
        isFirstAttack = true;
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

        // 偷窃光能技能
        if (infoList.Count > 1)
        {
            stealFireEnergySkillAbiliby = new StealFireEnergySkillAbiliby(this, infoList[1]);
            skillAbilityManager.AddSkillAbility(stealFireEnergySkillAbiliby);
        }
    }

    /// <summary>
    /// 第一次进入攻击状态时，立即恢复满技力，打断本次攻击以触发技能
    /// </summary>
    public override void OnAttackStateEnter()
    {
        if (isFirstAttack)
        {
            isFirstAttack = false;
            // 打断当前攻击
            generalAttackSkillAbility.EndActivate();
            // 恢复满技力
            stealFireEnergySkillAbiliby.FullCurrentEnergy();
        }
        else
        {
            base.OnAttackStateEnter();
        }
    }

    /// <summary>
    /// 进入技能
    /// </summary>
    public override void OnCastStateEnter()
    {
        isFirstAttack = false; // 进入技能第一次攻击恢复满技力的效果失效
        animatorController.Play("PreCast");
    }

    public override void OnCastState()
    {
        if (currentStateTimer == 0)
            return;

        // 每当动画播放完时，检查技能施放的状态来判断当前处于什么阶段： 施法前摇 施法中 施法后摇
        if (castState==1) // 在施术期间，等待施法结束
        {
            if (stealFireEnergySkillAbiliby.IsEndCasting())
            {
                castState = 2;
                animatorController.Play("PostCast");
                currentStateTimer = 0;
                stealFireEnergySkillAbiliby.EndCasting();
            }
        }
        else if (AnimatorManager.GetNormalizedTime(animator) > 1.0)
        {
            if (castState == 0)
            {
                // 前摇结束
                stealFireEnergySkillAbiliby.StartCasting(); // 开始施术
                animatorController.Play("Cast", true);
                currentStateTimer = 0;
                castState = 1;
            }else if(castState == 2)
            {
                // 后摇结束
                castState = 0;
                stealFireEnergySkillAbiliby.EndActivate();
            }
        }
    }
}
