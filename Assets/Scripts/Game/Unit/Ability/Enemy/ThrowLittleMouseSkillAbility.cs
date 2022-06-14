using System.Collections;
using System.Collections.Generic;
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
            //IceBombBullet iceBullet = GameController.Instance.CreateBullet(master, master.transform.position, Vector2.right, BulletStyle.IceBomb) as IceBombBullet;
            //iceBullet.SetAttribute(24.0f, true, 1.5f, iceBullet.transform.position, targetPosition, master.GetRowIndex());

            //BombBullet bombBullet = GameController.Instance.CreateBullet(master, master.transform.position, Vector2.right, BulletStyle.Bomb) as BombBullet;
            //bombBullet.SetAttribute(24.0f, true, 1.5f, bombBullet.transform.position, targetPosition, master.GetRowIndex());

            //
            PandaRetinueMouse m = GameController.Instance.CreateMouseUnit(master.GetRowIndex(), new BaseEnemyGroup.EnemyInfo() { type = 24, shape = master.mShape }).GetComponent<PandaRetinueMouse>();
            //PandaMouse m = GameController.Instance.CreateMouseUnit(master.GetRowIndex(), new BaseEnemyGroup.EnemyInfo() { type = 19, shape = master.mShape }).GetComponent<PandaMouse>();
            m.transform.position = new Vector3(master.transform.position.x, targetPosition.y, master.transform.position.z);
            //m.transform.position = master.transform.position;
            // ���л���ʱ���ж�
            m.CloseCollision();
            m.PlayFlyClip();
            // ʹ���������ƶ�״̬
            ParabolaMoveState s = new ParabolaMoveState(m, 24.0f, 1.2f, m.transform.position, targetPosition, false);
            s.SetExitAction(delegate {
                m.OpenCollision();
            });
            m.SetActionState(s);
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
