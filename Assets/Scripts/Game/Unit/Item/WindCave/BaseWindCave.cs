using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// 风洞
/// </summary>
public class BaseWindCave : BaseItem
{
    private List<MouseUnit> list = new List<MouseUnit>(); // 所有位于该道具上的老鼠单位
    private int maxTime;
    private int timeLeft;

    public override void Awake()
    {
        base.Awake();
    }

    public override void MInit()
    {
        maxTime = 480;
        timeLeft = maxTime;
        list.Clear();
        base.MInit();
        SetActionState(new IdleState(this));
    }

    /// <summary>
    /// 碰撞事件
    /// </summary>
    /// <param name="collision"></param>
    public void OnCollision(Collider2D collision)
    {
        // 死亡动画时不接受任何碰撞事件
        if (isDeathState)
        {
            return;
        }
        if (collision.tag.Equals("Mouse"))
        {
            // 把老鼠加到表中
            MouseUnit m = collision.GetComponent<MouseUnit>();
            if (list.Contains(m) || m.GetRowIndex() != GetRowIndex() || m.GetHeight() != GetHeight())
                return;
            list.Add(m);
        }
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        OnCollision(collision);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        OnCollision(collision);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag.Equals("Mouse"))
        {
            MouseUnit m = collision.GetComponent<MouseUnit>();
            list.Remove(m);
        }
    }

    /// <summary>
    /// 执行一次传送
    /// </summary>
    public void ExecuteTp()
    {
        foreach (var m in list)
        {
            float moveDistance = MapManager.gridWidth;
            // 根据目标的最大生命值来决定传送距离，其中，3600血以上的只传一格，1500~3600的传两格，1500以下传三格
            if(m.mMaxHp >= 3600)
            {
                moveDistance *= 1.0f;
            }
            else if(m.mMaxHp >= 1500)
            {
                moveDistance *= 2.0f;
            }
            else
            {
                moveDistance *= 3.0f; 
            }
            // 添加一个弹起的动任务
            Tasker t = GameController.Instance.AddTasker(new ParabolaMovePresetTasker(m, 24.0f, 1.2f, m.transform.position, m.transform.position + (Vector3)m.moveRotate * moveDistance, false));
            m.CloseCollision();
            t.AddOtherEndEvent(delegate { m.OpenCollision(); });
        }
    }

    public override BaseGrid GetGrid()
    {
        return mGrid;
    }

    /// <summary>
    /// 把自己加入格子
    /// </summary>
    /// <param name="grid"></param>
    public override void SetGrid(BaseGrid grid)
    {
        mGrid = grid;
    }

    /// <summary>
    /// 将自身移除出格子
    /// </summary>
    public override void RemoveFromGrid()
    {
        mGrid.RemoveItemUnit(this);
    }

    public override void OnIdleStateEnter()
    {
        animatorController.Play("Idle", true);
    }

    public override void OnIdleState()
    {
        if(timeLeft > 0)
        {
            timeLeft--;
        }
        else
        {
            timeLeft = maxTime;
            SetActionState(new CastState(this));
        }
    }

    public override void OnCastStateEnter()
    {
        ExecuteTp();
        animatorController.Play("TP");
    }

    public override void OnCastState()
    {
        if (timeLeft > 0)
        {
            timeLeft--;
            //Debug.Log("timeLeft="+timeLeft);
        }
        if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
        {
            //Debug.Log("Enter IdleState");
            SetActionState(new IdleState(this));
        }
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
}
