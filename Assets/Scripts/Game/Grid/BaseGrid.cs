using System.Collections.Generic;

using UnityEngine;

public class BaseGrid : MonoBehaviour, IGameControllerMember
{
    /// <summary>
    /// 当格子中存在以下Tag时，不允许放卡
    /// </summary>
    public static List<ItemNameTypeMap> NoAllowBuildTagList = new List<ItemNameTypeMap>()
    {
        ItemNameTypeMap.Barrier,
        ItemNameTypeMap.WindCave, 
    };

    protected Dictionary<GridType, BaseGridType> mGridTypeDict = new Dictionary<GridType, BaseGridType>(); // 依附于格子的地形状态
    protected List<MouseUnit> mMouseUnitList = new List<MouseUnit>(); //　位于格子上的老鼠单位表
    protected Dictionary<FoodInGridType, FoodUnit> mFoodUnitDict = new Dictionary<FoodInGridType, FoodUnit>(); // 恒定在此格子上的美食（即确实是种下去的而非临时性的）
    protected Dictionary<ItemNameTypeMap, BaseUnit> mItemUnitDict = new Dictionary<ItemNameTypeMap, BaseUnit>(); // 在此格子上的道具
    protected CharacterUnit characterUnit; // 所持有的人物单位（如果有）

    public bool canBuild;
    public int currentXIndex { get; private set; }
    public int currentYIndex { get; private set; }
    public bool isHeightLimit = true; // 地形效果是否只作用于特定高度
    public float mHeight = 0;

    private BoxCollider2D mBoxCollider2D;

    public GridActionPointManager gridActionPointManager = new GridActionPointManager();

    private void Awake()
    {
        mBoxCollider2D = GetComponent<BoxCollider2D>();
    }

    public void MInit()
    {
        mGridTypeDict.Clear();
        mMouseUnitList.Clear();
        mFoodUnitDict.Clear();
        mItemUnitDict.Clear();
        characterUnit = null;
        canBuild = true;
        currentXIndex = 0;
        currentYIndex = 0;
        isHeightLimit = true;
        mHeight = 0;
        gridActionPointManager.Initialize();
        SetBoxCollider2D(Vector2.zero, new Vector2(MapManager.gridWidth, MapManager.gridHeight));
    }

    public void SetBoxCollider2D(Vector2 offset, Vector2 size)
    {
        mBoxCollider2D.offset = offset;
        mBoxCollider2D.size = size;
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

    /// <summary>
    /// 添加一个地形类型在该格子上
    /// </summary>
    public void AddGridType(GridType type, BaseGridType t)
    {
        if (mGridTypeDict.ContainsKey(type))
            RemoveGridType(type);
        mGridTypeDict.Add(type, t);
        t.transform.SetParent(transform);
        t.transform.localPosition = Vector3.zero;
        t.MInit();
    }

    public BaseGridType RemoveGridType(GridType type)
    {
        BaseGridType t = mGridTypeDict[type];
        mGridTypeDict.Remove(type);
        t.MDestory();
        return t;
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
        ItemNameTypeMap tag = (ItemNameTypeMap)unit.mType;
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
            mFoodUnitDict[t].ExecuteDeath();
            // ExecuteDeath里已经包括RemoveFoodUnit了，这段注释请别删除！
            // 这里再保险处理一次
            if(IsContainTag(t))
                RemoveFoodUnit(mFoodUnitDict[t]);
        }
        // 之后再把自身加入
        gridActionPointManager.TriggerActionPoint(GridActionPointType.BeforeSetFoodUnit, new GridAction(food, this));
        mFoodUnitDict.Add(t, food);
        gridActionPointManager.TriggerActionPoint(GridActionPointType.AfterSetFoodUnit, new GridAction(food, this));
    }

    /// <summary>
    /// 本格移除美食引用
    /// </summary>
    public void RemoveFoodUnit(FoodUnit food)
    {
        gridActionPointManager.TriggerActionPoint(GridActionPointType.BeforeRemoveFoodUnit, new GridAction(food, this));
        mFoodUnitDict.Remove(BaseCardBuilder.GetFoodInGridType(food.mType));
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
    /// 本格移除老鼠引用
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
        mItemUnitDict.Add((ItemNameTypeMap)unit.mType, unit);
        gridActionPointManager.TriggerActionPoint(GridActionPointType.AfterRemoveItemUnit, new GridAction(unit, this));
    }

