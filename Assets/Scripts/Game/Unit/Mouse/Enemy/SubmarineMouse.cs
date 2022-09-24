using UnityEngine;
/// <summary>
/// Ǳˮͧ��
/// </summary>
public class SubmarineMouse : MouseUnit, IInWater
{
    private bool isEnterWater; // ��TransitionStateʱʹ�ã��ж��ǲ�����ˮ�������ǳ�ˮ����
    private FloatModifier SpeedModifier = new FloatModifier(50f); // �������Σ���������̬ʱ����ˮ��ʱ���50%���ټӳɣ�
    private bool isDieInWater; // �Ƿ���ˮ������

    public override void MInit()
    {
        isEnterWater = false;
        isDieInWater = false;
        base.MInit();
        // ���߻ҽ���ɱЧ������������Ч��������Ч����ˮʴЧ��
        NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreBombInstantKill, new BoolModifier(true));
        NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreFrozen, new BoolModifier(true));
        NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreFrozenSlowDown, new BoolModifier(true));
        NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreWaterGridState, new BoolModifier(true));
    }


    public override void SetCollider2DParam()
    {
        mBoxCollider2D.offset = new Vector2(1f/3 * MapManager.gridWidth, 0);
        mBoxCollider2D.size = new Vector2(1.0f * MapManager.gridWidth, 0.49f * MapManager.gridHeight);
    }

    public void OnEnterWater()
    {
        isEnterWater = true;
        // ����һ�����˽׶������ټӳ�
        if (mHertIndex == 0)
        {
            NumericBox.MoveSpeed.RemovePctAddModifier(SpeedModifier);
            NumericBox.MoveSpeed.AddPctAddModifier(SpeedModifier);
        }
        SetActionState(new TransitionState(this));
    }

    public void OnStayWater()
    {

    }

    public void OnExitWater()
    {
        // ��ˮҲҪ����������п������������DEBUFFЧ��Ȼ��ǿ�Ƴ�ˮ��
        if (isDeathState)
        {
            isEnterWater = false;
            isDieInWater = true;
        }
        else
        {
            SetNoCollideAllyUnit();
            isEnterWater = false;
            NumericBox.MoveSpeed.RemovePctAddModifier(SpeedModifier);
            SetActionState(new TransitionState(this));
        }
    }

    public override void UpdateRuntimeAnimatorController()
    {
        base.UpdateRuntimeAnimatorController();
        // ���л������˽׶�ʱȡ�����ټӳɣ������������̬�л�Ϊ��̬ͨ���ж�һ���Ƿ���ˮ�У�����ڵĻ�������ټӳɻص�
        if(mHertIndex == 0 && isEnterWater)
        {
            NumericBox.MoveSpeed.RemovePctAddModifier(SpeedModifier);
            NumericBox.MoveSpeed.AddPctAddModifier(SpeedModifier);
        }else if(mHertIndex > 0)
            NumericBox.MoveSpeed.RemovePctAddModifier(SpeedModifier);
    }

    public override void OnTransitionStateEnter()
    {
        if (isEnterWater)
            animatorController.Play("Enter");
        else
            animatorController.Play("Exit");
    }

    public override void OnTransitionState()
    {
        if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
        {
            SetActionState(new MoveState(this));
        }
    }

    public override void OnMoveStateEnter()
    {
        if (isEnterWater)
            animatorController.Play("Move1", true);
        else
            animatorController.Play("Move0", true);
    }

    /// <summary>
    /// ֻ����ˮ��ʱ�ſ��Ա��赲���ж��������Ƿ���ˮBUFF��
    /// </summary>
    /// <param name="unit"></param>
    /// <returns></returns>
    public override bool CanBlock(BaseUnit unit)
    {
        StatusAbility s = unit.GetUniqueStatus(StringManager.WaterGridState);
        return s!=null && base.CanBlock(unit);
    }

    public override void OnDieStateEnter()
    {
        if(isDieInWater)
            animatorController.Play("Die1");
        else
            animatorController.Play("Die0");
    }

    /// <summary>
    /// �����ѷ���λ����ʳ���������������ײ�ж�ʱ������һ��ˮ���ؾߣ�
    /// </summary>
    public override void OnAllyCollision(BaseUnit unit)
    {
        // ��Ȿ����ʳ����ܻ����ȼ���λ������ˮ���ؾߣ�
        BaseUnit target = unit.GetGrid().GetHighestAttackPriorityUnitIncludeWaterVehicle();
        if (!isBlock && UnitManager.CanBlock(this, target)) // ���˫���ܷ����赲
        {
            isBlock = true;
            mBlockUnit = target;
        }
    }

    /// <summary>
    /// ���з�ʽ����һ��ˮ���ؾ�
    /// </summary>
    /// <returns></returns>
    public override bool IsHasTarget()
    {
        if (isBlock && mBlockUnit.IsAlive())
        {
            // ��Ŀ�������ڸ��ӣ���Ŀ���л�ΪĿ�����ڸ����߹������ȼ�Ŀ��
            BaseGrid g = mBlockUnit.GetGrid();
            if (g != null)
            {
                mBlockUnit = g.GetHighestAttackPriorityUnitIncludeWaterVehicle();
            }
            return true;
        }
        return false;
    }

    /// <summary>
    /// ȭ��ȭ���󹥻���������AOE�˺�Ч��
    /// </summary>
    public override void ExecuteDamage()
    {
        if(mShape==2 || mShape == 5)
        {
            if (IsHasTarget())
            {
                BaseGrid g = GetCurrentTarget().GetGrid();
                if (g != null)
                {
                    foreach (var item in g.GetAttackableFoodUnitListIncludeWaterVehicle())
                    {
                        TakeDamage(item);
                    }
                }
                else
                    base.ExecuteDamage();
            } 
        }
        else
        {
            base.ExecuteDamage();
        }
    }

    /// <summary>
    /// Ǳˮͧ��������ˮ���Σ�����½��
    /// </summary>
    public override void SetGridDangerousWeightDict()
    {
        GridDangerousWeightDict[GridType.Water] = GridDangerousWeightDict[GridType.Default] - 1;
    }
}
