using System.Collections.Generic;

using UnityEngine;
//using DG.Tweening;

//UI�н飬�ϲ�������������������²���UI�����н���
public class UIFacade
{
    // ������
    private UIManager mUIManager;
    private GameManager mGameManager;
    private AudioSourceManager mAudioSourceManager;
    public PlayerManager mPlayerManager;
    // UI���
    public Dictionary<string, IBasePanel> currentScenePanelDict = new Dictionary<string, IBasePanel>();

    // ������Ա����
    public Canvas uiCanvas; //{ get { return GameObject.Find("Canvas").GetComponent<Canvas>(); } }
    // ����״̬
    public IBaseSceneState currentSceneState;
    public IBaseSceneState lastSceneState;

    public UIFacade(UIManager uIManager)
    {
        mGameManager = GameManager.Instance;
        mPlayerManager = mGameManager.playerManager;
        mUIManager = uIManager;
        mAudioSourceManager = mGameManager.audioSourceManager;
        // UI�Ĵ�Ŵ���
        uiCanvas = GameObject.Find("Canvas").GetComponent<Canvas>();
        GameManager.DontDestroyOnLoad(uiCanvas);
        //InitMask();
    }

    // ��ʼ������
    public void InitMask()
    {

        // mask = mGameManager.factoryManager.factoryDict[FactoryType.UIFactory].GetItem("Img_Mask");
        // mask = mGameManager.GetGameObjectResource(FactoryType.UIFactory, "Img_Mask");
        // mask = GetGameObjectResource(FactoryType.UIFactory, "Img_Mask");
        //mask = CreateUIAndSetUIPosition("Img_Mask");
        //maskImage = mask.GetComponent<Image>();
    }

    // �ı䵱ǰ������״̬
    public void ChangeSceneState(IBaseSceneState baseSceneState)
    {
        lastSceneState = currentSceneState;
        //ShowMask();
        currentSceneState = baseSceneState;
        if (lastSceneState != null)
            lastSceneState.ExitScene();
        currentSceneState.EnterScene();
    }

    //��ʾ����
    //public void ShowMask()
    //{
    //    mask.transform.SetSiblingIndex(10);
    //    Tween t = DOTween.To(() => maskImage.color, toColor => maskImage.color = toColor, new Color(0, 0, 0, 1), 2f);
    //    // ע���¼�����ʱ�Ļص��¼�
    //    t.OnComplete(ExitSceneComplete);
    //}

    // �뿪��ǰ����
    //private void ExitSceneComplete()
    //{
    //    if(lastSceneState!=null)
    //        lastSceneState.ExitScene();
    //    currentSceneState.EnterScene();
    //}

    // ��������
    //public void HideMask()
    //{
    //    mask.transform.SetSiblingIndex(10);
    //    DOTween.To(() => maskImage.color, toColor => maskImage.color = toColor, new Color(0, 0, 0, 0), 2f);
    //}

    //ʵ������ǰ����������壬������ֵ�
    public void InitDict()
    {
        foreach (var item in mUIManager.currentScenePanelDict)
        {
            // Ҫ����ֵValue,�����Ǽ�
            item.Value.transform.SetParent(uiCanvas.transform);
            item.Value.transform.localPosition = Vector3.zero;
            item.Value.transform.localScale = Vector3.one;
            IBasePanel basePanel = item.Value.GetComponent<IBasePanel>();
            if (basePanel == null)
            {
                Debug.Log("��ȡ�����IBasePanel�ű�ʧ��");
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

    // ���UIPanel��UIManager�ֵ�
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

    // ��ȡ��Ϸ����
    public GameObject GetGameObjectResource(FactoryType factoryType, string resourcePath)
    {
        return mGameManager.GetGameObjectResource(factoryType, resourcePath);
    }
    // ����Ϸ����Żض����
    public void PushGameObjectToFactory(FactoryType factoryType, string resourcePath, GameObject itemGo)
    {
        mGameManager.PushGameObjectToFactory(factoryType, resourcePath, itemGo);
    }

    // ��������
    public void CloseOrOpenBGMusic()
    {
        mAudioSourceManager.CloseOrOpenBGMusic();
    }

    public void CloseOrOpenEffectMusic()
    {
        mAudioSourceManager.CloseOrOpenEffectMusic();
    }

    /// <summary>
    /// ���µ�ǰ���ڵ����
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
