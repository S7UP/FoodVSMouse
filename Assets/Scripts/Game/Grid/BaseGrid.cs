using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

using UnityEngine;

public class BaseGrid : MonoBehaviour, IGameControllerMember
{
    /// <summary>
    /// �������д�������Tagʱ��������ſ�
    /// </summary>
    public static List<ItemInGridType> NoAllowBuildTagList = new List<ItemInGridType>()
    {
        ItemInGridType.TimelinessBarrier
    };


    public BaseGridState mMainGridState; // ��Ҫ����״̬
    public List<BaseGridState> mOtherGridStateList; // ��������״̬���������ʱ�ģ�
    protected List<MouseUnit> mMouseUnitList; //��λ�ڸ����ϵ�����λ��

    protected Dictionary<FoodInGridType, FoodUnit> mFoodUnitdict; // �㶨�ڴ˸����ϵ���ʳ����ȷʵ������ȥ�Ķ�����ʱ�Եģ�
    protected Dictionary<ItemInGridType, BaseUnit> mItemUnitDict; // �ڴ˸����ϵĵ���
    protected CharacterUnit characterUnit; // �����е����ﵥλ������У�

    public bool canBuild;
    public int currentXIndex { get; private set; }
    public int currentYIndex { get; private set; }

    public GridActionPointManager gridActionPointManager; 

    private void Awake()
    {
        canBuild = true;
        mMouseUnitList = new List<MouseUnit>();
        mMainGridState = new BaseGridState(this);
        mOtherGridStateList = new List<BaseGridState>();
        mFoodUnitdict = new Dictionary<FoodInGridType, FoodUnit>();
        mItemUnitDict = new Dictionary<ItemInGridType, BaseUnit>();

        gridActionPointManager = new GridActionPointManager();
        gridActionPointManager.Initialize();
    }

    /// <summary>
    /// �ɸ���ʵ�����������ⲿ������������������ÿ�����Ӷ���������
    /// </summary>
    /// <param name="xColumn"></param>
    /// <param name="yRow"></param>
    public void InitGrid(int xColumn, int yRow)
    {
        currentXIndex = xColumn;
        currentYIndex = yRow;
        transform.localPosition = MapManager.GetGridLocalPosition(xColumn, yRow);
    }

    /// <summary>
    /// ���ӱ����Ƿ��������
    /// </summary>
    /// <returns></returns>
    public virtual bool CanBuildCard(FoodInGridType foodInGridType)
    {
        if (!canBuild)
            return false;
        foreach (var tag in NoAllowBuildTagList)
        {
            if (IsContainTag(tag))
                return false;
        }
        return true;
    }

    // ����״̬����
    public void ChangeMainGridState(BaseGridState state)
    {
        if (mMainGridState != null)
        {
            mMainGridState.OnExit();
        }
        mMainGridState = state;
        state.OnEnter();
    }

    public void AddGridState(BaseGridState state)
    {
        mOtherGridStateList.Add(state);
        state.OnEnter();
    }

    public void RemoveGridState(BaseGridState state)
    {
        mOtherGridStateList.Remove(state);
        state.OnExit();
    }


    public void OnUnitEnter(BaseUnit unit)
    {
        mMainGridState.OnUnitEnter(unit);
        foreach (var item in mOtherGridStateList)
        {
            item.OnUnitEnter(unit);
        }
    }

    public void OnUnitExit(BaseUnit unit)
    {
        mMainGridState.OnUnitExit(unit);
        foreach (var item in mOtherGridStateList)
        {
            item.OnUnitExit(unit);
        }
    }

    /// <summary>
    /// ��ȡһ����λӦ���ڸ����ϵ�λ��
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    public Vector3 GetUnitInPosition(Vector2 pos)
    {
        return transform.position + new Vector3(pos.x * MapManager.gridWidth, pos.y * MapManager.gridHeight) / 2;
    }

