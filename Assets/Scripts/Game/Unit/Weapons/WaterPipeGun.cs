using System.Collections.Generic;

using UnityEngine;

public class WaterPipeGun : BaseWeapons
{
    private static RuntimeAnimatorController Bullet_RuntimeAnimatorController;

    private static List<int> maxFrontAttackCountList = new List<int> { 1, 2, 3 };
    private static List<int> maxBackAttackCountList = new List<int> { 2, 2, 3 };

    private int maxFrontAttackCount;
    private int maxBackAttackCount;
    private int maxAttackCount;
    private int currentAttackCount; // ��ǰ����������
    private float endAttackPercent; // �������һ���ӵ�ʱ�Ķ������Űٷֱ�
    private List<float> attackPercentList = new List<float>();
    private float dmgValue; // �����ӵ��˺�

    public override void Awake()
    {
        if (Bullet_RuntimeAnimatorController == null)
        {
            Bullet_RuntimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Food/6/Bullet");
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
        // ����Я��ˮ�ܿ�Ƭ��תְ���������ǰ������
        int shape = 0;
        int level = 0;
        BaseCardBuilder builder = GameController.Instance.mCardController.GetCardBuilderByType(FoodNameTypeMap.WaterPipe);
        if(builder != null)
        {
            shape = builder.mShape;
            level = builder.mLevel;
        }
        maxFrontAttackCount = maxFrontAttackCountList[shape];
        maxBackAttackCount = maxBackAttackCountList[shape];
        maxAttackCount = Mathf.Max(maxFrontAttackCount, maxBackAttackCount);
        endAttackPercent = 0.90f;
        attackPercentList.Clear();
        for (int i = 0; i < maxAttackCount; i++)
        {
            attackPercentList.Add(attackPercent + (endAttackPercent - attackPercent) * i / (maxAttackCount - 1));
        }
        currentAttackCount = 0;
        // ��ȡ�����ӵ��˺�
        dmgValue = FoodManager.GetAttack(FoodNameTypeMap.WaterPipe, level, shape);
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
        // ǰ����
        if (currentAttackCount < maxFrontAttackCount)
        {
            AllyBullet b = AllyBullet.GetInstance(BulletStyle.Normal, Bullet_RuntimeAnimatorController, master, master.mCurrentAttack/10* dmgValue);
            b.SetHitSoundEffect("Splat" + GameManager.Instance.rand.Next(0, 3));
            b.transform.position = transform.position;
            b.SetSpriteLocalPosition(master.GetSpriteLocalPosition());
            b.SetStandardVelocity(24);
            b.SetRotate(Vector2.right);
            b.AddHitAction((b, u) => {
                if (u is MouseUnit)
                {
                    MouseUnit m = u as MouseUnit;
                    if (!m.IsBoss())
                    {
                        new DamageAction(CombatAction.ActionType.CauseDamage, master, u, 0.01f * u.GetCurrentHp()).ApplyAction();
                    }
                }
            });
            GameController.Instance.AddBullet(b);
        }
        // �󹥻�
        if (currentAttackCount < maxBackAttackCount)
        {
            AllyBullet b = AllyBullet.GetInstance(BulletStyle.Normal, Bullet_RuntimeAnimatorController, master, master.mCurrentAttack/10* dmgValue);
            b.SetHitSoundEffect("Splat" + GameManager.Instance.rand.Next(0, 3));
            b.transform.position = transform.position;
            b.SetSpriteLocalPosition(master.GetSpriteLocalPosition());
            b.SetStandardVelocity(24);
            b.SetRotate(Vector2.left);
            b.AddHitAction((b, u) => {
                if (u is MouseUnit)
                {
                    MouseUnit m = u as MouseUnit;
                    if (!m.IsBoss())
                    {
                        new DamageAction(CombatAction.ActionType.RealDamage, master, u, 0.01f * u.GetCurrentHp()).ApplyAction();
                    }
                }
            });
            GameController.Instance.AddBullet(b);
            GameManager.Instance.audioSourceController.PlayEffectMusic("Throw" + GameManager.Instance.rand.Next(0, 2));
        }
    }
}
