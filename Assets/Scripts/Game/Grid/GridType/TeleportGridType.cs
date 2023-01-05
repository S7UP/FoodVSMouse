using System;

using UnityEngine;
/// <summary>
/// ���ͻ�����
/// </summary>
public class TeleportGridType : BaseGridType
{
    private static RuntimeAnimatorController Tp_AnimatorController;
    private int maxTime;
    private int timeLeft;
    private bool isPlayTpClip; // �Ƿ��ڲ���TP����

    public override void Awake()
    {
        if (Tp_AnimatorController == null)
            Tp_AnimatorController = GameManager.Instance.GetRuntimeAnimatorController("GridType/Teleport/0");
        base.Awake();
        animator = GetComponent<Animator>();
    }

    public override void MInit()
    {
        isPlayTpClip = false;
        maxTime = 480;
        timeLeft = maxTime;
        base.MInit();
        animator.runtimeAnimatorController = Tp_AnimatorController;
    }

    /// <summary>
    /// �Ƿ������������
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetingEnterCondition(BaseUnit unit)
    {
        // BOSS��λ���ӵ���Ч��
        if(unit is MouseUnit)
        {
            MouseUnit m = unit as MouseUnit;
            if (m.IsBoss())
                return false;
        }
        // ֻ����߶�Ϊ0 �� �ɱ�ѡȡ�ĵ�λͨ��
        return unit.GetHeight()==0 && UnitManager.CanBeSelectedAsTarget(null, unit);
    }

    /// <summary>
    /// ���е�λ�������ʱʩ�Ӹ���λ��Ч��
    /// </summary>
    /// <param name="unit"></param>
    public override void OnUnitEnter(BaseUnit unit)
    {

    }

    /// <summary>
    /// ���е�λ���ڵ���ʱ��������λ��Ч��
    /// </summary>
    /// <param name="unit"></param>
    public override void OnUnitStay(BaseUnit unit)
    {

    }

    /// <summary>
    /// ���е�λ�뿪����ʱʩ�Ӹ���λ��Ч��
    /// </summary>
    /// <param name="unit"></param>
    public override void OnUnitExit(BaseUnit unit)
    {

    }

    /// <summary>
    /// ִ��һ�δ���
    /// </summary>
    private void ExecuteTp()
    {
        animatorController.Play("TP");
        for (int i = 0; i < unitList.Count; i++)
        {
            BaseUnit m = unitList[i];
            float moveDistance = MapManager.gridWidth;
            // ����Ŀ����������ֵ���������;��룬���У�3600Ѫ���ϵ�ֻ��һ��1500~3600�Ĵ�����1500���´�����
            if (m.mMaxHp > 3600)
            {
                moveDistance *= 1.0f;
            }
            else if (m.mMaxHp > 1500)
            {
                moveDistance *= 2.0f;
            }
            else
            {
                moveDistance *= 3.0f;
            }
            // ���һ������Ķ�����
            Tasker t = GameController.Instance.AddTasker(new ParabolaMovePresetTasker(m, 12.0f, 1.2f, m.transform.position, m.transform.position + (Vector3)m.moveRotate * moveDistance, false));
            // m.CloseCollision();
            // ��Ծ�ڼ䲻�ɱ��赲Ҳ���ܱ������ӵ�����
            Func<BaseUnit, BaseUnit, bool> noBlockFunc = delegate { return false; };
            Func<BaseUnit, BaseBullet, bool> noHitFunc = delegate { return false; };
            m.AddCanBlockFunc(noBlockFunc);
            m.AddCanHitFunc(noHitFunc);

            m.DisableMove(true); // ��ʱ�����ƶ�
            t.AddOtherEndEvent(delegate 
            {
                // m.OpenCollision();
                m.RemoveCanBlockFunc(noBlockFunc);
                m.RemoveCanHitFunc(noHitFunc);
                m.DisableMove(false); // ��������ƶ�
                m.AddNoCountUniqueStatusAbility(StringManager.Stun, new StunStatusAbility(m, 180, false)); // Ŀ������غ���ѣ3��
            });
        }
        isPlayTpClip = true;
    }

    /// <summary>
    /// ������ʼCD������CD
    /// </summary>
    /// <param name="startTime">��ʼCD</param>
    /// <param name="maxTime">����CD</param>
    public void SetStartTimeAndMaxTime(int startTime, int maxTime)
    {
        timeLeft = startTime;
        this.maxTime = maxTime;
    }

    public override void MUpdate()
    {

        base.MUpdate();

        if (timeLeft > 0)
            timeLeft--;
        else
        {
            timeLeft = maxTime;
            ExecuteTp();
        }
    }
}
