using UnityEngine;

public class DamageAreaEffectExecution : RetangleAreaEffectExecution
{
    public BaseUnit creator;
    public float damage;
    public CombatAction.ActionType actionType;

    public override void Awake()
    {
        base.Awake();
        transform.localScale = new Vector3(MapManager.gridWidth, MapManager.gridHeight, 1);
        resourcePath += "DamageAreaEffect";
    }

    public void Init(BaseUnit creator, CombatAction.ActionType actionType, float damage, int currentRowIndex, float colCount, int rowCount, float offsetX, int offsetY, bool isAffectFood, bool isAffectMouse)
    {
        this.creator = creator;
        this.actionType = actionType;
        this.damage = damage;
        Init(currentRowIndex, colCount, rowCount, offsetX, offsetY, isAffectFood, isAffectMouse);
    }

    /// <summary>
    /// ÷ÿ÷√ ˝æ›
    /// </summary>
    public override void OnEnable()
    {
        base.OnEnable();
        creator = null;
        damage = 0;
    }

    public override void EventMouse(MouseUnit unit)
    {
        base.EventMouse(unit);
        if (actionType == CombatAction.ActionType.ReboundDamage)
        {
            new ReboundDamageAction(actionType, creator, unit, damage).ApplyAction();
        }
        else
        {
            new DamageAction(actionType, creator, unit, damage).ApplyAction();
        }
    }

    public override void EventFood(FoodUnit unit)
    {
        base.EventFood(unit);
        if (actionType == CombatAction.ActionType.ReboundDamage)
        {
            new ReboundDamageAction(actionType, creator, unit, damage).ApplyAction();
        }
        else
        {
            new DamageAction(actionType, creator, unit, damage).ApplyAction();
        }
    }

    public override void EventCharacter(CharacterUnit unit)
    {
        base.EventCharacter(unit);
        if (actionType == CombatAction.ActionType.ReboundDamage)
        {
            new ReboundDamageAction(actionType, creator, unit, damage).ApplyAction();
        }
        else
        {
            new DamageAction(actionType, creator, unit, damage).ApplyAction();
        }
    }
}
