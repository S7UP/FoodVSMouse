using System.Collections.Generic;

using UnityEngine;
//using DG.Tweening;

//UI中介，上层与管理者们做交互，下层与UI面板进行交互
public class UIFacade
{
    // 管理者
    private UIManager mUIManager;
    private GameManager mGameManager;
    private AudioSourceManager mAudioSourceManager;
    public PlayerManager mPlayerManager;
    // UI面板
    public Dictionary<string, IBasePanel> currentScenePanelDict = new Dictionary<string, IBasePanel>();

    // 其他成员变量
    public Canvas uiCanvas; //{ get { return GameObject.Find("Canvas").GetComponent<Canvas>(); } }
    // 场景状态
    public IBaseSceneState currentSceneState;
    public IBaseSceneState lastSceneState;

    public UIFacade(UIManager uIManager)
    {
        mGameManager = GameManager.Instance;
        mPlayerManager = mGameManager.playerManager;
        mUIManager = uIManager;
        mAudioSourceManager = mGameManager.audioSourceManager;
        // UI的存放窗口
        uiCanvas = GameObject.Find("Canvas").GetComponent<Canvas>();
        GameManager.DontDestroyOnLoad(uiCanvas);
        //InitMask();
    }

    // 初始化遮罩
    public void InitMask()
    {

        // mask = mGameManager.factoryManager.factoryDict[FactoryType.UIFactory].GetItem("Img_Mask");
        // mask = mGameManager.GetGameObjectResource(FactoryType.UIFactory, "Img_Mask");
        // mask = GetGameObjectResource(FactoryType.UIFactory, "Img_Mask");
        //mask = CreateUIAndSetUIPosition("Img_Mask");
        //maskImage = mask.GetComponent<Image>();
    }

    // 改变当前场景的状态
    public void ChangeSceneState(IBaseSceneState baseSceneState)
    {
        lastSceneState = currentSceneState;
        //ShowMask();
        currentSceneState = baseSceneState;
        if (lastSceneState != null)
            lastSceneState.ExitScene();
        currentSceneState.EnterScene();
    }

    //显示遮罩
    //public void ShowMask()
    //{
    //    mask.transform.SetSiblingIndex(10);
    //    Tween t = DOTween.To(() => maskImage.color, toColor => maskImage.color = toColor, new Color(0, 0, 0, 1), 2f);
    //    // 注册事件结束时的回调事件
    //    t.OnComplete(ExitSceneComplete);
    //}

    // 离开当前场景
    //private void ExitSceneComplete()
    //{
    //    if(lastSceneState!=null)
    //        lastSceneState.ExitScene();
    //    currentSceneState.EnterScene();
    //}

    // 隐藏遮罩
    //public void HideMask()
    //{
    //    mask.transform.SetSiblingIndex(10);
    //    DOTween.To(() => maskImage.color, toColor => maskImage.color = toColor, new Color(0, 0, 0, 0), 2f);
    //}

    //实例化当前场景所有面板，并存放字典
    public void InitDict()
    {
        foreach (var item in mUIManager.currentScenePanelDict)
        {
            // 要的是值Value,而不是键
            item.Value.transform.SetParent(uiCanvas.transform);
            item.Value.transform.localPosition = Vector3.zero;
            item.Value.transform.localScale = Vector3.one;
            IBasePanel basePanel = item.Value.GetComponent<IBasePanel>();
            if (basePanel == null)
            {
                Debug.Log("获取面板上IBasePanel脚本失败");
            }
            basePanel.InitPanel();
            currentScenePanelDict.Add(item.Key, basePanel);
        }
    }

    public void ClearDict()
    {
        currentScenePanelDict.Clear();
        mUIManager.ClearDict();
    }

    // 添加UIPanel到UIManager字典
    public void AddPanelToDict(string uiPanelName)
    {
        mUIManager.currentScenePanelDict.Add(uiPanelName, GetGameObjectResource(FactoryType.UIPanelFactory, uiPanelName));
    }


    public GameObject CreateUIAndSetUIPosition(string uiName)
    {
        GameObject itemGo = GetGameObjectResource(FactoryType.UIFactory, uiName);
        itemGo.transform.SetParent(uiCanvas.transform);
        itemGo.transform.localPosition = Vector3.zero;
        itemGo.transform.localScale = Vector3.one;
        return itemGo;
    }

    public Sprite GetSprite(string resourcePath)
    {
        return mGameManager.GetSprite(resourcePath);
    }

    public AudioClip GetAudioSource(string resourcePath)
    {
        return mGameManager.GetAudioClip(resourcePath);
    }

    public RuntimeAnimatorController GetRuntimeAnimatorController(string resourcePath)
    {
        return mGameManager.GetRuntimeAnimatorController(resourcePath);
    }

    // 获取游戏物体
    public GameObject GetGameObjectResource(FactoryType factoryType, string resourcePath)
    {
        return mGameManager.GetGameObjectResource(factoryType, resourcePath);
    }
    // 将游戏物体放回对象池
    public void PushGameObjectToFactory(FactoryType factoryType, string resourcePath, GameObject itemGo)
    {
        mGameManager.PushGameObjectToFactory(factoryType, resourcePath, itemGo);
    }

    // 开关音乐
    public void CloseOrOpenBGMusic()
    {
        mAudioSourceManager.CloseOrOpenBGMusic();
    }

    public void CloseOrOpenEffectMusic()
    {
        mAudioSourceManager.CloseOrOpenEffectMusic();
    }

    /// <summary>
    /// 更新当前存在的面板
    /// </summary>
    public void UpdatePanel()
    {
        foreach (var keyValuePair in currentScenePanelDict)
        {
            IBasePanel panel = keyValuePair.Value;
            panel.UpdatePanel();
        }
    }
}
