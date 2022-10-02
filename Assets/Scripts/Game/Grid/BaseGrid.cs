using System.Collections.Generic;

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

    protected Dictionary<GridType, BaseGridType> mGridTypeDict = new Dictionary<GridType, BaseGridType>(); // �����ڸ��ӵĵ���״̬
    protected List<MouseUnit> mMouseUnitList = new List<MouseUnit>(); //��λ�ڸ����ϵ�����λ��
    protected Dictionary<FoodInGridType, FoodUnit> mFoodUnitDict = new Dictionary<FoodInGridType, FoodUnit>(); // �㶨�ڴ˸����ϵ���ʳ����ȷʵ������ȥ�Ķ�����ʱ�Եģ�
    protected Dictionary<ItemNameTypeMap, BaseUnit> mItemUnitDict = new Dictionary<ItemNameTypeMap, BaseUnit>(); // �ڴ˸����ϵĵ���
    protected CharacterUnit characterUnit; // �����е����ﵥλ������У�

    public bool canBuild;
    public int currentXIndex { get; private set; }
    public int currentYIndex { get; private set; }
    public bool isHeightLimit = true; // ����Ч���Ƿ�ֻ�������ض��߶�
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
        // ���ﱻ��ΪĬ�����͵���ʳ����������Ĭ�����͵Ĺ���
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
    /// ���һ�����������ڸø�����
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
        if (IsContainTag(t))
        {
            mFoodUnitDict[t].ExecuteDeath();
            // ExecuteDeath���Ѿ�����RemoveFoodUnit�ˣ����ע�����ɾ����
            // �����ٱ��մ���һ��
            if(IsContainTag(t))
                RemoveFoodUnit(mFoodUnitDict[t]);
        }
        // ֮���ٰ��������
        gridActionPointManager.TriggerActionPoint(GridActionPointType.BeforeSetFoodUnit, new GridAction(food, this));
        mFoodUnitDict.Add(t, food);
        gridActionPointManager.TriggerActionPoint(GridActionPointType.AfterSetFoodUnit, new GridAction(food, this));
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
            if (!unit.isDeathState && unit.GetRowIndex() == currentYIndex && unit.CanBeSelectedAsTarget() && (!isHeightLimit || unit.GetHeight()==mHeight) && !mMouseUnitList.Contains(unit))
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

    public bool IsContainTag(ItemNameTypeMap itemInGridType)
    {
        return mItemUnitDict.ContainsKey(itemInGridType);
    }

    /// <summary>
    /// Ĭ������£���ȡ��������߹������ȼ��ĵ�λ
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
    /// ��ȡ��������߹������ȼ��ĵ�λ�����а���ˮ���ؾ�
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
}
