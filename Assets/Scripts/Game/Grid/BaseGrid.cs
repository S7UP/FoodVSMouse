using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

public class BaseGrid : MonoBehaviour, IGameControllerMember
{
    // ����ԭ������-�����ڸ����ϵķ���ӳ���
    // ����û˵�Ķ���Default���
    public static Dictionary<ItemNameTypeMap, ItemInGridType> ItemType_ItemInGridTypeMap = new Dictionary<ItemNameTypeMap, ItemInGridType>()
    {
        {ItemNameTypeMap.PigBarrier, ItemInGridType.TimelinessBarrier }, // ����
    };

    /// <summary>
    /// �������д�������Tagʱ��������ſ�
    /// </summary>
    public static List<ItemInGridType> NoAllowBuildTagList = new List<ItemInGridType>()
    {
        ItemInGridType.TimelinessBarrier
    };


    public BaseGridState mMainGridState; // ��Ҫ����״̬
    public List<BaseGridState> mOtherGridStateList; // ��������״̬���������ʱ�ģ�

    protected List<FoodUnit> mFoodUnitList; // λ�ڸ����ϵ���ʳ��λ��
    protected List<MouseUnit> mMouseUnitList; //��λ�ڸ����ϵ�����λ��
    protected List<BaseUnit> mItemUnitList; // λ�ڸ����ϵĵ��߱�

    protected Dictionary<FoodInGridType, FoodUnit> mFoodUnitdict; // �㶨�ڴ˸����ϵ���ʳ����ȷʵ������ȥ�Ķ�����ʱ�Եģ�
    protected Dictionary<ItemInGridType, BaseUnit> mItemUnitDict; // �ڴ˸����ϵĵ���

    public bool canBuild;
    public int currentXIndex { get; private set; }
    public int currentYIndex { get; private set; }

