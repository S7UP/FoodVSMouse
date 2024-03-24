
using UnityEngine;
/// <summary>
/// Ͷ��С����
/// </summary>
public class ThrowLittleMouseSkillAbility : SkillAbility
{
    public bool isThrow; // �ܷ��ٴ��ͷŵ�flag������Ϊtrueʱ�ü����޷��ٴ�ʩ��
    private bool canSkill; // �Ƿ����ʩ�ţ��ɸü��ܳ����߾���
    private bool canThrowEntity; // �Ƿ����Ͷ����ʵ�壬�ɸü��ܳ����߾���
    private Vector3 targetPosition;
    private bool canClose; // �Ƿ���Ҫ�رգ��ɸü��ܳ����߾���

    public ThrowLittleMouseSkillAbility(BaseUnit pmaster) : base(pmaster)
    {

    }

    public ThrowLittleMouseSkillAbility(BaseUnit pmaster, SkillAbilityInfo info) : base(pmaster, info)
    {

    }

    /// <summary>
    /// ���ü���Ϊ��ʩ��
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
    /// �رռ���
    /// </summary>
    public void CloseSkill()
    {
        canClose = true;
    }

    /// <summary>
    /// �����ͷŵ�����
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetSkillCondition()
    {
        // �����㹻����Ͷ����������ʩ��
        return (!isThrow && canSkill);
    }

    public override void BeforeSpell()
    {
        isThrow = true;
        // ͣ��������״̬��������
        master.SetActionState(new CastState(master));
    }

    /// <summary>
    /// �ڼ����ڼ�ľ���ʵ��
    /// </summary>
    public override void OnSpelling()
    {
        if (canThrowEntity)
        {
            // Ͷ����ʵ��
            canThrowEntity = false;
            // 
            PandaRetinueMouse m = GameController.Instance.CreateMouseUnit(master.GetRowIndex(), new BaseEnemyGroup.EnemyInfo() { type = 24, shape = master.mShape }).GetComponent<PandaRetinueMouse>();
            m.SetMaxHpAndCurrentHp(m.mMaxHp);
            m.transform.position = new Vector3(master.transform.position.x, targetPosition.y, master.transform.position.z);
            m.SetActionState(new TransitionState(m));
            m.SetMoveRoate(new Vector2(Mathf.Sign(targetPosition.x - master.transform.position.x), 0));
            m.transform.localScale = new Vector2(-m.moveRotate.x, 1);
            float dist = Mathf.Abs(targetPosition.x - master.transform.position.x);

            // ���һ�����������
            CustomizationTask t = TaskManager.GetParabolaTask(m, dist/60, dist/2, m.transform.position, targetPosition, false);
            // �ҽ�ֹ�ƶ�
            t.AddOnEnterAction(delegate {
                m.DisableMove(true);
            });
            t.AddTimeTaskFunc(15); // ��غ���Ҫ�ȴ�15s�Ż�ִ��OnExit��ͬʱ����赲�ж����Է�ֹ���ҽ���͵��
            t.AddOnExitAction(delegate {
                m.DisableMove(false);
                // ������غ���ѣ2s
                m.AddNoCountUniqueStatusAbility(StringManager.Stun, new StunStatusAbility(m, 120, false));
            });
            m.AddTask(t);
        }
    }

    /// <summary>
    /// �ڷǼ����ڼ�
    /// </summary>
    public override void OnNoSpelling()
    {

    }

    /// <summary>
    /// �ڼ����ڼ䣬������������Ҫ������
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
