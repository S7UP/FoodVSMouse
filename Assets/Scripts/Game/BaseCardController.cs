using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseCardController : MonoBehaviour, IBaseCardController, IGameControllerMember
{
    private GameController mGameController;
    private GameNormalPanel mGameNormalPanel;
    public List<BaseCardBuilder> mCardBuilderList; // ��Я���Ŀ�Ƭ�Ľ�������

    public int mSelcetCardBuilderIndex; // ��ǰ��ѡ�еĽ��������±�
    public BaseGrid mSelectGrid; // ��ǰ��ѡ�еĸ���

    public bool isSelectCard;
    public bool isSelectGrid;

    public void MInit()
    {
        mGameController = GameController.Instance;
        mGameNormalPanel = (GameNormalPanel)GameManager.Instance.uiManager.mUIFacade.currentScenePanelDict[StringManager.GameNormalPanel];
        mSelcetCardBuilderIndex = -1;
        isSelectCard = false;
        isSelectGrid = false;

        // ��ʼ��������Ϣ����Ҫ�ⲿ��ȡ��ֵ�����⸳ֵ
        List<int[]> inList = new List<int[]>();
        inList.Add(new int[] { 0, 2, 12 });
        inList.Add(new int[] { 1, 2, 12 });
        inList.Add(new int[] { 2, 2, 12 });
        inList.Add(new int[] { 4, 1, 12 });
        inList.Add(new int[] { 6, 2, 12 });
        inList.Add(new int[]{ 7, 2, 12 });
        inList.Add(new int[]{ 8, 2, 12 });
        inList.Add(new int[] { 11, 0, 12 });
        inList.Add(new int[] { 12, 1, 12 });
        inList.Add(new int[] { 15, 0, 12 });
        inList.Add(new int[] { 16, 0, 12 });
        inList.Add(new int[] { 17, 0, 12 });
        inList.Add(new int[] { 18, 0, 12 });

        mCardBuilderList = new List<BaseCardBuilder>();
        for (int i = 0; i < inList.Count; i++)
        {
            // ������Ƭ������Objectʵ����ͬʱȡ�����Ӧ�ű�����UI��������
            BaseCardBuilder cardBuilder = GameManager.Instance.GetGameObjectResource(FactoryType.UIFactory, "CardBuilder").GetComponent<BaseCardBuilder>();
            cardBuilder.MInit(inList[i][0], inList[i][1], inList[i][2]);
            mCardBuilderList.Add(cardBuilder);
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
        mSelcetCardBuilderIndex = selectBuilderIndex;
        isSelectCard = true;
        // ֪ͨUI���뽨��ģʽ�ˣ���һ��ִ�к������ͣ������ʾ��Ƭ����ģ��
        mGameNormalPanel.EnterCardConstructMode();
    }

    /// <summary>
    /// ����ҷ���ѡ��Ƭʱ����ԭ
    /// </summary>
    public void CancelSelectCard()
    {
        mSelcetCardBuilderIndex = -1;
        isSelectCard = false;
        // ֪ͨUIҪ��������ģʽ�ˣ���һ��ִ�����ؿ�Ƭ����ģ�ͣ�ʧ���
        mGameNormalPanel.ExitCardConstructMode();
    }

    /// <summary>
    /// ��ȡ��ѡ�еĿ�Ƭ������
    /// </summary>
    /// <returns></returns>
    public BaseCardBuilder GetSelectCardBuilder()
    {
        return mCardBuilderList[mSelcetCardBuilderIndex];
    }

    /// <summary>
    /// ָ����Ƭ���������һ�ο�Ƭ����ӹ�
    /// </summary>
    public bool Constructe()
    {
        BaseCardBuilder cardBuilder = GetSelectCardBuilder();
        if (cardBuilder.CanConstructe())
        {
            cardBuilder.Constructe(); // ����ʵ��
            cardBuilder.InitInstance(); // ��ʼ��ʵ����Ϣ
            // ��Ǯ��
            cardBuilder.Cost();
            return true;
        }
        else
        {
            Debug.Log("�޷����죡");
            return false;
        }
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
    /// ��Ƭ������ÿ֡Ҫ�����£��������п�Ƭ��������֡�߼����ж��ſ�
    /// </summary>
    public void MUpdate()
    {
        // �������п�Ƭ��������֡�߼�
        foreach (BaseCardBuilder cardBuilder in mCardBuilderList)
        {
            cardBuilder.MUpdate();
        }

        // ��ѡȡ��Ƭ״̬ʱ��ÿ֡��Ҫ�ж����İ������
        if (isSelectCard)
        {
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
        }
    }

    public void MPauseUpdate()
    {
        
    }
}
