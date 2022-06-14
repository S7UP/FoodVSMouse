using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Ͷ����ը��
/// </summary>
public class ThrowIceBombSkillAbility : SkillAbility
{
    public bool isThrow; // �ܷ��ٴ��ͷŵ�flag������Ϊtrueʱ�ü����޷��ٴ�ʩ��
    private bool canSkill; // �Ƿ����ʩ�ţ��ɸü��ܳ����߾���
    private bool canThrowEntity; // �Ƿ����Ͷ����ʵ�壬�ɸü��ܳ����߾���
    private Vector3 targetPosition;
    private bool canClose; // �Ƿ���Ҫ�رգ��ɸü��ܳ����߾���

    public ThrowIceBombSkillAbility(BaseUnit pmaster) : base(pmaster)
    {

    }

    public ThrowIceBombSkillAbility(BaseUnit pmaster, SkillAbilityInfo info) : base(pmaster, info)
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
            IceBombBullet iceBullet = GameController.Instance.CreateBullet(master, master.transform.position, Vector2.right, BulletStyle.IceBomb) as IceBombBullet;
            iceBullet.SetAttribute(24.0f, true, 1.5f, iceBullet.transform.position, targetPosition, master.GetRowIndex());
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
