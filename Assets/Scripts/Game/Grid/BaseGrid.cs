using System.Collections.Generic;
using System;
using UnityEngine;

public class BaseGrid : MonoBehaviour, IGameControllerMember
{
    /// <summary>
    /// �������д�������Tagʱ��������ſ�
    /// </summary>
    public static List<ItemNameTypeMap> NoAllowBuildTagList = new List<ItemNameTypeMap>()
    {
        ItemNameTypeMap.Barrier,
        ItemNameTypeMap.WindCave, 
    };

    /// <summary>
    /// �����Ӵ������µ���ʱ��������ſ�
    /// </summary>
    public static List<GridType> NoAllowBuildGridType = new List<GridType>() 
    { 
        GridType.NotBuilt, 
        GridType.Teleport
    };

    protected Dictionary<GridType, BaseGridType> mGridTypeDict = new Dictionary<GridType, BaseGridType>(); // �����ڸ��ӵĵ���״̬
    protected List<MouseUnit> mMouseUnitList = new List<MouseUnit>(); //��λ�ڸ����ϵ�����λ��
    protected Dictionary<FoodInGridType, FoodUnit> mFoodUnitDict = new Dictionary<FoodInGridType, FoodUnit>(); // �㶨�ڴ˸����ϵ���ʳ����ȷʵ������ȥ�Ķ�����ʱ�Եģ�
    protected Dictionary<ItemNameTypeMap, BaseUnit> mItemUnitDict = new Dictionary<ItemNameTypeMap, BaseUnit>(); // �ڴ˸����ϵĵ���
    protected CharacterUnit characterUnit; // �����е����ﵥλ������У�
    protected Dictionary<string, BaseUnit> mAttachedUnitDict = new Dictionary<string, BaseUnit>(); // ���������ڸ����ϵĵ�λ���ã��������ã����ӱ�����õ�λ��������ϵ��

    public bool canBuild;
    public List<Func<BaseGrid, FoodInGridType, bool>> canBuildFuncListenerList = new List<Func<BaseGrid, FoodInGridType, bool>>();
    public int currentXIndex { get; private set; }
    public int currentYIndex { get; private set; }
    public bool isHeightLimit = true; // ����Ч���Ƿ�ֻ�������ض��߶�
    public float mHeight = 0;
    public TaskController taskController = new TaskController();
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
        mAttachedUnitDict.Clear();
        canBuildFuncListenerList.Clear();
        characterUnit = null;
        canBuild = true;
        currentXIndex = 0;
        currentYIndex = 0;
        isHeightLimit = true;
        mHeight = 0;
        gridActionPointManager.Initialize();
        taskController.Initial();
        SetBoxCollider2D(Vector2.zero, new Vector2(MapManager.gridWidth, MapManager.gridHeight));
    }

    public void SetBoxCollider2D(Vector2 offset, Vector2 size)
    {
        mBoxCollider2D.offset = offset;
        mBoxCollider2D.size = size;
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
    public bool CanBuildCard(FoodInGridType foodInGridType)
    {
        if (!canBuild)
            return false;
        foreach (var func in canBuildFuncListenerList)
        {
            if (!func(this, foodInGridType))
                return false;
        }
        // ���ﱻ��ΪĬ�����͵���ʳ����������Ĭ�����͵Ĺ���
        if (characterUnit != null && foodInGridType.Equals(FoodInGridType.Default))
            return false;
        foreach (var tag in NoAllowBuildTagList)
        {
            if (IsContainTag(tag))
                return false;
        }
        foreach (var gridType in NoAllowBuildGridType)
        {
            if (IsContainGridType(gridType))
                return false;
        }
        return true;
    }

    /// <summary>
    /// ���һ�����������ڸø�����
    /// </summary>
    public void AddGridType(GridType type, BaseGridType t)
    {
        if (mGridTypeDict.ContainsKey(type))
            RemoveGridType(type);
        mGridTypeDict.Add(type, t);
        t.transform.SetParent(transform);
        t.transform.localPosition = Vector3.zero;
        t.masterGird = this;
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
    /// <returns>����ͬ��tag�ľ�ItemUnit</returns>
    public BaseUnit SetItemUnitInGrid(BaseUnit unit)
    {
        BaseUnit old = null; // ԭunit
        // Tag��� ���ظ�Tag�Ļ���ȡ����һ��
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
        FoodInGridType t = BaseCardBuilder.GetFoodInGridType(food.mType);
        // ��Ҫ�޳���������һ�����Լ���ͬ��ǩ�Ŀ�������������ڿ�����ݷſ����ú����
        KillFoodUnit(t);
        // ֮���ٰ��������
        gridActionPointManager.TriggerActionPoint(GridActionPointType.BeforeSetFoodUnit, new GridAction(food, this));
        mFoodUnitDict.Add(t, food);
        gridActionPointManager.TriggerActionPoint(GridActionPointType.AfterSetFoodUnit, new GridAction(food, this));
    }

    /// <summary>
    /// ǿ�ƻ�ɱĳ�����͵Ŀ�
    /// </summary>
    /// <param name="t"></param>
    public void KillFoodUnit(FoodInGridType t)
    {
        if (IsContainTag(t))
        {
            mFoodUnitDict[t].ExecuteDeath();
            // ExecuteDeath���Ѿ�����RemoveFoodUnit�ˣ����ע�����ɾ����
            // �����ٱ��մ���һ��
            if (IsContainTag(t))
                RemoveFoodUnit(mFoodUnitDict[t]);
        }
    }

    /// <summary>
    /// �����Ƴ���ʳ����
    /// </summary>
    public void RemoveFoodUnit(FoodUnit food)
    {
        gridActionPointManager.TriggerActionPoint(GridActionPointType.BeforeRemoveFoodUnit, new GridAction(food, this));
        mFoodUnitDict.Remove(BaseCardBuilder.GetFoodInGridType(food.mType));
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
    /// �����Ƴ���������
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
        mItemUnitDict.Add((ItemNameTypeMap)unit.mType, unit);
        gridActionPointManager.TriggerActionPoint(GridActionPointType.AfterRemoveItemUnit, new GridAction(unit, this));
    }

    /// <summary>
    /// �����Ƴ���������
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
        foreach (var item in mFoodUnitDict)
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

    /// <summary>
    /// ��ײ�¼�
    /// </summary>
    /// <param name="collision"></param>
    public void OnCollision(Collider2D collision)
    {
        if (collision.tag.Equals("Mouse"))
        {
            MouseUnit unit = collision.GetComponent<MouseUnit>();
            if (!unit.isDeathState && unit.GetRowIndex() == currentYIndex && UnitManager.CanBeSelectedAsTarget(null, unit) && (!isHeightLimit || unit.GetHeight()==mHeight) && !mMouseUnitList.Contains(unit))
            { 
                AddMouseUnit(unit);
            }
        }
    }

    /// <summary>
    /// ���ø����Ƿ�����ĳ�ֱ�ǩ����ʳ
    /// </summary>
    /// <returns></returns>
    public bool IsContainTag(FoodInGridType foodInGridType)
    {
        return mFoodUnitDict.ContainsKey(foodInGridType);
    }

    /// <summary>
    /// �ø����Ƿ�������
    /// </summary>
    /// <returns></returns>
    public bool IsContainCharacter()
    {
        return characterUnit != null;
    }

    public bool IsContainTag(ItemNameTypeMap itemInGridType)
    {
        return mItemUnitDict.ContainsKey(itemInGridType);
    }

    /// <summary>
    /// ��ȡ������߹������ȼ��Ŀɹ�����λ
    /// </summary>
    /// <param name="typeQueue">���ȼ�����</param>
    /// <param name="attacker">������</param>
    /// <returns></returns>
    public BaseUnit GetHighestAttackPriorityFoodUnit(Queue<FoodInGridType> typeQueue, BaseUnit attacker)
    {
        while(typeQueue.Count > 0)
        {
            FoodInGridType t = typeQueue.Dequeue();
            BaseUnit unit = null;
            if (IsContainTag(t))
            {
                unit = mFoodUnitDict[t];
            }
            else if (t.Equals(FoodInGridType.Default) && characterUnit != null)
            {
                unit = characterUnit;
            }
            if (unit != null)
                return unit;
        }
        return null;
    }

    /// <summary>
    /// Ĭ������£���ȡ��������߹������ȼ��ĵ�λ
    /// </summary>
    public BaseUnit GetHighestAttackPriorityUnit(BaseUnit attacker)
    {
        Queue<FoodInGridType> typeQueue = new Queue<FoodInGridType>();
        typeQueue.Enqueue(FoodInGridType.Shield);
        typeQueue.Enqueue(FoodInGridType.Bomb);
        typeQueue.Enqueue(FoodInGridType.Default);

        return GetHighestAttackPriorityFoodUnit(typeQueue, attacker);
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
    /// ��ȡ��������߹������ȼ��ĵ�λ�����а���ˮ���ؾ�
    /// </summary>
    /// <returns></returns>
    public BaseUnit GetHighestAttackPriorityUnitIncludeWaterVehicle(BaseUnit attacker)
    {
        if (IsContainTag(FoodInGridType.Shield))
        {
            return mFoodUnitDict[FoodInGridType.Shield];
        }
        else if (IsContainTag(FoodInGridType.Default))
        {
            return mFoodUnitDict[FoodInGridType.Default];
        }
        else if (IsContainTag(FoodInGridType.Bomb))
        {
            return mFoodUnitDict[FoodInGridType.Bomb];
        }
        else if (characterUnit != null)
        {
            return characterUnit;
        }
        else if (IsContainTag(FoodInGridType.WaterVehicle))
        {
            return mFoodUnitDict[FoodInGridType.WaterVehicle];
        }

        Queue<FoodInGridType> typeQueue = new Queue<FoodInGridType>();
        typeQueue.Enqueue(FoodInGridType.Shield);
        typeQueue.Enqueue(FoodInGridType.Default);
        typeQueue.Enqueue(FoodInGridType.Bomb);
        typeQueue.Enqueue(FoodInGridType.WaterVehicle);

        return GetHighestAttackPriorityFoodUnit(typeQueue, attacker);
    }

    /// <summary>
    /// ��ȡ��������߲������ȼ��ĵ�λ
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
    /// Ĭ������£���ȡ���������пɱ���������ʳ�б���ǰ���󹥻����ȼ��𼶵ݼ�
    /// </summary>
    /// <returns></returns>
    public List<FoodUnit> GetAttackableFoodUnitList()
    {
        List<FoodUnit> list = new List<FoodUnit>();
        if (IsContainTag(FoodInGridType.Shield))
        {
            list.Add(mFoodUnitDict[FoodInGridType.Shield]);
        }
        if (IsContainTag(FoodInGridType.Bomb))
        {
            list.Add(mFoodUnitDict[FoodInGridType.Bomb]);
        }
        if (IsContainTag(FoodInGridType.Default))
        {
            list.Add(mFoodUnitDict[FoodInGridType.Default]);
        }

        return list;
    }

    /// <summary>
    /// ��ȡ���������пɱ���������ʳ�б���ǰ���󹥻����ȼ��𼶵ݼ�������ˮ�ؾ�
    /// </summary>
    /// <returns></returns>
    public List<FoodUnit> GetAttackableFoodUnitListIncludeWaterVehicle()
    {
        List<FoodUnit> list = new List<FoodUnit>();
        if (IsContainTag(FoodInGridType.Shield))
        {
            list.Add(mFoodUnitDict[FoodInGridType.Shield]);
        }
        if (IsContainTag(FoodInGridType.Bomb))
        {
            list.Add(mFoodUnitDict[FoodInGridType.Bomb]);
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
    /// ����Ͷ����˵��ȡһ���ڿ�Ƭ�Ĺ������ȼ�
    /// </summary>
    /// <returns></returns>
    public BaseUnit GetThrowHighestAttackPriorityUnitInclude(BaseUnit attacker)
    {
        if (IsContainTag(FoodInGridType.Default))
        {
            return mFoodUnitDict[FoodInGridType.Default];
        }
        if (IsContainTag(FoodInGridType.Bomb))
        {
            return mFoodUnitDict[FoodInGridType.Bomb];
        }
        else if (characterUnit != null)
        {
            return characterUnit;
        }
        else if (IsContainTag(FoodInGridType.Shield))
        {
            return mFoodUnitDict[FoodInGridType.Shield];
        }

        Queue<FoodInGridType> typeQueue = new Queue<FoodInGridType>();
        typeQueue.Enqueue(FoodInGridType.Default);
        typeQueue.Enqueue(FoodInGridType.Shield);

        return GetHighestAttackPriorityFoodUnit(typeQueue, attacker);
    }

    /// <summary>
    /// ��ȡ�����������û���򷵻ؿ�ֵ
    /// </summary>
    /// <returns></returns>
    public CharacterUnit GetCharacterUnit()
    {
        return characterUnit;
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

    public void MUpdate()
    {
        // ���������λ�ֵ���Ч�ĵ�λ
        List<string> delList = new List<string>();
        foreach (var keyValuePair in mAttachedUnitDict)
        {
            if (keyValuePair.Value == null || !keyValuePair.Value.IsAlive())
                delList.Add(keyValuePair.Key);
        }
        foreach (var key in delList)
        {
            RemoveUnitFromDict(key);
        }
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
        foreach (var item in mFoodUnitDict)
        {
            FoodUnit u = item.Value;
            if (!u.IsAlive())
                f_List.Add(item.Key);
            else // ͬ��λ��
                SetUnitPosition(u, Vector2.zero);
        }
        foreach (var item in f_List)
        {
            mFoodUnitDict.Remove(item);
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
        List<ItemNameTypeMap> i_List = new List<ItemNameTypeMap>();
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

        // �����ڸ��ӵĵ��θ���
        foreach (var keyValuePair in mGridTypeDict)
        {
            keyValuePair.Value.MUpdate();
        }

        // �����������
        taskController.Update();
    }

    public void MPause()
    {
        foreach (var keyValuePair in mGridTypeDict)
        {
            keyValuePair.Value.MPause();
        }
    }

    public void MResume()
    {
        foreach (var keyValuePair in mGridTypeDict)
        {
            keyValuePair.Value.MResume();
        }
    }

    public void MDestory()
    {
        
    }

    public virtual void MPauseUpdate()
    {
        
    }

    /// <summary>
    /// ��ȡһ��ʵ��
    /// </summary>
    public static BaseGrid GetInstance()
    {
        return GameManager.Instance.GetGameObjectResource(FactoryType.GameFactory, "Grid/Grid").GetComponent<BaseGrid>();
    }

    /// <summary>
    /// ִ�л���
    /// </summary>
    public virtual void ExecuteRecycle()
    {
        GameManager.Instance.PushGameObjectToFactory(FactoryType.GameFactory, "Grid/Grid", this.gameObject);
    }

    /// <summary>
    /// ��ȡ�ø��ӵ���Ҫ��������
    /// </summary>
    public GridType GetGridType()
    {
        return GridType.NotBuilt;
    }

    /// <summary>
    /// �Ƿ����ĳ�ֵ���
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
    /// ��ȡ�ø��Ӹ��������е���
    /// </summary>
    /// <returns></returns>
    public List<GridType> GetAllGridType()
    {
        List<GridType> list = new List<GridType>();
        foreach (var keyValuePair in mGridTypeDict)
        {
            list.Add(keyValuePair.Key);
        }
        // ���ʲô����Ҳû�����Ǿ���Ĭ�ϵ�
        if (list.Count <= 0)
            list.Add(GridType.Default);
        return list;
    }


    /// <summary>
    /// ���unit����
    /// </summary>
    /// <param name="unit"></param>
    public void AddUnitToDict(string key, BaseUnit unit)
    {
        mAttachedUnitDict.Add(key, unit);
    }

    /// <summary>
    /// �Ƴ�unit����
    /// </summary>
    /// <param name="key"></param>
    public void RemoveUnitFromDict(string key)
    {
        mAttachedUnitDict.Remove(key);
    }

    /// <summary>
    /// ��ȡ���õĵ�λ�������
    /// </summary>
    /// <param name="key"></param>
    public BaseUnit GetUnitFromDict(string key)
    {
        if (IsContainUnit(key))
            return mAttachedUnitDict[key];
        else
            return null;
    }

    /// <summary>
    /// �Ƿ���ĳ��Ϊkey�ĵ�λ����
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public bool IsContainUnit(string key)
    {
        if (mAttachedUnitDict.ContainsKey(key))
        {
            BaseUnit unit = mAttachedUnitDict[key];
            if (unit.IsAlive())
                return true;
            else
            {
                mAttachedUnitDict.Remove(key);
                return false;
            }
        }
        return false;
    }

    /// <summary>
    /// ���Ψһ������
    /// </summary>
    public void AddUniqueTask(string key, ITask t)
    {
        taskController.AddUniqueTask(key, t);
    }

    /// <summary>
    /// ���һ������
    /// </summary>
    /// <param name="t"></param>
    public void AddTask(ITask t)
    {
        taskController.AddTask(t);
    }

    /// <summary>
    /// �Ƴ�Ψһ������
    /// </summary>
    public void RemoveUniqueTask(string key)
    {
        taskController.RemoveUniqueTask(key);
    }

    /// <summary>
    /// �Ƴ�һ������
    /// </summary>
    /// <param name="t"></param>
    public void RemoveTask(ITask t)
    {
        taskController.RemoveTask(t);
    }

    /// <summary>
    /// ��ȡĳ�����Ϊkey������
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public ITask GetTask(string key)
    {
        return taskController.GetTask(key);
    }

    public void AddCanBuildFuncListener(Func<BaseGrid, FoodInGridType, bool> func)
    {
        canBuildFuncListenerList.Add(func);
    }

    public void RemoveCanBuildFuncListener(Func<BaseGrid, FoodInGridType, bool> func)
    {
        canBuildFuncListenerList.Remove(func);
    }

    /// <summary>
    /// Ĭ�ϵĶԸ����ϵ��ѷ���λ����˺��ķ���
    /// </summary>
    public void TakeDamage(BaseUnit master, float dmg, bool isDamageCharacter)
    {
        BaseUnit unit = GetHighestAttackPriorityUnit(master);
        if (unit != null && (isDamageCharacter || !(unit is CharacterUnit)))
        {
            new DamageAction(CombatAction.ActionType.CauseDamage, master, unit, dmg).ApplyAction();
        }
    }
}
