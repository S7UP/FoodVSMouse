using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseCardController : MonoBehaviour, IBaseCardController, IGameControllerMember
{
    private GameController mGameController;
    private GameNormalPanel mGameNormalPanel;
    public List<BaseCardBuilder> mCardBuilderList; // 所携带的卡片的建造器表

    public int mSelcetCardBuilderIndex; // 当前被选中的建造器的下标
    public BaseGrid mSelectGrid; // 当前被选中的格子

    public bool isSelectCard;
    public bool isSelectGrid;

    public void MInit()
    {
        mGameController = GameController.Instance;
        mGameNormalPanel = (GameNormalPanel)GameManager.Instance.uiManager.mUIFacade.currentScenePanelDict[StringManager.GameNormalPanel];
        mSelcetCardBuilderIndex = -1;
        isSelectCard = false;
        isSelectGrid = false;

        // 初始化卡槽信息，需要外部读取赋值，现拟赋值
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
            // 产生卡片建造器Object实例的同时取得其对应脚本，与UI层作关联
            BaseCardBuilder cardBuilder = GameManager.Instance.GetGameObjectResource(FactoryType.UIFactory, "CardBuilder").GetComponent<BaseCardBuilder>();
            cardBuilder.MInit(inList[i][0], inList[i][1], inList[i][2]);
            mCardBuilderList.Add(cardBuilder);
        }
    }

    public void Awake()
    {

    }

    /// <summary>
    /// 当有卡片建造器的按钮被点击且通过选取条件时，会调用这个方法
    /// </summary>
    public void SelectCard(int selectBuilderIndex)
    {
        mSelcetCardBuilderIndex = selectBuilderIndex;
        isSelectCard = true;
        // 通知UI进入建造模式了，这一步执行后鼠标悬停处将显示卡片建造模型
        mGameNormalPanel.EnterCardConstructMode();
    }

    /// <summary>
    /// 当玩家放弃选择卡片时，复原
    /// </summary>
    public void CancelSelectCard()
    {
        mSelcetCardBuilderIndex = -1;
        isSelectCard = false;
        // 通知UI要结束建造模式了，这一步执行隐藏卡片建造模型（失活处理）
        mGameNormalPanel.ExitCardConstructMode();
    }

    /// <summary>
    /// 获取被选中的卡片建造器
    /// </summary>
    /// <returns></returns>
    public BaseCardBuilder GetSelectCardBuilder()
    {
        return mCardBuilderList[mSelcetCardBuilderIndex];
    }

    /// <summary>
    /// 指导卡片建造器完成一次卡片建造加工
    /// </summary>
    public bool Constructe()
    {
        BaseCardBuilder cardBuilder = GetSelectCardBuilder();
        if (cardBuilder.CanConstructe())
        {
            cardBuilder.Constructe(); // 产生实体
            cardBuilder.InitInstance(); // 初始化实体信息
            // 扣钱！
            cardBuilder.Cost();
            return true;
        }
        else
        {
            Debug.Log("无法建造！");
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
    /// 卡片控制器每帧要做的事：管理所有卡片建造器的帧逻辑，判定放卡
    /// </summary>
    public void MUpdate()
    {
        // 管理所有卡片建造器的帧逻辑
        foreach (BaseCardBuilder cardBuilder in mCardBuilderList)
        {
            cardBuilder.MUpdate();
        }

        // 在选取卡片状态时，每帧都要判断鼠标的按下情况
        if (isSelectCard)
        {
            if (Input.GetMouseButtonDown(0)) // 左键尝试放卡
            {
                // TODO 读取当前卡片和格子信息，综合判断能否放下去，放下去后进行后续处理然后退出放卡模式
                
                if(Constructe()) // 这一步是把卡放下去，如果放成功了则取消卡片选择
                {
                    Debug.Log("您放下了卡");
                    CancelSelectCard();
                }
                else
                {
                    Debug.Log("放卡失败，请选择合适位置放卡！");
                }
            }
            else if(Input.GetMouseButtonDown(1)){ // 右键直接取消
                Debug.Log("您取消了放卡");
                CancelSelectCard();
            }
        }
    }

    public void MPauseUpdate()
    {
        
    }
}
