/// <summary>
/// ը������Ч��
/// </summary>
public class BombAreaEffectExecution : RetangleAreaEffectExecution
{
    public BaseUnit creator;
    public float damage;

    public void Init(BaseUnit creator, float damage, int currentRowIndex, float colCount, int rowCount, float offsetX, int offsetY, bool isAffectFood, bool isAffectMouse)
    {
        this.creator = creator;
        this.damage = damage;
        Init(currentRowIndex, colCount, rowCount, offsetX, offsetY, isAffectFood, isAffectMouse);
    }

    /// <summary>
    /// ��������
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
        // ���Ŀ���Ƿ��ֹը����ɱЧ��������������ܵ��ض��Ļҽ��˺�������ֱ����ɱ
        if (unit.NumericBox.GetBoolNumericValue(StringManager.IgnoreBombInstantKill))
        {
            new BombDamageAction(CombatAction.ActionType.CauseDamage, creator, unit, damage).ApplyAction();
        }
        else
        {
            new BombDamageAction(CombatAction.ActionType.CauseDamage, creator, unit, unit.mCurrentHp).ApplyAction();
        }
        base.OnEnemyEnter(unit);
    }

    public override void OnFoodEnter(FoodUnit unit)
    {
        base.OnFoodEnter(unit);
        // ͬ��
        if (unit.NumericBox.GetBoolNumericValue(StringManager.IgnoreBombInstantKill))
        {
            new BombDamageAction(CombatAction.ActionType.CauseDamage, creator, unit, damage).ApplyAction();
        }
        else
        {
            new BombDamageAction(CombatAction.ActionType.CauseDamage, creator, unit, unit.mCurrentHp).ApplyAction();
        }
    }

    public static BombAreaEffectExecution GetInstance()
    {
        BombAreaEffectExecution e = GameManager.Instance.GetGameObjectResource(FactoryType.GameFactory, "AreaEffect/BombAreaEffect").GetComponent<BombAreaEffectExecution>();
        e.MInit();
        return e;
    }

    public override void ExecuteRecycle()
    {
        GameManager.Instance.PushGameObjectToFactory(FactoryType.GameFactory, "AreaEffect/BombAreaEffect", gameObject);
    }
}
