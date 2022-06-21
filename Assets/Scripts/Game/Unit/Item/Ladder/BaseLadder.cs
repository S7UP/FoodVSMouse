using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using static System.Collections.Specialized.BitVector32;
/// <summary>
/// ��������&&���������
/// </summary>
public class BaseLadder : BaseUnit
{
    private Transform spriteTrans; 
    public SpriteRenderer spriteRenderer;

    private BaseGrid mGrid;
    private float moveDistance; // ���;���
    private Action<GridAction> DefaultAfterRemoveFoodUnitActionPoint;

    public override void Awake()
    {
        base.Awake();
        spriteTrans = transform.Find("Ani_Ladder");
        spriteRenderer = spriteTrans.GetComponent<SpriteRenderer>();
        DefaultAfterRemoveFoodUnitActionPoint = (GridAction gridAction) => OnMasterGridAfterRemoveFood(gridAction);
    }

    public override void MInit()
    {
        base.MInit();
        moveDistance = 0;
        mGrid = null;
    }

    public override void SetUnitType()
    {
        mUnitType = UnitType.Item;
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
            if (m.GetRowIndex() != GetRowIndex())
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
    public virtual void RemoveFromGrid()
    {
        mGrid.RemoveItemUnit(this);
    }

    // �����󣬽�������Ϣ�Ӷ�Ӧ�����Ƴ���ͬʱ�Ƴ��¼�
    public override void AfterDeath()
    {
        RemoveFromGrid();
        if (mGrid != null)
            mGrid.gridActionPointManager.RemoveListener(GridActionPointType.AfterRemoveFoodUnit, DefaultAfterRemoveFoodUnitActionPoint);
    }

    /// <summary>
    /// ������ͬ������˵���Ⱦ�㼶
    /// </summary>
    /// <param name="arrayIndex"></param>
    public override void UpdateRenderLayer(int arrayIndex)
    {
        spriteRenderer.sortingOrder = LayerManager.CalculateSortingLayer(LayerManager.UnitType.Enemy, GetRowIndex(), -1, arrayIndex);
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
    /// ����ͼ����ʾ���ж�����ƫ����
    /// </summary>
    public override void SetSpriteLocalPosition(Vector2 vector2)
    {
        spriteTrans.localPosition = vector2;
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
