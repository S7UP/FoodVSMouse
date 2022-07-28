using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// ��Ƭ���������
/// </summary>
public class BaseCardController : MonoBehaviour, IBaseCardController, IGameControllerMember
{
    private GameNormalPanel mGameNormalPanel;
    public List<BaseCardBuilder> mCardBuilderList = new List<BaseCardBuilder>(); // ��Я���Ŀ�Ƭ�Ľ�������

    public BaseCardBuilder mSelectedCardBuilder;
    public BaseGrid mSelectGrid; // ��ǰ��ѡ�еĸ���

    public bool isSelectCard;
    public bool isSelectShovel; // �Ƿ�ѡ�����
    public bool isSelectGrid;

    /// <summary>
    /// ��ʼ������
    /// </summary>
    public void MInit()
    {
        // ������ʼ��
        mGameNormalPanel = (GameNormalPanel)GameManager.Instance.uiManager.mUIFacade.currentScenePanelDict[StringManager.GameNormalPanel];
        mGameNormalPanel.ClearAllSlot(); // ��տ���
        mSelectedCardBuilder = null;
        isSelectCard = false;
        isSelectGrid = false;
        mCardBuilderList.Clear();
        


        CancelSelectCard();
        CancelSelectShovel();

        // ��ʼ��������Ϣ����Ҫ�ⲿ��ȡ��ֵ�����⸳ֵ
        //List<int[]> inList = new List<int[]>();
        //inList.Add(new int[] { ((int)FoodNameTypeMap.CoffeePowder), 0, 13});
        //inList.Add(new int[] { 0, 2, 13 });
        //inList.Add(new int[] { 1, 2, 13 });
        //inList.Add(new int[] { 2, 2, 13 });
        //inList.Add(new int[] { 4, 1, 13 });
        //inList.Add(new int[] { ((int)FoodNameTypeMap.IceCream), 1, 13 });
        //inList.Add(new int[] { 6, 2, 13 });
        //inList.Add(new int[]{ 7, 2, 13 });
        //inList.Add(new int[]{ 8, 2, 13 });
        //inList.Add(new int[] { ((int)FoodNameTypeMap.SugarGourd), 2, 13});
        ////inList.Add(new int[] { (int)FoodNameTypeMap.MouseCatcher, 0, 13 });
        ////inList.Add(new int[] { (int)FoodNameTypeMap.SpicyStringBoom, 0, 13 });
        //inList.Add(new int[] { 11, 0, 13 });
        //inList.Add(new int[] { 12, 1, 13 });
        //inList.Add(new int[] { 15, 0, 13 });
        //inList.Add(new int[] { 16, 0, 13 });
        //inList.Add(new int[] { 17, 0, 13 });
        //inList.Add(new int[] { 18, 0, 13 });
        //inList.Add(new int[] { (int)FoodNameTypeMap.PineappleBreadBoom, 2, 13 });
        List<AvailableCardInfo> selectedCardList = GameManager.Instance.playerData.GetCurrentSelectedCardInfoList();

        CardBuilderManager m = new CardBuilderManager();
        for (int i = 0; i < selectedCardList.Count; i++)
        {
            // ������Ƭ������Objectʵ����ͬʱȡ�����Ӧ�ű�����UI��������
            BaseCardBuilder cardBuilder = BaseCardBuilder.GetResource();
            cardBuilder.MInit(); // ��ʼ������
            cardBuilder.Load(selectedCardList[i].type, selectedCardList[i].maxShape, selectedCardList[i].maxLevel); // ��ȡ��Ӧ��Ϣ
            m.SetDefaultActionListener(cardBuilder); // ����Ĭ�ϼ���
            if (mGameNormalPanel.AddCardSlot(cardBuilder))  // ��������ӵ�����UI�������ʾ�������ӳɹ���ִ�����£��������Ŀ�����
                mCardBuilderList.Add(cardBuilder); // �����������
            else
                cardBuilder.Recycle();
        }
    }

    public void Awake()
    {

    }

    /// <summary>
    /// ���п�Ƭ�������İ�ť�������ͨ��ѡȡ����ʱ��������������
    /// </summary>
    public void SelectCard(int selectBuilderIndex)
    {
        // ȡ����ǰ����ѡ��
        CancelSelectShovel();
        BaseCardBuilder old = mSelectedCardBuilder; // ��һ��������
        BaseCardBuilder new_builder = mCardBuilderList[selectBuilderIndex]; // ��һ��������
        // �����ͬһ������������ȡ���ſ�
        if (old == new_builder)
        {
            CancelSelectCard();
            return;
        }
        // ִ����һ���������˳�ѡ���¼�
        if(old!=null)
            old.TriggerOnSelectedExitEvent(new_builder);
        mSelectedCardBuilder = new_builder;
        // ִ����һ���������Ľ����¼�
        mSelectedCardBuilder.TriggerOnSelectedEnterEvent(old); 
        isSelectCard = true;
        // ֪ͨUI���뽨��ģʽ�ˣ���һ��ִ�к������ͣ������ʾ��Ƭ����ģ��
        mGameNormalPanel.EnterCardConstructMode();
    }