    // ��һ����λ�����������ڸ����ϣ��ڶ�������Ϊ��λ�������������ɵ�λ�ڸø��ӵ��ĸ��ߣ������� pos = Vector2.right������������λ���������Ϊ�ø��ӵ��ұ���
    public void SetUnitPosition(BaseUnit unit, Vector2 pos)
    {
        unit.transform.position = GetUnitInPosition(pos);
    }


    /// <summary>
    /// ����ʳ��λ�����ڸø�����
    /// </summary>
    public void SetFoodUnitInGrid(FoodUnit foodUnit)
    {
        if (foodUnit.isUseSingleGrid)
        {
            foodUnit.SetGrid(this);
            SetUnitPosition(foodUnit, Vector2.zero); // ����ҲҪͬ��������������
        }
        else
        {
            foodUnit.GetGridList().Add(this);
            // ���ʱ�������ô����
        }
        AddFoodUnit(foodUnit);
    }

    /// <summary>
    /// ������λ�����ڸø����ϣ���������λ��˵����֡���ܲ�����rigibody�����ֻ��ǿ����transform
    /// </summary>
    /// <param name="unit"></param>
    /// <param name="pos"></param>
    public void SetMouseUnitInGrid(MouseUnit unit, Vector2 pos)
    {
        unit.transform.position = MapManager.GetGridLocalPosition(currentXIndex, currentYIndex) + new Vector3(pos.x * MapManager.gridWidth, pos.y * MapManager.gridHeight) / 2;
        //SetUnitPosition(unit, Vector2.zero);
    }

    /// <summary>
    /// �����ߵ�λ�����ڸø�����
    /// </summary>
    /// <param name="unit"></param>
    /// <param name="pos"></param>
    public void SetItemUnitInGrid(BaseUnit unit)
    {
        // Tag��� ���ظ�Tag�Ļ���ȡ����һ��
        ItemInGridType tag = (ItemInGridType)unit.mType;
        if (IsContainTag(tag))
        {
            RemoveItemUnit(unit);
        }
        SetUnitPosition(unit, Vector2.zero);
        unit.SetGrid(this);
        AddItemUnit(unit);
    }

    /// <summary>
    /// ����ɫ��λ�����ڸø�����
    /// </summary>
    public void SetCharacterUnitInGrid(CharacterUnit c)
    {
        c.SetGrid(this);
        SetUnitPosition(c, Vector2.zero); // ����ҲҪͬ��������������
        AddCharacterUnit(c);
    }

    /// <summary>
    /// �����ʳ
    /// </summary>
    /// <param name="food"></param>
    public void AddFoodUnit(FoodUnit food)
    {
        gridActionPointManager.TriggerActionPoint(GridActionPointType.BeforeSetFoodUnit, new GridAction(food, this));
        //mFoodUnitList.Add(food);
        mFoodUnitdict.Add(BaseCardBuilder.GetFoodInGridType(food.mType), food);
        gridActionPointManager.TriggerActionPoint(GridActionPointType.AfterSetFoodUnit, new GridAction(food, this));
    }

    /// <summary>
    /// �����Ƴ���ʳ����
    /// </summary>
    public void RemoveFoodUnit(FoodUnit food)
    {
        gridActionPointManager.TriggerActionPoint(GridActionPointType.BeforeRemoveFoodUnit, new GridAction(food, this));
        //mFoodUnitList.Remove(food);
        mFoodUnitdict.Remove(BaseCardBuilder.GetFoodInGridType(food.mType));
        gridActionPointManager.TriggerActionPoint(GridActionPointType.AfterRemoveFoodUnit, new GridAction(food, this));
    }

    /// <summary>
    /// �������
    /// </summary>
    /// <param name="mouse"></param>
    public void AddMouseUnit(MouseUnit mouse)
    {
        gridActionPointManager.TriggerActionPoint(GridActionPointType.BeforeMouseUnitEnter, new GridAction(mouse, this));
        mMouseUnitList.Add(mouse);
        gridActionPointManager.TriggerActionPoint(GridActionPointType.AfterMouseUnitEnter, new GridAction(mouse, this));
    }

