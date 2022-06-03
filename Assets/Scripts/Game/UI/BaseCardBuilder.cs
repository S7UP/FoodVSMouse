using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using static BaseCardBuilder;

/// <summary>
/// ��Ƭ�������������Ϊ��Ϸ�����ϵĿ���UI
/// </summary>
public class BaseCardBuilder : MonoBehaviour, IBaseCardBuilder, IGameControllerMember
{
    // ��ʳԭ������-��ʳ�ڸ����ϵķ���ӳ���
    // ����û˵�Ķ���Default���
    public static Dictionary<FoodNameTypeMap, FoodInGridType> FoodType_FoodInGridTypeMap = new Dictionary<FoodNameTypeMap, FoodInGridType>()
    {
        {FoodNameTypeMap.CoffeePowder, FoodInGridType.NoAttach }, // ���ȷ�
        {FoodNameTypeMap.IceCream, FoodInGridType.NoAttach }, // �����
        {FoodNameTypeMap.WoodenDisk, FoodInGridType.WaterVehicle }, // ľ����
        {FoodNameTypeMap.CottonCandy, FoodInGridType.FloatVehicel }, // �޻���
        {FoodNameTypeMap.MelonShield, FoodInGridType.Shield }, // ��Ƥ
    };

    /// <summary>
    /// �洢���浵����Ϣ
    /// </summary>
    [System.Serializable]
    public struct CardBuilderInfo
    {
        public int index; // ��ǰ�����±�
        public int cardIndex; // ��ǰ���������п��ı��
    }

    // ��Ҫ���ش洢�ı���
    [System.Serializable]
    public struct Attribute
    {
        public string name; // ���۵ľ�������
        public int type; // ��λ���ڵķ���
        public int shape; // ��λ�ڵ�ǰ����ı��ֱ��

        public double cost; // ����
        public double baseCD; // ����cd
        public List<double> costList; // ���ı����Ǽ���
        public List<double> CDList; // cd�����Ǽ���
    }

    public CardBuilderInfo cardBuilderInfo;

    public Attribute attr;

    private GameController mGameController;

    // ��ӦUI��GameObject
    public GameObject mTex_FireCost;
    public GameObject mImg_Card;
    public GameObject mImg_CostMask;
    public GameObject mImg_CDMask;
    public GameObject mImg_CDMask2;
    public GameObject mTex_CDLeft;
    public GameObject mImg_Rank;

    // ��Ƭʵ���Ʒ
    public FoodUnit mProduct;
    
    // ��������
    public Dictionary<string, float> mBaseCostDict; // ��������
    public Dictionary<string, float> mCostDict; // ��ǰ����

    public int mType; // ��ǰ��Ƭ�ı�ţ��粻ͬ�Ŀ���
    public int mShape; // ��ǰ��Ƭ�����ģʽ����0��1��2ת��
    public int mLevel; // ��ǰ��Ƭ���Ǽ�


    protected int mBaseCD; // ����CD
    public int mCD; // ��ǰ���CD
    public int mCDLeft; // ��ǰʣ��CD
    public bool isDisable; // �Ƿ񱻽�����

    public void MInit()
    {

    }

    public void MInit(int type, int shape, int level)
    {
        // ��Json��ȡ�������
        attr = JsonManager.Load<Attribute>("CardBuilder/" + type + "/" + shape + "");
        // ��Ƭ������תְ���
        mType = type;
        mShape = shape;
        mLevel = level;
        // �Ǽ���ʾ
        mImg_Rank.GetComponent<Image>().sprite = GameManager.Instance.GetSprite("UI/Rank2/"+level);
        // ����
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
        isDisable = false; // ���ñ�־

        // ��ƬͼƬ��ʾ
        mImg_Card.GetComponent<Image>().sprite = GameManager.Instance.GetSprite("Food/"+mType+"/"+mShape+"/display");
        // ����
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
        


        // ��ȡ��Ϸ�������
        GameNormalPanel panel = (GameNormalPanel)GameManager.Instance.uiManager.mUIFacade.currentScenePanelDict[StringManager.GameNormalPanel];
        panel.AddCardSlot(this); // ��������Ϣ��ӵ�����UI�
    }

    public void OnEnable()
    {
        MInit();
    }