    /// <summary>
    /// ����ҷ���ѡ��Ƭʱ����ԭ
    /// </summary>
    public void CancelSelectCard()
    {
        if(mSelectedCardBuilder!=null)
            mSelectedCardBuilder.TriggerOnSelectedExitEvent(null); // ִ����һ���������Ľ����¼�
        mSelectedCardBuilder = null;
        isSelectCard = false;
        // ֪ͨUIҪ��������ģʽ�ˣ���һ��ִ�����ؿ�Ƭ����ģ�ͣ�ʧ���
        mGameNormalPanel.ExitCardConstructMode();
    }

    /// <summary>
    /// ѡ�����
    /// </summary>
    public void SelectShovel()
    {
        // ȡ����ǰ��Ƭѡ��
        CancelSelectCard();
        isSelectShovel = true;
        mGameNormalPanel.EnterCardRemoveMode();
    }

    /// <summary>
    /// ȡ��ѡ�����
    /// </summary>
    public void CancelSelectShovel()
    {
        isSelectShovel = false;
        mGameNormalPanel.ExitCardRemoveMode();
    }

    /// <summary>
    /// ��ȡ��ѡ�еĿ�Ƭ������
    /// </summary>
    /// <returns></returns>
    public BaseCardBuilder GetSelectCardBuilder()
    {
        return mSelectedCardBuilder;
    }

    /// <summary>
    /// ָ����Ƭ���������һ�ο�Ƭ����ӹ�
    /// </summary>
    public bool Constructe()
    {
        BaseCardBuilder cardBuilder = GetSelectCardBuilder();
        if (cardBuilder.CanConstructe())
        {
            cardBuilder.TriggerBeforeBuildAction();
            cardBuilder.Constructe(); // ����ʵ��
            cardBuilder.InitInstance(); // ��ʼ��ʵ����Ϣ
            // ��Ǯ��
            cardBuilder.Cost();
            cardBuilder.TriggerAfterBuildAction();
            return true;
        }
        else
        {
            Debug.Log("�޷����죡");
            return false;
        }
    }

    /// <summary>
    /// ���п�Ƭ�Ƴ����Ƴ�˳��Ĭ�ϰ��ձ�����˳���Ƴ�
    /// </summary>
    public bool Destructe()
    {
        BaseGrid g = GameController.Instance.GetOverGrid();
        if (g != null)
        {
            BaseUnit target = g.GetHighestAttackPriorityUnit();
            if (target is FoodUnit)
            {
                // ���һ���Ƴ���Ч
                BaseEffect e = GameManager.Instance.GetGameObjectResource(FactoryType.GameFactory, "Effect/ShovelEffect").GetComponent<BaseEffect>();
                e.transform.position = g.transform.position;
                GameController.Instance.AddEffect(e);
                target.BeforeDeath();
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// �Ƴ���ǰȫ������
    /// </summary>
    public void RemoveAllSlot()
    {
        foreach (var cardBuilder in mCardBuilderList)
        {
            cardBuilder.Recycle();
        }
        mCardBuilderList.Clear();
    }

    public void MDestory()
    {
        throw new System.NotImplementedException();
    }

    public void MPause()
    {
        
    }

    public void MResume()
    {
        
    }

    /// <summary>
    /// ��Ƭ������ÿ֡Ҫ�����£��������п�Ƭ��������֡�߼����ж��ſ�
    /// </summary>
    public void MUpdate()
    {
        // �������п�Ƭ��������֡�߼�
        foreach (BaseCardBuilder cardBuilder in mCardBuilderList)
        {
            cardBuilder.MUpdate();
        }

        if (isSelectCard)
        {
            // ��ѡȡ��Ƭ״̬ʱ��ÿ֡��Ҫ�ж����İ������
            if (Input.GetMouseButtonDown(0)) // ������Էſ�
            {
                // TODO ��ȡ��ǰ��Ƭ�͸�����Ϣ���ۺ��ж��ܷ����ȥ������ȥ����к�������Ȼ���˳��ſ�ģʽ
                
                if(Constructe()) // ��һ���ǰѿ�����ȥ������ųɹ�����ȡ����Ƭѡ��
                {
                    Debug.Log("�������˿�");
                    CancelSelectCard();
                }
                else
                {
                    Debug.Log("�ſ�ʧ�ܣ���ѡ�����λ�÷ſ���");
                }
            }
            else if(Input.GetMouseButtonDown(1)){ // �Ҽ�ֱ��ȡ��
                Debug.Log("��ȡ���˷ſ�");
                CancelSelectCard();
            }
        }else if (isSelectShovel)
        {
            // ��ʹ�ò���״̬ʱ��ÿ֡��Ҫ�ж����İ������
            if (Input.GetMouseButtonDown(0)) // ������Էſ�
            {
                if (Destructe()) // ִ��һ���Ƴ�����
                {
                    Debug.Log("���Ƴ��˿�");
                }
                else
                {
                    Debug.Log("�Ƴ�ʧ�ܣ���ѡ�����λ�÷ſ���");
                }
                CancelSelectShovel();
            }
            else if (Input.GetMouseButtonDown(1))
            { 
                // �Ҽ�ֱ��ȡ��
                Debug.Log("��ȡ�����Ƴ���");
                CancelSelectShovel();
            }
        }
    }

    public void MPauseUpdate()
    {
        
    }
}
