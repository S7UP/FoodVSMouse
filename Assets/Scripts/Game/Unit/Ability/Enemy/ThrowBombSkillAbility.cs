using UnityEngine;
/// <summary>
/// 投掷冰炸弹
/// </summary>
public class ThrowBombSkillAbility : SkillAbility
{
    public bool isThrow; // 能否再次释放的flag，当置为true时该技能无法再次施放
    private bool canSkill; // 是否可以施放，由该技能持有者决定
    private bool canThrowEntity; // 是否可以投掷出实体，由该技能持有者决定
    private bool isFinshPreCast; // 是否完成预警动作
    private Vector3 targetPosition;
    private bool canClose; // 是否需要关闭，由该技能持有者决定

    public ThrowBombSkillAbility(BaseUnit pmaster) : base(pmaster)
    {

    }

    public ThrowBombSkillAbility(BaseUnit pmaster, SkillAbilityInfo info) : base(pmaster, info)
    {

    }

    /// <summary>
    /// 设置技能为可施放
    /// </summary>
    public void SetSkillConditionEnable()
    {
        canSkill = true;
        FullCurrentEnergy(); // 将能量填装满
    }

    public void ThrowEntity(Vector3 targetPosition)
    {
        this.targetPosition = targetPosition;
        canThrowEntity = true;
    }

    /// <summary>
    /// 关闭技能
    /// </summary>
    public void CloseSkill()
    {
        canClose = true;
    }

    /// <summary>
    /// 满足释放的条件
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetSkillCondition()
    {
        // 能量足够，非投掷过，允许施放
        return (!isThrow && canSkill);
    }

    public override void BeforeSpell()
    {
        isThrow = true;
        // 停下来，切状态，播动画
        master.SetActionState(new CastState(master));
    }

    /// <summary>
    /// 在技能期间的具体实现
    /// </summary>
    public override void OnSpelling()
    {
        // 先判断是否完成预警动作
        if (isFinshPreCast)
        {
            // 完成预警动作后等待指令
            if (canThrowEntity)
            {
                // 投掷出实体
                canThrowEntity = false;
                BombBullet bombBullet = GameController.Instance.CreateBullet(master, master.transform.position, Vector2.right, BulletStyle.Bomb) as BombBullet;
                //bombBullet.SetHitSoundEffect("Bomb");
                bombBullet.AddHitAction(delegate {
                    GameManager.Instance.audioSourceController.PlayEffectMusic("Boom");
                });
                bombBullet.SetAttribute(TransManager.TranToStandardVelocity((bombBullet.transform.position - targetPosition).magnitude / 90f), true, 1.5f, bombBullet.transform.position, targetPosition, master.GetRowIndex());
            }
        }
        else
        {
            // 技能期间采用的是能量倒扣机制代表读条时间，读条结束后表明完成预警动作，并切换成投掷炸弹动作
            if (currentEnergy <= 0)
            {
                isFinshPreCast = true;
                // 再来一次动作切换
                master.SetActionState(new CastState(master));
            }
        }
    }

    /// <summary>
    /// 在非技能期间
    /// </summary>
    public override void OnNoSpelling()
    {

    }

    /// <summary>
    /// 在技能期间，结束技能所需要的条件
    /// </summary>
    public override bool IsMeetCloseSpellingCondition()
    {
        return canClose;
    }

    public override void AfterSpell()
    {
        master.SetActionState(new MoveState(master));
    }

    /// <summary>
    /// 由外部调用，是否完成预警动作
    /// </summary>
    /// <returns></returns>
    public bool IsFinishPreCast()
    {
        return isFinshPreCast;
    }
}
