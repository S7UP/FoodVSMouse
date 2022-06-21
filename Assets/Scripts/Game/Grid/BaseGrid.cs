using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

using UnityEngine;

public class BaseGrid : MonoBehaviour, IGameControllerMember
{
    // 道具原生种类-道具在格子上的分类映射表
    // 下面没说的都是Default类的
    //public static Dictionary<ItemNameTypeMap, ItemInGridType> ItemType_ItemInGridTypeMap = new Dictionary<ItemNameTypeMap, ItemInGridType>()
    //{
    //    {ItemNameTypeMap.PigBarrier, ItemInGridType.TimelinessBarrier }, // 飞猪
    //    {ItemNameTypeMap.Spring, ItemInGridType.Ladder }, // 弹簧
    //};

    /// <summary>
    /// 当格子中存在以下Tag时，不允许放卡
    /// </summary>
    public static List<ItemInGridType> NoAllowBuildTagList = new List<ItemInGridType>()
    {
        ItemInGridType.TimelinessBarrier
    };


    public BaseGridState mMainGridState; // 主要地形状态
    public List<BaseGridState> mOtherGridStateList; // 其它地形状态表（大多是临时的）

    //protected List<FoodUnit> mFoodUnitList; // 位于格子上的美食单位表
    protected List<MouseUnit> mMouseUnitList; //　位于格子上的老鼠单位表
    //protected List<BaseUnit> mItemUnitList; // 位于格子上的道具表

    protected Dictionary<FoodInGridType, FoodUnit> mFoodUnitdict; // 恒定在此格子上的美食（即确实是种下去的而非临时性的）
    protected Dictionary<ItemInGridType, BaseUnit> mItemUnitDict; // 在此格子上的道具

    public bool canBuild;
    public int currentXIndex { get; private set; }
    public int currentYIndex { get; private set; }

    public GridActionPointManager gridActionPointManager; 

    private void Awake()
    {
        canBuild = true;
        //mFoodUnitList = new List<FoodUnit>();
        mMouseUnitList = new List<MouseUnit>();
        //mItemUnitList = new List<BaseUnit>();
        mMainGridState = new BaseGridState(this);
        mOtherGridStateList = new List<BaseGridState>();
        mFoodUnitdict = new Dictionary<FoodInGridType, FoodUnit>();
        mItemUnitDict = new Dictionary<ItemInGridType, BaseUnit>();

        gridActionPointManager = new GridActionPointManager();
        gridActionPointManager.Initialize();
    }

    /// <summary>
    /// 由格子实例创建后在外部调用这个方法，分配给每个格子独立的坐标
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
    /// 格子本身是否允许放置
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

    // 地形状态操作
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

    // 把一个单位的坐标设置在格子上，第二个参数为单位向量，表明生成点位于该格子的哪个边，比如填 pos = Vector2.right，则代表这个单位的坐标会设为该格子的右边上
    public void SetUnitPosition(BaseUnit unit, Vector2 pos)
    {
        unit.transform.position = transform.position + new Vector3(pos.x * MapManager.gridWidth, pos.y * MapManager.gridHeight) / 2;
    }


    /// <summary>
    /// 将美食单位放置在该格子上
    /// </summary>
    public void SetFoodUnitInGrid(FoodUnit foodUnit)
    {
        if (foodUnit.isUseSingleGrid)
        {
            foodUnit.SetGrid(this);
            SetUnitPosition(foodUnit, Vector2.zero); // 坐标也要同步至自身正中心
        }
        else
        {
            foodUnit.GetGridList().Add(this);
            // 多格时坐标该怎么处理？
        }
        AddFoodUnit(foodUnit);
    }

    /// <summary>
    /// 将老鼠单位放置在该格子上，对于老鼠单位来说，首帧不能操纵其rigibody，因此只能强改其transform
    /// </summary>
    /// <param name="unit"></param>
    /// <param name="pos"></param>
    public void SetMouseUnitInGrid(MouseUnit unit, Vector2 pos)
    {
        unit.transform.position = MapManager.GetGridLocalPosition(currentXIndex, currentYIndex) + new Vector3(pos.x * MapManager.gridWidth, pos.y * MapManager.gridHeight) / 2;
        //SetUnitPosition(unit, Vector2.zero);
    }

