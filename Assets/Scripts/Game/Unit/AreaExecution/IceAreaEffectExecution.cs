using UnityEngine;
/// <summary>
/// 范围冻结效果
/// </summary>
public class IceAreaEffectExecution : RetangleAreaEffectExecution
{
    public BaseUnit creator;
    public int time;

    public override void Awake()
    {
        base.Awake();
        transform.localScale = new Vector3(MapManager.gridWidth, MapManager.gridHeight, 1);
        resourcePath += "IceAreaEffect";
    }

    public void Init(BaseUnit creator, int time, int currentRowIndex, float colCount, int rowCount, float offsetX, int offsetY, bool isAffectFood, bool isAffectMouse)
    {
        this.creator = creator;
        this.time = time;
        Init(currentRowIndex, colCount, rowCount, offsetX, offsetY, isAffectFood, isAffectMouse);
    }

    /// <summary>
    /// 重置数据
    /// </summary>
    public override void OnEnable()
    {
        base.OnEnable();
        creator = null;
        time = 0;
    }

    public override void EventMouse(MouseUnit unit)
    {
        base.EventMouse(unit);
        if (!unit.NumericBox.GetBoolNumericValue(StringManager.IgnoreFrozen))
        {
            unit.AddNoCountUniqueStatusAbility(StringManager.Frozen, new FrozenStatusAbility(unit, time, false));
        }
    }

    public override void EventFood(FoodUnit unit)
    {
        base.EventFood(unit);
        if (!unit.NumericBox.GetBoolNumericValue(StringManager.IgnoreFrozen))
        {
            unit.AddNoCountUniqueStatusAbility(StringManager.Frozen, new FrozenStatusAbility(unit, time, false));
        }
    }

    public override void EventCharacter(CharacterUnit unit)
    {
        base.EventCharacter(unit);
        if (!unit.NumericBox.GetBoolNumericValue(StringManager.IgnoreFrozen))
        {
            unit.AddNoCountUniqueStatusAbility(StringManager.Frozen, new FrozenStatusAbility(unit, time, false));
        }
    }
}
