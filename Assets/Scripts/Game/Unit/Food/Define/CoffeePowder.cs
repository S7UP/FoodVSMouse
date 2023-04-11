using System.Collections.Generic;

using S7P.Numeric;

using UnityEngine;
/// <summary>
/// ���ȷ�
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
        // ��ȡ100%���ˣ��Լ����߻ҽ���ɱЧ��
        NumericBox.Defense.SetBase(1);
        NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreBombInstantKill, new BoolModifier(true));
        NumericBox.AddDecideModifierToBoolDict(StringManager.Invincibility, new BoolModifier(true));
        // ����ѡȡ
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
    /// �Ƿ�������ͨ����������
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetGeneralAttackCondition()
    {
        return true;
    }

    /// <summary>
    /// ������ͨ��������״̬
    /// </summary>
    public override void BeforeGeneralAttack()
    {
        // �Ե�ǰ��Ƭʩ��Ч��
        ExecuteDamage();
        // �л�Ϊ����������ͼ
        SetActionState(new CastState(this));
    }

    /// <summary>
    /// ��ͨ�����ڼ�
    /// </summary>
    public override void OnGeneralAttack()
    {

    }

    /// <summary>
    /// �˳���ͨ����������
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
    /// �˳���ͨ������Ҫ������
    /// </summary>
    public override void AfterGeneralAttack()
    {
        // ֱ����������
        ExecuteDeath();
    }

    /// <summary>
    /// ִ�о���Ĺ�����λ���˺��ж�Ϊ��֮��
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
            // �Ƴ���Щ���ĸ������Ч��
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
