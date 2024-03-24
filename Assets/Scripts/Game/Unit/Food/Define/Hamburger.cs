using UnityEngine;

public class Hamburger : FoodUnit
{
    private int timeLeft = 0;
    private float fireRecord = 0;
    private RetangleAreaEffectExecution checkArea;

    public override void MInit()
    {
        timeLeft = 0;
        fireRecord = 0;
        base.MInit();
        // 生成检测区域
        CreateCheckArea();
    }

    public override void MDestory()
    {
        base.MDestory();
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
        if(timeLeft <= 0)
            animatorController.Play("Idle", true);
        else
            animatorController.Play("Eating", true);
    }

    public override void OnIdleState()
    {
        if (timeLeft == 1)
        {
            timeLeft--;
            fireRecord += 0.125f;
            SetActionState(new TransitionState(this));
        }
        else if(timeLeft > 0)
        {
            timeLeft--;
            fireRecord += 0.125f;
        }
        else if (checkArea!=null && checkArea.mouseUnitList.Count > 0)
        {
            SetActionState(new CastState(this));
        }
    }

    public override void OnCastStateEnter()
    {
        animatorController.Play("Attack");
        GameManager.Instance.audioSourceController.PlayEffectMusic("Bigchomp");
        mAttackFlag = true;
    }

    public override void OnCastState()
    {
        if(animatorController.GetCurrentAnimatorStateRecorder().GetNormalizedTime() > attackPercent && mAttackFlag)
        {
            mAttackFlag = false;
            float totalHp = 0;
            foreach (var m in checkArea.mouseUnitList.ToArray())
            {
                totalHp += m.mCurrentHp;
                m.MDestory();
            }
            timeLeft = Mathf.CeilToInt(Mathf.Min(3600, totalHp/45*60));
            checkArea.MDestory();
            checkArea = null;
        }
        else if(animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
        {
            // 如果没吃到东西需要重新给它吃东西的检测圈
            if (timeLeft == 0)
                CreateCheckArea();
            SetActionState(new IdleState(this));
        }
    }

    public override void OnTransitionStateEnter()
    {
        animatorController.Play("EatOver");
    }

    public override void OnTransitionState()
    {
        if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
        {
            SetActionState(new IdleState(this));
        }
    }

    public override void OnTransitionStateExit()
    {
        CreateCheckArea();
        SmallStove.CreateAddFireEffect(transform.position, fireRecord);
        fireRecord = 0;
    }

    /// <summary>
    /// 生成检测区域
    /// </summary>
    private void CreateCheckArea()
    {
        RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(transform.position + 0.5f * MapManager.gridWidth * Vector3.right, new Vector2(2f * MapManager.gridWidth, 0.5f * MapManager.gridHeight), "ItemCollideEnemy");
        r.isAffectMouse = true;
        r.SetAffectHeight(0);
        r.AddEnemyEnterConditionFunc((m) => {
            return !m.IsBoss() && MouseManager.IsGeneralMouse(m) && UnitManager.CanBeSelectedAsTarget(this, m);
        });
        GameController.Instance.AddAreaEffectExecution(r);

        CustomizationTask task = new CustomizationTask();
        task.AddTaskFunc(delegate {
            r.transform.position = transform.position + 0.5f * MapManager.gridWidth * Vector3.right;
            return !IsAlive();
        });
        task.AddOnExitAction(delegate {
            r.MDestory();
        });
        r.taskController.AddTask(task);

        checkArea = r;
    }
}
