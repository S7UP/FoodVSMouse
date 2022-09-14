using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

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
        {FoodNameTypeMap.CottonCandy, FoodInGridType.FloatVehicle }, // �޻���
        {FoodNameTypeMap.MelonShield, FoodInGridType.Shield }, // ��Ƥ
    };

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

        /// <summary>
        /// �����Ǽ���ȡ����
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
        /// �����Ǽ���ȡCD
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

    public int arrayIndex; // �ڿ����������е��±�
    public Attribute attr;

    // ��ӦUI��GameObject
    public GameObject mTex_FireCost;
    public GameObject mImg_Card;
    public GameObject mImg_CostMask;
    public GameObject mImg_CDMask;
    public GameObject mImg_CDMask2;
    public GameObject mTex_CDLeft;
    public GameObject mImg_Rank;
    private Text Tex_Key;

    
    public FoodUnit mProduct; // ��ǰ��Ƭʵ���Ʒ
    public List<FoodUnit> mProductList = new List<FoodUnit>(); // ��ǰ���ڵ�����ͬ���Ϳ�Ƭ����
    
    // ��������
    public Dictionary<string, float> mBaseCostDict = new Dictionary<string, float>(); // ��������
    public Dictionary<string, float> mCostDict = new Dictionary<string, float>(); // ��ǰ����

    public int mType; // ��ǰ��Ƭ�ı�ţ��粻ͬ�Ŀ���
    public int mShape; // ��ǰ��Ƭ�����ģʽ����0��1��2ת��
    public int mLevel; // ��ǰ��Ƭ���Ǽ�


    protected int mBaseCD; // ����CD
    public int mCD; // ��ǰ���CD
    public int mCDLeft; // ��ǰʣ��CD
    public bool isDisable; // �Ƿ񱻽�����

    // ���۱�ѡ�еļ���
    private Action<BaseCardBuilder, BaseCardBuilder> OnSelectedEnter; // �ڽ���ѡ��״̬ʱ
    private Action<BaseCardBuilder> OnDuringSelected; // ��ѡ��״̬��
    private Action<BaseCardBuilder, BaseCardBuilder> OnSelectedExit; // ���뿪ѡ��״̬
    private Action<BaseCardBuilder> OnTrySelect; // �ڳ���ѡ��ʱ 
    private Action<BaseCardBuilder, BaseCardBuilder> OnTrySelectOther; // �ڳ���ѡ�������Ŀ���ʱ

    // ���������¼��ļ���
    private List<Action<BaseCardBuilder>> BeforeBuildCardListener = new List<Action<BaseCardBuilder>>(); // �ڽ������Ƭʵ��ǰ���¼�����
    private List<Action<BaseCardBuilder>> AfterBuildCardListener = new List<Action<BaseCardBuilder>>(); // �ڽ������Ƭʵ�����¼�����
    private List<Action<BaseCardBuilder>> BeforeDestructeCardListener = new List<Action<BaseCardBuilder>>(); // �����ٿ�Ƭʵ��ǰ���¼�����
    private List<Action<BaseCardBuilder>> AfterDestructeCardListener = new List<Action<BaseCardBuilder>>(); // �����ٿ�Ƭʵ�����¼�����

    /// <summary>
    /// ��ʼ��
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
    /// ���ض�Ӧ���ݲ���CardBuilder
    /// </summary>
    /// <param name="type"></param>
    /// <param name="shape"></param>
    /// <param name="level"></param>
    public void Load(int type, int shape, int level)
    {
        // ��ȡ�������
        attr = GameManager.Instance.attributeManager.GetCardBuilderAttribute(type, shape);
        // ��Ƭ������תְ���
        mType = type;
        mShape = shape;
        mLevel = level;
        // �Ǽ���ʾ
        mImg_Rank.GetComponent<Image>().sprite = GameManager.Instance.GetSprite("UI/Rank2/"+level);
        // ����
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
        isDisable = false; // ���ñ�־

        // ��ƬͼƬ��ʾ
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
    /// ��ǰ��ťUI�����
    /// </summary>
    public void OnClick()
    {
        // ϵͳ��ǰ���۳����л�����������ʱ�¼�
        BaseCardBuilder lastBuilder = GameController.Instance.mCardController.GetSelectCardBuilder();
        if (lastBuilder!=null && lastBuilder.OnTrySelectOther != null)
        {
            lastBuilder.OnTrySelectOther(GameController.Instance.mCardController.GetSelectCardBuilder(), this);
        }

        // ������ѡ�񱾿���ʱ���¼����������տ����ܲ��ܱ�ѡȡ���ᾭ�������
        if (OnTrySelect != null)
        {
            OnTrySelect(this);
        }

        if (CanSelect())
        {
            GameController.Instance.mCardController.SelectCard(arrayIndex); // ֪ͨ���������ѡȡ����
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
        arrayIndex = index;
        Tex_Key.text = GameManager.Instance.playerData.GetCurrentCardKeyList()[index].ToString();
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
        return (!isDisable && IsColdDown() && IsCostEnough());
    }

    /// <summary>
    /// �ɷ񱻽���
    /// </summary>
    /// <returns></returns>
    public virtual bool CanConstructe()
    {
        BaseGrid baseGrid = GameController.Instance.GetOverGrid();
        if (baseGrid != null)
        {
            // �ȼ�����״̬�ܷ������쿨
            
            // ֮�����Ƿ�����ݷſ����ã������������ſ��� ������Ҫ�ٲ鿴�Ƿ��д˸��ӷ���Ŀ�Ƭ����û���������죬������
            return (baseGrid.CanBuildCard(GetFoodInGridType()) && (ConfigManager.isEnableQuickReleaseCard || !baseGrid.IsContainTag(GetFoodInGridType()))); 
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
            GameController.Instance.SetFoodAttribute(GameManager.Instance.attributeManager.GetFoodUnitAttribute(mType, mShape));
            mProduct.MInit();
            mProduct.SetLevel(mLevel);
            mProduct.mBuilder = this; // ���ÿ�Ƭ�Ľ�����
            overGrid.SetFoodUnitInGrid(mProduct); // ����Ƭ��ʼ���������µĸ��Ӱ�
            GameController.Instance.AddFoodUnit(mProduct, overGrid.currentYIndex); // �����ʵ����ӵ�ս����
            mProductList.Add(mProduct); // ��ӵ�����
        }
        else
        {
            // ������˵��Ӧ�ô����������������������Ǳ�����Ȼ����ոö���
            Destructe(mProduct);
            mProduct = null;
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
        FullCD();
    }

    /// <summary>
    /// ����ĳ�ſ�
    /// </summary>
    public void Destructe(FoodUnit foodUnit)
    {
        TriggerBeforeDestructeAction();
        GameManager.Instance.PushGameObjectToFactory(FactoryType.GameFactory, "Food/" + mType, foodUnit.gameObject);
        mProductList.Remove(foodUnit);
        TriggerAfterDestructeAction();
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
        UpdateDisplayer();
    }

    private void UpdateDisplayer()
    {
        // ����UI���CD�ַ���ʾ
        if (IsColdDown())
        {
            mTex_CDLeft.SetActive(false);
            mImg_CDMask2.SetActive(false);
        }
        else
        {
            mImg_CDMask2.SetActive(true);
            mTex_CDLeft.SetActive(true);
            mTex_CDLeft.GetComponent<Text>().text = ((float)mCDLeft / ConfigManager.fps).ToString("f2"); // ת��Ϊ��ʵ���ͬʱ������λС��
        }

        // ����UI���CD���֣�ʵ���Ͼ��ǿ���y�����ϵ�scale��
        mImg_CDMask.transform.localScale = new Vector3(mImg_CDMask.transform.localScale.x, (float)mCDLeft / mCD, mImg_CDMask.transform.localScale.y);
        // �����Ƿ��õ�����
        mImg_CostMask.SetActive(!IsCostEnough());
        // ���·�����ʾ
        mTex_FireCost.GetComponent<Text>().text = mCostDict["Fire"].ToString();
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

    public void MPauseUpdate()
    {
        
    }

    /// <summary>
    /// ����CD
    /// </summary>
    public void ResetCD()
    {
        mCDLeft = 0;
    }

    /// <summary>
    /// ʹCD����
    /// </summary>
    public void FullCD()
    {
        mCDLeft = mCD;
    }

    /// <summary>
    /// CD�Ƿ�ת��
    /// </summary>
    /// <returns></returns>
    public bool IsColdDown()
    {
        return mCDLeft <= 0;
    }

    /// <summary>
    /// ��ӽ��쿨Ƭ֮ǰ���¼�
    /// </summary>
    public void AddBeforeBuildAction(Action<BaseCardBuilder> action)
    {
        BeforeBuildCardListener.Add(action);
}

    /// <summary>
    /// �Ƴ����쿨Ƭ֮ǰ���¼�
    /// </summary>
    /// <param name="action"></param>
    public void RemoveBeforeBuildAction(Action<BaseCardBuilder> action)
    {
        BeforeBuildCardListener.Remove(action);
    }

    /// <summary>
    /// ��ӽ��쿨Ƭ֮����¼�
    /// </summary>
    /// <param name="action"></param>
    public void AddAfterBuildAction(Action<BaseCardBuilder> action)
    {
        AfterBuildCardListener.Add(action);
    }

    /// <summary>
    /// �Ƴ����쿨Ƭ֮����¼�
    /// </summary>
    /// <param name="action"></param>
    public void RemoveAfterBuildAction(Action<BaseCardBuilder> action)
    {
        AfterBuildCardListener.Remove(action);
    }

    /// <summary>
    /// ��Ӧһ�ν���ǰ���¼�
    /// </summary>
    public void TriggerBeforeBuildAction()
    {
        foreach (var item in BeforeBuildCardListener)
        {
            item(this);
        }
    }

    /// <summary>
    /// ��Ӧһ�ν������¼�
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
    /// ������ٿ�Ƭ֮ǰ���¼�
    /// </summary>
    public void AddBeforeDestructeAction(Action<BaseCardBuilder> action)
    {
        BeforeDestructeCardListener.Add(action);
    }

    /// <summary>
    /// �Ƴ����ٿ�Ƭ֮ǰ���¼�
    /// </summary>
    /// <param name="action"></param>
    public void RemoveBeforeDestructeAction(Action<BaseCardBuilder> action)
    {
        BeforeDestructeCardListener.Remove(action);
    }

    /// <summary>
    /// ������ٿ�Ƭ֮����¼�
    /// </summary>
    /// <param name="action"></param>
    public void AddAfterDestructeAction(Action<BaseCardBuilder> action)
    {
        AfterDestructeCardListener.Add(action);
    }

    /// <summary>
    /// �Ƴ����ٿ�Ƭ֮����¼�
    /// </summary>
    /// <param name="action"></param>
    public void RemoveAfterDestructeAction(Action<BaseCardBuilder> action)
    {
        AfterDestructeCardListener.Remove(action);
    }

    /// <summary>
    /// ��Ӧһ������ǰ���¼�
    /// </summary>
    public void TriggerBeforeDestructeAction()
    {
        foreach (var item in BeforeDestructeCardListener)
        {
            item(this);
        }
    }

    /// <summary>
    /// ��Ӧһ�����ٺ���¼�
    /// </summary>
    public void TriggerAfterDestructeAction()
    {
        foreach (var item in AfterDestructeCardListener)
        {
            item(this);
        }
    }

    /// <summary>
    /// ��ȡ�ɵ�ǰ��Ƭ������������������д��ʵ��
    /// </summary>
    public List<FoodUnit> GetAllProducts()
    {
        return mProductList;
    }

    /// <summary>
    /// ���õ�ǰ���۱�ѡ��ʱ���¼�
    /// </summary>
    /// <param name="action">��һ������Ϊ��ǰ���������ڶ�������Ϊ��һ�ű�ѡ��Ŀ�������Ǵӷǽ��쿨״̬��ѡ��ǰ����ΪNULL</param>
    public void SetOnSelectedEnterEvent(Action<BaseCardBuilder, BaseCardBuilder> action)
    {
        OnSelectedEnter = action;
    }

    /// <summary>
    /// ���õ�ǰ���۱�����ѡ���е��¼�
    /// </summary>
    /// <param name="action">����Ϊ��ǰ������</param>
    public void SetOnDuringSelectedEvent(Action<BaseCardBuilder> action)
    {
        OnDuringSelected = action;
    }

    /// <summary>
    /// ���õ�ǰ�����˳�ѡ��ʱ���¼�
    /// </summary>
    /// <param name="action">��һ������Ϊ��ǰ������������Ϊ��һ�ż�����ѡ��Ŀ������ֻ��ȡ��ѡ��ǰ����ΪNULL</param>
    public void SetOnSelectedExitEvent(Action<BaseCardBuilder, BaseCardBuilder> action)
    {
        OnSelectedExit = action;
    }

    /// <summary>
    /// ���ó���ѡ��ò��¼�
    /// </summary>
    /// <param name="action">����Ϊ������ѡ�Ľ�����</param>
    public void SetOnTrySelectEvent(Action<BaseCardBuilder> action)
    {
        OnTrySelect = action;
    }

    /// <summary>
    /// ���ó���ѡ���������¼�
    /// </summary>
    /// <param name="action">��һ������Ϊ��ǰ���������ڶ�������Ϊ������ѡ�Ľ�����</param>
    public void SetOnTrySelectOtherEvent(Action<BaseCardBuilder, BaseCardBuilder> action)
    {
        OnTrySelectOther = action;
    }

    /// <summary>
    /// �������뿨�۱�ѡ�е��¼�
    /// </summary>
    /// <param name="lastCardBuilder">��һ����ѡ�еĿ���</param>
    public void TriggerOnSelectedEnterEvent(BaseCardBuilder lastCardBuilder)
    {
        if (OnSelectedEnter != null)
            OnSelectedEnter(this, lastCardBuilder);
    }

    /// <summary>
    /// �������۱�ѡ���ڼ��¼�
    /// </summary>
    public void TriggerOnDuringSelectedEvent()
    {
        if (OnDuringSelected != null)
            OnDuringSelected(this);
    }

    /// <summary>
    /// �������۽���ѡ�е��¼�
    /// </summary>
    /// <param name="nextCardBuilder">��һ����ѡ�еĿ���</param>
    public void TriggerOnSelectedExitEvent(BaseCardBuilder nextCardBuilder)
    {
        if (OnSelectedExit != null)
            OnSelectedExit(this, nextCardBuilder);
    }

    /// <summary>
    /// �����Լ�
    /// </summary>
    public void Recycle()
    {
        GameManager.Instance.PushGameObjectToFactory(FactoryType.UIFactory, "CardBuilder", this.gameObject);
    }

    /// <summary>
    /// ��ȡһ���ö���ʵ��
    /// </summary>
    /// <returns></returns>
    public static BaseCardBuilder GetResource()
    {
        return GameManager.Instance.GetGameObjectResource(FactoryType.UIFactory, "CardBuilder").GetComponent<BaseCardBuilder>();
    }
}
