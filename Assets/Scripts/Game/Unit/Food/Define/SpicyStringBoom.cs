using System;
/// <summary>
/// ������ը��
/// </summary>
public class SpicyStringBoom : FoodUnit
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
        // �������е�λ��Ϊ����Ŀ��
        AddCanBeSelectedAsTargetFunc((u1, u2) => {
            return u2!=null && u2.mHeight != 1;
        });
    }

    /// <summary>
    /// ը�������Ч����������
    /// </summary>
    public override void BeforeDrop()
    {
        base.BeforeDeath();
    }

    public override void OnIdleStateEnter()
    {
        if (IsFinishPrepare())
        {
            animatorController.Play("Idle", true);
        }
        else
        {
            animatorController.Play("Prepare", true);
        }
    }

    public override void OnIdleState()
    {
        if (!IsFinishPrepare())
            prepareTime--;
        if (prepareTime == 1)
            SetActionState(new TransitionState(this));
    }

    private void CreateCheckArea()
    {
        RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(transform.position, 0.65f, 0.65f, "ItemCollideEnemy");
        r.isAffectMouse = true;
        r.SetAffectHeight(1);
        Action<MouseUnit> action = (u) =>
        {
            //if (r.isAlive && UnitManager.CanBeSelectedAsTarget(this, u))
            if (r.isAlive)
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
            if (IsAlive())
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
        // ��Ӽ�ⷶΧ
        CreateCheckArea();
    }

    public override void BeforeDeath()
    {
        base.BeforeDeath();
        
        if(isTriggerBoom && IsFinishPrepare())
        {
            // ����Ǳ����� �� �����׼�� ��Ч��ת��Ϊǿ�����ƣ�ǿ����ɱ�Ա���Ϊ���İ뾶Ϊ0.75��ķ�BOSS������ˣ�����3*3��Χ�ڵ����е������һ�λҽ��˺���
            CreateFortifyBoom();
            // ԭ�ز���һ����ը��Ч
            {
                BaseEffect e = BaseEffect.GetInstance("BoomEffect");
                e.animator.runtimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Food/" + mType + "/BoomEffect");
                e.transform.position = transform.position;
                e.MInit();
                GameController.Instance.AddEffect(e);
            }
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
    /// ǿ������
    /// </summary>
    private void CreateFortifyBoom()
    {
        // ����1.5���ڶԿ���ɱ��BOSSЧ��
        {
            BombAreaEffectExecution bombEffect = BombAreaEffectExecution.GetInstance();
            bombEffect.Init(this, 900 * mCurrentAttack / 10, GetRowIndex(), 1.5f, 1, 0, 0, false, true);
            bombEffect.transform.position = this.GetPosition();
            bombEffect.SetAffectHeight(1); // ���Կ�
            bombEffect.SetOnEnemyEnterAction(BurnNoBossEnemyUnit);
            GameController.Instance.AddAreaEffectExecution(bombEffect);
        }

        // 3*3���пվ�ǿ�ƻ�׹Ч��
        {
            BombAreaEffectExecution bombEffect = BombAreaEffectExecution.GetInstance();
            bombEffect.Init(this, 0, GetRowIndex(), 3, 3, 0, 0, false, true);
            bombEffect.transform.position = this.GetPosition();
            bombEffect.SetAffectHeight(1); // ���Կ�
            bombEffect.SetOnEnemyEnterAction(ExecuteDropEnemyFlyUnit);
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

    /// <summary>
    /// ǿ�ƻ�׹��BOSS�վ���λ
    /// </summary>
    private void ExecuteDropEnemyFlyUnit(MouseUnit m)
    {
        // ��BOSS��λ �� �ǿվ���λ
        if(!m.IsBoss() && typeof(IFlyUnit).IsAssignableFrom(m.GetType()) && m.IsAlive())
        {
            IFlyUnit flyUnit = (IFlyUnit)m;
            flyUnit.ExecuteDrop();
        }
        else
        {
            // ������λ�ܵ�900��ҽ��˺�
            new BombDamageAction(CombatAction.ActionType.CauseDamage, this, m, 900 * mCurrentAttack / 10).ApplyAction();
        }
    }
}
