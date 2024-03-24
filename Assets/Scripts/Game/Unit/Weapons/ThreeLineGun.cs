
using UnityEngine;

public class ThreeLineGun : BaseWeapons
{
    private static RuntimeAnimatorController Bullet_RuntimeAnimatorController;
    private int[] countArray;
    private float dmgValue; // 单发子弹伤害

    public override void Awake()
    {
        if (Bullet_RuntimeAnimatorController == null)
        {
            Bullet_RuntimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Food/6/Bullet");
        }
            
        base.Awake();
    }

    /// <summary>
    /// 加载技能，此处仅加载普通攻击，具体技能加载实现请在子类中重写
    /// </summary>
    public override void LoadSkillAbility()
    {
        // 普通攻击
        weaponsGeneralAttackSkillAbility = new WeaponsGeneralAttackSkillAbility(this, new SkillAbility.SkillAbilityInfo()
        {
            name = "普通攻击",
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

        // 根据携带三线卡片的转职情况来计算前后发射数
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
        // 获取单发子弹伤害
        dmgValue = FoodManager.GetAttack(FoodNameTypeMap.WaterPipe, level, shape);
    }

    /// <summary>
    /// 判断是否有有效的攻击目标
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
    /// 退出普通攻击的条件
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetEndGeneralAttackCondition()
    {
        return animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce();
    }

    /// <summary>
    /// 退出普通攻击后要做的事
    /// </summary>
    public override void AfterGeneralAttack()
    {
        base.AfterGeneralAttack();
        mAttackFlag = true;
    }

    /// <summary>
    /// 普通攻击期间
    /// </summary>
    public override void OnGeneralAttack()
    {
        // 伤害判定帧应当执行判定
        if (IsDamageJudgment())
        {
            ExecuteDamage();
            mAttackFlag = false;
        }
    }

    /// <summary>
    /// 是否为伤害判定时刻（近战攻击为打出实际伤害，远程攻击为确定发射弹体）
    /// </summary>
    /// <returns></returns>
    public override bool IsDamageJudgment()
    {
        return (animatorController.GetCurrentAnimatorStateRecorder().GetNormalizedTime() >= attackPercent && mAttackFlag);
    }

    /// <summary>
    /// 执行具体的攻击，位于伤害判定为真之后
    /// </summary>
    public override void ExecuteDamage()
    {
        GameManager.Instance.audioSourceController.PlayEffectMusic("Swing");
        int rowIndex = GetRowIndex(); // 获取当前行

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
                    // 添加一个纵向位移的任务
                    b.AddTask(new StraightMovePresetTask(b.transform, MapManager.gridHeight / 30, 0, Vector3.up * 0, MapManager.gridHeight));
                    // 横向位移
                    b.AddTask(new StraightMovePresetTask(b.transform, MapManager.gridWidth / 30 * (j + 0.5f), 0, Vector3.right, 60));
                }
                else
                {
                    // 添加一个纵向位移的任务
                    b.AddTask(new StraightMovePresetTask(b.transform, MapManager.gridHeight / 30, 0, Vector3.up * i, MapManager.gridHeight));
                    // 横向位移
                    b.AddTask(new StraightMovePresetTask(b.transform, MapManager.gridWidth / 30 * j, 0, Vector3.right, 60));
                }
            }
        }
    }
}
