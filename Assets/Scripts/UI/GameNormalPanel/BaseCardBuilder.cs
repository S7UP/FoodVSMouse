using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 卡片建造器，其绑定者为游戏画面上的卡槽UI
/// </summary>
public class BaseCardBuilder : MonoBehaviour, IBaseCardBuilder, IGameControllerMember
{
    // 美食原生种类-美食在格子上的分类映射表
    // 下面没说的都是Default类的
    public static Dictionary<FoodNameTypeMap, FoodInGridType> FoodType_FoodInGridTypeMap = new Dictionary<FoodNameTypeMap, FoodInGridType>()
    {
        {FoodNameTypeMap.CoffeePowder, FoodInGridType.NoAttach }, // 咖啡粉
        {FoodNameTypeMap.IceCream, FoodInGridType.NoAttach }, // 冰淇淋
        {FoodNameTypeMap.WoodenDisk, FoodInGridType.WaterVehicle }, // 木盘子
        {FoodNameTypeMap.CottonCandy, FoodInGridType.FloatVehicle }, // 棉花糖
        {FoodNameTypeMap.MelonShield, FoodInGridType.Shield }, // 瓜皮
    };

    // 需要本地存储的变量
    [System.Serializable]
    public struct Attribute
    {
        public string name; // 卡槽的具体名称
        public int type; // 单位属于的分类
        public int shape; // 单位在当前分类的变种编号

        public double cost; // 消耗
        public double baseCD; // 基础cd
        public List<double> costList; // 消耗表（随星级）
        public List<double> CDList; // cd表（随星级）

        /// <summary>
        /// 根据星级获取费用
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        public double GetCost(int level)
        {
            double cost;
            if (costList != null && costList.Count > 0)
            {
                if (level < 0)
                {
                    cost = costList[0];
                }
                else if (level >= costList.Count)
                {
                    cost = costList[costList.Count - 1];
                }
                else
                {
                    cost = costList[level];
                }
            }
            else
            {
                cost = this.cost;
            }
            return cost;
        }

        /// <summary>
        /// 根据星级获取CD
        /// </summary>
        /// <returns></returns>
        public double GetCD(int level)
        {
            double cd;
            if (CDList != null && CDList.Count > 0)
            {
                if (level < 0)
                {
                    cd = CDList[0];
                }
                else if (level >= CDList.Count)
                {
                    cd = CDList[CDList.Count - 1];
                }
                else
                {
                    cd = CDList[level];
                }
            }
            else
            {
                cd = baseCD;
            }
            return cd;
        }
    }

    public int arrayIndex; // 在控制器数组中的下标
    public Attribute attr;

    // 对应UI的GameObject
    public GameObject mTex_FireCost;
    public GameObject mImg_Card;
    public GameObject mImg_CostMask;
    public GameObject mImg_CDMask;
    public GameObject mImg_CDMask2;
    public GameObject mTex_CDLeft;
    public GameObject mImg_Rank;
    private Text Tex_Key;

    
    public FoodUnit mProduct; // 当前卡片实体产品
    public List<FoodUnit> mProductList = new List<FoodUnit>(); // 当前存在的所有同类型卡片集合
    
    // 基础属性
    public Dictionary<string, float> mBaseCostDict = new Dictionary<string, float>(); // 基础费用
    public Dictionary<string, float> mCostDict = new Dictionary<string, float>(); // 当前费用

    public int mType; // 当前卡片的编号（如不同的卡）
    public int mShape; // 当前卡片的外观模式（如0、1、2转）
    public int mLevel; // 当前卡片的星级


    protected int mBaseCD; // 基础CD
    public int mCD; // 当前最大CD
    public int mCDLeft; // 当前剩余CD
    public bool isDisable; // 是否被禁用了

    // 卡槽被选中的监听
    private Action<BaseCardBuilder, BaseCardBuilder> OnSelectedEnter; // 在进入选择状态时
    private Action<BaseCardBuilder> OnDuringSelected; // 在选择状态中
    private Action<BaseCardBuilder, BaseCardBuilder> OnSelectedExit; // 在离开选择状态
    private Action<BaseCardBuilder> OnTrySelect; // 在尝试选择时 
    private Action<BaseCardBuilder, BaseCardBuilder> OnTrySelectOther; // 在尝试选择其他的卡槽时

