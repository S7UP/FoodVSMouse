using System.Collections;
using System.Collections.Generic;
using TMPro;

using UnityEngine;
/// <summary>
/// ����ֱ��Ͷ��ը��
/// </summary>
public class FlyThrowBombSkillAbility : SkillAbility
{
    private bool canSkill; // �Ƿ����ʩ�ţ��ɸü��ܳ����߾���
    private bool canClose; // �Ƿ���Ҫ�رգ��ɸü��ܳ����߾���

    public FlyThrowBombSkillAbility(BaseUnit pmaster) : base(pmaster)
    {

    }

    public FlyThrowBombSkillAbility(BaseUnit pmaster, SkillAbilityInfo info) : base(pmaster, info)
    {

    }

    /// <summary>
    /// ���ü���Ϊ��ʩ��
    /// </summary>
    public void SetSkillConditionEnable()
    {
        canSkill = true;
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
        return canSkill;
    }

    public override void BeforeSpell()
    {
        // ����ը��ʵ��
        FlyBombBullet flybombBullet = GameController.Instance.CreateBullet(master, master.transform.position + Vector3.left*0.225f + Vector3.up*0.3f, Vector2.down, BulletStyle.FlyBomb) as FlyBombBullet;
        flybombBullet.InitVelocity(0, 1.0f/ConfigManager.fps, master.transform.position.y, master.GetRowIndex());
        flybombBullet.UpdateRenderLayer(0); // �ٸ���һ��ͼ���
        flybombBullet.transform.right = Vector3.right;
        // ͣ��������״̬��������
        master.SetActionState(new CastState(master));
    }

    /// <summary>
    /// �ڼ����ڼ�ľ���ʵ��
    /// </summary>
    public override void OnSpelling()
    {

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
        canSkill = false;
    }
}
