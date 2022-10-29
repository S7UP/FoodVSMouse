using System;
using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// ը������Ч��
/// </summary>
public class BombAreaEffectExecution : RetangleAreaEffectExecution
{
    public BaseUnit creator;
    public float damage;

    /// <summary>
    /// ��������Ը��ӵ���ʽ���Կ�Ƭ����˺�������Ҫѡ�����¼���ģʽ֮һ
    /// </summary>
    public enum GridDamageAllyType
    {
        AOE, // ��������
        AscendingOrder, // ��˳�����������˺�
        Descending, // ���������������˺�
    }

    /// <summary>
    /// ��������Ը��ӵ���ʽ���Կ�Ƭ����˺�������Ҫѡ���˺��ķ�Χ
    /// </summary>
    public enum GridDamageAllyRange
    {
        All, // �˺������ڸ����ϵ������ѷ���λ
        Attackable, // �ɹ������ѷ���λ
    }

    private static Dictionary<GridDamageAllyType, Action<BombAreaEffectExecution, List<BaseUnit>>> GridDamageAllyTypeFuncDict = new Dictionary<GridDamageAllyType, Action<BombAreaEffectExecution, List<BaseUnit>>>() 
    {
        // �����е�λ����˺�
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
        // ��˳�����������˺�
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
        // ��ȡ�����������ѷ���λ
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
        // ��ȡ���������пɹ������ѷ���λ
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
    /// �Ը���Ϊ��λ��ȡ�Ѿ�����㷨
    /// </summary>
    private Func<BaseGrid, List<BaseUnit>> GridGetAllyListFunc;

    /// <summary>
    /// �Ը���Ϊ��λ�˺��Ѿ����㷨
    /// </summary>
    private Action<BombAreaEffectExecution, List<BaseUnit>> GridDamageAllyListTypeAction;

    public void Init(BaseUnit creator, float damage, int currentRowIndex, float colCount, float rowCount, float offsetX, int offsetY, bool isAffectFood, bool isAffectMouse)
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
        GridGetAllyListFunc = null;
        GridDamageAllyListTypeAction = null;
        base.MInit();
        SetInstantaneous();
    }

    /// <summary>
    /// �˺���λ�ķ�ʽ
    /// </summary>
    /// <param name="unit"></param>
    /// <returns>��Ŀ����ɵ�ʵ���˺�</returns>
    private float BombDamageUnit(BaseUnit unit)
    {
        float hp0 = unit.GetCurrentHp();
        // ���Ŀ���Ƿ��ֹը����ɱЧ��������������ܵ��ض��Ļҽ��˺�������ֱ����ɱ
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
    /// ��������Ը��Ӽ��ģʽ�����Ϊ�˺������ϵĿɹ���Ŀ��
    /// </summary>
    /// <param name="unit"></param>
    public override void OnGridEnter(BaseGrid g)
    {
        base.OnGridEnter(g);
        // �Ըø��ӷ�Χ�ڵ���ʳ��λ���һ���˺����㷨�Զ���
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
    /// ��ȡһ����׼���Է�Χ�Ƶ�ʵ��
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
    /// ͨ�����ӵ���ʽ���˺��ѷ���λ
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
