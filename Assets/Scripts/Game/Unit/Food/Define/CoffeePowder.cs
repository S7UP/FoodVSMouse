using System;
using System.Collections.Generic;

using Environment;
using GameNormalPanel_UI;

using S7P.Numeric;

using UnityEngine;

using static UnityEngine.Rendering.DebugUI;
/// <summary>
/// ���ȷ�
/// </summary>
public class CoffeePowder : FoodUnit
{
    public static Sprite Coffee_Icon_Sprite;
    private static FloatModifier attackSpeedModifier = new FloatModifier(10);
    private static FloatModifier attackModifier = new FloatModifier(10);
    private static BoolModifier boolModifier = new BoolModifier(true);
    private int totalTime;
    private const string BuffKey = "CoffeePowder_Buff";
    private static RuntimeAnimatorController Run;

    public override void Awake()
    {
        if (Run == null)
        {
            Run = GameManager.Instance.GetRuntimeAnimatorController("Food/3/0");
            Coffee_Icon_Sprite = GameManager.Instance.GetSprite("UI/GameNormalPanel/Ring/Icon/Coffee");
        }
        base.Awake();
    }

    public override void MInit()
    {
        totalTime = 0;
        base.MInit();
        // ��ȡ100%����
        NumericBox.Defense.SetBase(1);
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
        r.isAffectCharacter = true;
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
        r.SetOnCharacterEnterAction((u) => {
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
            count++;
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
        private RingUI ru;
        private int totalTime;
        public int timeLeft;

        public BuffTask(BaseUnit u, int timeLeft)
        {
            master = u;
            totalTime = timeLeft;
            this.timeLeft = timeLeft;

            GameNormalPanel panel = GameNormalPanel.Instance;
            ru = RingUI.GetInstance(0.15f * Vector2.one);
            Action<BaseUnit> beforeDestoryAction = delegate { if (ru != null && ru.IsValid()) ru.MDestory(); };
            u.AddOnDestoryAction(beforeDestoryAction);
            ru.AddBeforeDestoryAction(delegate { ru = null; });
            panel.AddUI(ru);

            {
                float r = 255f / 255;
                float g = 204f / 255;
                float b = 122f / 255;
                ru.SetIcon(Coffee_Icon_Sprite);
                ru.SetPercent(0);
                ru.SetColor(new Color(r, g, b, 1));
                ru.transform.position = u.transform.position + 0.25f * MapManager.gridHeight * Vector3.down + 0.25f * MapManager.gridWidth * Vector3.left;
            }
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
            ru.transform.position = master.transform.position + 0.25f * MapManager.gridHeight * Vector3.down + 0.25f * MapManager.gridWidth * Vector3.left;
            ru.SetPercent((float)timeLeft/totalTime);
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
            ru.MDestory();
        }

        public void ShutDown()
        {
            
        }

        public bool IsClearWhenDie()
        {
            return true;
        }
    }
}
