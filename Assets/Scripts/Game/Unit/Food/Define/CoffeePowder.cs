using System.Collections.Generic;

using S7P.Numeric;

using UnityEngine;
/// <summary>
/// 咖啡粉
/// </summary>
public class CoffeePowder : FoodUnit
{
    private static FloatModifier attackSpeedModifier = new FloatModifier(10);
    private static FloatModifier attackModifier = new FloatModifier(10);
    private static BoolModifier boolModifier = new BoolModifier(true);
    private int totalTime;
    private const string BuffKey = "CoffeePowder_Buff";
    private static RuntimeAnimatorController Run;

    public override void Awake()
    {
        if (Run == null)
            Run = GameManager.Instance.GetRuntimeAnimatorController("Food/3/0");
        base.Awake();
    }

    public override void MInit()
    {
        totalTime = 0;
        base.MInit();
        // 获取100%减伤，以及免疫灰烬秒杀效果
        NumericBox.Defense.SetBase(1);
        NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreBombInstantKill, new BoolModifier(true));
        NumericBox.AddDecideModifierToBoolDict(StringManager.Invincibility, new BoolModifier(true));
        // 不可选取
        CloseCollision();
    }

    public override void UpdateAttributeByLevel()
    {
        totalTime = Mathf.FloorToInt(60 * attr.valueList[mLevel]);
    }

    public override void OnCastStateEnter()
    {
        animatorController.Play("Cast");
    }

    /// <summary>
    /// 是否满足普通攻击的条件
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetGeneralAttackCondition()
    {
        return true;
    }

    /// <summary>
    /// 进入普通攻击动画状态
    /// </summary>
    public override void BeforeGeneralAttack()
    {
        // 对当前格卡片施加效果
        ExecuteDamage();
        // 切换为攻击动画贴图
        SetActionState(new CastState(this));
    }

    /// <summary>
    /// 普通攻击期间
    /// </summary>
    public override void OnGeneralAttack()
    {

    }

    /// <summary>
    /// 退出普通攻击的条件
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetEndGeneralAttackCondition()
    {
        AnimatorStateRecorder a = animatorController.GetCurrentAnimatorStateRecorder();
        if (a != null)
        {
            return a.IsFinishOnce();
        }
        return false;
    }

    /// <summary>
    /// 退出普通攻击后要做的事
    /// </summary>
    public override void AfterGeneralAttack()
    {
        // 直接销毁自身
        ExecuteDeath();
    }

    /// <summary>
    /// 执行具体的攻击，位于伤害判定为真之后
    /// </summary>
    public override void ExecuteDamage()
    {
        int count = 0;
        Vector2 pos = transform.position;
        List<Vector2> effPosList = new List<Vector2>();

        RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(pos, 2.75f, 2.75f, "EnemyAllyGrid");
        r.isAffectGrid = true;
        r.SetInstantaneous();
        r.SetOnGridEnterAction((g) => {
            bool flag = false;
            foreach (var u in g.GetAttackableFoodUnitList())
            {
                ITask task = u.GetTask(BuffKey);
                if (task == null)
                {
                    task = new BuffTask(u, totalTime);
                    u.AddUniqueTask(BuffKey, task);
                }
                else
                {
                    BuffTask buffTask = task as BuffTask;
                    buffTask.timeLeft = Mathf.Max(buffTask.timeLeft, totalTime);
                }
                flag = true;
            }
            if (flag)
            {
                effPosList.Add(g.transform.position);
                count++;
            }
        });
        r.AddBeforeDestoryAction(delegate {
            float reply = (9 - count) * 50;
            if(reply > 0)
                SmallStove.CreateAddFireEffect(pos, reply);
            foreach (var v2 in effPosList)
            {
                BaseEffect eff = BaseEffect.CreateInstance(Run, null, "Cast", null, false);
                GameController.Instance.AddEffect(eff);
                eff.transform.position = v2;
            }
        });
        GameController.Instance.AddAreaEffectExecution(r);
    }


    private class BuffTask : ITask
    {
        private BaseUnit master;
        public int timeLeft;

        public BuffTask(BaseUnit master, int timeLeft)
        {
            this.master = master;
            this.timeLeft = timeLeft;
        }

        public void OnEnter()
        {
            // 移除这些卡的负面控制效果
            StatusManager.RemoveAllSettleDownDebuff(master);
            master.NumericBox.AttackSpeed.AddPctAddModifier(attackSpeedModifier);
            master.NumericBox.Attack.AddPctAddModifier(attackModifier);
            StatusManager.AddIgnoreSettleDownBuff(master, boolModifier);
        }

        public void OnUpdate()
        {
            timeLeft--;
        }

        public bool IsMeetingExitCondition()
        {
            return timeLeft <= 0;
        }

        public void OnExit()
        {
            master.NumericBox.AttackSpeed.RemovePctAddModifier(attackSpeedModifier);
            master.NumericBox.Attack.RemovePctAddModifier(attackModifier);
            StatusManager.RemoveIgnoreSettleDownBuff(master, boolModifier);
        }
    }
}
