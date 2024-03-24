using S7P.Numeric;

using System;

using UnityEngine;

public class ValiMouse : MouseUnit
{
    private static RuntimeAnimatorController Boom_Run;
    private FloatModifier burnRateMod = new FloatModifier(0.01f); // ����ը����ɱЧ��
    private bool isAngry = false;

    public override void Awake()
    {
        if (Boom_Run == null)
            Boom_Run = GameManager.Instance.GetRuntimeAnimatorController("Mouse/31/Boom");
        base.Awake();
    }

    public override void MInit()
    {
        isAngry = false;
        base.MInit();
        // ��ʼ����ը����ɱЧ��
        NumericBox.BurnRate.AddModifier(burnRateMod);
        // �ܵ��ҽ�Ч����������Ա�����
        AddActionPointListener(ActionPointType.PostReceiveDamage, (combat) => {
            if (combat is DamageAction)
            {
                DamageAction dmgAction = combat as DamageAction;
                if (dmgAction.IsDamageType(DamageAction.DamageType.BombBurn))
                {
                    if (isAngry)
                    {
                        ExecuteBurn();
                    }
                    else
                    {
                        SetAngry();
                    }
                }
            }
        });
        SetActionState(new MoveState(this));
    }

    public override void MUpdate()
    {
        base.MUpdate();
    }

    private void SetAngry()
    {
        if (!isAngry)
        {
            isAngry = true;
            // ��1Ѫ
            SetGetCurrentHpFunc(delegate {
                return Mathf.Max(1, _mCurrentHp);
            });
            // ���ɱ�ѡΪ����Ŀ�ꡢ�����赲�����ɱ�����
            AddCanBeSelectedAsTargetFunc(delegate { return false; });
            AddCanBlockFunc(delegate { return false; });
            AddCanHitFunc(delegate { return false; });
            // ͬʱ�Ƴ��������ж��������Ч��
            StatusManager.RemoveAllSettleDownDebuff(this);
            // ��Ӷ����붳������Ч��
            NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreStun, new BoolModifier(true));
            NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreSlowDown, new BoolModifier(true));
            // ���500%�ƶ��ٶȼӳ�
            NumericBox.MoveSpeed.AddPctAddModifier(new FloatModifier(500));

            SetActionState(new TransitionState(this));

            float start_x = transform.position.x;
            float end_x = Mathf.Max(MapManager.GetColumnX(0), transform.position.x + moveRotate.x * 4 * MapManager.gridWidth);
            CustomizationTask task = new CustomizationTask();
            task.AddTaskFunc(delegate {
                if (moveRotate.x < 0)
                    return transform.position.x < end_x;
                else
                    return transform.position.x > end_x;
            });
            task.AddOnExitAction(delegate {
                SetActionState(new CastState(this));
            });
            taskController.AddTask(task);
        }
    }

    public override void OnTransitionStateEnter()
    {
        animatorController.Play("Drop");
    }

    public override void OnTransitionState()
    {
        if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
        {
            SetActionState(new MoveState(this));
        }
    }

    public override void OnCastStateEnter()
    {
        animatorController.Play("BeforeBoom");
    }

    public override void OnCastState()
    {
        if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
        {
            CreateCheckArea();
            ExecuteDeath();
        }
    }

    private void CreateCheckArea()
    {
        float bomb_dmg_rate = 1.0f;

        // ��������б�������debuff
        if (Environment.EnvironmentFacade.GetIceDebuff(this) != null)
        {
            bomb_dmg_rate = 0.5f;
            Action<BaseUnit> action = (u) =>
            {
                Environment.EnvironmentFacade.AddIceDebuff(u, 100);
            };
            // ��3*3���е�λʩ��100��������
            RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(transform.position, new Vector2(2.75f * MapManager.gridWidth, 2.75f * MapManager.gridHeight), "BothCollide");
            r.SetAffectHeight(0);
            r.isAffectFood = true;
            r.isAffectMouse = true;
            r.SetInstantaneous();
            r.SetOnFoodEnterAction(action);
            r.SetOnEnemyEnterAction(action);
            r.AddExcludeMouseUnit(this);
            GameController.Instance.AddAreaEffectExecution(r);
        }

        // ��ر�ը�ж�(����ʳ��
        {
            RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(transform.position, new Vector2(0.5f * MapManager.gridWidth, 0.5f * MapManager.gridHeight), "ItemCollideAlly");
            r.SetAffectHeight(0);
            r.isAffectFood = true;
            r.isAffectMouse = true;
            r.SetTotoalTimeAndTimeLeft(2);
            r.AddExcludeMouseUnit(this);
            r.AddBeforeDestoryAction(delegate
            {
                // ������ʳ��λ��̯�˺�
                int count = r.foodUnitList.Count;
                foreach (var u in r.foodUnitList.ToArray())
                    new DamageAction(CombatAction.ActionType.BurnDamage, this, u, Mathf.Max(50, u.mMaxHp / 2) / count * bomb_dmg_rate).ApplyAction();
            });
            GameController.Instance.AddAreaEffectExecution(r);
        }
        // ������
        {
            RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(transform.position, new Vector2(1f * MapManager.gridWidth, 1f * MapManager.gridHeight), "ItemCollideEnemy");
            r.SetAffectHeight(0);
            r.isAffectMouse = true;
            r.SetTotoalTimeAndTimeLeft(2);
            r.AddExcludeMouseUnit(this);
            r.AddBeforeDestoryAction(delegate
            {
                // ��������λ�ܵ�һ�λҽ�Ч��
                foreach (var u in r.mouseUnitList.ToArray())
                    BurnManager.BurnDamage(this, u, bomb_dmg_rate);
            });
            GameController.Instance.AddAreaEffectExecution(r);
        }

        // ��ը��Ч
        {
            BaseEffect e = BaseEffect.CreateInstance(Boom_Run, null, "Boom", null, false);
            e.SetSpriteRendererSorting(GetSpriteRenderer().sortingLayerName, GetSpriteRenderer().sortingOrder + 1);
            e.transform.position = transform.position;
            GameController.Instance.AddEffect(e);
        }
    }

    public override void OnDieStateEnter()
    {
        if (isAngry)
            DeathEvent();
        else
            base.OnDieStateEnter();
    }

    public override void OnMoveStateEnter()
    {
        base.OnMoveStateEnter();
        if (isAngry)
            animatorController.Play("AngryMove", true);
        else
            animatorController.Play("Move", true);
    }
}
