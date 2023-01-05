using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// �綴
/// </summary>
public class BaseWindCave : BaseItem
{
    private List<MouseUnit> list = new List<MouseUnit>(); // ����λ�ڸõ����ϵ�����λ
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
    /// ��ײ�¼�
    /// </summary>
    /// <param name="collision"></param>
    public void OnCollision(Collider2D collision)
    {
        // ��������ʱ�������κ���ײ�¼�
        if (isDeathState)
        {
            return;
        }
        if (collision.tag.Equals("Mouse"))
        {
            // ������ӵ�����
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
    /// ִ��һ�δ���
    /// </summary>
    public void ExecuteTp()
    {
        foreach (var m in list)
        {
            float moveDistance = MapManager.gridWidth;
            // ����Ŀ����������ֵ���������;��룬���У�3600Ѫ���ϵ�ֻ��һ��1500~3600�Ĵ�����1500���´�����
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
            // ���һ������Ķ�����
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
    /// ���Լ��������
    /// </summary>
    /// <param name="grid"></param>
    public override void SetGrid(BaseGrid grid)
    {
        mGrid = grid;
    }

    /// <summary>
    /// �������Ƴ�������
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
    /// ������ʼCD������CD
    /// </summary>
    /// <param name="startTime">��ʼCD</param>
    /// <param name="maxTime">����CD</param>
    public void SetStartTimeAndMaxTime(int startTime, int maxTime)
    {
        timeLeft = startTime;
        this.maxTime = maxTime;
    }
}
