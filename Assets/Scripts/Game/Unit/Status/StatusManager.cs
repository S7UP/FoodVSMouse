using UnityEngine;
using S7P.Numeric;
/// <summary>
/// BUFF���������ṩ��̬������
/// </summary>
public class StatusManager
{
    /// <summary>
    /// ������߶�����Ч��
    /// </summary>
    public static void AddIgnoreSettleDownBuff(BaseUnit unit, BoolModifier mod)
    {
        unit.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreFrozen, mod);
        unit.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreStun, mod);
    }

    /// <summary>
    /// �Ƴ����߶�����Ч��
    /// </summary>
    public static void RemoveIgnoreSettleDownBuff(BaseUnit unit, BoolModifier mod)
    {
        unit.NumericBox.RemoveDecideModifierToBoolDict(StringManager.IgnoreFrozen, mod);
        unit.NumericBox.RemoveDecideModifierToBoolDict(StringManager.IgnoreStun, mod);
    }

    /// <summary>
    /// ������߼�����Ч��
    /// </summary>
    public static void AddIgnoreSlowDownBuff(BaseUnit unit, BoolModifier mod)
    {
        unit.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreFrozenSlowDown, mod);
        unit.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreSlowDown, mod);
    }

    /// <summary>
    /// �Ƴ����߼�����Ч��
    /// </summary>
    public static void RemoveIgnoreSlowDownBuff(BaseUnit unit, BoolModifier mod)
    {
        unit.NumericBox.RemoveDecideModifierToBoolDict(StringManager.IgnoreFrozenSlowDown, mod);
        unit.NumericBox.RemoveDecideModifierToBoolDict(StringManager.IgnoreSlowDown, mod);
    }


    /// <summary>
    /// �Ƴ����ж��������Ч��
    /// </summary>
    /// <param name="unit"></param>
    public static void RemoveAllSettleDownDebuff(BaseUnit unit)
    {
        if (unit.IsAlive())
        {
            unit.statusAbilityManager.EndNoCountUniqueStatusAbility(StringManager.Stun);
            unit.statusAbilityManager.EndNoCountUniqueStatusAbility(StringManager.Frozen);
        }  
    }

    /// <summary>
    /// �Ƴ�����������йص�Ч��
    /// </summary>
    public static void RemoveAllSlowDownDebuff(BaseUnit unit)
    {
        foreach (var s in unit.statusAbilityManager.GetAllStatusAbility())
        {
            if(s is SlowStatusAbility)
            {
                s.TryEndActivate();
            }
        }
    }

    /// <summary>
    /// ʩ�ӻ����ܹ����ٶȵ�DEBUFF�����ճ��㣩
    /// </summary>
    /// <param name="unit">Ŀ�굥λ</param>
    /// <param name="percent">�����ٷֱȣ���25%��д��25</param>
    /// <param name="timeLeft">����ʱ�䣨֡��</param>
    public static void AddFinalAttackSpeedDeBuff(BaseUnit unit, float percent, int timeLeft)
    {
        FloatModifier mod = new FloatModifier(percent);
        CustomizationTask t = new CustomizationTask();
        t.AddOnEnterAction(delegate {
            unit.NumericBox.AttackSpeed.AddFinalPctAddModifier(mod);
        });
        t.AddTaskFunc(delegate {
            if(timeLeft > 0)
            {
                timeLeft--;
                return false;
            }
            else
            {
                return true;
            }
        });
        t.AddOnExitAction(delegate {
            unit.NumericBox.AttackSpeed.RemoveFinalPctAddModifier(mod);
        });
        unit.AddTask(t);
    }

    /// <summary>
    /// ʩ�ӻ��ڻ��������ٶȵ�BUFF
    /// </summary>
    /// <param name="unit">Ŀ�굥λ</param>
    /// <param name="percent">�����ٷֱȣ���25%��д��25</param>
    /// <param name="timeLeft">����ʱ�䣨֡��</param>
    public static void AddBaseAttackSpeedBuff(BaseUnit unit, float percent, int timeLeft)
    {
        FloatModifier mod = new FloatModifier(percent);
        CustomizationTask t = new CustomizationTask();
        t.AddOnEnterAction(delegate {
            unit.NumericBox.AttackSpeed.AddPctAddModifier(mod);
        });
        t.AddTaskFunc(delegate {
            if (timeLeft > 0)
            {
                timeLeft--;
                return false;
            }
            else
            {
                return true;
            }
        });
        t.AddOnExitAction(delegate {
            unit.NumericBox.AttackSpeed.RemovePctAddModifier(mod);
        });
        unit.AddTask(t);
    }

    /// <summary>
    /// ʩ�ӻ��ڻ�����������BUFF
    /// </summary>
    /// <param name="unit">Ŀ�굥λ</param>
    /// <param name="percent">�����ٷֱȣ���25%��д��25</param>
    /// <param name="timeLeft">����ʱ�䣨֡��</param>
    public static void AddBaseAttackBuff(BaseUnit unit, float percent, int timeLeft)
    {
        FloatModifier mod = new FloatModifier(percent);
        CustomizationTask t = new CustomizationTask();
        t.AddOnEnterAction(delegate {
            unit.NumericBox.Attack.AddPctAddModifier(mod);
        });
        t.AddTaskFunc(delegate {
            if (timeLeft > 0)
            {
                timeLeft--;
                return false;
            }
            else
            {
                return true;
            }
        });
        t.AddOnExitAction(delegate {
            unit.NumericBox.Attack.RemovePctAddModifier(mod);
        });
        unit.AddTask(t);
    }

    /// <summary>
    /// ʩ�ӻ����ܹ����ٶȵ�DEBUFF�����ճ��㣩
    /// </summary>
    /// <param name="unit">Ŀ�굥λ</param>
    /// <param name="percent">�����ٷֱȣ���25%��д��25</param>
    /// <param name="timeLeft">����ʱ�䣨֡��</param>
    public static void AddFinalAttackDeBuff(BaseUnit unit, float percent, int timeLeft)
    {
        FloatModifier mod = new FloatModifier(percent);
        CustomizationTask t = new CustomizationTask();
        t.AddOnEnterAction(delegate {
            unit.NumericBox.Attack.AddFinalPctAddModifier(mod);
        });
        t.AddTaskFunc(delegate {
            if (timeLeft > 0)
            {
                timeLeft--;
                return false;
            }
            else
            {
                return true;
            }
        });
        t.AddOnExitAction(delegate {
            unit.NumericBox.Attack.RemoveFinalPctAddModifier(mod);
        });
        unit.AddTask(t);
    }

    /// <summary>
    /// ����޵�Ч��
    /// </summary>
    public static void AddInvincibilityBuff(BaseUnit unit, int timeLeft)
    {
        BoolModifier mod = new BoolModifier(true);
        CustomizationTask t = new CustomizationTask();
        t.AddOnEnterAction(delegate {
            unit.NumericBox.AddDecideModifierToBoolDict(StringManager.Invincibility, mod);
            BaseEffect e = BaseEffect.CreateInstance(GameManager.Instance.GetSprite("Effect/Shield"));
            GameController.Instance.AddEffect(e);
            unit.mEffectController.AddEffectToDict("InvincibilityBuff", e, Vector2.zero);
        });
        t.AddTaskFunc(delegate {
            if (timeLeft > 0)
            {
                timeLeft--;
                return false;
            }
            else
            {
                return true;
            }
        });
        t.AddOnExitAction(delegate {
            unit.NumericBox.RemoveDecideModifierToBoolDict(StringManager.Invincibility, mod);
            if (!unit.NumericBox.GetBoolNumericValue(StringManager.Invincibility))
            {
                // unit.RemoveEffect("InvincibilityBuff");
                unit.mEffectController.RemoveEffectFromDict("InvincibilityBuff");
            }
        });
        unit.AddTask(t);
    }

    /// <summary>
    /// ʩ��������BUFF
    /// </summary>
    /// <param name="unit">Ŀ�굥λ</param>
    /// <param name="rate">�˺����ʣ�������20%��д��1.2������20%��д��0.8</param>
    /// <param name="timeLeft">����ʱ�䣨֡��</param>
    public static void AddDamageRateBuff(BaseUnit unit, float rate, int timeLeft)
    {
        FloatModifier mod = new FloatModifier(rate);
        CustomizationTask t = new CustomizationTask();
        t.AddOnEnterAction(delegate {
            unit.NumericBox.DamageRate.AddModifier(mod);
        });
        t.AddTaskFunc(delegate {
            if (timeLeft > 0)
            {
                timeLeft--;
                return false;
            }
            else
            {
                return true;
            }
        });
        t.AddOnExitAction(delegate {
            unit.NumericBox.DamageRate.RemoveModifier(mod);
        });
        unit.AddTask(t);
    }


    /// <summary>
    /// ʩ�ӳ�������BUFF�������ظ���
    /// </summary>
    /// <param name="master">ʩ����</param>
    /// <param name="unit">Ŀ�굥λ</param>
    /// <param name="cureValue">���λظ�ֵ</param>
    /// <param name="interval">�ظ����</param>
    /// <param name="timeLeft">����ʱ��</param>
    public static void AddCureBuff(BaseUnit master, BaseUnit unit, float cureValue, int interval, int timeLeft)
    {
        int next = interval;
        CustomizationTask t = new CustomizationTask();
        t.AddOnEnterAction(delegate {
            
        });
        t.AddTaskFunc(delegate {
            next--;
            if(next <= 0)
            {
                new CureAction(CombatAction.ActionType.GiveCure, master, unit, cureValue).ApplyAction();
                next += interval;
            }
            if (timeLeft > 0)
            {
                timeLeft--;
                return false;
            }
            else
            {
                return true;
            }
        });
        t.AddOnExitAction(delegate {
            
        });
        unit.AddTask(t);
    }

    /// <summary>
    /// Ŀ���Ƿ��ܵ��������١�������״̬�����ڴ���Ŀ�������
    /// </summary>
    public static bool IsUnitFrozen(BaseUnit unit)
    {
        if (unit.NumericBox.GetBoolNumericValue(StringManager.Frozen) || unit.NumericBox.GetBoolNumericValue(StringManager.FrozenSlowDown))
        {
            return true;
        }
        return false;
    }
}
