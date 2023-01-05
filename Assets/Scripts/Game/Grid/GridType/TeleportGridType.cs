using System;

using UnityEngine;
/// <summary>
/// 传送机地形
/// </summary>
public class TeleportGridType : BaseGridType
{
    private static RuntimeAnimatorController Tp_AnimatorController;
    private int maxTime;
    private int timeLeft;
    private bool isPlayTpClip; // 是否在播放TP动画

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
    /// 是否满足进入条件
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetingEnterCondition(BaseUnit unit)
    {
        // BOSS单位无视地形效果
        if(unit is MouseUnit)
        {
            MouseUnit m = unit as MouseUnit;
            if (m.IsBoss())
                return false;
        }
        // 只允许高度为0 且 可被选取的单位通过
        return unit.GetHeight()==0 && UnitManager.CanBeSelectedAsTarget(null, unit);
    }

    /// <summary>
    /// 当有单位进入地形时施加给单位的效果
    /// </summary>
    /// <param name="unit"></param>
    public override void OnUnitEnter(BaseUnit unit)
    {

    }

    /// <summary>
    /// 当有单位处于地形时持续给单位的效果
    /// </summary>
    /// <param name="unit"></param>
    public override void OnUnitStay(BaseUnit unit)
    {

    }

    /// <summary>
    /// 当有单位离开地形时施加给单位的效果
    /// </summary>
    /// <param name="unit"></param>
    public override void OnUnitExit(BaseUnit unit)
    {

    }

    /// <summary>
    /// 执行一次传送
    /// </summary>
    private void ExecuteTp()
    {
        animatorController.Play("TP");
        for (int i = 0; i < unitList.Count; i++)
        {
            BaseUnit m = unitList[i];
            float moveDistance = MapManager.gridWidth;
            // 根据目标的最大生命值来决定传送距离，其中，3600血以上的只传一格，1500~3600的传两格，1500以下传三格
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
            // 添加一个弹起的动任务
            Tasker t = GameController.Instance.AddTasker(new ParabolaMovePresetTasker(m, 12.0f, 1.2f, m.transform.position, m.transform.position + (Vector3)m.moveRotate * moveDistance, false));
            // m.CloseCollision();
            // 跳跃期间不可被阻挡也不能被常规子弹击中
            Func<BaseUnit, BaseUnit, bool> noBlockFunc = delegate { return false; };
            Func<BaseUnit, BaseBullet, bool> noHitFunc = delegate { return false; };
            m.AddCanBlockFunc(noBlockFunc);
            m.AddCanHitFunc(noHitFunc);

            m.DisableMove(true); // 暂时禁用移动
            t.AddOtherEndEvent(delegate 
            {
                // m.OpenCollision();
                m.RemoveCanBlockFunc(noBlockFunc);
                m.RemoveCanHitFunc(noHitFunc);
                m.DisableMove(false); // 解除禁用移动
                m.AddNoCountUniqueStatusAbility(StringManager.Stun, new StunStatusAbility(m, 180, false)); // 目标在落地后晕眩3秒
            });
        }
        isPlayTpClip = true;
    }

    /// <summary>
    /// 设置起始CD与周期CD
    /// </summary>
    /// <param name="startTime">起始CD</param>
    /// <param name="maxTime">周期CD</param>
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
