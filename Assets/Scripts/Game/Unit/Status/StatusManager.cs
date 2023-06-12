using UnityEngine;
using S7P.Numeric;
/// <summary>
/// BUFF管理器（提供静态方法）
/// </summary>
public class StatusManager
{
    /// <summary>
    /// 添加免疫定身类效果
    /// </summary>
    public static void AddIgnoreSettleDownBuff(BaseUnit unit, BoolModifier mod)
    {
        unit.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreFrozen, mod);
        unit.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreStun, mod);
    }

    /// <summary>
    /// 移除免疫定身类效果
    /// </summary>
    public static void RemoveIgnoreSettleDownBuff(BaseUnit unit, BoolModifier mod)
    {
        unit.NumericBox.RemoveDecideModifierToBoolDict(StringManager.IgnoreFrozen, mod);
        unit.NumericBox.RemoveDecideModifierToBoolDict(StringManager.IgnoreStun, mod);
    }

    /// <summary>
    /// 添加免疫减速类效果
    /// </summary>
    public static void AddIgnoreSlowDownBuff(BaseUnit unit, BoolModifier mod)
    {
        unit.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreFrozenSlowDown, mod);
        unit.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreSlowDown, mod);
    }

    /// <summary>
    /// 移除免疫减速类效果
    /// </summary>
    public static void RemoveIgnoreSlowDownBuff(BaseUnit unit, BoolModifier mod)
    {
        unit.NumericBox.RemoveDecideModifierToBoolDict(StringManager.IgnoreFrozenSlowDown, mod);
        unit.NumericBox.RemoveDecideModifierToBoolDict(StringManager.IgnoreSlowDown, mod);
    }


    /// <summary>
    /// 移除所有定身类控制效果
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
    /// 移除所有与减速有关的效果
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
    /// 施加基于总攻击速度的DEBUFF（最终乘算）
    /// </summary>
    /// <param name="unit">目标单位</param>
    /// <param name="percent">提升百分比，如25%请写成25</param>
    /// <param name="timeLeft">持续时间（帧）</param>
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
    /// 施加基于基础攻击速度的BUFF
    /// </summary>
    /// <param name="unit">目标单位</param>
    /// <param name="percent">提升百分比，如25%请写成25</param>
    /// <param name="timeLeft">持续时间（帧）</param>
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
    /// 施加基于基础攻击力的BUFF
    /// </summary>
    /// <param name="unit">目标单位</param>
    /// <param name="percent">提升百分比，如25%请写成25</param>
    /// <param name="timeLeft">持续时间（帧）</param>
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
    /// 施加基于总攻击速度的DEBUFF（最终乘算）
    /// </summary>
    /// <param name="unit">目标单位</param>
    /// <param name="percent">提升百分比，如25%请写成25</param>
    /// <param name="timeLeft">持续时间（帧）</param>
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
    /// 添加无敌效果
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
    /// 施加增减伤BUFF
    /// </summary>
    /// <param name="unit">目标单位</param>
    /// <param name="rate">伤害倍率，如增伤20%请写成1.2，减伤20%请写成0.8</param>
    /// <param name="timeLeft">持续时间（帧）</param>
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
    /// 施加持续治疗BUFF（生命回复）
    /// </summary>
    /// <param name="master">施加者</param>
    /// <param name="unit">目标单位</param>
    /// <param name="cureValue">单次回复值</param>
    /// <param name="interval">回复间隔</param>
    /// <param name="timeLeft">持续时间</param>
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
    /// 目标是否受到冰冻减速、冻结类状态（用于处理目标变蓝）
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
