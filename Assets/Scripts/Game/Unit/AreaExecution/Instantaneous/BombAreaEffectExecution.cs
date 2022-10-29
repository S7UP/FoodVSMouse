using System;
using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// 炸弹爆破效果
/// </summary>
public class BombAreaEffectExecution : RetangleAreaEffectExecution
{
    public BaseUnit creator;
    public float damage;

    /// <summary>
    /// 如果采用以格子的形式来对卡片造成伤害，则需要选择以下几种模式之一
    /// </summary>
    public enum GridDamageAllyType
    {
        AOE, // 集体受伤
        AscendingOrder, // 按顺序依次吸收伤害
        Descending, // 按逆序依次吸收伤害
    }

    /// <summary>
    /// 如果采用以格子的形式来对卡片造成伤害，则需要选择伤害的范围
    /// </summary>
    public enum GridDamageAllyRange
    {
        All, // 伤害依附于格子上的所有友方单位
        Attackable, // 可攻击的友方单位
    }

    private static Dictionary<GridDamageAllyType, Action<BombAreaEffectExecution, List<BaseUnit>>> GridDamageAllyTypeFuncDict = new Dictionary<GridDamageAllyType, Action<BombAreaEffectExecution, List<BaseUnit>>>() 
    {
        // 对所有单位造成伤害
        {GridDamageAllyType.AOE,
            (bomb, list) =>
            {
                foreach (var unit in list)
                {
                    if(unit is CharacterUnit)
                        continue;
                    bomb.BombDamageUnit(unit);
                }
            } },
        // 按顺序依次吸收伤害
        {GridDamageAllyType.AscendingOrder,
            (bomb, list) =>
            {
                float damage = bomb.damage;
                foreach (var unit in list)
                {
                    if(unit is CharacterUnit)
                        continue;
                    if(damage > 0)
                        damage -= bomb.BombDamageUnit(unit);
                    else
                        break;
                }
            }
        },
    };

    private static Dictionary<GridDamageAllyRange, Func<BaseGrid, List<BaseUnit>>> GridDamageAllyRangeFuncDict = new Dictionary<GridDamageAllyRange, Func<BaseGrid, List<BaseUnit>>>()
    { 
        // 获取本格中所有友方单位
        {GridDamageAllyRange.All, (g)=>
            {
                List<BaseUnit> list = new List<BaseUnit>();
                foreach (var item in g.GetFoodUnitList())
                {
                    list.Add(item);
                }
                return list;
             }
        },
        // 获取本格中所有可攻击的友方单位
        {GridDamageAllyRange.Attackable, (g)=>
            {
                List<BaseUnit> list = new List<BaseUnit>();
                foreach (var item in g.GetAttackableFoodUnitList())
                {
                    list.Add(item);
                }
                return list;
            }
        }
    };

    /// <summary>
    /// 以格子为单位获取友军表的算法
    /// </summary>
    private Func<BaseGrid, List<BaseUnit>> GridGetAllyListFunc;

    /// <summary>
    /// 以格子为单位伤害友军的算法
    /// </summary>
    private Action<BombAreaEffectExecution, List<BaseUnit>> GridDamageAllyListTypeAction;

    public void Init(BaseUnit creator, float damage, int currentRowIndex, float colCount, float rowCount, float offsetX, int offsetY, bool isAffectFood, bool isAffectMouse)
    {
        this.creator = creator;
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
        GridGetAllyListFunc = null;
        GridDamageAllyListTypeAction = null;
        base.MInit();
        SetInstantaneous();
    }

    /// <summary>
    /// 伤害单位的方式
    /// </summary>
    /// <param name="unit"></param>
    /// <returns>对目标造成的实际伤害</returns>
    private float BombDamageUnit(BaseUnit unit)
    {
        float hp0 = unit.GetCurrentHp();
        // 检测目标是否防止炸弹秒杀效果，如果不防则受到特定的灰烬伤害，否则直接秒杀
        if (unit.NumericBox.GetBoolNumericValue(StringManager.IgnoreBombInstantKill))
            new BombDamageAction(CombatAction.ActionType.CauseDamage, creator, unit, damage).ApplyAction();
        else
            new BombDamageAction(CombatAction.ActionType.CauseDamage, creator, unit, unit.mCurrentHp).ApplyAction();
        float hp1 = Math.Max(0, unit.GetCurrentHp());
        return hp0 - hp1;
    }

