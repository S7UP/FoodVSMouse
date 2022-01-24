using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseCardBuilder : MonoBehaviour, IBaseCardBuilder, IGameControllerMember
{
    private GameController mGameController;
    public Dictionary<string, float> mCostDict;
    protected int mBaseCD; // 基础CD
    public int mCD; // 当前最大CD
    public int mCDLeft; // 当前剩余CD
    public bool isDisable; // 是否被禁用了
    public GameObject mUIGo; // 所持有的UI实例

    public void MInit()
    {
        mGameController = GameController.Instance;

        // 以下只是拟赋值，实际实现请读取本地Json文件数据
        mBaseCD = 7*ConfigManager.fps;
        mCD = mBaseCD;
        mCDLeft = 0;
        isDisable = false;
        mUIGo = GameManager.Instance.GetGameObjectResource(FactoryType.UIFactory, "CardBuilder");
        // mUIGo.transform.SetParent(GameController.Instance.uICanvasGo.transform);
        // 获取游戏场景面板
        GameNormalPanel panel = (GameNormalPanel)GameManager.Instance.uiManager.mUIFacade.currentScenePanelDict[StringManager.GameNormalPanel];
        panel.AddCardSlot(this); // 将自身信息添加到卡槽UI里！
    }

    public void Awake()
    {
        MInit();
    }

    public void SetPosition(Vector3 v3)
    {
        mUIGo.transform.position = v3;
    }

    public bool CanConstructe()
    {
        throw new System.NotImplementedException();
    }

    public bool CanSelect()
    {
        throw new System.NotImplementedException();
    }

    public void CardBuilderUpdate()
    {
        throw new System.NotImplementedException();
    }

    public void Constructe()
    {
        throw new System.NotImplementedException();
    }

    public BaseUnit GetResult()
    {
        throw new System.NotImplementedException();
    }

    public void InitInstance()
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
        throw new System.NotImplementedException();
    }
}
