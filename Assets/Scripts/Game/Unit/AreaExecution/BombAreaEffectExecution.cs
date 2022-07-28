using UnityEngine;
/// <summary>
/// ը������Ч��
/// </summary>
public class BombAreaEffectExecution : RetangleAreaEffectExecution
{
    public BaseUnit creator;
    public float damage;

    public override void Awake()
    {
        base.Awake();
        transform.localScale = new Vector3(MapManager.gridWidth, MapManager.gridHeight, 1);
        resourcePath += "BombAreaEffect";
    }

    public void Init(BaseUnit creator, float damage, int currentRowIndex, float colCount, int rowCount, float offsetX, int offsetY, bool isAffectFood, bool isAffectMouse)
    {
        this.creator = creator;
        this.damage = damage;
        Init(currentRowIndex, colCount, rowCount, offsetX, offsetY, isAffectFood, isAffectMouse);
    }

    /// <summary>
    /// ��������
    /// </summary>
    public override void OnEnable()
    {
        base.OnEnable();
        creator = null;
        damage = 0;
    }

    public override void EventMouse(MouseUnit unit)
    {
        // ���Ŀ���Ƿ��ֹը����ɱЧ��������������ܵ��ض��Ļҽ��˺�������ֱ����ɱ
        if (unit.NumericBox.GetBoolNumericValue(StringManager.IgnoreBombInstantKill))
        {
            new BurnDamageAction(CombatAction.ActionType.CauseDamage, creator, unit, damage).ApplyAction();
        }
        else
        {
            new BurnDamageAction(CombatAction.ActionType.CauseDamage, creator, unit, unit.mCurrentHp).ApplyAction();
        }
    }

    public override void EventFood(FoodUnit unit)
    {
        // ͬ��
        if (unit.NumericBox.GetBoolNumericValue(StringManager.IgnoreBombInstantKill))
        {
            new BurnDamageAction(CombatAction.ActionType.CauseDamage, creator, unit, damage).ApplyAction();
        }
        else
        {
            new BurnDamageAction(CombatAction.ActionType.CauseDamage, creator, unit, unit.mCurrentHp).ApplyAction();
        }
    }
}
