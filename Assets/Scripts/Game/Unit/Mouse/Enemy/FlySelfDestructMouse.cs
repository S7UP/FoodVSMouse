using UnityEngine;
using S7P.Numeric;
public class FlySelfDestructMouse : MouseUnit, IFlyUnit
{
    private bool isDrop; // �Ƿ񱻻���
    private int dropColumn; // ������
    private FloatModifier burnRateMod = new FloatModifier(0.01f); // ����ը����ɱЧ��
    private BoolModifier IgnoreFrozen = new BoolModifier(true); // ���߶���
    private Vector2 start_position; // ��ʼ׹����
    private Vector2 target_position; // Ŀ�����
    private Vector3 last_pos;

    public override void MInit()
    {
        isDrop = false;
        dropColumn = 3; // ������Ĭ��Ϊ3����������
        base.MInit();
        mHeight = 1;
        // ��ʼ����ը����ɱЧ��
        NumericBox.BurnRate.AddModifier(burnRateMod);
        // ���ܵ��˺�����֮��ֱ���ж�Ϊ��׹״̬
        AddActionPointListener(ActionPointType.PostReceiveDamage, delegate { ExecuteDestruct(); });
        AddActionPointListener(ActionPointType.PostReceiveReboundDamage, delegate { ExecuteDestruct(); });
    }

    public override void MUpdate()
    {
        base.MUpdate();
        if (IsMeetDropCondition())
        {
            ExecuteDestruct();
        }
    }

    /// <summary>
    /// ����Ƿ����㽵������
    /// </summary>
    private bool IsMeetDropCondition()
    {
        return (transform.position.x <= MapManager.GetColumnX(dropColumn) && !isDrop);
    }

    /// <summary>
    /// ִ�н��䣬��һ��
    /// </summary>
    public void ExecuteDestruct()
    {
        if (!isDrop)
        {
            // ����Ϊ����ѡȡ���Լ����ɻ���
            AddCanBeSelectedAsTargetFunc(delegate { return false; });
            AddCanHitFunc(delegate { return false; });
            // �����׹�����˺��ʵ��һЩ��Ϊ�ᷢ���仯
            isDrop = true;
            // �Ƴ�����ը����ɱЧ��
            NumericBox.BurnRate.RemoveModifier(burnRateMod);
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
            last_pos = start_position;
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
        if (isDrop && !isDeathState)
        {
            float t = animatorController.GetCurrentAnimatorStateRecorder().GetNormalizedTime();
            Vector3 pos = Vector3.Lerp(start_position, target_position, t);
            SetPosition(GetPosition()+(pos - last_pos));
            last_pos = pos;
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
            BaseUnit unit = grid.GetHighestAttackPriorityUnit(this);
            if (unit != null && unit.tag!="Character")
            {
                new BombDamageAction(CombatAction.ActionType.CauseDamage, this, unit, unit.mCurrentHp).ApplyAction();
            }
        }
    }

    /// <summary>
    /// �Ƿ��ܱ���ΪĿ��ѡ��
    /// </summary>
    /// <returns></returns>
    public override bool CanBeSelectedAsTarget(BaseUnit otherUnit)
    {
        // �����׹��״̬�򲻿ɱ�ѡΪĿ��
        if (isDrop)
            return false;
        else
            return base.CanBeSelectedAsTarget(otherUnit);
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
