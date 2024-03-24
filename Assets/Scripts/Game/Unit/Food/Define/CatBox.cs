using S7P.Numeric;

using UnityEngine;

public class CatBox : FoodUnit
{
    private RetangleAreaEffectExecution checkArea;

    private bool canTrigger = false;

    public override void MInit()
    {
        base.MInit();
        AddActionPointListener(ActionPointType.PostReceiveDamage, (combat) => {
            if (!(combat is DamageAction) || !canTrigger)
                return;
            SetActionState(new CastState(this));
        });
    }

    /// <summary>
    /// 根据等级表和等级来更新对应数据
    /// </summary>
    public override void UpdateAttributeByLevel()
    {
        NumericBox.Hp.SetBase((float)(attr.baseAttrbute.baseHP + attr.valueList[mLevel]));
        SetMaxHpAndCurrentHp(NumericBox.Hp.Value);
    }

    public override void LoadSkillAbility()
    {

    }

    public override void OnIdleStateEnter()
    {
        canTrigger = true;
        animatorController.Play("Idle", true);
    }

    public override void OnIdleStateContinue()
    {
        canTrigger = true;
    }

    public override void OnIdleStateInterrupt()
    {
        canTrigger = false;
    }

    public override void OnIdleStateExit()
    {
        canTrigger = false;
    }


    public override void OnCastStateEnter()
    {
        animatorController.Play("Attack");
        CreateCheckArea();
    }

    public override void OnCastStateContinue()
    {
        CreateCheckArea();
    }

    public override void OnCastState()
    {
        if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
            SetActionState(new IdleState(this));
    }

    public override void OnCastStateInterrupt()
    {
        checkArea.MDestory();
        checkArea = null;
    }

    public override void OnCastStateExit()
    {
        checkArea.MDestory();
        checkArea = null;
    }


    /// <summary>
    /// 生成检测区域
    /// </summary>
    private void CreateCheckArea()
    {
        RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(transform.position, new Vector2(0.5f * MapManager.gridWidth, 0.5f * MapManager.gridHeight), "ItemCollideEnemy");
        r.isAffectMouse = true;
        r.SetAffectHeight(0);
        r.AddEnemyEnterConditionFunc((m) => {
            return !m.IsBoss() && MouseManager.IsGeneralMouse(m) && UnitManager.CanBeSelectedAsTarget(this, m) && !m.NumericBox.GetBoolNumericValue(StringManager.BeFrightened) && m.CanDrivenAway();
        });
        r.SetOnEnemyEnterAction((m) => {
            // 添加一个被惊吓的标签
            m.NumericBox.AddDecideModifierToBoolDict(StringManager.BeFrightened, new BoolModifier(true));
            m.DrivenAway();
            // 一转后附带三秒晕眩
            if (mShape >= 1)
                m.AddNoCountUniqueStatusAbility(StringManager.Stun, new StunStatusAbility(m, 180, false));
        });
        GameController.Instance.AddAreaEffectExecution(r);

        CustomizationTask task = new CustomizationTask();
        task.AddTaskFunc(delegate {
            r.transform.position = transform.position;
            return !IsAlive();
        });
        task.AddOnExitAction(delegate {
            r.MDestory();
        });
        r.taskController.AddTask(task);

        checkArea = r;
    }
}
