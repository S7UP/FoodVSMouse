
using UnityEngine;
/// <summary>
/// 投掷小老鼠
/// </summary>
public class ThrowLittleMouseSkillAbility : SkillAbility
{
    public bool isThrow; // 能否再次释放的flag，当置为true时该技能无法再次施放
    private bool canSkill; // 是否可以施放，由该技能持有者决定
    private bool canThrowEntity; // 是否可以投掷出实体，由该技能持有者决定
    private Vector3 targetPosition;
    private bool canClose; // 是否需要关闭，由该技能持有者决定

    public ThrowLittleMouseSkillAbility(BaseUnit pmaster) : base(pmaster)
    {

    }

    public ThrowLittleMouseSkillAbility(BaseUnit pmaster, SkillAbilityInfo info) : base(pmaster, info)
    {

    }

    /// <summary>
    /// 设置技能为可施放
    /// </summary>
    public void SetSkillConditionEnable()
    {
        canSkill = true;
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
        if (canThrowEntity)
        {
            // 投掷出实体
            canThrowEntity = false;
            // 
            PandaRetinueMouse m = GameController.Instance.CreateMouseUnit(master.GetRowIndex(), new BaseEnemyGroup.EnemyInfo() { type = 24, shape = master.mShape }).GetComponent<PandaRetinueMouse>();
            m.SetMaxHpAndCurrentHp(m.mMaxHp);
            m.transform.position = new Vector3(master.transform.position.x, targetPosition.y, master.transform.position.z);
            m.SetActionState(new TransitionState(m));
            m.SetMoveRoate(new Vector2(Mathf.Sign(targetPosition.x - master.transform.position.x), 0));
            m.transform.localScale = new Vector2(-m.moveRotate.x, 1);
            float dist = Mathf.Abs(targetPosition.x - master.transform.position.x);

            // 添加一个弹起的任务
            CustomizationTask t = TaskManager.GetParabolaTask(m, dist/60, dist/2, m.transform.position, targetPosition, false);
            // 且禁止移动
            t.AddOnEnterAction(delegate {
                m.DisableMove(true);
            });
            t.AddTimeTaskFunc(15); // 落地后还需要等待15s才会执行OnExit，同时获得阻挡判定，以防止在岩浆上偷卡
            t.AddOnExitAction(delegate {
                m.DisableMove(false);
                // 但是落地后晕眩2s
                m.AddNoCountUniqueStatusAbility(StringManager.Stun, new StunStatusAbility(m, 120, false));
            });
            m.AddTask(t);
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
}