    /// <summary>
    /// 本格移除道具引用
    /// </summary>
    public BaseUnit RemoveItemUnit(BaseUnit unit)
    {
        gridActionPointManager.TriggerActionPoint(GridActionPointType.BeforeRemoveItemUnit, new GridAction(unit, this));
        BaseUnit old = null;
        if (mItemUnitDict.ContainsKey((ItemNameTypeMap)unit.mType))
        {
            old = mItemUnitDict[(ItemNameTypeMap)unit.mType];
            mItemUnitDict.Remove((ItemNameTypeMap)unit.mType);
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
    }

    /// <summary>
    /// 移除角色在本格上的引用
    /// </summary>
    public CharacterUnit RemoveCharacterUnit()
    {
        CharacterUnit c = characterUnit;
        characterUnit = null;
        return c;
    }

    /// <summary>
    /// 获取本格上的所有美食
    /// </summary>
    public virtual List<FoodUnit> GetFoodUnitList()
    {
        List<FoodUnit> l = new List<FoodUnit>();
        foreach (var item in mFoodUnitDict)
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
        return mFoodUnitDict.ContainsKey(foodInGridType);
    }

    public bool IsContainTag(ItemNameTypeMap itemInGridType)
    {
        return mItemUnitDict.ContainsKey(itemInGridType);
    }

    /// <summary>
    /// 默认情况下，获取本格中最高攻击优先级的单位
    /// </summary>
    public BaseUnit GetHighestAttackPriorityUnit()
    {
        if (IsContainTag(FoodInGridType.Shield))
        {
            return mFoodUnitDict[FoodInGridType.Shield];
        }
        else if (IsContainTag(FoodInGridType.Default))
        {
            return mFoodUnitDict[FoodInGridType.Default];
        }else if (characterUnit != null)
        {
            return characterUnit;
        }
        else if (IsContainTag(FoodInGridType.Bomb))
        {
            return mFoodUnitDict[FoodInGridType.Bomb];
        }
        return null;
    }

    public FoodUnit GetFoodByTag(FoodInGridType type)
    {
        if (IsContainTag(type))
            return mFoodUnitDict[type];
        else
            return null;
    }

    public BaseUnit GetItemByTag(ItemNameTypeMap type)
    {
        if (IsContainTag(type))
            return mItemUnitDict[type];
        else
            return null;
    }

    /// <summary>
    /// 获取本格中最高攻击优先级的单位，其中包括水中载具
    /// </summary>
    /// <returns></returns>
    public BaseUnit GetHighestAttackPriorityUnitIncludeWaterVehicle()
    {
        if (IsContainTag(FoodInGridType.Shield))
        {
            return mFoodUnitDict[FoodInGridType.Shield];
        }
        else if (IsContainTag(FoodInGridType.Default))
        {
            return mFoodUnitDict[FoodInGridType.Default];
        }
        else if (characterUnit != null)
        {
            return characterUnit;
        }
        else if (IsContainTag(FoodInGridType.Bomb))
        {
            return mFoodUnitDict[FoodInGridType.Bomb];
        }
        else if (IsContainTag(FoodInGridType.WaterVehicle))
        {
            return mFoodUnitDict[FoodInGridType.WaterVehicle];
        }
        return null;
    }

    /// <summary>
    /// 获取本格中最高铲除优先级的单位
    /// </summary>
    /// <returns></returns>
    public BaseUnit GetHighestRemovePriorityUnit()
    {   
        if (IsContainTag(FoodInGridType.Bomb))
        {
            return mFoodUnitDict[FoodInGridType.Bomb];
        }
        else if (IsContainTag(FoodInGridType.Shield))
        {
            return mFoodUnitDict[FoodInGridType.Shield];
        }
        else if (IsContainTag(FoodInGridType.Default))
        {
            return mFoodUnitDict[FoodInGridType.Default];
        }
        else if (characterUnit != null)
        {
            return characterUnit;
        }
        else if (IsContainTag(FoodInGridType.WaterVehicle))
        {
            return mFoodUnitDict[FoodInGridType.WaterVehicle];
        }
        else if (IsContainTag(FoodInGridType.LavaVehicle))
        {
            return mFoodUnitDict[FoodInGridType.LavaVehicle];
        }
        return null;
    }

    /// <summary>
    /// 默认情况下，获取本格中所有可被攻击的美食列表，从前往后攻击优先级逐级递减
    /// </summary>
    /// <returns></returns>
    public List<FoodUnit> GetAttackableFoodUnitList()
    {
        List<FoodUnit> list = new List<FoodUnit>();
        if (IsContainTag(FoodInGridType.Shield))
        {
            list.Add(mFoodUnitDict[FoodInGridType.Shield]);
        }
        if (IsContainTag(FoodInGridType.Default))
        {
            list.Add(mFoodUnitDict[FoodInGridType.Default]);
        }
        return list;
    }

    /// <summary>
    /// 获取本格中所有可被攻击的美食列表，从前往后攻击优先级逐级递减，包括水载具
    /// </summary>
    /// <returns></returns>
    public List<FoodUnit> GetAttackableFoodUnitListIncludeWaterVehicle()
    {
        List<FoodUnit> list = new List<FoodUnit>();
        if (IsContainTag(FoodInGridType.Shield))
        {
            list.Add(mFoodUnitDict[FoodInGridType.Shield]);
        }
        if (IsContainTag(FoodInGridType.Default))
        {
            list.Add(mFoodUnitDict[FoodInGridType.Default]);
        }
        if (IsContainTag(FoodInGridType.WaterVehicle))
        {
            list.Add(mFoodUnitDict[FoodInGridType.WaterVehicle]);
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
        foreach (var item in mFoodUnitDict)
        {
            FoodUnit u = item.Value;
            if (!u.IsAlive())
                f_List.Add(item.Key);
            else // 同步位置
                SetUnitPosition(u, Vector2.zero);
        }
        foreach (var item in f_List)
        {
            mFoodUnitDict.Remove(item);
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
        List<ItemNameTypeMap> i_List = new List<ItemNameTypeMap>();
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

        // 依附于格子的地形更新
        foreach (var keyValuePair in mGridTypeDict)
        {
            keyValuePair.Value.MUpdate();
        }
    }

    public void MPause()
    {
        
    }

    public void MResume()
    {
        
    }

    public void MDestory()
    {
        
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
        return GridType.NotBuilt;
    }

    /// <summary>
    /// 是否包含某种地形
    /// </summary>
    /// <returns></returns>
    public bool IsContainGridType(GridType type)
    {
        foreach (var keyValuePair in mGridTypeDict)
        {
            if (keyValuePair.Key == type)
                return true;
        }
        return false;
    }

    /// <summary>
    /// 获取该格子附带的所有地形
    /// </summary>
    /// <returns></returns>
    public List<GridType> GetAllGridType()
    {
        List<GridType> list = new List<GridType>();
        foreach (var keyValuePair in mGridTypeDict)
        {
            list.Add(keyValuePair.Key);
        }
        // 如果什么地形也没包含那就是默认的
        if (list.Count <= 0)
            list.Add(GridType.Default);
        return list;
    }
}