    // 建造与拆除事件的监听
    private List<Action<BaseCardBuilder>> BeforeBuildCardListener = new List<Action<BaseCardBuilder>>(); // 在建造出卡片实体前的事件监听
    private List<Action<BaseCardBuilder>> AfterBuildCardListener = new List<Action<BaseCardBuilder>>(); // 在建造出卡片实体后的事件监听
    private List<Action<BaseCardBuilder>> BeforeDestructeCardListener = new List<Action<BaseCardBuilder>>(); // 在销毁卡片实体前的事件监听
    private List<Action<BaseCardBuilder>> AfterDestructeCardListener = new List<Action<BaseCardBuilder>>(); // 在销毁卡片实体后的事件监听

    /// <summary>
    /// 初始化
    /// </summary>
    public void MInit()
    {
        arrayIndex = 0;
        attr = new Attribute();

        mProduct = null;
        mProductList.Clear();
        mBaseCostDict.Clear();
        mCostDict.Clear();
        mType = 0;
        mShape = 0;
        mLevel = 0;
        mBaseCD = 0;
        mCD = 0;
        mCDLeft = 0;
        isDisable = false;

        OnSelectedEnter = null;
        OnDuringSelected = null;
        OnSelectedExit = null;
        OnTrySelect = null;
        OnTrySelectOther = null;

        BeforeBuildCardListener.Clear();
        AfterBuildCardListener.Clear();
        BeforeDestructeCardListener.Clear();
        AfterDestructeCardListener.Clear();
    }

    /// <summary>
    /// 加载对应数据产生CardBuilder
    /// </summary>
    /// <param name="type"></param>
    /// <param name="shape"></param>
    /// <param name="level"></param>
    public void Load(int type, int shape, int level)
    {
        // 读取相关数据
        attr = GameManager.Instance.attributeManager.GetCardBuilderAttribute(type, shape);
        // 卡片种类与转职情况
        mType = type;
        mShape = shape;
        mLevel = level;
        // 星级显示
        mImg_Rank.GetComponent<Image>().sprite = GameManager.Instance.GetSprite("UI/Rank2/"+level);
        // 费用
        double cost = attr.GetCost(level);
        if (!mBaseCostDict.ContainsKey("Fire"))
        {
            mBaseCostDict.Add("Fire", (float)cost);
        }
        if (!mCostDict.ContainsKey("Fire"))
        {
            mCostDict.Add("Fire", mBaseCostDict["Fire"]);
        }

        // CD
        double cd = attr.GetCD(level);
        mBaseCD = Mathf.FloorToInt((float)cd * ConfigManager.fps);
        mCD = mBaseCD;
        mCDLeft = 0;
        isDisable = false; // 禁用标志

        // 卡片图片显示
        mImg_Card.GetComponent<Image>().sprite = GameManager.Instance.GetSprite("Food/"+mType+"/"+mShape+"/display");

        UpdateDisplayer();
    }

    public void Awake()
    {
        mTex_FireCost = transform.Find("Tex_FireCost").gameObject;
        mImg_Card = transform.Find("Img_Card").gameObject;
        mImg_CostMask = transform.Find("Img_CostMask").gameObject;
        mImg_CDMask = transform.Find("Img_CDMask").gameObject;
        mImg_CDMask2 = transform.Find("Img_CDMask2").gameObject;
        mTex_CDLeft = transform.Find("Tex_CDLeft").gameObject;
        mImg_Rank = transform.Find("Img_Rank").gameObject;
        Tex_Key = transform.Find("Img_Key").Find("Text").GetComponent<Text>();
    }

    /// <summary>
    /// 当前按钮UI被点击
    /// </summary>
    public void OnClick()
    {
        // 系统当前卡槽尝试切换成其他卡槽时事件
        BaseCardBuilder lastBuilder = GameController.Instance.mCardController.GetSelectCardBuilder();
        if (lastBuilder!=null && lastBuilder.OnTrySelectOther != null)
        {
            lastBuilder.OnTrySelectOther(GameController.Instance.mCardController.GetSelectCardBuilder(), this);
        }

        // 当尝试选择本卡槽时的事件（无论最终卡槽能不能被选取都会经过这个）
        if (OnTrySelect != null)
        {
            OnTrySelect(this);
        }

        if (CanSelect())
        {
            GameController.Instance.mCardController.SelectCard(arrayIndex); // 通知建造控制器选取自身
        }
        else
        {
            Debug.Log("不可选取，原因是：");
        }
    }

    public void SetPosition(Vector3 v3)
    {
        transform.position = v3;
    }

