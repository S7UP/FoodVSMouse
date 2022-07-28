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

        // ��Ȿ���Ƿ������㴥�������ĵ���
        foreach (var enemy in GetGrid().GetMouseUnitList())
        {
            if (GetHeight() == enemy.GetHeight())
            {
                isTriggerBoom = true;
                ExecuteDeath();
                break;
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
        }
        else
        {
            // ��������Ա���Ϊ���İ뾶Ϊ0.75��ĵ���������һ��900�ҽ��˺�
            CreateNormalBoom();
        }
        // ԭ�ز���һ����ը��Ч
        {
            GameObject instance = GameManager.Instance.GetGameObjectResource(FactoryType.GameFactory, "Effect/BoomEffect");
            BaseEffect effect = instance.GetComponent<BaseEffect>();
            effect.MInit();
            effect.animator.runtimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Food/" + mType + "/BoomEffect");
            effect.transform.position = transform.position;
            GameController.Instance.AddEffect(effect);
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
        GameObject instance = GameManager.Instance.GetGameObjectResource(FactoryType.GameFactory, "AreaEffect/BombAreaEffect");
        BombAreaEffectExecution bombEffect = instance.GetComponent<BombAreaEffectExecution>();
        bombEffect.Init(this, 900, GetRowIndex(), 1.5f, 1, 0, 0, false, true);
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
            GameObject instance = GameManager.Instance.GetGameObjectResource(FactoryType.GameFactory, "AreaEffect/BombAreaEffect");
            BombAreaEffectExecution bombEffect = instance.GetComponent<BombAreaEffectExecution>();
            bombEffect.Init(this, 900, GetRowIndex(), 1.5f, 1, 0, 0, false, true);
            bombEffect.transform.position = this.GetPosition();
            bombEffect.SetAffectHeight(0); // ���Ե�
            bombEffect.SetEventMouseAction(BurnNoBossEnemyUnit);
            GameController.Instance.AddAreaEffectExecution(bombEffect);
        }

        // 3*3һ��900�ҽ��˺�
        {
            GameObject instance = GameManager.Instance.GetGameObjectResource(FactoryType.GameFactory, "AreaEffect/BombAreaEffect");
            BombAreaEffectExecution bombEffect = instance.GetComponent<BombAreaEffectExecution>();
            bombEffect.Init(this, 900, GetRowIndex(), 3, 3, 0, 0, false, true);
            bombEffect.transform.position = this.GetPosition();
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
}