    /// <summary>
    /// 将道具单位放置在该格子上
    /// </summary>
    /// <param name="unit"></param>
    /// <param name="pos"></param>
    public void SetItemUnitInGrid(BaseUnit unit)
    {
        // Tag检测 有重复Tag的话则取消上一个
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
    /// 添加美食
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
    /// 本格移除美食引用
    /// </summary>
    public void RemoveFoodUnit(FoodUnit food)
    {
        gridActionPointManager.TriggerActionPoint(GridActionPointType.BeforeRemoveFoodUnit, new GridAction(food, this));
        //mFoodUnitList.Remove(food);
        mFoodUnitdict.Remove(BaseCardBuilder.GetFoodInGridType(food.mType));
        gridActionPointManager.TriggerActionPoint(GridActionPointType.AfterRemoveFoodUnit, new GridAction(food, this));
    }

    /// <summary>
    /// 添加老鼠
    /// </summary>
    /// <param name="mouse"></param>
    public void AddMouseUnit(MouseUnit mouse)
    {
        gridActionPointManager.TriggerActionPoint(GridActionPointType.BeforeMouseUnitEnter, new GridAction(mouse, this));
        mMouseUnitList.Add(mouse);
        gridActionPointManager.TriggerActionPoint(GridActionPointType.AfterMouseUnitEnter, new GridAction(mouse, this));
    }

    /// <summary>
    /// 本格移除美食引用
    /// </summary>
    public void RemoveMouseUnit(MouseUnit mouse)
    {
        gridActionPointManager.TriggerActionPoint(GridActionPointType.BeforeMouseUnitExit, new GridAction(mouse, this));
        mMouseUnitList.Remove(mouse);
        gridActionPointManager.TriggerActionPoint(GridActionPointType.AfterMouseUnitExit, new GridAction(mouse, this));
    }

    /// <summary>
    /// 本格添加道具引用
    /// </summary>
    public void AddItemUnit(BaseUnit unit)
    {
        gridActionPointManager.TriggerActionPoint(GridActionPointType.BeforeRemoveItemUnit, new GridAction(unit, this));
        //mItemUnitList.Add(unit);
        mItemUnitDict.Add((ItemInGridType)unit.mType, unit);
        gridActionPointManager.TriggerActionPoint(GridActionPointType.AfterRemoveItemUnit, new GridAction(unit, this));
    }

    /// <summary>
    /// 本格移除道具引用
    /// </summary>
    public void RemoveItemUnit(BaseUnit unit)
    {
        gridActionPointManager.TriggerActionPoint(GridActionPointType.BeforeRemoveItemUnit, new GridAction(unit, this));
        //mItemUnitList.Remove(unit);
        mItemUnitDict.Remove((ItemInGridType)unit.mType);
        gridActionPointManager.TriggerActionPoint(GridActionPointType.AfterRemoveItemUnit, new GridAction(unit, this));
    }

    /// <summary>
    /// 获取本格上的所有美食
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
    /// 获取本格上的所有老鼠
    /// </summary>
    public virtual List<MouseUnit> GetMouseUnitList()
    {
        return mMouseUnitList;
    }

    // 退出时取消标记
    private void OnMouseExit()
    {
        GameController.Instance.overGrid = null;
    }

    /// <summary>
    /// 碰撞事件
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
    /// 检测该格子是否种有某种标签的美食
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
    /// 获取本格中最高攻击优先级的美食
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
    /// 获取本格中所有可被攻击的美食列表
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

    // rigibody相关
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
    /// 获取当前单位所在行下标
    /// </summary>
    /// <returns></returns>
    public virtual int GetRowIndex()
    {
        return currentYIndex;
    }

    /// <summary>
    /// 获取当前单位所在列下标
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
        // 当格子的判定坐标发生改变
        if(lastYIndex != currentYIndex)
        {
            foreach (var unit in GetFoodUnitList())
            {
                // 换行
                GameController.Instance.ChangeAllyRow(lastYIndex, currentYIndex, unit);
            }
            
        }

        // 检查单位存活
        List<FoodInGridType> f_List = new List<FoodInGridType>();
        foreach (var item in mFoodUnitdict)
        {
            FoodUnit u = item.Value;
            if (!u.IsAlive())
                f_List.Add(item.Key);
            else // 同步位置
                SetUnitPosition(u, Vector2.zero);
        }
        foreach (var item in f_List)
        {
            mFoodUnitdict.Remove(item);
        }

        // 老鼠
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

        // 道具
        List<ItemInGridType> i_List = new List<ItemInGridType>();
        foreach (var item in mItemUnitDict)
        {
            
            BaseUnit u = item.Value;
            if (!u.IsAlive())
            {
                i_List.Add(item.Key);
            }
            else // 同步位置
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

    /// <summary>
    /// 获取道具在格子上的分类
    /// </summary>
    /// <returns></returns>
    //public ItemInGridType GetItemInGridType(ItemNameTypeMap itemNameTypeMap)
    //{
    //    if(ItemType_ItemInGridTypeMap.ContainsKey(itemNameTypeMap))
    //        return ItemType_ItemInGridTypeMap[itemNameTypeMap];
    //    return ItemInGridType.Default;
    //}
}
