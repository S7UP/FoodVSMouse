using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static UnityEngine.GraphicsBuffer;

public class BombAreaEffectExecution : AreaEffectExecution
{
    public BaseUnit creator;
    public float damage;
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
        resourcePath += "BombAreaEffect";
    }

    public void Init(BaseUnit creator, float damage, int currentRowIndex, int colCount, int rowCount, float offsetX, int offsetY)
    {
        this.creator = creator;
        this.damage = damage;
        this.currentRowIndex = currentRowIndex;
        this.rowCount = rowCount;
        this.colCount = colCount;
        this.offsetX = offsetX;
        this.offsetY = offsetY;
        boxCollider2D.offset = new Vector2(offsetX, offsetY);
        boxCollider2D.size = new Vector2(colCount*1.05f, rowCount);
    }

    /// <summary>
    /// 回收后重置数据
    /// </summary>
    public void OnDisable()
    {
        creator = null;
        damage = 0;
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
        int endIndex = Mathf.Min(MapMaker.yRow-1, currentRowIndex + c - offsetY);
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
        // 检测目标是否防止炸弹秒杀效果，如果不防则受到特定的灰烬伤害，否则直接秒杀
        if (unit.NumericBox.GetBoolNumericValue(StringManager.IgnoreBombInstantKill))
        {
            new BurnDamageAction(CombatAction.ActionType.CauseDamage, creator, unit, damage).ApplyAction();
        }
        else
        {
            new BurnDamageAction(CombatAction.ActionType.CauseDamage, creator, unit, float.MaxValue).ApplyAction();
        }
    }

    public override void EventFood(FoodUnit unit)
    {
        // 美食不受影响
    }
}
