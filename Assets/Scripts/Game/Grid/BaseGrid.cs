using System.Collections.Generic;

using UnityEngine;

public class BaseGrid : MonoBehaviour, IGameControllerMember
{
    /// <summary>
    /// 当格子中存在以下Tag时，不允许放卡
    /// </summary>
    public static List<ItemInGridType> NoAllowBuildTagList = new List<ItemInGridType>()
    {
        ItemInGridType.TimelinessBarrier
    };

    /// <summary>
    /// 格子类型危险权重表
    /// </summary>
    public static Dictionary<GridType, int> GridDangerousWeightDict = new Dictionary<GridType, int>()
    {
        { GridType.None, 1},
        { GridType.Default, 1},
        { GridType.Water, 3}, // 众所周知水是剧毒的
        { GridType.Lava, 2},
    };


    public BaseGridState mMainGridState; // 主要地形状态
    public List<BaseGridState> mOtherGridStateList = new List<BaseGridState>(); // 其它地形状态表（大多是临时的）
    protected List<MouseUnit> mMouseUnitList = new List<MouseUnit>(); //　位于格子上的老鼠单位表

    protected Dictionary<FoodInGridType, FoodUnit> mFoodUnitdict = new Dictionary<FoodInGridType, FoodUnit>(); // 恒定在此格子上的美食（即确实是种下去的而非临时性的）
    protected Dictionary<ItemInGridType, BaseUnit> mItemUnitDict = new Dictionary<ItemInGridType, BaseUnit>(); // 在此格子上的道具
    protected CharacterUnit characterUnit; // 所持有的人物单位（如果有）

    public bool canBuild;
    public int currentXIndex { get; private set; }
    public int currentYIndex { get; private set; }
    public bool isHeightLimit = true; // 地形效果是否只作用于特定高度
    public float mHeight = 0;

    public GridActionPointManager gridActionPointManager = new GridActionPointManager();

    private void Awake()
    {

    }

