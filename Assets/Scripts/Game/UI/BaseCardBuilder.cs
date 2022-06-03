using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using static BaseCardBuilder;

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
        {FoodNameTypeMap.CottonCandy, FoodInGridType.FloatVehicel }, // 棉花糖
        {FoodNameTypeMap.MelonShield, FoodInGridType.Shield }, // 瓜皮
    };

    /// <summary>
    /// 存储至存档的信息
    /// </summary>
    [System.Serializable]
    public struct CardBuilderInfo
    {
        public int index; // 当前卡槽下标
        public int cardIndex; // 当前卡槽所持有卡的编号
    }

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
    }

    public CardBuilderInfo cardBuilderInfo;

    public Attribute attr;

    private GameController mGameController;

    // 对应UI的GameObject
    public GameObject mTex_FireCost;
    public GameObject mImg_Card;
    public GameObject mImg_CostMask;
    public GameObject mImg_CDMask;
    public GameObject mImg_CDMask2;
    public GameObject mTex_CDLeft;
    public GameObject mImg_Rank;

    // 卡片实体产品
    public FoodUnit mProduct;
    
    // 基础属性
    public Dictionary<string, float> mBaseCostDict; // 基础费用
    public Dictionary<string, float> mCostDict; // 当前费用

    public int mType; // 当前卡片的编号（如不同的卡）
    public int mShape; // 当前卡片的外观模式（如0、1、2转）
    public int mLevel; // 当前卡片的星级


    protected int mBaseCD; // 基础CD
    public int mCD; // 当前最大CD
    public int mCDLeft; // 当前剩余CD
    public bool isDisable; // 是否被禁用了

    public void MInit()
    {

    }

    public void MInit(int type, int shape, int level)
    {
        // 从Json读取相关数据
        attr = JsonManager.Load<Attribute>("CardBuilder/" + type + "/" + shape + "");
        // 卡片种类与转职情况
        mType = type;
        mShape = shape;
        mLevel = level;
        // 星级显示
        mImg_Rank.GetComponent<Image>().sprite = GameManager.Instance.GetSprite("UI/Rank2/"+level);
        // 费用
        double cost;
        if(attr.costList!=null && attr.costList.Count > 0)
        {
            if (level < 0)
            {
                cost = attr.costList[0];
            }else if(level >= attr.costList.Count)
            {
                cost = attr.costList[attr.costList.Count-1];
            }
            else
            {
                cost = attr.costList[level];
            }
        }
        else
        {
            cost = attr.cost;
        }
        if (!mBaseCostDict.ContainsKey("Fire"))
        {
            mBaseCostDict.Add("Fire", (float)cost);
        }
        if (!mCostDict.ContainsKey("Fire"))
        {
            mCostDict.Add("Fire", mBaseCostDict["Fire"]);
        }

        // CD
        double cd;
        if (attr.CDList != null && attr.CDList.Count > 0)
        {
            if (level < 0)
            {
                cd = attr.CDList[0];
            }
            else if (level >= attr.CDList.Count)
            {
                cd = attr.CDList[attr.CDList.Count - 1];
            }
            else
            {
                cd = attr.CDList[level];
            }
        }
        else
        {
            cd = attr.baseCD;
        }
        mBaseCD = Mathf.FloorToInt((float)cd * ConfigManager.fps);
        mCD = mBaseCD;
        mCDLeft = 0;
        isDisable = false; // 禁用标志

        // 卡片图片显示
        mImg_Card.GetComponent<Image>().sprite = GameManager.Instance.GetSprite("Food/"+mType+"/"+mShape+"/display");
        // 费用
        mTex_FireCost.GetComponent<Text>().text = mCostDict["Fire"].ToString();
    }

    public void Awake()
    {
        mBaseCostDict = new Dictionary<string, float>();
        mCostDict = new Dictionary<string, float>();
        mGameController = GameController.Instance;
        mTex_FireCost = transform.Find("Tex_FireCost").gameObject;
        mImg_Card = transform.Find("Img_Card").gameObject;
        mImg_CostMask = transform.Find("Img_CostMask").gameObject;
        mImg_CDMask = transform.Find("Img_CDMask").gameObject;
        mImg_CDMask2 = transform.Find("Img_CDMask2").gameObject;
        mTex_CDLeft = transform.Find("Tex_CDLeft").gameObject;
        mImg_Rank = transform.Find("Img_Rank").gameObject;
        cardBuilderInfo = new CardBuilderInfo();
        


        // 获取游戏场景面板
        GameNormalPanel panel = (GameNormalPanel)GameManager.Instance.uiManager.mUIFacade.currentScenePanelDict[StringManager.GameNormalPanel];
        panel.AddCardSlot(this); // 将自身信息添加到卡槽UI里！
    }

    public void OnEnable()
    {
        MInit();
    }

    /// <summary>
    /// 当前按钮UI被点击
    /// </summary>
    public void OnClick()
    {
        if (CanSelect())
        {
            mGameController.mCardController.SelectCard(cardBuilderInfo.index); // 通知建造控制器选取自身
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
        cardBuilderInfo.index = index;
    }

    public void SetCardIndex(int index)
    {
        cardBuilderInfo.cardIndex = index;
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
        return (!isDisable && mCDLeft <= 0 && IsCostEnough());
    }

    /// <summary>
    /// 可否被建造
    /// </summary>
    /// <returns></returns>
    public virtual bool CanConstructe()
    {
        BaseGrid baseGrid = GameController.Instance.GetOverGrid();
        // 当前只有，有没有选中格子
        if (baseGrid != null)
        {
            return !baseGrid.IsContainTag(GetFoodInGridType()); // 查看是否含有此格子分类的卡片，若没有则允许建造，否则不行
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
            mGameController.SetFoodAttribute(JsonManager.Load<FoodUnit.Attribute>("Food/" + mType + "/" + mShape + "")); // 在自动MInit（）之前需要先获取初始化信息
            mProduct.MInit();
            overGrid.SetFoodUnitInGrid(mProduct); // 将卡片初始化并与种下的格子绑定
            overGrid.AddFoodUnit(GetFoodInGridType(), mProduct);
            GameController.Instance.AddFoodUnit(mProduct, overGrid.gridIndex.yIndex); // 将这个实体添加到战场上
        }
        else
        {
            // 按理来说不应该存在这种情况，如果存在了那报个错然后回收该对象
            GameManager.Instance.PushGameObjectToFactory(FactoryType.GameFactory, "Food/" + mType, mProduct.gameObject);
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
        // CD重置
        mCDLeft = mCD;
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
        // 控制UI层的CD字符显示
        if (mCDLeft > 0)
        {
            mImg_CDMask2.SetActive(true);
            mTex_CDLeft.SetActive(true);
            mTex_CDLeft.GetComponent<Text>().text = ((float)mCDLeft / ConfigManager.fps).ToString("f2"); // 转化为真实秒的同时保留两位小数
        }
        else
        {
            mTex_CDLeft.SetActive(false);
            mImg_CDMask2.SetActive(false);
        }

        // 控制UI层的CD遮罩（实际上就是控制y方向上的scale）
        mImg_CDMask.transform.localScale = new Vector3(mImg_CDMask.transform.localScale.x, (float)mCDLeft / mCD, mImg_CDMask.transform.localScale.y);
        // 费用是否够用的遮罩
        mImg_CostMask.SetActive(!IsCostEnough());
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
}
