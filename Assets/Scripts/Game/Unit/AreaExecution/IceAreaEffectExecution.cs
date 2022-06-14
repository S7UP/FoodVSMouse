using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceAreaEffectExecution : AreaEffectExecution
{
    public BaseUnit creator;
    public int time;
    public int currentRowIndex; // 当前行下标
    public float offsetX;
    public int offsetY;
    public int colCount; // 受影响列数
    public int rowCount; // 受影响行数
    public BoxCollider2D boxCollider2D;

    public override void Awake()
    {
        base.Awake();
        transform.localScale = new Vector3(MapManager.gridWidth, MapManager.gridHeight, 1);
        resourcePath += "IceAreaEffect";
    }

    public void Init(BaseUnit creator, int time, int currentRowIndex, int colCount, int rowCount, float offsetX, int offsetY)
    {
        this.creator = creator;
        this.time = time;
        this.currentRowIndex = currentRowIndex;
        this.rowCount = rowCount;
        this.colCount = colCount;
        this.offsetX = offsetX;
        this.offsetY = offsetY;
        boxCollider2D.offset = new Vector2(offsetX, offsetY);
        boxCollider2D.size = new Vector2(colCount * 1.05f, rowCount);
    }

    /// <summary>
    /// 回收后重置数据
    /// </summary>
    public void OnDisable()
    {
        creator = null;
        time = 0;
        currentRowIndex = 0;
        rowCount = 0;
    }

    /// <summary>
    /// 行判断（列判断已经包含在碰撞之中了，因此可以不做）
    /// </summary>
    /// <param name="baseUnit"></param>
    /// <returns></returns>
    public override bool IsMeetingCondition(BaseUnit baseUnit)
    {
        if (baseUnit.isDeathState)
            return false;
        int c = (rowCount - 1) / 2;
        int startIndex = Mathf.Max(0, currentRowIndex - c - offsetY);
        int endIndex = Mathf.Min(MapController.yRow - 1, currentRowIndex + c - offsetY);
        int index = baseUnit.GetRowIndex();
        for (int i = startIndex; i <= endIndex; i++)
        {
            if (index == i)
                return true;
        }
        return false;
    }

    public override void EventMouse(MouseUnit unit)
    {
        // 老鼠不受影响
    }

    public override void EventFood(FoodUnit unit)
    {
        if (!unit.NumericBox.GetBoolNumericValue(StringManager.IgnoreFrozen))
        {
            unit.AddNoCountUniqueStatusAbility(StringManager.Frozen, new FrozenStatusAbility(unit, time));
        }
    }
}
