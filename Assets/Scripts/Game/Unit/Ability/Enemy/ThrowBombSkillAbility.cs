using UnityEngine;
/// <summary>
/// Ͷ����ը��
/// </summary>
public class ThrowBombSkillAbility : SkillAbility
{
    public bool isThrow; // �ܷ��ٴ��ͷŵ�flag������Ϊtrueʱ�ü����޷��ٴ�ʩ��
    private bool canSkill; // �Ƿ����ʩ�ţ��ɸü��ܳ����߾���
    private bool canThrowEntity; // �Ƿ����Ͷ����ʵ�壬�ɸü��ܳ����߾���
    private bool isFinshPreCast; // �Ƿ����Ԥ������
    private Vector3 targetPosition;
    private bool canClose; // �Ƿ���Ҫ�رգ��ɸü��ܳ����߾���

    public ThrowBombSkillAbility(BaseUnit pmaster) : base(pmaster)
    {

    }

    public ThrowBombSkillAbility(BaseUnit pmaster, SkillAbilityInfo info) : base(pmaster, info)
    {

    }

    /// <summary>
    /// ���ü���Ϊ��ʩ��
    /// </summary>
    public void SetSkillConditionEnable()
    {
        canSkill = true;
        FullCurrentEnergy(); // ��������װ��
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
        // ���ж��Ƿ����Ԥ������
        if (isFinshPreCast)
        {
            // ���Ԥ��������ȴ�ָ��
            if (canThrowEntity)
            {
                // Ͷ����ʵ��
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
            // �����ڼ���õ����������ۻ��ƴ������ʱ�䣬����������������Ԥ�����������л���Ͷ��ը������
            if (currentEnergy <= 0)
            {
                isFinshPreCast = true;
                // ����һ�ζ����л�
                master.SetActionState(new CastState(master));
            }
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

    /// <summary>
    /// ���ⲿ���ã��Ƿ����Ԥ������
    /// </summary>
    /// <returns></returns>
    public bool IsFinishPreCast()
    {
        return isFinshPreCast;
    }
}
