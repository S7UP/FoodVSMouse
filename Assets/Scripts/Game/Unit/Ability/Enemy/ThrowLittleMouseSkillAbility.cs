using System;

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
            m.transform.position = new Vector3(master.transform.position.x, targetPosition.y, master.transform.position.z);
            m.SetActionState(new TransitionState(m));
            // ���һ���������������������л�Ϊ����״̬
            Tasker t = GameController.Instance.AddTasker(new ParabolaMovePresetTasker(m, 24.0f, 1.2f, m.transform.position, targetPosition, false));
            //m.CloseCollision();
            // ��Ծ�ڼ䲻�ɱ��赲Ҳ���ܱ������ӵ�����
            Func<BaseUnit, BaseUnit, bool> noBlockFunc = delegate { return false; };
            Func<BaseUnit, BaseBullet, bool> noHitFunc = delegate { return false; };
            m.AddCanBlockFunc(noBlockFunc);
            m.AddCanHitFunc(noHitFunc);


            m.DisableMove(true); // ��ʱ�����ƶ�
            t.AddOtherEndEvent(delegate 
            {
                m.RemoveCanBlockFunc(noBlockFunc);
                m.RemoveCanHitFunc(noHitFunc);
                //m.OpenCollision();
                m.DisableMove(false); });
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
