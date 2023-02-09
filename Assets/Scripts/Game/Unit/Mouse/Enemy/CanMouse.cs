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
    private BoolModifier IgnoreBombInstantKill = new BoolModifier(true);
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
    public override void OnBombBurnDamage(float dmg)
    {
        // ԭ�ȵ��˺�ִ��
        base.OnBombBurnDamage(dmg);
        // ������װ״̬�������Ƿ��ж����ڼ�
        if (isReinstallState && !(mCurrentActionState is TransitionState))
        {
            // ��ɿ���������
            SetActionState(new TransitionState(this));
            //Tasker t = GameController.Instance.AddTasker(new ParabolaMovePresetTasker(this, 24.0f, 0.75f, transform.position, transform.position + (Vector3)moveRotate*3*MapManager.gridWidth, false));
            //DisableMove(true);
            //// ���н���ʱ�л��ɶ�װ������ѣ״̬
            //t.AddOtherEndEvent(delegate { SetActionState(new CastState(this)); DisableMove(false); });

            float dist = 3 * MapManager.gridWidth;
            CustomizationTask t = TaskManager.AddParabolaTask(this, dist / 60, dist / 2, transform.position, transform.position + (Vector3)moveRotate * dist, false);
            DisableMove(true);
            Action oldExit = t.OnExitFunc;
            t.OnExitFunc = delegate
            {
                if (oldExit != null)
                    oldExit();
                SetActionState(new CastState(this));
                DisableMove(false);
            };
        }
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
        // ���߻ҽ������붨��
        NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreBombInstantKill, IgnoreBombInstantKill);
        NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreSlowDown, IgnoreSlowDown);
        NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreStun, IgnoreStun);

        mHertIndex = 0;
        UpdateRuntimeAnimatorController();
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
        NumericBox.RemoveDecideModifierToBoolDict(StringManager.IgnoreBombInstantKill, IgnoreBombInstantKill);
        NumericBox.RemoveDecideModifierToBoolDict(StringManager.IgnoreSlowDown, IgnoreSlowDown);
        NumericBox.RemoveDecideModifierToBoolDict(StringManager.IgnoreStun, IgnoreStun);


        // ��ȡ����Ч��
        NumericBox.MoveSpeed.AddPctAddModifier(moveSpeedModifier2);
        // ��ȡ���ټӳ�Ч��
        NumericBox.AttackSpeed.AddPctAddModifier(attackSpeedModifier);

        mHertIndex = 1;
        UpdateRuntimeAnimatorController();
    }


    public override void OnTransitionStateEnter()
    {
        animatorController.Play("Fly");
        // ���벻��ѡȡ״̬
        CloseCollision();
    }

    public override void OnTransitionStateExit()
    {
        // תΪ��װ״̬
        SetLightState();
        OpenCollision();
    }

    public override bool CanBlock(BaseUnit unit)
    {
        return !isDroping && base.CanBlock(unit);
    }

    public override void OnCastStateEnter()
    {
        animatorController.Play("Drop");
        isDroping = true;
    }

    public override void OnCastState()
    {
        if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
            SetActionState(new MoveState(this));
    }

    public override void OnCastStateExit()
    {
        isDroping = false;
    }

    /// <summary>
    /// ���������ͼ�߼��Ƚ����⣬ֱ����д�ÿգ����߳����߼�������ܱ���
    /// </summary>
    public override void UpdateHertMap()
    {

    }
}
