using UnityEngine;

public class FlySelfDestructMouse : MouseUnit, IFlyUnit
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
        AddActionPointListener(ActionPointType.PostReceiveDamage, delegate { ExecuteDrop(); });
        AddActionPointListener(ActionPointType.PostReceiveReboundDamage, delegate { ExecuteDrop(); });
    }

    public override void MUpdate()
    {
        base.MUpdate();
        if (IsMeetDropCondition())
        {
            ExecuteDrop();
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
    public void ExecuteDrop()
    {
        if (!isDrop)
        {
            // �����׹�����˺��ʵ��һЩ��Ϊ�ᷢ���仯
            isDrop = true; 
            // �Ƴ�����ը����ɱЧ��
            NumericBox.RemoveDecideModifierToBoolDict(StringManager.IgnoreBombInstantKill, IgnoreBombInstantKill);
            // ��Ӷ�������Ч��
            NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreFrozen, IgnoreFrozen);
            NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreStun, IgnoreFrozen);
            // ͬʱ�Ƴ��������ж��������Ч��
            StatusManager.RemoveAllSettleDownDebuff(this);
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
            float t = animatorController.GetCurrentAnimatorStateRecorder().GetNormalizedTime();
            Debug.Log("t=" + t);
        }
        else
        {
            animatorController.Play("Move0", true);
        }
    }

    public override void OnMoveState()
    {
        if (isDrop && !isDeathState)
        {
            float t = animatorController.GetCurrentAnimatorStateRecorder().GetNormalizedTime();
            Debug.Log("t=" + t);
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

    /// <summary>
    /// �Ƿ��ܱ���ΪĿ��ѡ��
    /// </summary>
    /// <returns></returns>
    public override bool CanBeSelectedAsTarget()
    {
        // �����׹��״̬�򲻿ɱ�ѡΪĿ��
        if (isDrop)
            return false;
        else
            return base.CanBeSelectedAsTarget();
    }

    /// <summary>
    /// �Ƿ��ܱ��ӵ�����
    /// </summary>
    /// <param name="bullet"></param>
    /// <returns></returns>
    public override bool CanHit(BaseBullet bullet)
    {
        // �����׹��״̬���򲻿ɱ��ӵ�����
        if (isDrop)
            return false;
        else
            return base.CanHit(bullet);
    }
}
