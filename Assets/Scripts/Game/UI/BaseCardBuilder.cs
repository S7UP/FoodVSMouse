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
    /// <summary>
    /// �洢���浵����Ϣ
    /// </summary>
    [System.Serializable]
    public struct CardBuilderInfo
    {
        public int index; // ��ǰ�����±�
        public int cardIndex; // ��ǰ���������п��ı��
    }

    public CardBuilderInfo cardBuilderInfo;

    private GameController mGameController;

    // ��ӦUI��GameObject
    public GameObject mTex_FireCost;
    public GameObject mImg_Card;
    public GameObject mImg_CostMask;
    public GameObject mImg_CDMask;
    public GameObject mImg_CDMask2;
    public GameObject mTex_CDLeft;

    // ��Ƭʵ���Ʒ
    public FoodUnit mProduct;
    
    // ��������
    public Dictionary<string, float> mBaseCostDict; // ��������
    public Dictionary<string, float> mCostDict; // ��ǰ����

    public int mType; // ��ǰ��Ƭ�ı�ţ��粻ͬ�Ŀ���
    public int mShape; // ��ǰ��Ƭ�����ģʽ����0��1��2ת��


    protected int mBaseCD; // ����CD
    public int mCD; // ��ǰ���CD
    public int mCDLeft; // ��ǰʣ��CD
    public bool isDisable; // �Ƿ񱻽�����

    public void MInit()
    {
        // ����ֻ���⸳ֵ��ʵ��ʵ�����ȡ����Json�ļ�����

        // ��Ƭ������תְ���
        mType = 7;
        mShape = 2;

        // ����
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
        isDisable = false; // ���ñ�־

        // ��ƬͼƬ��ʾ
        mImg_Card.GetComponent<Image>().sprite = GameManager.Instance.GetSprite("Food/"+mType+"/"+mShape+"/0");
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
        // TODO ��ȡ��Ӧ���ӣ��Ƿ���������Ƭ���ܷ�Ƕ����ֲ���Լ���Ӧ�����ܷ���ֲ��
        return (GameController.Instance.GetOverGrid() != null); // ��ǰֻ�У���û��ѡ�и���
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
}
