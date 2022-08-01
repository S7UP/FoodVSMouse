using UnityEngine;

/// <summary>
/// 角色控制器，总控制器中对角色部分的管理
/// </summary>
public class CharacterController:IGameControllerMember
{
    private GameNormalPanel mGameNormalPanel;
    public CharacterUnit mCurrentCharacter; // 当前角色

    public CharacterController()
    {
        mGameNormalPanel = (GameNormalPanel)GameManager.Instance.uiManager.mUIFacade.currentScenePanelDict[StringManager.GameNormalPanel];
    }

    public void MInit()
    {
        // 读取玩家选取的角色，生成角色初始实例，并暂时设置为非活动状态，需要玩家放下后才可以激活
        CharacterInfo info = GameManager.Instance.playerData.GetCharacterInfo();
        mCurrentCharacter = GameController.Instance.CreateCharacter(info.type, info.shape);
        mCurrentCharacter.gameObject.SetActive(false);
        // 直接进入角色放置模式
        mGameNormalPanel.EnterCharacterConstructMode();
        GameController.Instance.Pause(); // 暂停游戏，直到把人放下去为止
    }

    public void MUpdate()
    {
        //HandleSetCharacter();
    }

    public void MPauseUpdate()
    {
        HandleSetCharacter();
    }

    /// <summary>
    /// 处理放置角色的相关事件
    /// </summary>
    private void HandleSetCharacter()
    {
        if (!IsSetCharacter())
        {
            if (Input.GetMouseButtonDown(0)) // 左键尝试放人物
            {
                // TODO 读取当前卡片和格子信息，综合判断能否放下去，放下去后进行后续处理然后退出放卡模式

                if (CanSetCharacter()) // 这一步是把卡放下去，如果放成功了则取消卡片选择
                {
                    ExecuteSetCharacter();
                }
                else
                {
                    Debug.Log("放置角色失败，请选择合适位置放置角色！");
                }
            }
        }
    }

    /// <summary>
    /// 是否放置角色
    /// </summary>
    /// <returns></returns>
    public bool IsSetCharacter()
    {
        return mCurrentCharacter.isActiveAndEnabled;
    }

    /// <summary>
    /// 当前状态下能否把人放下去
    /// </summary>
    /// <returns></returns>
    public bool CanSetCharacter()
    {
        BaseGrid baseGrid = GameController.Instance.GetOverGrid();
        if (baseGrid != null)
        {
            // 先检查格子状态能否允许造卡
            // 然后再查看是否含有此格子分类的卡片，若没有则允许建造，否则不行
            return (baseGrid.CanBuildCard(FoodInGridType.Default) && !baseGrid.IsContainTag(FoodInGridType.Default));
        }
        return false;
    }

    /// <summary>
    /// 执行下人操作
    /// </summary>
    /// <returns></returns>
    public void ExecuteSetCharacter()
    {
        // 退出角色放置模式
        mGameNormalPanel.ExitCharacterConstructMode();
        // 解除暂停
        GameController.Instance.Resume();
        BaseGrid g = GameController.Instance.GetOverGrid();
        mCurrentCharacter.gameObject.SetActive(true);
        mCurrentCharacter.MInit();
        GameController.Instance.AddCharacter(mCurrentCharacter, g.GetColumnIndex(), g.GetRowIndex());
    }

    /// <summary>
    /// 回收当前的角色
    /// </summary>
    public void RecycleCurrentCharacter()
    {
        if (mCurrentCharacter != null)
        {
            mCurrentCharacter.ExcuteRecycle();
        }
    }


    public void MPause()
    {
        mCurrentCharacter.MPause();
    }

    public void MResume()
    {
        mCurrentCharacter.MResume();
    }

    public void MDestory()
    {
        throw new System.NotImplementedException();
    }
}
