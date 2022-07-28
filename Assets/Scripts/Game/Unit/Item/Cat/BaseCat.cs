using UnityEngine;

/// <summary>
/// èè
/// </summary>
public class BaseCat : BaseItem
{
    private int rowIndex; // �̶������±�
    private bool isTrigger; // �Ƿ񴥷������ѣ�

    public override void MInit()
    {
        base.MInit();
        rowIndex = 0;
        isTrigger = false;
        mBoxCollider2D.size = new Vector2(MapManager.gridWidth/2, MapManager.gridHeight/2);
        moveRotate = Vector2.right; // Ĭ�Ϸ���Ϊ����
        NumericBox.MoveSpeed.SetBase(TransManager.TranToVelocity(12.0f)); // Ĭ������Ϊ2��/s
    }

    /// <summary>
    /// �������±��ͬʱ����y����ͬ��
    /// </summary>
    public void SetRowIndex(int rowIndex)
    {
        this.rowIndex = rowIndex;
        transform.position = new Vector3(transform.position.x, MapManager.GetRowY(rowIndex), transform.position.z);
    }

    /// <summary>
    /// �㾪����èè��������һ�ε�����Ч��
    /// </summary>
    public void OnTriggerEvent()
    {
        if (!isTrigger)
        {
            isTrigger = true;
            SetActionState(new TransitionState(this));
        }
    }

    public override void OnTransitionStateEnter()
    {
        animatorController.Play("Trigger");
    }

    public override void OnTransitionState()
    {
        if (currentStateTimer == 0)
            return;
        if(AnimatorManager.GetNormalizedTime(animator)>1.0)
            SetActionState(new MoveState(this));
    }

    public override void OnMoveStateEnter()
    {
        animatorController.Play("Move");
    }

    public override void OnMoveState()
    {
        SetPosition((Vector2)GetPosition() + moveRotate * GetMoveSpeed());
        // �����Ҳ���ʧ
        if (GetColumnIndex() >= MapController.xColumn)
        {
            ExecuteDeath();
        }
    }


    /// <summary>
    /// ��ײ�¼�
    /// </summary>
    /// <param name="collision"></param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // ��������ʱ�������κ���ײ�¼�
        if (isDeathState)
        {
            return;
        }
        if (collision.tag.Equals("Mouse"))
        {
            MouseUnit m = collision.GetComponent<MouseUnit>();
            if (m.GetRowIndex() == GetRowIndex())
            {
                OnTriggerEvent();

                if (m.IsBoss())
                    // ��BOSS���900���˺�
                    new DamageAction(CombatAction.ActionType.CauseDamage, this, m, 900).ApplyAction();
                else
                    // ��������
                    m.ExecuteDeath();
            }
        }
    }

    /// <summary>
    /// �ж�����y����ı䣬������һ��
    /// </summary>
    /// <returns></returns>
    public override int GetRowIndex()
    {
        return rowIndex;
    }
}
