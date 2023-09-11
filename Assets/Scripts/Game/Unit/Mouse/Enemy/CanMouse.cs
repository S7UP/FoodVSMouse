using S7P.Numeric;

using System;

using UnityEngine;
/// <summary>
/// ��ͷ��
/// </summary>
public class CanMouse : MouseUnit
{
    private FloatModifier defenseModifier = new FloatModifier(0.5f); // ��������
    private FloatModifier moveSpeedModifier1 = new FloatModifier(-50); // ��������
    private FloatModifier moveSpeedModifier2 = new FloatModifier(300); // ��������
    private FloatModifier attackSpeedModifier = new FloatModifier(100); // �ӹ�������
    private FloatModifier burnRateMod = new FloatModifier(0);
    private BoolModifier IgnoreSlowDown = new BoolModifier(true);
    private BoolModifier IgnoreStun = new BoolModifier(true);

    private bool isReinstallState; // �Ƿ�Ϊ��װ̬

    private bool isDroping; // �Ƿ�����װ��Ȼ����ѣ�Ĺ�����

    public override void MInit()
    {
        isDroping = false;
        base.MInit();
        isReinstallState = false; // ֻ�ǳ�ʼ�������õ��ģ����Ҳ���ô������ķ�����ʧЧ
        // ��ʼ̬Ϊ��װ̬
        SetReinstallState();
    }

    /// <summary>
    /// �ܵ��ҽ��˺�ʱ
    /// </summary>
    /// <param name="dmg"></param>
    public override float OnBombBurnDamage(float dmg)
    {
        float maxDist = transform.position.x - MapManager.GetColumnX(0);

        // ������װ״̬�������Ƿ��ж����ڼ�
        if (isReinstallState && (!NumericBox.IntDict.ContainsKey(StringManager.Flying) || NumericBox.IntDict[StringManager.Flying].Value <= 0) && maxDist > 0)
        {
            float dist = Mathf.Min(maxDist, 3 * MapManager.gridWidth);
            CustomizationTask t = TaskManager.GetParabolaTask(this, dist / 60, dist / 2, transform.position, transform.position + (Vector3)moveRotate * dist, false);
            t.AddOnEnterAction(delegate {
                // ��ɿ���������
                DisableMove(true);
                // SetActionState(new TransitionState(this));
                animatorController.Play("Fly");
            });
            t.AddOnExitAction(delegate
            {
                DisableMove(false);
                // SetActionState(new CastState(this));
                // תΪ��װ״̬
                SetLightState();
                // ��������ѣ����
                taskController.AddTask(GetStunTask());
            });
            //AddUniqueTask("CanMouseFly", t);
            taskController.AddTask(t);
        }
        // ԭ�ȵ��˺�ִ��
        return base.OnBombBurnDamage(dmg);
    }

    /// <summary>
    /// ����Ϊ��װ״̬
    /// </summary>
    private void SetReinstallState()
    {
        if (isReinstallState)
            return;
        isReinstallState = true;
        // ��װ״̬Ч���Ƴ�
        NumericBox.MoveSpeed.RemovePctAddModifier(moveSpeedModifier2);
        NumericBox.AttackSpeed.RemovePctAddModifier(attackSpeedModifier);

        // ��ȡ����
        NumericBox.Defense.AddAddModifier(defenseModifier);
        // �����ٽ���Ч��
        NumericBox.MoveSpeed.AddFinalPctAddModifier(moveSpeedModifier1);
        // ���߼����붨��
        NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreSlowDown, IgnoreSlowDown);
        NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreStun, IgnoreStun);
        // 100%�ҽ�����
        NumericBox.BurnRate.AddModifier(burnRateMod);
        mHertIndex = 0;
        OnHertStageChanged();
    }

    /// <summary>
    /// ����Ϊ��װ״̬
    /// </summary>
    private void SetLightState()
    {
        if (!isReinstallState)
            return;
        isReinstallState = false;
        // ��װЧ���Ƴ�
        NumericBox.Defense.RemoveAddModifier(defenseModifier);
        NumericBox.MoveSpeed.RemoveFinalPctAddModifier(moveSpeedModifier1);
        NumericBox.RemoveDecideModifierToBoolDict(StringManager.IgnoreSlowDown, IgnoreSlowDown);
        NumericBox.RemoveDecideModifierToBoolDict(StringManager.IgnoreStun, IgnoreStun);
        NumericBox.BurnRate.RemoveModifier(burnRateMod);

        // ��ȡ����Ч��
        NumericBox.MoveSpeed.AddPctAddModifier(moveSpeedModifier2);
        // ��ȡ���ټӳ�Ч��
        NumericBox.AttackSpeed.AddPctAddModifier(attackSpeedModifier);

        mHertIndex = 1;
        OnHertStageChanged();
    }

    private CustomizationTask GetStunTask()
    {
        CustomizationTask t = new CustomizationTask();
        Func<BaseUnit, BaseUnit, bool> canBlockFunc = delegate { return false; };
        
        t.AddOnEnterAction(delegate
        {
            animatorController.Play("Drop");
            AddCanBlockFunc(canBlockFunc);
            DisableMove(true);
        });
        t.AddTaskFunc(delegate
        {
            return animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce();
        });
        t.AddOnExitAction(delegate
        {
            RemoveCanBlockFunc(canBlockFunc);
            DisableMove(false);
            animatorController.Play("Move");
        });
        return t;
    }

    /// <summary>
    /// ���������ͼ�߼��Ƚ����⣬ֱ����д�ÿգ����߳����߼�������ܱ���
    /// </summary>
    public override void UpdateHertMap()
    {

    }
}