    private void Awake()
    {
        canBuild = true;
        mFoodUnitList = new List<FoodUnit>();
        mMouseUnitList = new List<MouseUnit>();
        mItemUnitList = new List<BaseUnit>();
        mMainGridState = new BaseGridState(this);
        mOtherGridStateList = new List<BaseGridState>();
        mFoodUnitdict = new Dictionary<FoodInGridType, FoodUnit>();
        mItemUnitDict = new Dictionary<ItemInGridType, BaseUnit>();
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
    public virtual bool CanBuildCard()
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

    // ��һ����λ�����������ڸ����ϣ��ڶ�������Ϊ��λ�������������ɵ�λ�ڸø��ӵ��ĸ��ߣ������� pos = Vector2.right������������λ���������Ϊ�ø��ӵ��ұ���
    public void SetUnitPosition(BaseUnit unit, Vector2 pos)
    {
        unit.transform.position = transform.position + new Vector3(pos.x * MapManager.gridWidth, pos.y * MapManager.gridHeight) / 2;
    }


    /// <summary>
    /// ����ʳ��λ�����ڸø�����
    /// </summary>
    public void SetFoodUnitInGrid(FoodUnit foodUnit)
    {
        mFoodUnitList.Add(foodUnit);
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
    /// ���ϰ���λ�����ڸø�����
    /// </summary>
    /// <param name="unit"></param>
    /// <param name="pos"></param>
    public void SetBarrierUnitInGrid(BaseUnit unit)
    {
        // Tag��� ���ظ�Tag�Ļ���ȡ����һ��
        ItemInGridType tag = GetItemInGridType((ItemNameTypeMap)unit.mType);
        if (IsContainTag(tag))
        {
            RemoveBarrierUnit(unit);
        }
        SetUnitPosition(unit, Vector2.zero);
        unit.SetGrid(this);
        mItemUnitList.Add(unit);
        mItemUnitDict.Add(tag, unit);
    }

    /// <summary>
    /// �����Ƴ���ʳ����
    /// </summary>
    public void RemoveFoodUnit(FoodUnit food)
    {
        mFoodUnitList.Remove(food);
        mFoodUnitdict.Remove(BaseCardBuilder.GetFoodInGridType(food.mType));
    }

    /// <summary>
    /// �����Ƴ��ϰ�����
    /// </summary>
    public void RemoveBarrierUnit(BaseUnit unit)
    {
        mItemUnitList.Remove(unit);
        mItemUnitDict.Remove(GetItemInGridType((ItemNameTypeMap)unit.mType));
    }

    /// <summary>
    /// ��ȡ�����ϵ�������ʳ
    /// </summary>
    public virtual List<FoodUnit> GetFoodUnitList()
    {
        return mFoodUnitList;
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
        if (collision.tag.Equals("Food"))
        {
            FoodUnit unit = collision.GetComponent<FoodUnit>();
            if (!unit.isDeathState && unit.GetRowIndex()==currentYIndex && !mFoodUnitList.Contains(unit))
            {
                Debug.Log("Insert Food Unit!");
                mFoodUnitList.Add(unit);
                OnUnitEnter(unit);
            }
        }else if (collision.tag.Equals("Mouse"))
        {
            MouseUnit unit = collision.GetComponent<MouseUnit>();
            if (!unit.isDeathState && unit.GetRowIndex() == currentYIndex && !mMouseUnitList.Contains(unit))
            {
                //Debug.Log("Insert Mouse Unit!");
                mMouseUnitList.Add(unit);
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
    /// ��ĳ�ֱ�ǩ����ʳ���뱾���߼����ж��ڱ���
    /// </summary>
    public void AddFoodUnitTag(FoodInGridType foodInGridType, FoodUnit foodUnit)
    {
        mFoodUnitdict.Add(foodInGridType, foodUnit);
    }

    public void AddItemUnitTag(ItemInGridType itemInGridType, BaseUnit baseUnit)
    {
        mItemUnitDict.Add(itemInGridType, baseUnit);
    }

    /// <summary>
    /// ��ȡ��������߹������ȼ�����ʳ
    /// </summary>
    public FoodUnit GetHighestAttackPriorityFoodUnit()
    {
        if (IsContainTag(FoodInGridType.Shield))
        {
            return mFoodUnitdict[FoodInGridType.Shield];
        }
        else if (IsContainTag(FoodInGridType.Default))
        {
            return mFoodUnitdict[FoodInGridType.Default];
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
        if (collision.tag.Equals("Food"))
        {
            Debug.Log("Remove Food Unit!");
            FoodUnit unit = collision.GetComponent<FoodUnit>();
            mFoodUnitList.Remove(unit);
            OnUnitExit(unit);
        }
        else if (collision.tag.Equals("Mouse"))
        {
            //Debug.Log("Remove Mouse Unit!");
            MouseUnit unit = collision.GetComponent<MouseUnit>();
            mMouseUnitList.Remove(unit);
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

        // ��鵥λ���
        List<FoodUnit> f_List = new List<FoodUnit>();
        foreach (var item in mFoodUnitList)
        {
            if (item.isDeathState)
                f_List.Add(item);
            else // ͬ��λ��
                SetUnitPosition(item, Vector2.zero);
        }
        foreach (var item in f_List)
        {
            mFoodUnitList.Remove(item);
        }

        List<MouseUnit> m_List = new List<MouseUnit>();
        foreach (var item in mMouseUnitList)
        {
            if (item.isDeathState)
                m_List.Add(item);
        }
        foreach (var item in m_List)
        {
            mMouseUnitList.Remove(item);
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

    /// <summary>
    /// ��ȡ�����ڸ����ϵķ���
    /// </summary>
    /// <returns></returns>
    public ItemInGridType GetItemInGridType(ItemNameTypeMap itemNameTypeMap)
    {
        if(ItemType_ItemInGridTypeMap.ContainsKey(itemNameTypeMap))
            return ItemType_ItemInGridTypeMap[itemNameTypeMap];
        return ItemInGridType.Default;
    }
}
