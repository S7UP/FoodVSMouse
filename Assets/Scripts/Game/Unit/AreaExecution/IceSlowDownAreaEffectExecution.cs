using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// ��������AOEЧ��
/// </summary>
public class IceSlowDownAreaEffectExecution : RetangleAreaEffectExecution
{
    public BaseUnit creator;
    public int time;

    public override void Awake()
    {
        base.Awake();
        transform.localScale = new Vector3(MapManager.gridWidth, MapManager.gridHeight, 1);
        resourcePath += "IceSlowDownAreaEffect";
    }

    public void Init(BaseUnit creator, int time, int currentRowIndex, int colCount, int rowCount, float offsetX, int offsetY, bool isAffectFood, bool isAffectMouse)
    {
        this.creator = creator;
        this.time = time;
        Init(currentRowIndex, colCount, rowCount, offsetX, offsetY, isAffectFood, isAffectMouse);
    }

    /// <summary>
    /// ���պ���������
    /// </summary>
    public void OnDisable()
    {
        creator = null;
        time = 0;
    }

    public override void EventMouse(MouseUnit unit)
    {
        if (!unit.NumericBox.GetBoolNumericValue(StringManager.IgnoreFrozen))
        {
            unit.AddNoCountUniqueStatusAbility(StringManager.FrozenSlowDown, new FrozenSlowStatusAbility(unit, time));
        }
    }

    public override void EventFood(FoodUnit unit)
    {
        if (!unit.NumericBox.GetBoolNumericValue(StringManager.IgnoreFrozen))
        {
            unit.AddNoCountUniqueStatusAbility(StringManager.FrozenSlowDown, new FrozenSlowStatusAbility(unit, time));
        }
    }
}
