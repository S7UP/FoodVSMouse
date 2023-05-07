using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// �ɿ������
/// </summary>
public class ChocolateBread : FoodUnit
{
    private const string ShieldEffectKey = "ChocolateBreadShield";
    private static Sprite Shield_Sprite; 

    private int mHertIndex; // ���˽׶� 0������ 1��С�� 2������
    private List<float> mHertRateList = new List<float>()
    {
        0.67f, 0.33f
    };
    private int MaxShieldCount; // ʥ�ܲ�������
    private int CurrentShieldCount; // ��ǰʥ�ܲ���
    private const int GetShieldTime = 600; // ��ȡһ��ʥ������Ҫ��ʱ��
    private int nextShieldTimeLeft; // ��ȡ��һ��ʥ������Ҫ��ʱ��
    private int decTimeWhenHit; // ����ʱ���ٵĻ�ȡ��һ��ʥ�ܵ�ʱ��

    public override void Awake()
    {
        if (Shield_Sprite == null)
            Shield_Sprite = GameManager.Instance.GetSprite("Effect/Shield");
        base.Awake();
    }

    public override void MInit()
    {
        mHertIndex = 0;
        nextShieldTimeLeft = GetShieldTime;
        CurrentShieldCount = 0;
        base.MInit();
        // ����תְ�������ʼ����ֵ
        switch (mShape)
        {
            case 1:
                MaxShieldCount = 2;
                decTimeWhenHit = 180;
                AddShield();
                break;
            default:
                MaxShieldCount = 1;
                decTimeWhenHit = 120;
                break;
        }
        // ���ܵ��˺�����֮ǰ���������Ļ��ܰѱ����˺�ֵ��Ч��
        AddActionPointListener(ActionPointType.PreReceiveDamage, (action) => { TryToResistDamage(action); });
        // ���ܵ��˺�����֮�󣬸���������ͼ״̬�����Ҹ����˺��������Ƿ����ٻ��ܵ�CD
        AddActionPointListener(ActionPointType.PostReceiveDamage, (action) => { UpdateHertMap(); OnHit(action);  });
        // �ڽ������ƽ���֮�󣬸���������ͼ״̬
        AddActionPointListener(ActionPointType.PostReceiveCure, delegate { UpdateHertMap(); });
    }

    public override void MUpdate()
    {
        // ʥ�ָܻ�����
        if(CurrentShieldCount < MaxShieldCount)
        {
            nextShieldTimeLeft--;
            if (nextShieldTimeLeft <= 0)
            {
                nextShieldTimeLeft += GetShieldTime;
                AddShield();
            }
        }

        base.MUpdate();
    }

    /// <summary>
    /// ���ݵȼ���͵ȼ������¶�Ӧ����
    /// </summary>
    public override void UpdateAttributeByLevel()
    {
        SetMaxHpAndCurrentHp((float)(attr.baseAttrbute.baseHP + attr.valueList[mLevel]));
    }


    public override void OnIdleStateEnter()
    {
        animatorController.Play("Idle" + mHertIndex);
    }

    /// <summary>
    /// �����˻��߱�����ʱ�����µ�λ��ͼ״̬
    /// </summary>
    protected void UpdateHertMap()
    {
        // Ҫ�����˵Ļ������˰�
        if (isDeathState)
            return;

        // �Ƿ�Ҫ�л���������flag
        bool flag = false;
        // �ָ�����һ��������ͼ���
        while (mHertIndex > 0 && GetHeathPercent() > mHertRateList[mHertIndex - 1])
        {
            mHertIndex--;
            flag = true;
        }
        // ��һ��������ͼ�ļ��
        while (mHertIndex < mHertRateList.Count && GetHeathPercent() <= mHertRateList[mHertIndex])
        {
            mHertIndex++;
            flag = true;
        }
        // ���л�֪ͨʱ���л�
        if (flag)
        {
            animatorController.Play("Idle" + mHertIndex);
        }
    }

