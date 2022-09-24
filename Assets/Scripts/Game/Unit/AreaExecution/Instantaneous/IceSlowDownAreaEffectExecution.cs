using UnityEngine;
/// <summary>
/// 寒冰减速AOE效果
/// </summary>
public class IceSlowDownAreaEffectExecution : RetangleAreaEffectExecution
{
    public BaseUnit creator;
    public int time;

    public void Init(BaseUnit creator, int time, int currentRowIndex, float colCount, int rowCount, float offsetX, int offsetY, bool isAffectFood, bool isAffectMouse)
    {
        this.creator = creator;
        this.time = time;
        Init(currentRowIndex, colCount, rowCount, offsetX, offsetY, isAffectFood, isAffectMouse);
    }

    public override void MInit()
    {
        creator = null;
        time = 0;
        base.MInit();
        SetInstantaneous();
    }

    public override void OnEnemyEnter(MouseUnit unit)
    {
        base.OnEnemyEnter(unit);
        if (!unit.NumericBox.GetBoolNumericValue(StringManager.IgnoreFrozen))
        {
            unit.AddNoCountUniqueStatusAbility(StringManager.FrozenSlowDown, new FrozenSlowStatusAbility(unit, time));
        }
    }

    public override void OnFoodEnter(FoodUnit unit)
    {
        base.OnFoodEnter(unit);
        if (!unit.NumericBox.GetBoolNumericValue(StringManager.IgnoreFrozen))
        {
            unit.AddNoCountUniqueStatusAbility(StringManager.FrozenSlowDown, new FrozenSlowStatusAbility(unit, time));
        }
    }

    public static IceSlowDownAreaEffectExecution GetInstance()
    {
        IceSlowDownAreaEffectExecution e = GameManager.Instance.GetGameObjectResource(FactoryType.GameFactory, "AreaEffect/IceSlowDownAreaEffect").GetComponent<IceSlowDownAreaEffectExecution>();
        e.MInit();
        return e;
    }

    /// <summary>
    /// 执行回收
    /// </summary>
    public override void ExecuteRecycle()
    {
        GameManager.Instance.PushGameObjectToFactory(FactoryType.GameFactory, "AreaEffect/IceSlowDownAreaEffect", this.gameObject);
    }
}