    /// <summary>
    /// �����Ƴ���ʳ����
    /// </summary>
    public void RemoveMouseUnit(MouseUnit mouse)
    {
        gridActionPointManager.TriggerActionPoint(GridActionPointType.BeforeMouseUnitExit, new GridAction(mouse, this));
        mMouseUnitList.Remove(mouse);
        gridActionPointManager.TriggerActionPoint(GridActionPointType.AfterMouseUnitExit, new GridAction(mouse, this));
    }

    /// <summary>
    /// ������ӵ�������
    /// </summary>
    public void AddItemUnit(BaseUnit unit)
    {
        gridActionPointManager.TriggerActionPoint(GridActionPointType.BeforeRemoveItemUnit, new GridAction(unit, this));
        mItemUnitDict.Add((ItemInGridType)unit.mType, unit);
        gridActionPointManager.TriggerActionPoint(GridActionPointType.AfterRemoveItemUnit, new GridAction(unit, this));
    }

    /// <summary>
    /// �����Ƴ���������
    /// </summary>
    public void RemoveItemUnit(BaseUnit unit)
    {
        gridActionPointManager.TriggerActionPoint(GridActionPointType.BeforeRemoveItemUnit, new GridAction(unit, this));
        mItemUnitDict.Remove((ItemInGridType)unit.mType);
        gridActionPointManager.TriggerActionPoint(GridActionPointType.AfterRemoveItemUnit, new GridAction(unit, this));
    }

    /// <summary>
    /// ��ӽ�ɫ�ڸø�����
    /// </summary>
    public void AddCharacterUnit(CharacterUnit c)
    {
        characterUnit = c;
    }

    /// <summary>
    /// �Ƴ���ɫ�ڱ����ϵ�����
    /// </summary>
    public CharacterUnit RemoveCharacterUnit()
    {
        CharacterUnit c = characterUnit;
        characterUnit = null;
        return c;
    }

    /// <summary>
    /// ��ȡ�����ϵ�������ʳ
    /// </summary>
    public virtual List<FoodUnit> GetFoodUnitList()
    {
        List<FoodUnit> l = new List<FoodUnit>();
        foreach (var item in mFoodUnitdict)
        {
            l.Add(item.Value);
        }
        return l;
    }

    /// <summary>
    /// ��ȡ�����ϵ���������
    /// </summary>
    public virtual List<MouseUnit> GetMouseUnitList()
    {
        return mMouseUnitList;
    }

    // �˳�ʱȡ�����
    private void OnMouseExit()
    {
        GameController.Instance.overGrid = null;
    }

    /// <summary>
    /// ��ײ�¼�
    /// </summary>
    /// <param name="collision"></param>
    public void OnCollision(Collider2D collision)
    {
        if (collision.tag.Equals("Mouse"))
        {
            MouseUnit unit = collision.GetComponent<MouseUnit>();
            if (!unit.isDeathState && unit.GetRowIndex() == currentYIndex && !mMouseUnitList.Contains(unit))
            { 
                AddMouseUnit(unit);
                OnUnitEnter(unit);
            }
        }
    }

    /// <summary>
    /// ���ø����Ƿ�����ĳ�ֱ�ǩ����ʳ
    /// </summary>
    /// <returns></returns>
    public bool IsContainTag(FoodInGridType foodInGridType)
    {
        return mFoodUnitdict.ContainsKey(foodInGridType);
    }

    public bool IsContainTag(ItemInGridType itemInGridType)
    {
        return mItemUnitDict.ContainsKey(itemInGridType);
    }

    /// <summary>
    /// ��ȡ��������߹������ȼ��ĵ�λ
    /// </summary>
    public BaseUnit GetHighestAttackPriorityUnit()
    {
        if (IsContainTag(FoodInGridType.Shield))
        {
            return mFoodUnitdict[FoodInGridType.Shield];
        }
        else if (IsContainTag(FoodInGridType.Default))
        {
            return mFoodUnitdict[FoodInGridType.Default];
        }else if (characterUnit != null)
        {
            return characterUnit;
        }
        else if (IsContainTag(FoodInGridType.WaterVehicle))
        {
            return mFoodUnitdict[FoodInGridType.WaterVehicle];
        }
        return null;
    }

