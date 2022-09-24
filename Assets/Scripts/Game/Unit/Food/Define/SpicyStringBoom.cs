using UnityEngine;
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
        //else
        //{
        //    // ׼���ú�Ż��Ȿ���Ƿ������㴥�������ĵ���
        //    foreach (var enemy in GetGrid().GetMouseUnitList())
        //    {
        //        if (enemy.GetHeight() == 1)
        //        {
        //            isTriggerBoom = true;
        //            ExecuteDeath();
        //            break;
        //        }
        //    }
        //}
        if (prepareTime == 1)
            SetActionState(new TransitionState(this));
    }

    /// <summary>
    /// ���վ���������ը
    /// </summary>
    /// <param name="collision"></param>
    public void OnTriggerEnter2D(Collider2D collision)
    {
        if(IsFinishPrepare() && collision.tag.Equals("Mouse"))
        {
            MouseUnit m = collision.GetComponent<MouseUnit>();
            if (m.GetHeight() == 1 && GetRowIndex()==m.GetRowIndex() && m.CanBeSelectedAsTarget())
            {
                isTriggerBoom = true;
                ExecuteDeath();
            }
        }
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
            bombEffect.Init(this, 900, GetRowIndex(), 1.5f, 1, 0, 0, false, true);
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
            new BurnDamageAction(CombatAction.ActionType.CauseDamage, this, m, m.mCurrentHp).ApplyAction();
        else
            // ��BOSS���900��ҽ��˺�
            new BurnDamageAction(CombatAction.ActionType.CauseDamage, this, m, 900).ApplyAction();
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
    }
}
