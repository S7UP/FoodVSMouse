using System.Collections;
using System.Collections.Generic;

using UnityEngine;
//using DG.Tweening;

//UI中介，上层与管理者们做交互，下层与UI面板进行交互
public class UIFacade
{
    // 管理者
    private UIManager mUIManager;
    private GameManager mGameManager;
    private AudioSourceController mAudioSourceController;
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
        mAudioSourceController = mGameManager.audioSourceController;
        // UI的存放窗口
        uiCanvas = GameObject.Find("Canvas").GetComponent<Canvas>();
        GameManager.DontDestroyOnLoad(uiCanvas);
        //InitMask();
    }

    // 改变当前场景的状态
    public IEnumerator ChangeSceneState(IBaseSceneState baseSceneState, int StayTime, int MaskTime)
    {
        lastSceneState = currentSceneState;
        //ShowMask();
        currentSceneState = baseSceneState;
        // 渐出至加载界面
        yield return GameManager.Instance.StartCoroutine(GameManager.Instance.ChangeToStartLoadPanel(MaskTime, delegate {
            // 如果上一个场景是战斗场景，就回收场景的东西并调整loading面板大小
            if (lastSceneState is GameNormalSceneState)
            {
                GameController.Instance.RecycleAndDestoryAllInstance();
                GameManager.Instance.startLoadPanel.transform.localScale = new Vector2(0.78125f, 0.78125f);
                string id = TipsManager.GetRandomExitTipsId();
                string content = TipsManager.GetExitContent(id);
                TipsManager.AddExitVal(id); // 添加权值
                GameManager.Instance.startLoadPanel.ShowTips(content);
            }
            else
            {
                GameManager.Instance.startLoadPanel.transform.localScale = Vector2.one;
            }
            // 加载完Loading面板后，如果该场景是战斗场景，播放tips
            if (baseSceneState is GameNormalSceneState)
            {
                string id = TipsManager.GetRandomEnterTipsId();
                string content = TipsManager.GetEnterContent(id);
                TipsManager.AddEnterVal(id); // 添加权值
                GameManager.Instance.startLoadPanel.ShowTips(content);
            }
        }));
        // 先加载这个场景
        yield return GameManager.Instance.StartCoroutine(baseSceneState.LoadScene());
        // 场景加载完一瞬间，如果下一个场景是战斗场景，那么缩小
        if (baseSceneState is GameNormalSceneState) 
            GameManager.Instance.startLoadPanel.transform.localScale = new Vector2(0.78125f, 0.78125f);
        if (lastSceneState is GameNormalSceneState)
            GameManager.Instance.startLoadPanel.transform.localScale = Vector2.one;
        if (lastSceneState != null)
            lastSceneState.ExitScene();
        // 等待固定时间
        for (int i = 0; i < StayTime; i++)
        {
            yield return null;
        }
        // 从加载界面渐出
        yield return GameManager.Instance.StartCoroutine(GameManager.Instance.StartLoadPanelDisappear(MaskTime, delegate { currentSceneState.EnterScene(); }));
    }

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
        mAudioSourceController.CloseOrOpenBGMusic();
    }

    public void CloseOrOpenEffectMusic()
    {
        mAudioSourceController.CloseOrOpenEffectMusic();
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