    public void SetIndex(int index)
    {
        arrayIndex = index;
        Tex_Key.text = GameManager.Instance.playerData.GetCurrentCardKeyList()[index].ToString();
    }

    /// <summary>
    /// 费用是否足够
    /// </summary>
    /// <returns></returns>
    public virtual bool IsCostEnough()
    {
        return (GameController.Instance.GetFire() >= mCostDict["Fire"]);
    }

    /// <summary>
    /// 可否被选择
    /// </summary>
    /// <returns></returns>
    public virtual bool CanSelect()
    {
        return (!isDisable && IsColdDown() && IsCostEnough());
    }

    /// <summary>
    /// 可否被建造
    /// </summary>
    /// <returns></returns>
    public virtual bool CanConstructe()
    {
        BaseGrid baseGrid = GameController.Instance.GetOverGrid();
        if (baseGrid != null)
        {
            // 先检查格子状态能否允许造卡
            
            // 之后检查是否开启快捷放卡设置，如果是则允许放卡， 否则需要再查看是否含有此格子分类的卡片，若没有则允许建造，否则不行
            return (baseGrid.CanBuildCard(GetFoodInGridType()) && (ConfigManager.isEnableQuickReleaseCard || !baseGrid.IsContainTag(GetFoodInGridType()))); 
        }
        // TODO 读取对应格子，是否有其他卡片，能否嵌套种植，以及对应地形能否种植等
        return false; 
    }

    /// <summary>
    /// 生成一个实体并由该建造类暂时保管
    /// </summary>
    public void Constructe()
    {
        mProduct = GameManager.Instance.GetGameObjectResource(FactoryType.GameFactory, "Food/" + mType).GetComponent<FoodUnit>();
    }

    /// <summary>
    /// 对产生的卡片实体赋值加工包装
    /// </summary>
    public virtual void InitInstance()
    {
        BaseGrid overGrid = GameController.Instance.GetOverGrid();
        if (overGrid != null)
        {
            GameController.Instance.SetFoodAttribute(GameManager.Instance.attributeManager.GetFoodUnitAttribute(mType, mShape));
            mProduct.MInit();
            mProduct.SetLevel(mLevel);
            mProduct.mBuilder = this; // 设置卡片的建造器
            overGrid.SetFoodUnitInGrid(mProduct); // 将卡片初始化并与种下的格子绑定
            GameController.Instance.AddFoodUnit(mProduct, overGrid.currentYIndex); // 将这个实体添加到战场上
            mProductList.Add(mProduct); // 添加到表内
        }
        else
        {
            // 按理来说不应该存在这种情况，如果存在了那报个错然后回收该对象
            Destructe(mProduct);
            mProduct = null;
            Debug.LogError("在种植卡片时未找到指定的格子！！");
        }
    }

    /// <summary>
    /// 扣费、CD计算等处理
    /// </summary>
    public virtual void Cost()
    {
        // 扣除费用
        GameController.Instance.mCostController.AddCost("Fire", -mCostDict["Fire"]);
        // CD填满
        FullCD();
    }

    /// <summary>
    /// 销毁某张卡
    /// </summary>
    public void Destructe(FoodUnit foodUnit)
    {
        TriggerBeforeDestructeAction();
        GameManager.Instance.PushGameObjectToFactory(FactoryType.GameFactory, "Food/" + mType, foodUnit.gameObject);
        mProductList.Remove(foodUnit);
        TriggerAfterDestructeAction();
    }

    /// <summary>
    /// 生产出真正的卡片实体
    /// </summary>
    /// <returns></returns>
    public BaseUnit GetResult()
    {
        return mProduct;
    }

    public void MDestory()
    {
        throw new System.NotImplementedException();
    }

    public void MPause()
    {
        throw new System.NotImplementedException();
    }

    public void MResume()
    {
        throw new System.NotImplementedException();
    }

    /// <summary>
    /// 每帧要做的数据更新
    /// </summary>
    public void MUpdate()
    {
        // TODO 对禁用的处理
        if (isDisable)
        {

        }

        mCDLeft = Mathf.Max(mCDLeft - 1, 0);
        UpdateDisplayer();
    }

