using UnityEngine;

using static UnityEngine.UI.CanvasScaler;

public class FlySelfDestructMouse : MouseUnit
{
    private bool isDrop; // �Ƿ񱻻���
    private int dropColumn; // ������
    private BoolModifier IgnoreBombInstantKill = new BoolModifier(true); // ����ը����ɱЧ��
    private BoolModifier IgnoreFrozen = new BoolModifier(true); // ���߶���
    private Vector2 start_position; // ��ʼ׹����
    private Vector2 target_position; // Ŀ�����

    public override void MInit()
    {
        isDrop = false;
        dropColumn = 3; // ������Ĭ��Ϊ3����������
        base.MInit();
        // ��ʼ����ը����ɱЧ��
        NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreBombInstantKill, IgnoreBombInstantKill);
        // ���ܵ��˺�����֮��ֱ���ж�Ϊ��׹״̬
        AddActionPointListener(ActionPointType.PostReceiveDamage, delegate { ExcuteDrop(); });
        AddActionPointListener(ActionPointType.PostReceiveReboundDamage, delegate { ExcuteDrop(); });
    }

    public override void MUpdate()
    {
        base.MUpdate();
        if (IsMeetDropCondition())
        {
            ExcuteDrop();
        }
    }

    /// <summary>
    /// ����Ƿ����㽵������
    /// </summary>
    private bool IsMeetDropCondition()
    {
        return (GetColumnIndex() <= dropColumn && !isDrop);
    }

    /// <summary>
    /// ִ�н��䣬��һ��
    /// </summary>
    private void ExcuteDrop()
    {
        if (!isDrop)
        {
            // �����׹�����˺��ʵ��һЩ��Ϊ�ᷢ���仯
            isDrop = true; 
            // �Ƴ�����ը����ɱЧ��
            if (IgnoreBombInstantKill != null)
                NumericBox.RemoveDecideModifierToBoolDict(StringManager.IgnoreBombInstantKill, IgnoreBombInstantKill);
            // ��Ӷ�������Ч��
            if(IgnoreFrozen != null)
            {
                NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreFrozen, IgnoreFrozen);
                // ͬʱ�Ƴ��������ж��������Ч��
                StatusManager.RemoveAllSettleDownDebuff(this);
            }
            // ȷ����ʼ������յ㣬�������յ�Ϊ��ʼ��+1.5��*��ǰ��׼�ƶ��ٶ�ֵ������С����Ϊ��һ��
            start_position = transform.position;
            target_position = transform.position + (Vector3)moveRotate * 1.5f * MapManager.gridWidth * TransManager.TranToStandardVelocity(NumericBox.MoveSpeed.Value);
            if(target_position.x < MapManager.GetColumnX(0))
            {
                target_position = new Vector2(MapManager.GetColumnX(0), target_position.y);
            }
            // ��Ϊת��״̬
            SetActionState(new MoveState(this));
        }
    }

    public override void OnMoveStateEnter()
    {
        if (isDrop)
        {
            animatorController.Play("Move1");
        }
        else
        {
            animatorController.Play("Move0", true);
        }
    }

    public override void OnMoveState()
    {
        if (currentStateTimer == 0)
            return;

        if (isDrop && !isDeathState)
        {
            float t = AnimatorManager.GetNormalizedTime(animator);
            rigibody2D.MovePosition(Vector2.Lerp(start_position, target_position, t));
            // ��������±�����׹�䣬ִ������
            if (t >= 1)
            {
                ExecuteDeath();
            }
        }
        else
        {
            base.OnMoveState();
        }
    }

    /// <summary>
    /// ��������ʱ����һ����Ƭ
    /// </summary>
    public override void BeforeDeath()
    {
        base.BeforeDeath();
        BaseGrid grid = GameController.Instance.mMapController.GetGrid(GetColumnIndex(), GetRowIndex());
        if (grid != null)
        {
            BaseUnit unit = grid.GetHighestAttackPriorityUnit();
            if (unit != null && unit.tag!="Character")
            {
                new BurnDamageAction(CombatAction.ActionType.CauseDamage, this, unit, float.MaxValue).ApplyAction();
            }
        }
    }
}