    /// <summary>
    /// ��ȡ���������пɱ���������ʳ�б�
    /// </summary>
    /// <returns></returns>
    public List<FoodUnit> GetAttackableFoodUnitList()
    {
        List<FoodUnit> list = new List<FoodUnit>();
        if (IsContainTag(FoodInGridType.Shield))
        {
            list.Add(mFoodUnitdict[FoodInGridType.Shield]);
        }
        if (IsContainTag(FoodInGridType.Default))
        {
            list.Add(mFoodUnitdict[FoodInGridType.Default]);
        }
        if (IsContainTag(FoodInGridType.WaterVehicle))
        {
            list.Add(mFoodUnitdict[FoodInGridType.WaterVehicle]);
        }
        return list;
    }

    // rigibody���
    private void OnTriggerEnter2D(Collider2D collision)
    {
        OnCollision(collision);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        OnCollision(collision);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag.Equals("Mouse"))
        {
            MouseUnit unit = collision.GetComponent<MouseUnit>();
            RemoveMouseUnit(unit);
            OnUnitExit(unit);
        }
    }

    /// <summary>
    /// ��ȡ��ǰ��λ�������±�
    /// </summary>
    /// <returns></returns>
    public virtual int GetRowIndex()
    {
        return currentYIndex;
    }

    /// <summary>
    /// ��ȡ��ǰ��λ�������±�
    /// </summary>
    /// <returns></returns>
    public virtual int GetColumnIndex()
    {
        return currentXIndex;
    }

    public void MInit()
    {
        
    }

    public void MUpdate()
    {
        int lastXIndex = currentXIndex;
        int lastYIndex = currentYIndex;
        currentXIndex = MapManager.GetXIndex(transform.position.x);
        currentYIndex = MapManager.GetYIndex(transform.position.y);
        // �����ӵ��ж����귢���ı�
        if(lastYIndex != currentYIndex)
        {
            foreach (var unit in GetFoodUnitList())
            {
                // ����
                GameController.Instance.ChangeAllyRow(lastYIndex, currentYIndex, unit);
            }
            
        }

        // ͬ����ɫλ��
        if (characterUnit != null)
            SetUnitPosition(characterUnit, Vector2.zero);

        // ��鵥λ���
        List<FoodInGridType> f_List = new List<FoodInGridType>();
        foreach (var item in mFoodUnitdict)
        {
            FoodUnit u = item.Value;
            if (!u.IsAlive())
                f_List.Add(item.Key);
            else // ͬ��λ��
                SetUnitPosition(u, Vector2.zero);
        }
        foreach (var item in f_List)
        {
            mFoodUnitdict.Remove(item);
        }

        // ����
        List<MouseUnit> m_List = new List<MouseUnit>();
        foreach (var item in mMouseUnitList)
        {
            if (!item.IsAlive())
                m_List.Add(item);
        }
        foreach (var item in m_List)
        {
            mMouseUnitList.Remove(item);
        }

        // ����
        List<ItemInGridType> i_List = new List<ItemInGridType>();
        foreach (var item in mItemUnitDict)
        {
            
            BaseUnit u = item.Value;
            if (!u.IsAlive())
            {
                i_List.Add(item.Key);
            }
            else // ͬ��λ��
               SetUnitPosition(item.Value, Vector2.zero);
        }
        foreach (var item in i_List)
        {
            mItemUnitDict.Remove(item);
        }

        mMainGridState.OnUpdate();
        foreach (var item in mOtherGridStateList)
        {
            item.OnUpdate();
        }
    }

    public void MPause()
    {
        throw new System.NotImplementedException();
    }

    public void MResume()
    {
        throw new System.NotImplementedException();
    }

    public void MDestory()
    {
        throw new System.NotImplementedException();
    }

    public virtual void MPauseUpdate()
    {
        
    }
}
