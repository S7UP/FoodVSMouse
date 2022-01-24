using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseCardController : MonoBehaviour, IBaseCardController, IGameControllerMember
{
    public GameController mGameController;
    public List<BaseCardBuilder> mCardBuilderList; // ��Я���Ŀ�Ƭ�Ľ�������
    public BaseCard mSelcetCard; // ��ǰ��ѡ�еĿ�
    public BaseGrid mSelectGrid; // ��ǰ��ѡ�еĸ���

    public bool isSelectCard;
    public bool isSelectGrid;

    public void MInit()
    {
        mGameController = GameController.Instance;
        isSelectCard = false;
        isSelectGrid = false;

        // ��ʼ��������Ϣ����Ҫ�ⲿ��ȡ��ֵ�����⸳ֵ
        mCardBuilderList = new List<BaseCardBuilder>();
        for (int i = 0; i < 10; i++)
        {
            BaseCardBuilder cardBuilder = new BaseCardBuilder();
            cardBuilder.MInit();
            cardBuilder.SetPosition(new Vector3(0.5f*i, 2, 0));
            mCardBuilderList.Add(cardBuilder);
        }
    }

    public void Awake()
    {
        MInit();
    }

    public void Constructe()
    {
        throw new System.NotImplementedException();
    }

    public void SelectCard()
    {
        throw new System.NotImplementedException();
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

    public void MUpdate()
    {
        
    
    }
}
