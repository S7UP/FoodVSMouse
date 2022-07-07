using System.Collections.Generic;
using UnityEngine;
using System;
/// <summary>
/// ��������&&���������
/// </summary>
public class BaseLadder : BaseItem
{

    private float moveDistance; // ���;���
    private Action<GridAction> DefaultAfterRemoveFoodUnitActionPoint;

    public override void Awake()
    {
        base.Awake();
        DefaultAfterRemoveFoodUnitActionPoint = (GridAction gridAction) => OnMasterGridAfterRemoveFood(gridAction);
    }

    public override void MInit()
    {
        base.MInit();
        moveDistance = 0;
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
            // ����������
            MouseUnit m = collision.GetComponent<MouseUnit>();
            if (m.GetRowIndex() != GetRowIndex() || m.GetHeight() != GetHeight())
                return;
            // ���һ������Ķ�����
            Tasker t = GameController.Instance.AddTasker(new ParabolaMovePresetTasker(m, 24.0f, 1.2f, m.transform.position, m.transform.position + (Vector3)m.moveRotate * moveDistance, false));
            m.CloseCollision();
            t.AddOtherEndEvent(delegate { m.OpenCollision(); });
        }
    }

    // rigibody���
    private void OnTriggerEnter2D(Collider2D collision)
    {
        OnCollision(collision);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        OnCollision(collision);
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
        grid.gridActionPointManager.AddListener(GridActionPointType.AfterRemoveFoodUnit, DefaultAfterRemoveFoodUnitActionPoint);
    }

    /// <summary>
    /// �������Ƴ�������
    /// </summary>
    public override void RemoveFromGrid()
    {
        mGrid.RemoveItemUnit(this);
        mGrid.gridActionPointManager.RemoveListener(GridActionPointType.AfterRemoveFoodUnit, DefaultAfterRemoveFoodUnitActionPoint);
    }

    /// <summary>
    /// �����ƶ�����
    /// </summary>
    /// <param name="d"></param>
    public void SetMoveDistance(float d)
    {
        moveDistance = d;
    }

    /// <summary>
    /// ��������Ӻ��Զ�Ϊ������ӵļ����¼�
    /// </summary>
    /// <param name="action"></param>
    public void OnMasterGridAfterRemoveFood(GridAction action)
    {
        // ÿ��������ʳ���Ƴ�ʱ��������������ʳ����û�з����͵ģ����û�����Ի�
        List<FoodUnit> fL = action.grid.GetFoodUnitList();
        bool flag = true;
        foreach (var item in fL)
        {
            FoodInGridType t = BaseCardBuilder.GetFoodInGridType(item.mType);
            if(t.Equals(FoodInGridType.Defence) || t.Equals(FoodInGridType.Shield))
            {
                flag = false;
                break;
            }
        }
        if (flag)
        {
            ExecuteDeath();
        }
    }
}
