using System.Collections.Generic;

using UnityEngine;

public class V5Gun : BaseWeapons
{
    private static RuntimeAnimatorController Bullet_RuntimeAnimatorController;
    private const int maxAttackCount = 4;
    private int currentAttackCount; // ��ǰ����������
    private static List<float> attackPercentList = new List<float>();

    public override void Awake()
    {
        if (Bullet_RuntimeAnimatorController == null)
        {
            Bullet_RuntimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Weapons/2/Bullet");
            for (int i = 0; i < 4; i++)
            {
                attackPercentList.Add(0.5f + 0.4f*((float)i/3));
            }
        }
            
        base.Awake();
    }

    /// <summary>
    /// ���ؼ��ܣ��˴���������ͨ���������弼�ܼ���ʵ��������������д
    /// </summary>
    public override void LoadSkillAbility()
    {
        // ��ͨ����
        weaponsGeneralAttackSkillAbility = new WeaponsGeneralAttackSkillAbility(this, new SkillAbility.SkillAbilityInfo()
        {
            name = "��ͨ����",
            needEnergy = 57,
            startEnergy = 57,
            energyRegeneration = 1.0f,
            skillType = SkillAbility.Type.GeneralAttack,
            isExclusive = true,
            canActiveInDeathState = false,
            priority = 0
        });
        skillAbilityManager.AddSkillAbility(weaponsGeneralAttackSkillAbility);
    }
    public override void MInit()
    {
        base.MInit();
        currentAttackCount = 0;
    }

    /// <summary>
    /// �ж��Ƿ�����Ч�Ĺ���Ŀ��
    /// </summary>
    /// <returns></returns>
    public override bool IsHasTarget()
    {
        return GameController.Instance.CheckRowCanAttack(master, GetRowIndex());
    }

    /// <summary>
    /// �˳���ͨ����������
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetEndGeneralAttackCondition()
    {
        return animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce();
    }

    /// <summary>
    /// �˳���ͨ������Ҫ������
    /// </summary>
    public override void AfterGeneralAttack()
    {
        base.AfterGeneralAttack();
        currentAttackCount = 0;
    }

    /// <summary>
    /// ��ͨ�����ڼ�
    /// </summary>
    public override void OnGeneralAttack()
    {
        // �˺��ж�֡Ӧ��ִ���ж�
        if (IsDamageJudgment())
        {
            ExecuteDamage();
            currentAttackCount++;
        }
    }

    /// <summary>
    /// �Ƿ�Ϊ�˺��ж�ʱ�̣���ս����Ϊ���ʵ���˺���Զ�̹���Ϊȷ�����䵯�壩
    /// </summary>
    /// <returns></returns>
    public override bool IsDamageJudgment()
    {
        return currentAttackCount < attackPercentList.Count && animatorController.GetCurrentAnimatorStateRecorder().GetNormalizedTime() >= attackPercentList[currentAttackCount];
    }

    /// <summary>
    /// ִ�о���Ĺ�����λ���˺��ж�Ϊ��֮��
    /// </summary>
    public override void ExecuteDamage()
    {
        GameManager.Instance.audioSourceManager.PlayEffectMusic("Throw" + GameManager.Instance.rand.Next(0, 2));
        if (currentAttackCount < maxAttackCount)
        {
            AllyBullet b = AllyBullet.GetInstance(BulletStyle.NoStrengthenNormal, Bullet_RuntimeAnimatorController, master, 0);
            b.SetHitSoundEffect("Splat" + GameManager.Instance.rand.Next(0, 3));
            float ori_dmg = master.mCurrentAttack;
            b.SetStandardVelocity(36);
            b.SetRotate(Vector2.right);
            b.AddHitAction((b, u) => { 
                if(u is MouseUnit)
                {
                    MouseUnit m = u as MouseUnit;
                    float dmg = ori_dmg;
                    if (!m.IsBoss())
                    {
                        dmg += 0.03f * u.GetCurrentHp();
                    }
                    new DamageAction(CombatAction.ActionType.RealDamage, master, u, dmg).ApplyAction();
                }
            });
            GameController.Instance.AddBullet(b);
        }
    }
}