    /// <summary>
    /// ����һ��ʥ��
    /// </summary>
    public void AddShield()
    {
        if(CurrentShieldCount < MaxShieldCount)
        {
            CurrentShieldCount++;
        }
        // ����Ǵ�0�ӵ�1
        if(CurrentShieldCount == 1)
        {
            BaseEffect e = BaseEffect.CreateInstance(Shield_Sprite);
            GameController.Instance.AddEffect(e);
            AddEffectToDict(ShieldEffectKey, e, Vector2.zero);
        }
    }

    /// <summary>
    /// ����һ��ʥ��
    /// </summary>
    public void DecShield()
    {
        if (CurrentShieldCount > 0)
        {
            CurrentShieldCount--;
        }
        // �������0
        if(CurrentShieldCount == 0)
        {
            RemoveEffectFromDict(ShieldEffectKey);
        }
    }

    /// <summary>
    /// ��������ʥ��ʱ��
    /// </summary>
    /// <param name="action"></param>
    private void OnHit(CombatAction action)
    {
        // ����ʥ���ǲ����ʱ��ģ��������������Ŀ��ܷ�����
        if (CurrentShieldCount == MaxShieldCount)
            return;

        if (action is DamageAction)
        {
            DamageAction dmgAction = action as DamageAction;
            if(dmgAction.DamageValue >= 10)
            {
                nextShieldTimeLeft -= decTimeWhenHit;
            }
        }
    }

    /// <summary>
    /// ���Ե���һ���˺�
    /// </summary>
    private void TryToResistDamage(CombatAction action)
    {
        if (CurrentShieldCount <= 0)
            return;

        if(action is DamageAction)
        {
            DamageAction dmgAction = action as DamageAction;
            dmgAction.DamageValue = 0; // ǿ������˺�Ϊ0
            DecShield();
            // ��5*5�����ѷ���λ����һ�������ظ�Ч��
            RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(transform.position, 5, 5, "ItemCollideAlly");
            r.SetInstantaneous();
            r.isAffectFood = true;
            r.SetOnFoodEnterAction((unit) => {
                EffectManager.AddHealEffectToUnit(unit);
                new CureAction(CombatAction.ActionType.GiveCure, this, unit, mCurrentAttack).ApplyAction();
            });
            GameController.Instance.AddAreaEffectExecution(r);
        }
    }


    /////////////////////////////////���¹��ܾ�ʧЧ������Ҫ���·���/////////////////////////////////////

    /// <summary>
    /// �ж��Ƿ�����Ч�Ĺ���Ŀ��
    /// </summary>
    /// <returns></returns>
    protected override bool IsHasTarget()
    {
        // �����Ϳ�Ƭ����Ҫ
        return false;
    }

    /// <summary>
    /// �Ƿ�������ͨ����������
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetGeneralAttackCondition()
    {
        // �����Ϳ�Ƭ����Ҫ
        return true;
    }

    /// <summary>
    /// ������ͨ��������״̬
    /// </summary>
    public override void BeforeGeneralAttack()
    {
        // �����Ϳ�Ƭ�޹���״̬
    }

    /// <summary>
    /// ��ͨ�����ڼ�
    /// </summary>
    public override void OnGeneralAttack()
    {
        // �����Ϳ�Ƭ��
    }

    /// <summary>
    /// �˳���ͨ����������
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetEndGeneralAttackCondition()
    {
        // �����Ϳ�Ƭ��
        return true;
    }

    /// <summary>
    /// �˳���ͨ������Ҫ������
    /// </summary>
    public override void AfterGeneralAttack()
    {
        // �����Ϳ�Ƭ��
    }

    /// <summary>
    /// �Ƿ�Ϊ�˺��ж�ʱ�̣���ս����Ϊ���ʵ���˺���Զ�̹���Ϊȷ�����䵯�壩
    /// </summary>
    /// <returns></returns>
    public override bool IsDamageJudgment()
    {
        // �����Ϳ�Ƭ��
        return false;
    }

    /// <summary>
    /// ִ�о���Ĺ�����λ���˺��ж�Ϊ��֮��
    /// </summary>
    public override void ExecuteDamage()
    {
        // �����Ϳ�Ƭ��
    }
}
