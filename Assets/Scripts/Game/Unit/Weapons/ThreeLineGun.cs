
using UnityEngine;

public class ThreeLineGun : BaseWeapons
{
    private static RuntimeAnimatorController Bullet_RuntimeAnimatorController;
    private int[] countArray;
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

        // ����Я�����߿�Ƭ��תְ���������ǰ������
        int shape = 0;
        int level = 0;
        BaseCardBuilder builder = GameController.Instance.mCardController.GetCardBuilderByType(FoodNameTypeMap.ThreeLinesVine);
        if (builder != null)
        {
            shape = builder.mShape;
            level = builder.mLevel;
        }
        switch (shape)
        {
            case 1:
                countArray = new int[] { 1, 2, 1 };
                break;
            case 2:
                countArray = new int[] { 2, 2, 2 };
                break;
            default:
                countArray = new int[] { 1, 1, 1 };
                break;
        }
        // ��ȡ�����ӵ��˺�
        dmgValue = FoodManager.GetAttack(FoodNameTypeMap.WaterPipe, level, shape);
    }

    /// <summary>
    /// �ж��Ƿ�����Ч�Ĺ���Ŀ��
    /// </summary>
    /// <returns></returns>
    public override bool IsHasTarget()
    {
        int start = Mathf.Max(0, GetRowIndex() - 1);
        int end = Mathf.Min(GetRowIndex() + 1, MapController.yRow - 1);
        for (int i = start; i <= end; i++)
        {
            if (GameController.Instance.CheckRowCanAttack(master, i))
                return true;
        }
        return false;
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
        mAttackFlag = true;
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
            mAttackFlag = false;
        }
    }

    /// <summary>
    /// �Ƿ�Ϊ�˺��ж�ʱ�̣���ս����Ϊ���ʵ���˺���Զ�̹���Ϊȷ�����䵯�壩
    /// </summary>
    /// <returns></returns>
    public override bool IsDamageJudgment()
    {
        return (animatorController.GetCurrentAnimatorStateRecorder().GetNormalizedTime() >= attackPercent && mAttackFlag);
    }

    /// <summary>
    /// ִ�о���Ĺ�����λ���˺��ж�Ϊ��֮��
    /// </summary>
    public override void ExecuteDamage()
    {
        GameManager.Instance.audioSourceController.PlayEffectMusic("Swing");
        int rowIndex = GetRowIndex(); // ��ȡ��ǰ��

        for (int i = -1; i <= 1; i++)
        {
            for (int j = 0; j < countArray[i + 1]; j++)
            {
                AllyBullet b = AllyBullet.GetInstance(BulletStyle.Normal, Bullet_RuntimeAnimatorController, master, master.mCurrentAttack / 10 * dmgValue);
                b.SetHitSoundEffect("Splat" + GameManager.Instance.rand.Next(0, 3));
                b.transform.position = master.transform.position;
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
                if ((rowIndex == 0 && i == 1) || (rowIndex == 6 && i == -1))
                {
                    // ���һ������λ�Ƶ�����
                    b.AddTask(new StraightMovePresetTask(b.transform, MapManager.gridHeight / 30, 0, Vector3.up * 0, MapManager.gridHeight));
                    // ����λ��
                    b.AddTask(new StraightMovePresetTask(b.transform, MapManager.gridWidth / 30 * (j + 0.5f), 0, Vector3.right, 60));
                }
                else
                {
                    // ���һ������λ�Ƶ�����
                    b.AddTask(new StraightMovePresetTask(b.transform, MapManager.gridHeight / 30, 0, Vector3.up * i, MapManager.gridHeight));
                    // ����λ��
                    b.AddTask(new StraightMovePresetTask(b.transform, MapManager.gridWidth / 30 * j, 0, Vector3.right, 60));
                }
            }
        }
    }
}
