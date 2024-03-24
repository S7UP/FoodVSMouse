using S7P.Numeric;

using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// ��е�ߴ���
/// </summary>
public class GUNDAMMouse : MouseUnit
{
    private GeneralAttackSkillAbility generalAttackSkill;
    private static BoolModifier IgnoreModifier = new BoolModifier(true);
    private int attackCountLeft = 0;
    private static RuntimeAnimatorController Missile_Run;
    private bool missleFlag = false;

    public override void Awake()
    {
        if(Missile_Run == null)
        {
            Missile_Run = GameManager.Instance.GetRuntimeAnimatorController("Mouse/30/Missile");
        }
        base.Awake();
    }

    public override void MInit()
    {
        attackCountLeft = 0;
        missleFlag = false;
        base.MInit();
        // ������ѣ������Ч��
        NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreFrozen, IgnoreModifier);
        NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreStun, IgnoreModifier);
        // ��ը�����һ������������
        AddActionPointListener(ActionPointType.PostReceiveDamage, (combat) => {
            if (!(combat is DamageAction))
                return;
            DamageAction d = combat as DamageAction;
            if (d.IsDamageType(DamageAction.DamageType.BombBurn))
                attackCountLeft++;
        });
    }

    /// <summary>
    /// �����ж�����
    /// </summary>
    public override void SetCollider2DParam()
    {
        mBoxCollider2D.offset = Vector2.zero;
        mBoxCollider2D.size = new Vector2(0.75f * MapManager.gridWidth, 0.49f * MapManager.gridHeight);
    }
    public override void LoadSkillAbility()
    {
        List<SkillAbility.SkillAbilityInfo> infoList = AbilityManager.Instance.GetSkillAbilityInfoList(mUnitType, mType, mShape);
        // ��ͨ����
        if (infoList.Count > 0)
        {
            GeneralAttackSkillAbility s = new GeneralAttackSkillAbility(this, infoList[0]);
            skillAbilityManager.AddSkillAbility(s);
            s.isExclusive = false;
            s.priority = 0;
            generalAttackSkill = s;
        }

        // Ͷ����������
        SkillAbility.SkillAbilityInfo info = new SkillAbility.SkillAbilityInfo()
        {
            name = "��������",
            needEnergy = 0,
            startEnergy = 0,
            energyRegeneration = 1,
            skillType = SkillAbility.Type.SpecialAbility,
            isExclusive = true,
            canActiveInDeathState = false,
            priority = 999
        };
        {
            CustomizationSkillAbility s = new CustomizationSkillAbility(this, info);
            s.noClearEnergyWhenStart = false;
            s.noClearEnergyWhenEnd = true;
            s.IsMeetSkillConditionFunc = delegate { 
                return attackCountLeft > 0 && transform.position.x > MapManager.GetColumnX(2) && !(mCurrentActionState is CastState);
            };
            s.BeforeSpellFunc = delegate {
                generalAttackSkill.TryEndActivate();
                SetActionState(new CastState(this));
            };
            s.OnSpellingFunc = delegate { };
            s.IsMeetCloseSpellingConditionFunc = delegate { return true; };
            s.AfterSpellFunc = delegate { };
            skillAbilityManager.AddSkillAbility(s);
        }
    }


    public override void OnCastStateEnter()
    {
        generalAttackSkill.enableEnergyRegeneration = false; // ����ƽA����
        animatorController.Play("Throw");
        missleFlag = true;
    }

    public override void OnCastState()
    {
        if(animatorController.GetCurrentAnimatorStateRecorder().GetNormalizedTime() >0.6f && missleFlag)
        {
            // ����һ������
            missleFlag = false;
            attackCountLeft--;
            CreateBullet();
        }
        else if(animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
        {
            SetActionState(new MoveState(this));
        }
    }

    public override void OnCastStateExit()
    {
        generalAttackSkill.enableEnergyRegeneration = true; // �ָ�ƽA���ܲ�����ƽA
        generalAttackSkill.FullCurrentEnergy();
    }

    private void CreateBullet()
    {
        EnemyBullet b = EnemyBullet.GetInstance(Missile_Run, this, 0);
        b.isnDelOutOfBound = true; // ��������ɾ
        b.AddSpriteOffsetY(new FloatModifier(0.5f*MapManager.gridHeight));
        b.SetHitSoundEffect("Boom");
        b.AddHitAction((b, u)=> {
            CreateBurnArea(b.transform.position);
            CreateDamageArea(b.transform.position);
        });
        PitcherManager.AddDefaultFlyTask(b, transform.position, new Vector2(Mathf.Max(MapManager.GetColumnX(2), transform.position.x + 4*MapManager.gridWidth*moveRotate.x), transform.position.y), true, false);
        GameController.Instance.AddBullet(b);
    }

    private void CreateBurnArea(Vector2 pos)
    {
        RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(pos, 0.5f, 0.5f, "ItemCollideAlly");
        r.isAffectFood = true;
        r.SetInstantaneous();
        r.SetOnFoodEnterAction((u) => {
            BurnManager.BurnDamage(this, u);
        });
        GameController.Instance.AddAreaEffectExecution(r);
    }

    private void CreateDamageArea(Vector2 pos)
    {
        RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(pos, 2.5f, 2.5f, "ItemCollideAlly");
        r.isAffectFood = true;
        r.SetInstantaneous();
        r.SetOnFoodEnterAction((u) => {
            DamageAction d = new DamageAction(CombatAction.ActionType.BurnDamage, this, u, Mathf.Max(0.2f * u.mMaxHp, 20)*mCurrentAttack/200);
            d.ApplyAction();
        });
        GameController.Instance.AddAreaEffectExecution(r);
    }

    #region ��ͨ�������
    /// <summary>
    /// ִ�о���Ĺ�����λ���˺��ж�Ϊ��֮��
    /// </summary>
    public override void ExecuteDamage()
    {
        if (IsHasTarget())
        {
            // ������Ⱥ��
            BaseGrid grid = GetCurrentTarget().GetGrid();
            if (grid != null)
            {
                foreach (var item in grid.GetAttackableFoodUnitList())
                {
                    TakeDamage(item);
                }
                if (grid.GetCharacterUnit() != null)
                {
                    TakeDamage(grid.GetCharacterUnit());
                }
            }
            else
            {
                TakeDamage(GetCurrentTarget());
            }
        }
        GameManager.Instance.audioSourceController.PlayEffectMusic("Thump");
    }

    /// <summary>
    /// ������ͨ��������״̬
    /// </summary>
    public override void BeforeGeneralAttack()
    {
        // �л�Ϊ����������ͼ
        mAttackFlag = true;
        SetActionState(new AttackState(this));
    }
    #endregion
}