    private void UpdateDisplayer()
    {
        // 控制UI层的CD字符显示
        if (IsColdDown())
        {
            mTex_CDLeft.SetActive(false);
            mImg_CDMask2.SetActive(false);
        }
        else
        {
            mImg_CDMask2.SetActive(true);
            mTex_CDLeft.SetActive(true);
            mTex_CDLeft.GetComponent<Text>().text = ((float)mCDLeft / ConfigManager.fps).ToString("f2"); // 转化为真实秒的同时保留两位小数
        }

        // 控制UI层的CD遮罩（实际上就是控制y方向上的scale）
        mImg_CDMask.transform.localScale = new Vector3(mImg_CDMask.transform.localScale.x, (float)mCDLeft / mCD, mImg_CDMask.transform.localScale.y);
        // 费用是否够用的遮罩
        mImg_CostMask.SetActive(!IsCostEnough());
        // 更新费用显示
        mTex_FireCost.GetComponent<Text>().text = mCostDict["Fire"].ToString();
    }


    public static void SaveNewCardBuilderInfo()
    {
        Attribute attr = new Attribute()
        {
            name = "终结者酒架", // 卡槽的具体名称
            type = 7, // 单位属于的分类
            shape = 2, // 单位在当前分类的变种编号

            cost = 325, // 基础消耗
            baseCD = 7, // 基础cd
            costList = new List<double>(), // 消耗表（随星级）
            CDList = new List<double>() // cd表（随星级）
        };

        Debug.Log("开始存档美食卡槽信息！");
        JsonManager.Save(attr, "CardBuilder/" + attr.type + "/" + attr.shape + "");
        Debug.Log("美食卡槽信息存档完成！");
    }

    /// <summary>
    /// 获取美食在格子上的分类
    /// </summary>
    /// <returns></returns>
    public static FoodInGridType GetFoodInGridType(int type)
    {
        if (FoodType_FoodInGridTypeMap.ContainsKey((FoodNameTypeMap)type))
        {
            return FoodType_FoodInGridTypeMap[(FoodNameTypeMap)type];
        }
        return FoodInGridType.Default;
    }

    /// <summary>
    /// 获取美食在格子上的分类
    /// </summary>
    /// <returns></returns>
    public FoodInGridType GetFoodInGridType()
    {
        return GetFoodInGridType(mType);
    }

    public void MPauseUpdate()
    {
        
    }

    /// <summary>
    /// 重置CD
    /// </summary>
    public void ResetCD()
    {
        mCDLeft = 0;
    }

    /// <summary>
    /// 使CD填满
    /// </summary>
    public void FullCD()
    {
        mCDLeft = mCD;
    }

    /// <summary>
    /// CD是否转好
    /// </summary>
    /// <returns></returns>
    public bool IsColdDown()
    {
        return mCDLeft <= 0;
    }

    /// <summary>
    /// 添加建造卡片之前的事件
    /// </summary>
    public void AddBeforeBuildAction(Action<BaseCardBuilder> action)
    {
        BeforeBuildCardListener.Add(action);
}

    /// <summary>
    /// 移除建造卡片之前的事件
    /// </summary>
    /// <param name="action"></param>
    public void RemoveBeforeBuildAction(Action<BaseCardBuilder> action)
    {
        BeforeBuildCardListener.Remove(action);
    }

    /// <summary>
    /// 添加建造卡片之后的事件
    /// </summary>
    /// <param name="action"></param>
    public void AddAfterBuildAction(Action<BaseCardBuilder> action)
    {
        AfterBuildCardListener.Add(action);
    }

    /// <summary>
    /// 移除建造卡片之后的事件
    /// </summary>
    /// <param name="action"></param>
    public void RemoveAfterBuildAction(Action<BaseCardBuilder> action)
    {
        AfterBuildCardListener.Remove(action);
    }

    /// <summary>
    /// 响应一次建造前的事件
    /// </summary>
    public void TriggerBeforeBuildAction()
    {
        foreach (var item in BeforeBuildCardListener)
        {
            item(this);
        }
    }

    /// <summary>
    /// 响应一次建造后的事件
    /// </summary>
    public void TriggerAfterBuildAction()
    {
        foreach (var item in AfterBuildCardListener)
        {
            item(this);
        }
    }

    //
    /// <summary>
    /// 添加销毁卡片之前的事件
    /// </summary>
    public void AddBeforeDestructeAction(Action<BaseCardBuilder> action)
    {
        BeforeDestructeCardListener.Add(action);
    }

    /// <summary>
    /// 移除销毁卡片之前的事件
    /// </summary>
    /// <param name="action"></param>
    public void RemoveBeforeDestructeAction(Action<BaseCardBuilder> action)
    {
        BeforeDestructeCardListener.Remove(action);
    }

