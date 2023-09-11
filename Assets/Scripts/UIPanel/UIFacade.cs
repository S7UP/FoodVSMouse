using System.Collections;
using System.Collections.Generic;

using UnityEngine;
//using DG.Tweening;

//UI�н飬�ϲ�������������������²���UI�����н���
public class UIFacade
{
    // ������
    private UIManager mUIManager;
    private GameManager mGameManager;
    private AudioSourceController mAudioSourceController;
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
        mAudioSourceController = mGameManager.audioSourceController;
        // UI�Ĵ�Ŵ���
        uiCanvas = GameObject.Find("Canvas").GetComponent<Canvas>();
        GameManager.DontDestroyOnLoad(uiCanvas);
        //InitMask();
    }

    // �ı䵱ǰ������״̬
    public IEnumerator ChangeSceneState(IBaseSceneState baseSceneState, int StayTime, int MaskTime)
    {
        lastSceneState = currentSceneState;
        //ShowMask();
        currentSceneState = baseSceneState;
        // ���������ؽ���
        yield return GameManager.Instance.StartCoroutine(GameManager.Instance.ChangeToStartLoadPanel(MaskTime, delegate {
            // �����һ��������ս���������ͻ��ճ����Ķ���������loading����С
            if (lastSceneState is GameNormalSceneState)
            {
                GameController.Instance.RecycleAndDestoryAllInstance();
                GameManager.Instance.startLoadPanel.transform.localScale = new Vector2(0.78125f, 0.78125f);
                string id = TipsManager.GetRandomExitTipsId();
                string content = TipsManager.GetExitContent(id);
                TipsManager.AddExitVal(id); // ���Ȩֵ
                GameManager.Instance.startLoadPanel.ShowTips(content);
            }
            else
            {
                GameManager.Instance.startLoadPanel.transform.localScale = Vector2.one;
            }
            // ������Loading��������ó�����ս������������tips
            if (baseSceneState is GameNormalSceneState)
            {
                string id = TipsManager.GetRandomEnterTipsId();
                string content = TipsManager.GetEnterContent(id);
                TipsManager.AddEnterVal(id); // ���Ȩֵ
                GameManager.Instance.startLoadPanel.ShowTips(content);
            }
        }));
        // �ȼ����������
        yield return GameManager.Instance.StartCoroutine(baseSceneState.LoadScene());
        // ����������һ˲�䣬�����һ��������ս����������ô��С
        if (baseSceneState is GameNormalSceneState) 
            GameManager.Instance.startLoadPanel.transform.localScale = new Vector2(0.78125f, 0.78125f);
        if (lastSceneState is GameNormalSceneState)
            GameManager.Instance.startLoadPanel.transform.localScale = Vector2.one;
        if (lastSceneState != null)
            lastSceneState.ExitScene();
        // �ȴ��̶�ʱ��
        for (int i = 0; i < StayTime; i++)
        {
            yield return null;
        }
        // �Ӽ��ؽ��潥��
        yield return GameManager.Instance.StartCoroutine(GameManager.Instance.StartLoadPanelDisappear(MaskTime, delegate { currentSceneState.EnterScene(); }));
    }

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
        mAudioSourceController.CloseOrOpenBGMusic();
    }

    public void CloseOrOpenEffectMusic()
    {
        mAudioSourceController.CloseOrOpenEffectMusic();
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
