using UnityEngine;
using S7P.Numeric;
using System;

public class FlySelfDestructMouse : MouseUnit
{
    private static RuntimeAnimatorController BoomEffect;

    private bool isDrop; // �Ƿ񱻻���
    private int dropColumn; // ������
    private FloatModifier burnRateMod = new FloatModifier(0.01f); // ����ը����ɱЧ��
    private BoolModifier IgnoreFrozen = new BoolModifier(true); // ���߶���

    public override void Awake()
    {
        if (BoomEffect == null)
            BoomEffect = GameManager.Instance.GetRuntimeAnimatorController("Mouse/8/BoomEffect");
        base.Awake();
    }

    public override void MInit()
    {
        isDrop = false;
        dropColumn = 3; // ������Ĭ��Ϊ3����������
        base.MInit();
        mHeight = 1;
        // ��ʼ����ը����ɱЧ��
        NumericBox.BurnRate.AddModifier(burnRateMod);
        // ���ܵ��˺�����֮��ֱ���ж�Ϊ��׹״̬
        AddActionPointListener(ActionPointType.PostReceiveDamage, (combat)=> { 
            if(!isDrop)
                ExecuteDestruct();
            else
            {
                // ��׹��״̬���ܵ��ҽ�Ч����ֱ�ӻ���
                if(combat is DamageAction)
                {
                    DamageAction dmgAction = combat as DamageAction;
                    if(dmgAction.IsDamageType(DamageAction.DamageType.BombBurn))
                    {
                        ExecuteBurn();
                    }
                }
            }
        });
        // ��1Ѫ
        SetGetCurrentHpFunc(delegate {
            return Mathf.Max(1, _mCurrentHp);
        });
    }

    public override void MUpdate()
    {
        base.MUpdate();
        if (IsMeetDropCondition())
            ExecuteDestruct();
    }

    /// <summary>
    /// ����Ƿ����㽵������
    /// </summary>
    private bool IsMeetDropCondition()
    {
        return (transform.position.x <= MapManager.GetColumnX(dropColumn) && !isDrop);
    }

    /// <summary>
    /// ִ�н��䣬��һ��
    /// </summary>
    public void ExecuteDestruct()
    {
        if (!isDrop)
        {
            // �����׹�����˺��ʵ��һЩ��Ϊ�ᷢ���仯
            isDrop = true;
            // ����Ϊ����ѡȡ���Լ����ɻ���
            AddCanBeSelectedAsTargetFunc(delegate { return false; });
            AddCanHitFunc(delegate { return false; });
            // �Ƴ�����ը����ɱЧ��
            NumericBox.BurnRate.RemoveModifier(burnRateMod);
            // ��Ӷ�������Ч��
            NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreStun, IgnoreFrozen);
            // ��������б������ˣ������������100��������ˣ���ʵ�����ȶ�Я������������أ�
            if (Environment.EnvironmentFacade.GetIceDebuff(this) != null)
            {
                Environment.EnvironmentFacade.AddIceDebuff(this, 100);
            }
            // ͬʱ�Ƴ��������ж��������Ч��
            StatusManager.RemoveAllSettleDownDebuff(this);
            // �޸Ļ�������Ϊ��200֡���ƶ�3��
            NumericBox.MoveSpeed.SetBase(3 * MapManager.gridWidth / 200);
            // ��Ϊת��״̬
            SetActionState(new MoveState(this));

            int timeLeft = 200;
            CustomizationTask task = new CustomizationTask();
            task.AddTaskFunc(delegate {
                timeLeft--;
                if(timeLeft <= 0)
                {
                    return true;
                }
                return false;
            });
            task.AddOnExitAction(delegate {
                CreateCheckArea();
                ExecuteDeath();
            });
            taskController.AddTask(task);
        }
    }

    private void CreateCheckArea()
    {
        float bomb_dmg_rate = 1.0f;

        // ��������б�������debuff
        if(Environment.EnvironmentFacade.GetIceDebuff(this)!=null)
        {
            bomb_dmg_rate = 0.01f;
            Action<BaseUnit> action = (u) => 
            {
                Environment.EnvironmentFacade.AddIceDebuff(u, 100);
            };
            // ��3*3���е�λʩ��100��������
            RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(transform.position, new Vector2(0.5f*MapManager.gridWidth, 0.5f * MapManager.gridHeight), "BothCollide");
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
            RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(transform.position, new Vector2(0.5f*MapManager.gridWidth, 0.5f * MapManager.gridHeight), "ItemCollideAlly");
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
            BaseEffect e = BaseEffect.CreateInstance(BoomEffect, null, "Disappear", null, false);
            e.SetSpriteRendererSorting(GetSpriteRenderer().sortingLayerName, GetSpriteRenderer().sortingOrder + 1);
            e.transform.position = transform.position;
            GameController.Instance.AddEffect(e);
        }
    }

    public override void OnMoveStateEnter()
    {
        if (isDrop)
            animatorController.Play("Move1");
        else
            animatorController.Play("Move0", true);
    }

    public override bool CanTriggerCat()
    {
        return false;
    }

    public override bool CanTriggerLoseWhenEnterLoseLine()
    {
        return false;
    }

    /// <summary>
    /// û����������
    /// </summary>
    public override void OnDieStateEnter()
    {
        DeathEvent();
    }

    public override void DuringDeath()
    {
        
    }
}