    public override void OnEnemyEnter(MouseUnit unit)
    {
        base.OnEnemyEnter(unit);
        if(!isAffectGrid)
            BombDamageUnit(unit);
    }

    public override void OnFoodEnter(FoodUnit unit)
    {
        base.OnFoodEnter(unit);
        if(!isAffectGrid)
            BombDamageUnit(unit);
    }

    /// <summary>
    /// 如果开启以格子检测模式，则改为伤害格子上的可攻击目标
    /// </summary>
    /// <param name="unit"></param>
    public override void OnGridEnter(BaseGrid g)
    {
        base.OnGridEnter(g);
        // 对该格子范围内的美食单位造成一次伤害（算法自定）
        if(isAffectFood && GridGetAllyListFunc != null && GridDamageAllyListTypeAction != null)
        {
            GridDamageAllyListTypeAction(this, GridGetAllyListFunc(g));
        }
    }

    public static BombAreaEffectExecution GetInstance()
    {
        BombAreaEffectExecution e = GameManager.Instance.GetGameObjectResource(FactoryType.GameFactory, "AreaEffect/BombAreaEffect").GetComponent<BombAreaEffectExecution>();
        e.MInit();
        return e;
    }


    /// <summary>
    /// 获取一个标准的以范围计的实例
    /// </summary>
    /// <param name="creator"></param>
    /// <param name="damage"></param>
    /// <param name="pos"></param>
    /// <param name="colCount"></param>
    /// <param name="rowCount"></param>
    /// <returns></returns>
    public static BombAreaEffectExecution GetInstance(BaseUnit creator, float damage, Vector3 pos, float colCount, int rowCount)
    {
        BombAreaEffectExecution e = GameManager.Instance.GetGameObjectResource(FactoryType.GameFactory, "AreaEffect/BombAreaEffect").GetComponent<BombAreaEffectExecution>();
        e.MInit();
        e.transform.position = pos;
        e.Init(creator, damage, MapManager.GetYIndex(pos.y), colCount, rowCount, 0, 0, false, false);
        return e;
    }

    /// <summary>
    /// 通过格子的形式来伤害友方单位
    /// </summary>
    /// <param name="creator"></param>
    /// <param name="damage"></param>
    /// <param name="pos"></param>
    /// <param name="colCount"></param>
    /// <param name="rowCount"></param>
    /// <param name="damageAllyRange"></param>
    /// <param name="damageAllyType"></param>
    /// <returns></returns>
    public static BombAreaEffectExecution GetInstance(BaseUnit creator, float damage, Vector3 pos, float colCount, float rowCount, GridDamageAllyRange damageAllyRange, GridDamageAllyType damageAllyType)
    {
        BombAreaEffectExecution e = GameManager.Instance.GetGameObjectResource(FactoryType.GameFactory, "AreaEffect/BombAreaEffect").GetComponent<BombAreaEffectExecution>();
        e.MInit();
        e.Init(creator, damage, MapManager.GetYIndex(pos.y), colCount, rowCount, 0, 0, true, false);
        e.isAffectGrid = true;
        e.GridGetAllyListFunc = GridDamageAllyRangeFuncDict[damageAllyRange];
        e.GridDamageAllyListTypeAction = GridDamageAllyTypeFuncDict[damageAllyType];
        e.transform.position = pos;
        return e;
    }


    public override void ExecuteRecycle()
    {
        GameManager.Instance.PushGameObjectToFactory(FactoryType.GameFactory, "AreaEffect/BombAreaEffect", gameObject);
    }
}
