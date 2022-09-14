using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static UnityEngine.GraphicsBuffer;
/// <summary>
/// ��ͧ��
/// </summary>
public class RowboatMouse : MouseUnit, IInWater
{
    private List<BaseUnit> blockedUnitList = new List<BaseUnit>(); // ���赲�ĵ�λ��
    private FloatModifier SpeedModifier = new FloatModifier(50); // ���ټӳɱ�ǩ
    private bool isAddSpeed;
    private const int DamageInterval = 30; // �˺����
    private int timeLeft; // �����˺��ж�ʣ��ʱ��
    private const int AdvanceTime = 720; // ǰ��ʱ��
    private const int BackTime = 360; // ����ʱ��
    private int moveTimeLeft; // �ƶ�ʣ��ʱ�䣨ʱ�䵽���е���һ�׶�)
    private bool isAdvance; // �Ƿ���ǰ���׶�

    public override void MInit()
    {
        blockedUnitList.Clear();
        isAddSpeed = false;
        timeLeft = 0;
        moveTimeLeft = 0;
        isAdvance = false;
        base.MInit();
        // ���߻ҽ���ɱЧ������������Ч��������Ч����ˮʴЧ��
        NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreBombInstantKill, new BoolModifier(true));
        NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreFrozen, new BoolModifier(true));
        NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreFrozenSlowDown, new BoolModifier(true));
        NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreWaterGridState, new BoolModifier(true));
        // ��ʼΪ��ˮ״̬
        SetActionState(new TransitionState(this));
    }

    public override void SetCollider2DParam()
    {
        mBoxCollider2D.offset = new Vector2(1.24f * MapManager.gridWidth, 0);
        mBoxCollider2D.size = new Vector2(2.99f * MapManager.gridWidth, 0.49f * MapManager.gridHeight);
    }

    public override void LoadSkillAbility()
    {
        // �����ؼ��ܣ���û��ƽA
    }

    /// <summary>
    /// �����ѷ���λ����ʳ���������������ײ�ж�ʱ
    /// </summary>
    public override void OnAllyCollision(BaseUnit target)
    {
        if (UnitManager.CanBlock(this, target)) // ���˫���ܷ����赲
        {
            if (!blockedUnitList.Contains(target))
                blockedUnitList.Add(target);
        }
    }

    /// <summary>
    /// ���ѷ���λ�뿪ʱ
    /// </summary>
    /// <param name="collision"></param>
    public override void OnAllyTriggerExit(Collider2D collision)
    {
        if (collision.tag.Equals("Food") || collision.tag.Equals("Character"))
        {
            BaseUnit unit = collision.GetComponent<BaseUnit>();
            blockedUnitList.Remove(unit);
        }
    }

    public override void MUpdate()
    {
        // �޳���Ч�ĵ�λ
        List<BaseUnit> delList = new List<BaseUnit>();
        foreach (var item in blockedUnitList)
        {
            if (!item.IsAlive())
                delList.Add(item);
        }
        foreach (var item in delList)
        {
            blockedUnitList.Remove(item);
        }
        base.MUpdate();
    }

    public override void OnTransitionStateEnter()
    {
        CloseCollision();
        animatorController.Play("Appear");
    }

    public override void OnTransitionState()
    {
        if (animatorController.GetCurrentAnimatorStateRecorder().timer == 1)
        {
            // ���ֵ�һ֡ʱǿ�ư�����ͬ��Ϊ��ǰ�������Ҷ���
            transform.position = MapManager.GetGridLocalPosition(8, currentYIndex);
        }else if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
        {
            OpenCollision();
            SetActionState(new MoveState(this));
        }
    }

    /// <summary>
    /// ���ƶ��Ĺ����У�����нӴ���Ŀ��������˺��Ӵ�Ŀ�� ��ÿ����10�˺���
    /// ���û�нӴ�Ŀ������100%�ƶ��ٶȼӳ�
    /// </summary>
    public override void OnMoveState()
    {
        // ǰ�������л��߼�
        if (moveTimeLeft > 0)
            moveTimeLeft--;
        else
        {
            if (isAdvance)
            {
                // ǰ����Ϊ����
                isAdvance = false;
                moveTimeLeft = BackTime;
                SetMoveRoate(Vector2.right);
            }
            else
            {
                // ������Ϊǰ��
                isAdvance = true;
                moveTimeLeft = AdvanceTime;
                SetMoveRoate(Vector2.left);
            }
        }

        base.OnMoveState();
        if (timeLeft > 0)
            timeLeft--;
        if (blockedUnitList.Count > 0)
        {
            if (isAddSpeed)
            {
                NumericBox.MoveSpeed.RemovePctAddModifier(SpeedModifier);
                isAddSpeed = false;
            }

            // �˺��Ӵ�Ŀ��
            if(timeLeft <= 0)
            {
                timeLeft = DamageInterval;
                foreach (var item in blockedUnitList)
                {
                    TakeDamage(item);
                }
            }
        }
        else
        {
            if (!isAddSpeed)
            {
                NumericBox.MoveSpeed.AddPctAddModifier(SpeedModifier);
                isAddSpeed = true;
            }
        }
    }

    /// <summary>
    /// ���ɱ��赲
    /// </summary>
    /// <param name="unit"></param>
    /// <returns></returns>
    //public override bool CanBlock(BaseUnit unit)
    //{
    //    return false;
    //}

    public void OnEnterWater()
    {
        
    }

    public void OnStayWater()
    {
        
    }

    public void OnExitWater()
    {
        
    }
}
