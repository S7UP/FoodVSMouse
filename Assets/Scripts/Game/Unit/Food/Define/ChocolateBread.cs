using S7P.Numeric;

using System;
using System.Collections.Generic;
using UnityEngine;

using static UnityEngine.Rendering.DebugUI;
/// <summary>
/// �ɿ������
/// </summary>
public class ChocolateBread : FoodUnit
{
    private static Sprite Shield_Sprite;
    private FloatModifier costMod = new FloatModifier(-20f / 7 / 60);

    private int mHertIndex; // ���˽׶� 0������ 1��С�� 2������
    private List<float> mHertRateList = new List<float>()
    {
        0.67f, 0.33f
    };
    private List<RetangleAreaEffectExecution> checkAreaList = new List<RetangleAreaEffectExecution>();

    public override void Awake()
    {
        if (Shield_Sprite == null)
            Shield_Sprite = GameManager.Instance.GetSprite("Effect/Shield");
        base.Awake();
    }

    public override void MInit()
    {
        mHertIndex = 0;
        base.MInit();
        // �ڽ������ƽ���֮�󣬸���������ͼ״̬
        AddActionPointListener(ActionPointType.PostReceiveCure, delegate { UpdateHertMap(); });
        AddActionPointListener(ActionPointType.PreReceiveDamage, (act) => { 
            if(act is DamageAction)
            {
                DamageAction dmgAction = act as DamageAction;
                if(dmgAction.DamageValue > 0.6f * mMaxHp)
                {
                    dmgAction.DamageValue = 0.6f * mMaxHp;
                }
            }
        });
        Vector3[] v3Array = new Vector3[] {
            new Vector2(-MapManager.gridWidth, 0), new Vector2(-2*MapManager.gridWidth, 0),
            new Vector2(-MapManager.gridWidth, MapManager.gridHeight), new Vector2(-MapManager.gridWidth, -MapManager.gridHeight),
        };
        // ����������򣬼���������Ƿ��п����ṩ�ӻ���Ŀ��
        foreach (var v3 in v3Array)
        {
            RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(transform.position + v3, 0.5f, 0.5f, "ItemCollideAlly");
            r.transform.SetParent(transform);
            r.transform.localPosition = v3;
            checkAreaList.Add(r);
            r.isAffectCharacter = true;
            r.isAffectFood = true;
            Action<BaseUnit> StayAction = (u) =>
            {
                if (u.mType == (int)FoodNameTypeMap.ChocolateBread || u.GetTask("ChocolateBread_Protect") != null)
                    return;
                u.taskController.AddUniqueTask("ChocolateBread_Protect", new ProtectedTask(this, u));
            };
            r.SetOnFoodStayAction(StayAction);
            r.SetOnCharacterStayAction(StayAction);
            Action<BaseUnit> ExitAction = (u) =>
            {
                if (u.GetTask("ChocolateBread_Protect") != null)
                {
                    ProtectedTask t = u.GetTask("ChocolateBread_Protect") as ProtectedTask;
                    if (t.GetMaster() == this)
                        t.SetCanExit();
                }
            };
            r.SetOnFoodExitAction(ExitAction);
            r.SetOnCharacterExitAction(ExitAction);
            GameController.Instance.AddAreaEffectExecution(r);

            Action<BaseUnit> action = delegate { r.MDestory(); };
            AddBeforeDeathEvent(delegate {
                action(this);
                RemoveOnDestoryAction(action);
            });
            AddOnDestoryAction(action);
        }

        if(mShape >= 1)
        {
            int timeLeft = 60;
            CustomizationTask t = new CustomizationTask();
            t.AddTaskFunc(delegate {
                timeLeft--;
                if(timeLeft <= 0)
                {
                    new CureAction(CombatAction.ActionType.GiveCure, this, this, 0.02f * mMaxHp).ApplyAction();
                    timeLeft += 60;
                }
                return false;
            });
            AddTask(t);
        }
        GameController.Instance.AddCostResourceModifier("Fire", costMod);
    }

    public override void MUpdate()
    {
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

    /// <summary>
    /// �ӻ�����
    /// </summary>
    private class ProtectedTask : ITask
    {
        private bool canExit;
        private BaseUnit master; // �����ṩ��
        private BaseUnit target; // �ܱ����Ķ���
        private Action<CombatAction> action;

        private BaseEffect eff; // ��Ч

        public ProtectedTask(BaseUnit master, BaseUnit target)
        {
            this.master = master;
            this.target = target;
            action = (act) => {
                if(act is DamageAction)
                {
                    DamageAction DmgAction = act as DamageAction;
                    float temp_dmg = DmgAction.DamageValue;
                    DmgAction.DamageValue = 0;
                    new DamageAction(DmgAction.mActionType, DmgAction.Creator, master, temp_dmg).ApplyAction();
                }
            };

            // ��Ч����
            {
                eff = BaseEffect.CreateInstance(Shield_Sprite);
                eff.spriteRenderer.sortingLayerName = target.GetSpriteRenderer().sortingLayerName;
                eff.spriteRenderer.sortingOrder = target.GetSpriteRenderer().sortingOrder + 1;
                GameController.Instance.AddEffect(eff);
                target.mEffectController.AddEffectToDict("ChocolateBread_Protect", eff, Vector2.zero);
                eff.transform.localPosition = Vector2.zero;
            }
        }

        public void OnEnter()
        {
            target.actionPointController.AddListener(ActionPointType.PreReceiveDamage, action);
        }
        public void OnUpdate()
        {
            // ������Ч��ʾ
            eff.spriteRenderer.sortingLayerName = target.GetSpriteRenderer().sortingLayerName;
            eff.spriteRenderer.sortingOrder = target.GetSpriteRenderer().sortingOrder + 1;
        }
        public void OnExit()
        {
            target.actionPointController.RemoveListener(ActionPointType.PreReceiveDamage, action);
            // ��Ч�Ƴ�
            eff.ExecuteDeath();
        }
        public bool IsMeetingExitCondition()
        {
            return !master.IsAlive() || !target.IsAlive() || canExit;
        }
        public bool IsClearWhenDie()
        {
            return true;
        }
        public void ShutDown()
        {
            
        }

        // ��������
        public void SetCanExit()
        {
            canExit = true;
        }

        public BaseUnit GetMaster()
        {
            return master;
        }
    }

}