    /// <summary>
    /// ��ǰ��ťUI�����
    /// </summary>
    public void OnClick()
    {
        if (CanSelect())
        {
            mGameController.mCardController.SelectCard(cardBuilderInfo.index); // ֪ͨ���������ѡȡ����
        }
        else
        {
            Debug.Log("����ѡȡ��ԭ���ǣ�");
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
    /// �����Ƿ��㹻
    /// </summary>
    /// <returns></returns>
    public virtual bool IsCostEnough()
    {
        return (GameController.Instance.GetFire() >= mCostDict["Fire"]);
    }

    /// <summary>
    /// �ɷ�ѡ��
    /// </summary>
    /// <returns></returns>
    public virtual bool CanSelect()
    {
        return (!isDisable && mCDLeft <= 0 && IsCostEnough());
    }

    /// <summary>
    /// �ɷ񱻽���
    /// </summary>
    /// <returns></returns>
    public virtual bool CanConstructe()
    {
        BaseGrid baseGrid = GameController.Instance.GetOverGrid();
        // ��ǰֻ�У���û��ѡ�и���
        if (baseGrid != null)
        {
            return !baseGrid.IsContainTag(GetFoodInGridType()); // �鿴�Ƿ��д˸��ӷ���Ŀ�Ƭ����û���������죬������
        }
        // TODO ��ȡ��Ӧ���ӣ��Ƿ���������Ƭ���ܷ�Ƕ����ֲ���Լ���Ӧ�����ܷ���ֲ��
        return false; 
    }

    /// <summary>
    /// ����һ��ʵ�岢�ɸý�������ʱ����
    /// </summary>
    public void Constructe()
    {
        mProduct = GameManager.Instance.GetGameObjectResource(FactoryType.GameFactory, "Food/" + mType).GetComponent<FoodUnit>();
    }

    /// <summary>
    /// �Բ����Ŀ�Ƭʵ�帳ֵ�ӹ���װ
    /// </summary>
    public virtual void InitInstance()
    {
        BaseGrid overGrid = GameController.Instance.GetOverGrid();
        if (overGrid != null)
        {
            mGameController.SetFoodAttribute(JsonManager.Load<FoodUnit.Attribute>("Food/" + mType + "/" + mShape + "")); // ���Զ�MInit����֮ǰ��Ҫ�Ȼ�ȡ��ʼ����Ϣ
            mProduct.MInit();
            overGrid.SetFoodUnitInGrid(mProduct); // ����Ƭ��ʼ���������µĸ��Ӱ�
            overGrid.AddFoodUnit(GetFoodInGridType(), mProduct);
            GameController.Instance.AddFoodUnit(mProduct, overGrid.gridIndex.yIndex); // �����ʵ����ӵ�ս����
        }
        else
        {
            // ������˵��Ӧ�ô����������������������Ǳ�����Ȼ����ոö���
            GameManager.Instance.PushGameObjectToFactory(FactoryType.GameFactory, "Food/" + mType, mProduct.gameObject);
            Debug.LogError("����ֲ��Ƭʱδ�ҵ�ָ���ĸ��ӣ���");
        }
    }

    /// <summary>
    /// �۷ѡ�CD����ȴ���
    /// </summary>
    public virtual void Cost()
    {
        // �۳�����
        GameController.Instance.mCostController.AddCost("Fire", -mCostDict["Fire"]);
        // CD����
        mCDLeft = mCD;
    }

    /// <summary>
    /// �����������Ŀ�Ƭʵ��
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
    /// ÿ֡Ҫ�������ݸ���
    /// </summary>
    public void MUpdate()
    {
        // TODO �Խ��õĴ���
        if (isDisable)
        {
           
        }

        mCDLeft = Mathf.Max(mCDLeft - 1, 0);
        // ����UI���CD�ַ���ʾ
        if (mCDLeft > 0)
        {
            mImg_CDMask2.SetActive(true);
            mTex_CDLeft.SetActive(true);
            mTex_CDLeft.GetComponent<Text>().text = ((float)mCDLeft / ConfigManager.fps).ToString("f2"); // ת��Ϊ��ʵ���ͬʱ������λС��
        }
        else
        {
            mTex_CDLeft.SetActive(false);
            mImg_CDMask2.SetActive(false);
        }

        // ����UI���CD���֣�ʵ���Ͼ��ǿ���y�����ϵ�scale��
        mImg_CDMask.transform.localScale = new Vector3(mImg_CDMask.transform.localScale.x, (float)mCDLeft / mCD, mImg_CDMask.transform.localScale.y);
        // �����Ƿ��õ�����
        mImg_CostMask.SetActive(!IsCostEnough());
    }


    public static void SaveNewCardBuilderInfo()
    {
        Attribute attr = new Attribute()
        {
            name = "�ս��߾Ƽ�", // ���۵ľ�������
            type = 7, // ��λ���ڵķ���
            shape = 2, // ��λ�ڵ�ǰ����ı��ֱ��

            cost = 325, // ��������
            baseCD = 7, // ����cd
            costList = new List<double>(), // ���ı����Ǽ���
            CDList = new List<double>() // cd�����Ǽ���
        };

        Debug.Log("��ʼ�浵��ʳ������Ϣ��");
        JsonManager.Save(attr, "CardBuilder/" + attr.type + "/" + attr.shape + "");
        Debug.Log("��ʳ������Ϣ�浵��ɣ�");
    }

    /// <summary>
    /// ��ȡ��ʳ�ڸ����ϵķ���
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
    /// ��ȡ��ʳ�ڸ����ϵķ���
    /// </summary>
    /// <returns></returns>
    public FoodInGridType GetFoodInGridType()
    {
        return GetFoodInGridType(mType);
    }
}
