
using UnityEngine;
using S7P.Numeric;
using Environment;
/// <summary>
/// ������
/// </summary>
public class KangarooMouse : MouseUnit
{
    private BoolModifier IgnoreStunModifier = new BoolModifier(true); // ������ѣ������
    private bool isFirstRoot; // �Ƿ��гԹ�����Ч��  Ҳ������Ϊ�Ƿ�Ϊ��Ծ״̬�ı�׼  false = ��Ծ״̬  true = ����Ծ״̬
    private bool isJumping; // ������Ծ������
    private float jumpDistance; // ��Ծ����

    public override void MInit()
    {
        isFirstRoot = false;
        isJumping = false;
        base.MInit();
        // ��һ���ܵ�����Ч��ǰ���߶���Ч��
        NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreStun, IgnoreStunModifier);
        // ��Ӽ���
        // �ڵ�һ�ζ���Ч���������������ߵ�ԭ����ʩ�Ӷ���Ч�����������������Ƴ����߶���Ч��
        statusAbilityManager.AddAfterRemoveStatusAbilityEvent(StringManager.Stun,
            delegate {
                if (isFirstRoot)
                    return;
                isFirstRoot = true;
                // NumericBox.RemoveDecideModifierToBoolDict(StringManager.IgnoreFrozen, IgnoreFrozenModifier);
                // ������б�������Ч��
                IceTask t = Environment.EnvironmentFacade.GetIceDebuff(this) as IceTask;
                if(t != null)
                {
                    t.AddValue(-t.GetValue());
                }
                NumericBox.RemoveDecideModifierToBoolDict(StringManager.IgnoreStun, IgnoreStunModifier);
                animatorController.Play("Drop");
            });
        if (mShape == 0 || mShape == 3)
            jumpDistance = 1;
        else if (mShape == 1 || mShape == 4)
            jumpDistance = 1.5f;
        else
            jumpDistance = 2;
    }

    /// <summary>
    /// ��ͨ��������
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetGeneralAttackCondition()
    {
        // ����Ծ״̬�£���ͨ�������˺�ִ�б��滻Ϊ������ǰ��һ��,����ж�Ϊ��������Ծ
        // ����Ծ״̬���������߼�
        if (isFirstRoot)
        {
            return base.IsMeetGeneralAttackCondition();
        }
        else
        {
            return base.IsMeetGeneralAttackCondition() && !isJumping;
        }
    }

    /// <summary>
    /// ִ�о���Ĺ�����λ���˺��ж�Ϊ��֮��
    /// </summary>
    public override void ExecuteDamage()
    {
        // ����Ծ״̬�£���ͨ�������˺�ִ�б��滻Ϊ������ǰ��һ��
        if (isFirstRoot)
        {
            if (IsHasTarget())
                TakeDamage(GetCurrentTarget());
        }
        else
        {
            // ���һ������������һ��
            isJumping = true;
            //Tasker t = GameController.Instance.AddTasker(new ParabolaMovePresetTasker(this, 12.0f, 0.8f, transform.position, transform.position + (Vector3)moveRotate * jumpDistance * MapManager.gridWidth, false));
            //DisableMove(true);
            //t.AddOtherEndEvent(delegate { isJumping = false; DisableMove(false); });

            float dist = jumpDistance * MapManager.gridWidth;
            CustomizationTask t = TaskManager.GetParabolaTask(this, dist / 60, dist / 2, transform.position, transform.position + (Vector3)moveRotate * dist, false, true);
            t.AddOnEnterAction(delegate {
                DisableMove(true);
            });
            t.AddOnExitAction(delegate
            {
                isJumping = false; 
                DisableMove(false);
            });
            AddTask(t);
        }
    }

    public override void OnAttackStateEnter()
    {
        if (isFirstRoot)
            animatorController.Play("Attack");
        else
            animatorController.Play("Jump");
    }

    public override void OnMoveStateEnter()
    {
        if (isFirstRoot)
            animatorController.Play("Move1", true);
        else
            animatorController.Play("Move0", true);
    }

    public override void OnDieStateEnter()
    {
        if (isFirstRoot)
            animatorController.Play("Die1");
        else
            animatorController.Play("Die0");
    }

    public override void OnIdleStateEnter()
    {
        if (isFirstRoot)
            animatorController.Play("Idle", true);
        else
            animatorController.Play("Move0", true);
    }

    public override void OnIdleState()
    {
        if (!IsHasTarget())
            SetActionState(new MoveState(this));
    }
}