    public void MInit()
    {
        mMainGridState = new BaseGridState(this);
        mOtherGridStateList.Clear();
        mMouseUnitList.Clear();
        mFoodUnitdict.Clear();
        mItemUnitDict.Clear();
        characterUnit = null;
        canBuild = true;
        currentXIndex = 0;
        currentYIndex = 0;
        isHeightLimit = true;
        mHeight = 0;
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
    public virtual bool CanBuildCard(FoodInGridType foodInGridType)
    {
        if (!canBuild)
            return false;
        // 人物被视为默认类型的美食，即不能与默认类型的共存
        if (characterUnit != null && foodInGridType.Equals(FoodInGridType.Default))
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

    /// <summary>
    /// 获取一个单位应该在格子上的位置
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    public Vector3 GetUnitInPosition(Vector2 pos)
    {
        return transform.position + new Vector3(pos.x * MapManager.gridWidth, pos.y * MapManager.gridHeight) / 2;
    }

    // 把一个单位的坐标设置在格子上，第二个参数为单位向量，表明生成点位于该格子的哪个边，比如填 pos = Vector2.right，则代表这个单位的坐标会设为该格子的右边上
    public void SetUnitPosition(BaseUnit unit, Vector2 pos)
    {
        unit.transform.position = GetUnitInPosition(pos);
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
    /// <returns>含有同样tag的旧ItemUnit</returns>
    public BaseUnit SetItemUnitInGrid(BaseUnit unit)
    {
        BaseUnit old = null; // 原unit
        // Tag检测 有重复Tag的话则取消上一个
        ItemInGridType tag = (ItemInGridType)unit.mType;
        if (IsContainTag(tag))
        {
            old = RemoveItemUnit(unit);
        }
        SetUnitPosition(unit, Vector2.zero);
        unit.SetGrid(this);
        AddItemUnit(unit);
        return old;
    }

    /// <summary>
    /// 将角色单位放置在该格子上
    /// </summary>
    public void SetCharacterUnitInGrid(CharacterUnit c)
    {
        c.SetGrid(this);
        SetUnitPosition(c, Vector2.zero); // 坐标也要同步至自身正中心
        AddCharacterUnit(c);
    }

    /// <summary>
    /// 添加美食
    /// </summary>
    /// <param name="food"></param>
    public void AddFoodUnit(FoodUnit food)
    {
        FoodInGridType t = BaseCardBuilder.GetFoodInGridType(food.mType);
        // 需要剔除并销毁上一个与自己相同标签的卡，这种情况会在开启快捷放卡设置后出现
        if (IsContainTag(t))
        {
            mFoodUnitdict[t].DeathEvent();
            RemoveFoodUnit(mFoodUnitdict[t]);
        }
        // 之后再把自身加入
        gridActionPointManager.TriggerActionPoint(GridActionPointType.BeforeSetFoodUnit, new GridAction(food, this));
        mFoodUnitdict.Add(t, food);
        gridActionPointManager.TriggerActionPoint(GridActionPointType.AfterSetFoodUnit, new GridAction(food, this));
        OnUnitEnter(food);
    }

    /// <summary>
    /// 本格移除美食引用
    /// </summary>
    public void RemoveFoodUnit(FoodUnit food)
    {
        gridActionPointManager.TriggerActionPoint(GridActionPointType.BeforeRemoveFoodUnit, new GridAction(food, this));
        mFoodUnitdict.Remove(BaseCardBuilder.GetFoodInGridType(food.mType));
        OnUnitExit(food);
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
        OnUnitEnter(mouse);
        gridActionPointManager.TriggerActionPoint(GridActionPointType.AfterMouseUnitEnter, new GridAction(mouse, this));
    }

    /// <summary>
    /// 本格移除老鼠引用
    /// </summary>
    public void RemoveMouseUnit(MouseUnit mouse)
    {
        gridActionPointManager.TriggerActionPoint(GridActionPointType.BeforeMouseUnitExit, new GridAction(mouse, this));
        mMouseUnitList.Remove(mouse);
        OnUnitExit(mouse);
        gridActionPointManager.TriggerActionPoint(GridActionPointType.AfterMouseUnitExit, new GridAction(mouse, this));
    }

    /// <summary>
    /// 本格添加道具引用
    /// </summary>
    public void AddItemUnit(BaseUnit unit)
    {
        gridActionPointManager.TriggerActionPoint(GridActionPointType.BeforeRemoveItemUnit, new GridAction(unit, this));
        mItemUnitDict.Add((ItemInGridType)unit.mType, unit);
        OnUnitEnter(unit);
        gridActionPointManager.TriggerActionPoint(GridActionPointType.AfterRemoveItemUnit, new GridAction(unit, this));
    }

    /// <summary>
    /// 本格移除道具引用
    /// </summary>
    public BaseUnit RemoveItemUnit(BaseUnit unit)
    {
        gridActionPointManager.TriggerActionPoint(GridActionPointType.BeforeRemoveItemUnit, new GridAction(unit, this));
        BaseUnit old = null;
        if (mItemUnitDict.ContainsKey((ItemInGridType)unit.mType))
        {
            old = mItemUnitDict[(ItemInGridType)unit.mType];
            OnUnitExit(old);
            mItemUnitDict.Remove((ItemInGridType)unit.mType);
        }
        gridActionPointManager.TriggerActionPoint(GridActionPointType.AfterRemoveItemUnit, new GridAction(unit, this));
        return old;
    }

    /// <summary>
    /// 添加角色在该格子上
    /// </summary>
    public void AddCharacterUnit(CharacterUnit c)
    {
        characterUnit = c;
        OnUnitEnter(c);
    }

    /// <summary>
    /// 移除角色在本格上的引用
    /// </summary>
    public CharacterUnit RemoveCharacterUnit()
    {
        CharacterUnit c = characterUnit;
        characterUnit = null;
        OnUnitExit(c);
        return c;
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

    /// <summary>
    /// 碰撞事件
    /// </summary>
    /// <param name="collision"></param>
    public void OnCollision(Collider2D collision)
    {
        if (collision.tag.Equals("Mouse"))
        {
            MouseUnit unit = collision.GetComponent<MouseUnit>();
            if (!unit.isDeathState && unit.GetRowIndex() == currentYIndex && unit.CanBeSelectedAsTarget() && (!isHeightLimit || unit.GetHeight()==mHeight) && !mMouseUnitList.Contains(unit))
            { 
                AddMouseUnit(unit);
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
    /// 获取本格中最高攻击优先级的单位
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
    /// 获取本格中所有可被攻击的美食列表，从前往后攻击优先级逐级递减
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

    /// <summary>
    /// 获取本格的人物，如果没有则返回空值
    /// </summary>
    /// <returns></returns>
    public CharacterUnit GetCharacterUnit()
    {
        return characterUnit;
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

        // 同步角色位置
        if (characterUnit != null)
            SetUnitPosition(characterUnit, Vector2.zero);

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

    public virtual void MPauseUpdate()
    {
        
    }

    /// <summary>
    /// 获取一个实例
    /// </summary>
    public static BaseGrid GetInstance()
    {
        return GameManager.Instance.GetGameObjectResource(FactoryType.GameFactory, "Grid/Grid").GetComponent<BaseGrid>();
    }

    /// <summary>
    /// 执行回收
    /// </summary>
    public virtual void ExecuteRecycle()
    {
        GameManager.Instance.PushGameObjectToFactory(FactoryType.GameFactory, "Grid/Grid", this.gameObject);
    }

    /// <summary>
    /// 获取该格子的主要地形类型
    /// </summary>
    public GridType GetGridType()
    {
        if (mMainGridState != null)
            return mMainGridState.gridType;
        else
            return GridType.None;
    }

    /// <summary>
    /// 获取本格子的默认危险权重
    /// </summary>
    /// <returns></returns>
    public int GetDefaultDangerousWeight()
    {
        if (GridDangerousWeightDict.ContainsKey(GetGridType()))
        {
            return GridDangerousWeightDict[GetGridType()];
        }
        else
        {
            return 1;
        }
    }
}
