using System;

using UnityEngine;
/// <summary>
/// �������
/// </summary>
public class MouseCatcher : FoodUnit
{
    private int prepareTime;
    private int totalPrepareTime; // ��׼��ʱ��
    private bool isTriggerBoom; // �Ƿ񱻴���
    private BoolModifier boolModifier = new BoolModifier(true);

    public override void MInit()
    {
        totalPrepareTime = 60 * 5; // 5s׼��ʱ��
        prepareTime = totalPrepareTime;
        isTriggerBoom = false;
        base.MInit();

        if(mShape >= 2)
        {
            AddCanBeSelectedAsTargetFunc(delegate { return false; });
            AddCanBlockFunc(delegate { return false; });
            // ���������Ч
            BaseEffect e = BaseEffect.CreateInstance(GameManager.Instance.GetRuntimeAnimatorController("Effect/HiddenEffect"), "Appear", "Idle", "Disappear", true);
            e.SetSpriteRendererSorting("Effect", 2);
            GameController.Instance.AddEffect(e);
            AddEffectToDict("SpinCoffeeHidden", e, new Vector2(0, 0 * 0.5f * MapManager.gridWidth));
        }
    }

    /// <summary>
    /// ը�������Ч����������
    /// </summary>
    public override void BeforeDrop()
    {
        base.BeforeDeath();
    }

    private void CreateCheckArea()
    {
        RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(transform.position, 0.65f, 0.65f, "ItemCollideEnemy");
        r.isAffectMouse = true;
        r.SetAffectHeight(0);
        Action<MouseUnit> action = (u) =>
        {
            if (r.isAlive && UnitManager.CanBeSelectedAsTarget(this, u))
            {
                isTriggerBoom = true;
                ExecuteDeath();
                r.MDestory();
            }
        };
        r.SetOnEnemyEnterAction(action);
        r.SetOnEnemyStayAction(action);
        CustomizationTask t = new CustomizationTask();
        t.AddTaskFunc(delegate {
            if(IsAlive())
                r.transform.position = transform.position;
            else
            {
                r.MDestory();
            }
            return false;
        });
        r.AddTask(t);
        GameController.Instance.AddAreaEffectExecution(r);
    }

    public override void OnIdleStateEnter()
    {
        if (IsFinishPrepare())
        {
            animatorController.Play("Idle");
        }
        else
        {
            animatorController.Play("Prepare");
        }
    }

    public override void OnIdleState()
    {
        if (!IsFinishPrepare())
            prepareTime--;
        if (prepareTime == 1)
            SetActionState(new TransitionState(this));
    }

    public override void OnTransitionStateEnter()
    {
        animatorController.Play("PreIdle");
        prepareTime = 0;
        // ��׼��������֡�ڣ������޵С����߻ҽ���ɱ�����߶���Ч��
        NumericBox.AddDecideModifierToBoolDict(StringManager.Invincibility, boolModifier);
        NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreBombInstantKill, boolModifier);
        NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreFrozen, boolModifier);
    }

    public override void OnTransitionState()
    {
        if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
            SetActionState(new IdleState(this));
    }

    public override void OnTransitionStateExit()
    {
        // �Ƴ���ЩЧ��
        NumericBox.RemoveDecideModifierToBoolDict(StringManager.Invincibility, boolModifier);
        NumericBox.RemoveDecideModifierToBoolDict(StringManager.IgnoreBombInstantKill, boolModifier);
        NumericBox.RemoveDecideModifierToBoolDict(StringManager.IgnoreFrozen, boolModifier);
        // ��Ӽ��Ч��
        CreateCheckArea();
    }

    public override void BeforeDeath()
    {
        base.BeforeDeath();
        
        if(isTriggerBoom && IsFinishPrepare())
        {
            // ����Ǳ����� �� �����׼�� ��Ч��ת��Ϊǿ�����ƣ�ǿ����ɱ�Ա���Ϊ���İ뾶Ϊ0.75��ķ�BOSS������ˣ�����3*3��Χ�ڵ����е������һ�λҽ��˺���
            CreateFortifyBoom();
        }
        else
        {
            // ��������Ա���Ϊ���İ뾶Ϊ0.75��ĵ���������һ��900�ҽ��˺�
            CreateNormalBoom();
        }
        // ԭ�ز���һ����ը��Ч
        {
            BaseEffect e = BaseEffect.GetInstance("BoomEffect");
            e.animator.runtimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Food/" + mType + "/BoomEffect");
            e.transform.position = transform.position;
            e.MInit();
            GameController.Instance.AddEffect(e);
        }
    }

    /// <summary>
    /// �Ƿ�׼�����
    /// </summary>
    /// <returns></returns>
    private bool IsFinishPrepare()
    {
        return prepareTime <= 0;
    }

    /// <summary>
    /// ��ͨ��ը
    /// </summary>
    /// <returns></returns>
    private void CreateNormalBoom()
    {
        BombAreaEffectExecution bombEffect = BombAreaEffectExecution.GetInstance();
        bombEffect.Init(this, 0, GetRowIndex(), 1.5f, 1, 0, 0, false, true);
        bombEffect.transform.position = this.GetPosition();
        bombEffect.SetAffectHeight(0); // ���Ե�
        GameController.Instance.AddAreaEffectExecution(bombEffect);
    }

    /// <summary>
    /// ǿ������
    /// </summary>
    private void CreateFortifyBoom()
    {
        // �Ե���ɱ��BOSSЧ��
        {
            BombAreaEffectExecution bombEffect = BombAreaEffectExecution.GetInstance();
            bombEffect.Init(this, 900 * mCurrentAttack / 10, GetRowIndex(), 1.5f, 1, 0, 0, false, true);
            bombEffect.transform.position = this.GetPosition();
            bombEffect.SetAffectHeight(0); // ���Ե�
            bombEffect.SetOnEnemyEnterAction(BurnNoBossEnemyUnit);
            GameController.Instance.AddAreaEffectExecution(bombEffect);
        }

        // 3*3һ��900�ҽ��˺�
        {
            BombAreaEffectExecution bombEffect = BombAreaEffectExecution.GetInstance();
            bombEffect.Init(this, 900 * mCurrentAttack / 10, GetRowIndex(), 3, 3, 0, 0, false, true);
            bombEffect.transform.position = this.GetPosition();
            bombEffect.SetAffectHeight(0); // ���Ե�
            GameController.Instance.AddAreaEffectExecution(bombEffect);
        }
    }

    /// <summary>
    /// ֱ����ɱ��BOSS���˵�λ
    /// </summary>
    private void BurnNoBossEnemyUnit(MouseUnit m)
    {
        if(!m.IsBoss())
            new BombDamageAction(CombatAction.ActionType.CauseDamage, this, m, m.mCurrentHp).ApplyAction();
        else
            // ��BOSS���900��ҽ��˺�
            new BombDamageAction(CombatAction.ActionType.CauseDamage, this, m, 900 * mCurrentAttack / 10).ApplyAction();
    }
}