    /// <summary>
    /// 添加销毁卡片之后的事件
    /// </summary>
    /// <param name="action"></param>
    public void AddAfterDestructeAction(Action<BaseCardBuilder> action)
    {
        AfterDestructeCardListener.Add(action);
    }

    /// <summary>
    /// 移除销毁卡片之后的事件
    /// </summary>
    /// <param name="action"></param>
    public void RemoveAfterDestructeAction(Action<BaseCardBuilder> action)
    {
        AfterDestructeCardListener.Remove(action);
    }

    /// <summary>
    /// 响应一次销毁前的事件
    /// </summary>
    public void TriggerBeforeDestructeAction()
    {
        foreach (var item in BeforeDestructeCardListener)
        {
            item(this);
        }
    }

    /// <summary>
    /// 响应一次销毁后的事件
    /// </summary>
    public void TriggerAfterDestructeAction()
    {
        foreach (var item in AfterDestructeCardListener)
        {
            item(this);
        }
    }

    /// <summary>
    /// 获取由当前卡片建造器建造出来的所有存活实例
    /// </summary>
    public List<FoodUnit> GetAllProducts()
    {
        return mProductList;
    }

    /// <summary>
    /// 设置当前卡槽被选择时的事件
    /// </summary>
    /// <param name="action">第一个参数为当前建造器，第二个参数为上一张被选择的卡，如果是从非建造卡状态到选择当前卡则为NULL</param>
    public void SetOnSelectedEnterEvent(Action<BaseCardBuilder, BaseCardBuilder> action)
    {
        OnSelectedEnter = action;
    }

    /// <summary>
    /// 设置当前卡槽被持续选择中的事件
    /// </summary>
    /// <param name="action">参数为当前建造器</param>
    public void SetOnDuringSelectedEvent(Action<BaseCardBuilder> action)
    {
        OnDuringSelected = action;
    }

    /// <summary>
    /// 设置当前卡槽退出选择时的事件
    /// </summary>
    /// <param name="action">第一个参数为当前建造器，参数为下一张即将被选择的卡，如果只是取消选择当前卡则为NULL</param>
    public void SetOnSelectedExitEvent(Action<BaseCardBuilder, BaseCardBuilder> action)
    {
        OnSelectedExit = action;
    }

    /// <summary>
    /// 设置尝试选择该槽事件
    /// </summary>
    /// <param name="action">参数为被尝试选的建造器</param>
    public void SetOnTrySelectEvent(Action<BaseCardBuilder> action)
    {
        OnTrySelect = action;
    }

    /// <summary>
    /// 设置尝试选择其他槽事件
    /// </summary>
    /// <param name="action">第一个参数为当前建造器，第二个参数为被尝试选的建造器</param>
    public void SetOnTrySelectOtherEvent(Action<BaseCardBuilder, BaseCardBuilder> action)
    {
        OnTrySelectOther = action;
    }

    /// <summary>
    /// 触发进入卡槽被选中的事件
    /// </summary>
    /// <param name="lastCardBuilder">上一个被选中的卡槽</param>
    public void TriggerOnSelectedEnterEvent(BaseCardBuilder lastCardBuilder)
    {
        if (OnSelectedEnter != null)
            OnSelectedEnter(this, lastCardBuilder);
    }

    /// <summary>
    /// 触发卡槽被选中期间事件
    /// </summary>
    public void TriggerOnDuringSelectedEvent()
    {
        if (OnDuringSelected != null)
            OnDuringSelected(this);
    }

    /// <summary>
    /// 触发卡槽结束选中的事件
    /// </summary>
    /// <param name="nextCardBuilder">下一个被选中的卡槽</param>
    public void TriggerOnSelectedExitEvent(BaseCardBuilder nextCardBuilder)
    {
        if (OnSelectedExit != null)
            OnSelectedExit(this, nextCardBuilder);
    }

    /// <summary>
    /// 回收自己
    /// </summary>
    public void Recycle()
    {
        GameManager.Instance.PushGameObjectToFactory(FactoryType.UIFactory, "CardBuilder", this.gameObject);
    }

    /// <summary>
    /// 获取一个该对象实例
    /// </summary>
    /// <returns></returns>
    public static BaseCardBuilder GetResource()
    {
        return GameManager.Instance.GetGameObjectResource(FactoryType.UIFactory, "CardBuilder").GetComponent<BaseCardBuilder>();
    }
}
