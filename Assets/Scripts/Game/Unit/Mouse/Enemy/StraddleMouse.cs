using UnityEngine;
/// <summary>
/// ������
/// </summary>
public class StraddleMouse : MouseUnit
{
    private bool isJumped; // �Ƿ�����һ��
    private float maxSpeed; // ����ٶ�
    private float a; // ���ٶȣ�ÿ֡��

    public override void MInit()
    {
        isJumped = false;
        maxSpeed = TransManager.TranToVelocity(4.0f); // ����ٶ�Ϊ4.0��׼�ƶ��ٶ�
        a = TransManager.TranToVelocity(3.0f) / 9 / 60; // ���ٶ�Ϊ��9��������3.0��׼�ƶ��ٶȵļ��ٶ�
        base.MInit();
    }

    public override bool IsDamageJudgment()
    {
        if (!isJumped)
            return true;
        else
            return base.IsDamageJudgment();
    }

    /// <summary>
    /// ִ�о���Ĺ�����λ���˺��ж�Ϊ��֮��
    /// </summary>
    public override void ExecuteDamage()
    {
        // ��δ���е�һ����Ծʱ����ͨ�������滻Ϊ��Ծ
        if (!isJumped)
        {
            // ���һ����������
            isJumped = true;
            // ���벻��ѡȡ״̬
            CloseCollision();
            // ��Ծ���������� 0.75*��ǰ�ƶ��ٶȱ�׼ֵ
            float dist = 0.75f*TransManager.TranToStandardVelocity(GetMoveSpeed());
            Tasker t = GameController.Instance.AddTasker(new ParabolaMovePresetTasker(this, 12.0f, 0.8f, transform.position, transform.position + (Vector3)moveRotate * dist * MapManager.gridWidth, false));
            t.AddOtherEndEvent(delegate { OpenCollision(); NumericBox.MoveSpeed.SetBase(TransManager.TranToVelocity(1.0f)); });
        }
        else
        {
            base.ExecuteDamage();
        }
    }

    public override void OnAttackStateEnter()
    {
        if (isJumped)
            animatorController.Play("Attack");
        else
            animatorController.Play("Jump");
    }

    public override void OnMoveStateEnter()
    {
        if (isJumped)
            animatorController.Play("Move1", true);
        else
            animatorController.Play("Move0", true);
    }

    public override void OnMoveState()
    {
        // ����Ծǰ���٣��ӵ��ǻ����ƶ��ٶ�
        if (!isJumped)
        {
            NumericBox.MoveSpeed.SetBase(NumericBox.MoveSpeed.baseValue + a);
            if (NumericBox.MoveSpeed.baseValue > maxSpeed)
                NumericBox.MoveSpeed.SetBase(maxSpeed);
        }
        base.OnMoveState();
    }

    public override void OnIdleStateEnter()
    {
        if (isJumped)
            animatorController.Play("Idle", true);
    }

    public override void OnIdleState()
    {
        if (!IsHasTarget())
            SetActionState(new MoveState(this));
    }
}
