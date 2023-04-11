using System.Collections.Generic;
using UnityEngine;
using System;
/// <summary>
/// �������� && ���������
/// </summary>
public class BaseLadder : BaseItem
{
    private List<BaseUnit> unitList = new List<BaseUnit>();
    private float moveDistance; // ���;���
    private float maxHight;
    private Action<GridAction> DefaultAfterRemoveFoodUnitActionPoint;

    public override void Awake()
    {
        base.Awake();
        DefaultAfterRemoveFoodUnitActionPoint = (GridAction gridAction) => OnMasterGridAfterRemoveFood(gridAction);
    }

    public override void MInit()
    {
        unitList.Clear();
        base.MInit();
        moveDistance = 0;
        maxHight = 0;
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
            if (unitList.Contains(m) || m.IsBoss() || !UnitManager.CanBeSelectedAsTarget(this, m))
                return;
            if (m.GetRowIndex() != GetRowIndex() || m.GetHeight() != GetHeight() || !m.moveRotate.Equals(Vector2.left))
                return;
            // ��һ���ж�����������Ŀ�겻�Ϳ�
            if (UnitManager.IsFlying(m))
                return;

            // �������λ��ӵ�����
            unitList.Add(m);
            // ���һ�����������
            CustomizationTask t = TaskManager.AddParabolaTask(m, TransManager.TranToVelocity(12), maxHight, m.transform.position, m.transform.position + (Vector3)m.moveRotate * moveDistance, false);
            // �ҽ�ֹ�ƶ�
            m.DisableMove(true);
            t.AddOnExitAction(delegate {
                m.DisableMove(false);
            });
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


    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag.Equals("Mouse"))
        {
            // ����������
            MouseUnit m = collision.GetComponent<MouseUnit>();
            unitList.Remove(m);
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
    public void SetMoveDistanceAndMaxHight(float d, float h)
    {
        moveDistance = d;
        maxHight = h;
    }

    /// <summary>
    /// ��������Ӻ��Զ�Ϊ������ӵļ����¼�
    /// </summary>
    /// <param name="action"></param>
    public void OnMasterGridAfterRemoveFood(GridAction action)
    {
        // ÿ��������ʳ���Ƴ�ʱ��������������ʳ����û�з����͵ģ����û�����Ի�
        List<FoodUnit> list = action.grid.GetFoodUnitList();
        bool flag = true;
        foreach (var unit in list)
        {
            FoodNameTypeMap f_type = (FoodNameTypeMap)unit.mType;
            if (FoodManager.DenfenceCard.Contains(f_type))
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
