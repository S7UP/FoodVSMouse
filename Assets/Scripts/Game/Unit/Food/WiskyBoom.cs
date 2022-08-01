using UnityEngine;

public class WiskyBoom : FoodUnit
{
    public override void MInit()
    {
        base.MInit();
        // 获取100%减伤，接近无限的生命值，以及免疫灰烬秒杀效果
        NumericBox.Defense.SetBase(1);
        NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreBombInstantKill, new BoolModifier(true));
        NumericBox.AddDecideModifierToBoolDict(StringManager.Invincibility, new BoolModifier(true));
        NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreFrozen, new BoolModifier(true));
        SetMaxHpAndCurrentHp(float.MaxValue);
    }

    /// <summary>
    /// 根据等级表和等级来更新对应数据
    /// </summary>
    public override void UpdateAttributeByLevel()
    {
        //NumericBox.Attack.SetBase((float)(attr.baseAttrbute.baseAttack + attr.valueList[mLevel]));
    }

    /// <summary>
    /// 判断是否有有效的攻击目标
    /// </summary>
    /// <returns></returns>
    protected override bool IsHasTarget()
    {
        // 即时型炸弹不需要
        return false;
    }

    /// <summary>
    /// 是否满足普通攻击的条件
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetGeneralAttackCondition()
    {
        // 即时型炸弹不需要
        return true;
    }

    /// <summary>
    /// 进入普通攻击动画状态
    /// </summary>
    public override void BeforeGeneralAttack()
    {
        // 切换为攻击动画贴图
        SetActionState(new AttackState(this));
    }

    /// <summary>
    /// 普通攻击期间
    /// </summary>
    public override void OnGeneralAttack()
    {
        if (IsDamageJudgment())
        {
            // 灰烬型卡片直接销毁自身
            ExecuteDeath();
        }
    }

    /// <summary>
    /// 退出普通攻击的条件
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetEndGeneralAttackCondition()
    {
        return animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce();
    }

    /// <summary>
    /// 是否为伤害判定时刻（近战攻击为打出实际伤害，远程攻击为确定发射弹体）
    /// </summary>
    /// <returns></returns>
    public override bool IsDamageJudgment()
    {
        return animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce();
    }

    /// <summary>
    /// 执行具体的攻击，位于伤害判定为真之后
    /// </summary>
    public override void ExecuteDamage()
    {
        // 原地产生一个爆炸效果
        {
            GameObject instance = GameManager.Instance.GetGameObjectResource(FactoryType.GameFactory, "Effect/FireVertical");
            BaseEffect effect = instance.GetComponent<BaseEffect>();
            effect.MInit();
            effect.transform.position = new Vector3(transform.position.x, MapManager.GetRowY(3), transform.position.z);
            GameController.Instance.AddEffect(effect);
        }
        // 添加对应的判定检测器
        {

            GameObject instance = GameManager.Instance.GetGameObjectResource(FactoryType.GameFactory, "AreaEffect/BombAreaEffect");
            BombAreaEffectExecution bombEffect = instance.GetComponent<BombAreaEffectExecution>();
            bombEffect.Init(this, 900, GetRowIndex(), 1, int.MaxValue, 0, 0, false, true);
            bombEffect.transform.position = this.GetPosition();
            GameController.Instance.AddAreaEffectExecution(bombEffect);
        }
    }

    /// <summary>
    /// 亡语
    /// </summary>
    public override void AfterDeath()
    {
        base.AfterDeath();
        // 伤害判定为消失时触发
        ExecuteDamage();
    }
}
