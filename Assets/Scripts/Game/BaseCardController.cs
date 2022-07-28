using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// 卡片建造控制器
/// </summary>
public class BaseCardController : MonoBehaviour, IBaseCardController, IGameControllerMember
{
    private GameNormalPanel mGameNormalPanel;
    public List<BaseCardBuilder> mCardBuilderList = new List<BaseCardBuilder>(); // 所携带的卡片的建造器表

    public BaseCardBuilder mSelectedCardBuilder;
    public BaseGrid mSelectGrid; // 当前被选中的格子

    public bool isSelectCard;
    public bool isSelectShovel; // 是否选择铲子
    public bool isSelectGrid;

    /// <summary>
    /// 初始化方法
    /// </summary>
    public void MInit()
    {
        // 变量初始化
        mGameNormalPanel = (GameNormalPanel)GameManager.Instance.uiManager.mUIFacade.currentScenePanelDict[StringManager.GameNormalPanel];
        mGameNormalPanel.ClearAllSlot(); // 清空卡槽
        mSelectedCardBuilder = null;
        isSelectCard = false;
        isSelectGrid = false;
        mCardBuilderList.Clear();
        


        CancelSelectCard();
        CancelSelectShovel();

        // 初始化卡槽信息，需要外部读取赋值，现拟赋值
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
            // 产生卡片建造器Object实例的同时取得其对应脚本，与UI层作关联
            BaseCardBuilder cardBuilder = BaseCardBuilder.GetResource();
            cardBuilder.MInit(); // 初始化数据
            cardBuilder.Load(selectedCardList[i].type, selectedCardList[i].maxShape, selectedCardList[i].maxLevel); // 读取对应信息
            m.SetDefaultActionListener(cardBuilder); // 设置默认监听
            if (mGameNormalPanel.AddCardSlot(cardBuilder))  // 将对象添加到卡槽UI里，负责显示，如果添加成功则执行以下，否则回收目标对象！
                mCardBuilderList.Add(cardBuilder); // 负责更新数据
            else
                cardBuilder.Recycle();
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
        // 取消当前铲子选择
        CancelSelectShovel();
        BaseCardBuilder old = mSelectedCardBuilder; // 上一个建造器
        BaseCardBuilder new_builder = mCardBuilderList[selectBuilderIndex]; // 下一个建造器
        // 如果是同一个建造器，则取消放卡
        if (old == new_builder)
        {
            CancelSelectCard();
            return;
        }
        // 执行上一个建造器退出选择事件
        if(old!=null)
            old.TriggerOnSelectedExitEvent(new_builder);
        mSelectedCardBuilder = new_builder;
        // 执行下一个建造器的进入事件
        mSelectedCardBuilder.TriggerOnSelectedEnterEvent(old); 
        isSelectCard = true;
        // 通知UI进入建造模式了，这一步执行后鼠标悬停处将显示卡片建造模型
        mGameNormalPanel.EnterCardConstructMode();
    }

    /// <summary>
    /// 当玩家放弃选择卡片时，复原
    /// </summary>
    public void CancelSelectCard()
    {
        if(mSelectedCardBuilder!=null)
            mSelectedCardBuilder.TriggerOnSelectedExitEvent(null); // 执行上一个建造器的结束事件
        mSelectedCardBuilder = null;
        isSelectCard = false;
        // 通知UI要结束建造模式了，这一步执行隐藏卡片建造模型（失活处理）
        mGameNormalPanel.ExitCardConstructMode();
    }

    /// <summary>
    /// 选择铲子
    /// </summary>
    public void SelectShovel()
    {
        // 取消当前卡片选择
        CancelSelectCard();
        isSelectShovel = true;
        mGameNormalPanel.EnterCardRemoveMode();
    }

    /// <summary>
    /// 取消选择铲子
    /// </summary>
    public void CancelSelectShovel()
    {
        isSelectShovel = false;
        mGameNormalPanel.ExitCardRemoveMode();
    }

    /// <summary>
    /// 获取被选中的卡片建造器
    /// </summary>
    /// <returns></returns>
    public BaseCardBuilder GetSelectCardBuilder()
    {
        return mSelectedCardBuilder;
    }

    /// <summary>
    /// 指导卡片建造器完成一次卡片建造加工
    /// </summary>
    public bool Constructe()
    {
        BaseCardBuilder cardBuilder = GetSelectCardBuilder();
        if (cardBuilder.CanConstructe())
        {
            cardBuilder.TriggerBeforeBuildAction();
            cardBuilder.Constructe(); // 产生实体
            cardBuilder.InitInstance(); // 初始化实体信息
            // 扣钱！
            cardBuilder.Cost();
            cardBuilder.TriggerAfterBuildAction();
            return true;
        }
        else
        {
            Debug.Log("无法建造！");
            return false;
        }
    }

    /// <summary>
    /// 进行卡片移除，移除顺序默认按照被攻击顺序移除
    /// </summary>
    public bool Destructe()
    {
        BaseGrid g = GameController.Instance.GetOverGrid();
        if (g != null)
        {
            BaseUnit target = g.GetHighestAttackPriorityUnit();
            if (target is FoodUnit)
            {
                // 添加一个移除特效
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
    /// 移除当前全部卡槽
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
    /// 卡片控制器每帧要做的事：管理所有卡片建造器的帧逻辑，判定放卡
    /// </summary>
    public void MUpdate()
    {
        // 管理所有卡片建造器的帧逻辑
        foreach (BaseCardBuilder cardBuilder in mCardBuilderList)
        {
            cardBuilder.MUpdate();
        }

        if (isSelectCard)
        {
            // 在选取卡片状态时，每帧都要判断鼠标的按下情况
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
        }else if (isSelectShovel)
        {
            // 在使用铲子状态时，每帧都要判断鼠标的按下情况
            if (Input.GetMouseButtonDown(0)) // 左键尝试放卡
            {
                if (Destructe()) // 执行一次移除操作
                {
                    Debug.Log("您移除了卡");
                }
                else
                {
                    Debug.Log("移除失败，请选择合适位置放卡！");
                }
                CancelSelectShovel();
            }
            else if (Input.GetMouseButtonDown(1))
            { 
                // 右键直接取消
                Debug.Log("您取消了移除卡");
                CancelSelectShovel();
            }
        }
    }

    public void MPauseUpdate()
    {
        
    }
}
