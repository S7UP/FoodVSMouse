using S7P.Numeric;

using System;
using System.Collections.Generic;
using UnityEngine;

using static UnityEngine.Rendering.DebugUI;
/// <summary>
/// 巧克力面包
/// </summary>
public class ChocolateBread : FoodUnit
{
    private static Sprite Shield_Sprite;
    private FloatModifier costMod = new FloatModifier(-20f / 7 / 60);

    private int mHertIndex; // 受伤阶段 0：正常 1：小伤 2：重伤
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
        // 在接收治疗结算之后，更新受伤贴图状态
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
        // 产生检测区域，检测区域内是否有可以提供庇护的目标
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
    /// 根据等级表和等级来更新对应数据
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
    /// 当受伤或者被治疗时，更新单位贴图状态
    /// </summary>
    protected void UpdateHertMap()
    {
        // 要是死了的话就免了吧
        if (isDeathState)
            return;

        // 是否要切换控制器的flag
        bool flag = false;
        // 恢复到上一个受伤贴图检测
        while (mHertIndex > 0 && GetHeathPercent() > mHertRateList[mHertIndex - 1])
        {
            mHertIndex--;
            flag = true;
        }
        // 下一个受伤贴图的检测
        while (mHertIndex < mHertRateList.Count && GetHeathPercent() <= mHertRateList[mHertIndex])
        {
            mHertIndex++;
            flag = true;
        }
        // 有切换通知时才切换
        if (flag)
        {
            animatorController.Play("Idle" + mHertIndex);
        }
    }


    /////////////////////////////////以下功能均失效，不需要往下翻看/////////////////////////////////////

    /// <summary>
    /// 判断是否有有效的攻击目标
    /// </summary>
    /// <returns></returns>
    protected override bool IsHasTarget()
    {
        // 功能型卡片不需要
        return false;
    }

    /// <summary>
    /// 是否满足普通攻击的条件
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetGeneralAttackCondition()
    {
        // 功能型卡片不需要
        return true;
    }

    /// <summary>
    /// 进入普通攻击动画状态
    /// </summary>
    public override void BeforeGeneralAttack()
    {
        // 功能型卡片无攻击状态
    }

    /// <summary>
    /// 普通攻击期间
    /// </summary>
    public override void OnGeneralAttack()
    {
        // 功能型卡片无
    }

    /// <summary>
    /// 退出普通攻击的条件
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetEndGeneralAttackCondition()
    {
        // 功能型卡片无
        return true;
    }

    /// <summary>
    /// 退出普通攻击后要做的事
    /// </summary>
    public override void AfterGeneralAttack()
    {
        // 功能型卡片无
    }

    /// <summary>
    /// 是否为伤害判定时刻（近战攻击为打出实际伤害，远程攻击为确定发射弹体）
    /// </summary>
    /// <returns></returns>
    public override bool IsDamageJudgment()
    {
        // 功能型卡片无
        return false;
    }

    /// <summary>
    /// 执行具体的攻击，位于伤害判定为真之后
    /// </summary>
    public override void ExecuteDamage()
    {
        // 功能型卡片无
    }

    /// <summary>
    /// 庇护任务
    /// </summary>
    private class ProtectedTask : ITask
    {
        private bool canExit;
        private BaseUnit master; // 保护提供者
        private BaseUnit target; // 受保护的对象
        private Action<CombatAction> action;

        private BaseEffect eff; // 特效

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

            // 特效生成
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
            // 更新特效显示
            eff.spriteRenderer.sortingLayerName = target.GetSpriteRenderer().sortingLayerName;
            eff.spriteRenderer.sortingOrder = target.GetSpriteRenderer().sortingOrder + 1;
        }
        public void OnExit()
        {
            target.actionPointController.RemoveListener(ActionPointType.PreReceiveDamage, action);
            // 特效移除
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

        // 供外界调用
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
