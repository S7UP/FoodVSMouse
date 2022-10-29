using UnityEngine;
/// <summary>
/// 范围伤害效果
/// </summary>
public class DamageAreaEffectExecution : RetangleAreaEffectExecution
{
    public BaseUnit creator;
    public float damage;
    public CombatAction.ActionType actionType;

    public void Init(BaseUnit creator, CombatAction.ActionType actionType, float damage, int currentRowIndex, float colCount, float rowCount, float offsetX, float offsetY, bool isAffectFood, bool isAffectMouse)
    {
        this.creator = creator;
        this.actionType = actionType;
        this.damage = damage;
        Init(currentRowIndex, colCount, rowCount, offsetX, offsetY, isAffectFood, isAffectMouse);
    }

    /// <summary>
    /// 重置数据
    /// </summary>
    public override void MInit()
    {
        creator = null;
        damage = 0;
        base.MInit();
        SetInstantaneous();
    }

    public override void OnEnemyEnter(MouseUnit unit)
    {
        base.OnEnemyEnter(unit);
        if (actionType == CombatAction.ActionType.ReboundDamage)
        {
            new ReboundDamageAction(actionType, creator, unit, damage).ApplyAction();
        }
        else
        {
            new DamageAction(actionType, creator, unit, damage).ApplyAction();
        }
    }

    public override void OnFoodEnter(FoodUnit unit)
    {
        base.OnFoodEnter(unit);
        if (actionType == CombatAction.ActionType.ReboundDamage)
        {
            new ReboundDamageAction(actionType, creator, unit, damage).ApplyAction();
        }
        else
        {
            new DamageAction(actionType, creator, unit, damage).ApplyAction();
        }
    }

    public override void OnCharacterEnter(CharacterUnit unit)
    {
        base.OnCharacterEnter(unit);
        if (actionType == CombatAction.ActionType.ReboundDamage)
        {
            new ReboundDamageAction(actionType, creator, unit, damage).ApplyAction();
        }
        else
        {
            new DamageAction(actionType, creator, unit, damage).ApplyAction();
        }
    }

    public static DamageAreaEffectExecution GetInstance(BaseUnit creator, Vector3 pos, float colCount, float rowCount, CombatAction.ActionType actionType, float dmg)
    {
        DamageAreaEffectExecution e = GameManager.Instance.GetGameObjectResource(FactoryType.GameFactory, "AreaEffect/DamageAreaEffect").GetComponent<DamageAreaEffectExecution>();
        e.MInit();
        e.transform.position = pos;
        e.Init(creator, actionType, dmg, MapManager.GetYIndex(pos.y), colCount, rowCount, 0, 0, false, false);
        return e;
    }

    public static DamageAreaEffectExecution GetInstance()
    {
        DamageAreaEffectExecution e = GameManager.Instance.GetGameObjectResource(FactoryType.GameFactory, "AreaEffect/DamageAreaEffect").GetComponent<DamageAreaEffectExecution>();
        e.MInit();
        return e;
    }

    public override void ExecuteRecycle()
    {
        GameManager.Instance.PushGameObjectToFactory(FactoryType.GameFactory, "AreaEffect/DamageAreaEffect", gameObject);
    }

}
