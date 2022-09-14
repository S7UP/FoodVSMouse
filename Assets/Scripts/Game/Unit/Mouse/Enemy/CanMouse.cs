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
    private BoolModifier IgnoreFrozen = new BoolModifier(true);

    private bool isReinstallState; // �Ƿ�Ϊ��װ̬

    public override void MInit()
    {
        base.MInit();
        isReinstallState = false; // ֻ�ǳ�ʼ�������õ��ģ����Ҳ���ô������ķ�����ʧЧ
        // ��ʼ̬Ϊ��װ̬
        SetReinstallState();
    }

    /// <summary>
    /// �ܵ��ҽ��˺�ʱ
    /// </summary>
    /// <param name="dmg"></param>
    public override void OnBurnDamage(float dmg)
    {
        // ԭ�ȵ��˺�ִ��
        base.OnBurnDamage(dmg);
        // ������װ״̬�������Ƿ��ж����ڼ�
        if (isReinstallState && !(mCurrentActionState is TransitionState))
        {
            // ��ɿ���������
            SetActionState(new TransitionState(this));
            Tasker t = GameController.Instance.AddTasker(new ParabolaMovePresetTasker(this, 24.0f, 0.75f, transform.position, transform.position + (Vector3)moveRotate*3*MapManager.gridWidth, false));
            // ���н���ʱ�л��ɶ�װ������ѣ״̬
            t.AddOtherEndEvent(delegate { SetActionState(new CastState(this)); });
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
        NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreFrozen, IgnoreFrozen);

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
        NumericBox.RemoveDecideModifierToBoolDict(StringManager.IgnoreFrozen, IgnoreFrozen);


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

    public override void OnCastStateEnter()
    {
        animatorController.Play("Drop");
    }

    public override void OnCastState()
    {
        if (currentStateTimer == 0)
            return;
        if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
            SetActionState(new MoveState(this));
    }

    /// <summary>
    /// ���������ͼ�߼��Ƚ����⣬ֱ����д�ÿգ����߳����߼�������ܱ���
    /// </summary>
    public override void UpdateHertMap()
    {

    }
}
