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
    /// <summary>
    /// 存储至存档的信息
    /// </summary>
    [System.Serializable]
    public struct CardBuilderInfo
    {
        public int index; // 当前卡槽下标
        public int cardIndex; // 当前卡槽所持有卡的编号
    }

    public CardBuilderInfo cardBuilderInfo;

    private GameController mGameController;

    // 对应UI的GameObject
    public GameObject mTex_FireCost;
    public GameObject mImg_Card;
    public GameObject mImg_CostMask;
    public GameObject mImg_CDMask;
    public GameObject mImg_CDMask2;
    public GameObject mTex_CDLeft;

    // 卡片实体产品
    public FoodUnit mProduct;
    
    // 基础属性
    public Dictionary<string, float> mBaseCostDict; // 基础费用
    public Dictionary<string, float> mCostDict; // 当前费用

    public int mType; // 当前卡片的编号（如不同的卡）
    public int mShape; // 当前卡片的外观模式（如0、1、2转）


    protected int mBaseCD; // 基础CD
    public int mCD; // 当前最大CD
    public int mCDLeft; // 当前剩余CD
    public bool isDisable; // 是否被禁用了

    public void MInit()
    {
        // 以下只是拟赋值，实际实现请读取本地Json文件数据

        // 卡片种类与转职情况
        mType = 7;
        mShape = 2;

        // 费用
        if (!mBaseCostDict.ContainsKey("Fire"))
        {
            mBaseCostDict.Add("Fire", 50);
        }
        if (!mCostDict.ContainsKey("Fire"))
        {
            mCostDict.Add("Fire", mBaseCostDict["Fire"]);
        }

        // CD
        mBaseCD = 7 * ConfigManager.fps;
        mCD = mBaseCD;
        mCDLeft = 0;
        isDisable = false; // 禁用标志

        // 卡片图片显示
        mImg_Card.GetComponent<Image>().sprite = GameManager.Instance.GetSprite("Food/"+mType+"/"+mShape+"/0");
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
        // TODO 读取对应格子，是否有其他卡片，能否嵌套种植，以及对应地形能否种植等
        return (GameController.Instance.GetOverGrid() != null); // 当前只有，有没有选中格子
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
}
