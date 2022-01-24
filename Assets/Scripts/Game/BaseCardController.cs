using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseCardController : MonoBehaviour, IBaseCardController, IGameControllerMember
{
    public GameController mGameController;
    public List<BaseCardBuilder> mCardBuilderList; // 所携带的卡片的建造器表
    public BaseCard mSelcetCard; // 当前被选中的卡
    public BaseGrid mSelectGrid; // 当前被选中的格子

    public bool isSelectCard;
    public bool isSelectGrid;

    public void MInit()
    {
        mGameController = GameController.Instance;
        isSelectCard = false;
        isSelectGrid = false;

        // 初始化卡槽信息，需要外部读取赋值，现拟赋值
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
