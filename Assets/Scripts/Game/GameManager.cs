
using System;
using System.Collections;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public System.Random rand;
    public PlayerManager playerManager;
    public FactoryManager factoryManager;
    public AudioSourceManager audioSourceManager;
    public UIManager uiManager;
    public ConfigManager configManager;
    public AbilityManager abilityManager;
    public AttributeManager attributeManager;
    public PlayerData playerData;
    public StartLoadPanel startLoadPanel; // �ɱ༭����ֵ��

    private static GameManager _instance;

    public static GameManager Instance { get => _instance; }

    private float deltaTime = 0.0f;

    private Image TopMask; // ���ϲ��ɫ����

    private void Awake()
    {
        DontDestroyOnLoad(gameObject); // �г���ʱ������
        _instance = this;
        rand = new System.Random();
        playerManager = new PlayerManager();
        factoryManager = new FactoryManager();
        audioSourceManager = new AudioSourceManager();
        configManager = new ConfigManager(); // ����ConfigManager��������ҵ�����
        abilityManager = AbilityManager.GetSingleton();
        uiManager = new UIManager();
        attributeManager = new AttributeManager();
        attributeManager.Initial();
        startLoadPanel = GameObject.Find("Canvas").transform.Find("StartLoadPanel").GetComponent<StartLoadPanel>();
        startLoadPanel.gameObject.SetActive(false);
        TopMask = GameObject.Find("Canvas").transform.Find("TopMask").GetComponent<Image>();
        TopMask.gameObject.SetActive(false);

        // �����������(��̬)
        playerData = PlayerData.GetInstance();
        // ��ʼ����
        // StartCoroutine(uiManager.mUIFacade.ChangeSceneState(new StartLoadSceneState(uiManager.mUIFacade)));
        Load();
        //Test
    }

    /// <summary>
    /// ��Loading������صĶ���
    /// </summary>
    public void Load()
    {
        MusicManager.Load();
        MouseManager.LoadAll();
        TipsManager.Load();
        TagsManager.Load();
    }

    /// <summary>
    /// ��һ�ν�����Ϸ������ǰ��Ҫ���غõ�����
    /// </summary>
    /// <returns></returns>
    public IEnumerator LoadWhenBeforeEnterMainScene()
    {
        yield return SoundsManager.Load();
    }

    /// <summary>
    /// ����༭������
    /// </summary>
    public void EnterEditorScene()
    {
        StartCoroutine(uiManager.mUIFacade.ChangeSceneState(new EditorSceneState(uiManager.mUIFacade), 0, 0));
    }

    /// <summary>
    /// ����ս������
    /// </summary>
    public void EnterComBatScene()
    {
        StartCoroutine(uiManager.mUIFacade.ChangeSceneState(new GameNormalSceneState(uiManager.mUIFacade), 240, 60));
    }

    /// <summary>
    /// ������ζ�򳡾�
    /// </summary>
    public void EnterTownScene()
    {
        if(uiManager.mUIFacade.currentSceneState != null && uiManager.mUIFacade.currentSceneState is GameNormalSceneState)
            StartCoroutine(uiManager.mUIFacade.ChangeSceneState(new TownSceneState(uiManager.mUIFacade), 240, 60));
        else
            StartCoroutine(uiManager.mUIFacade.ChangeSceneState(new TownSceneState(uiManager.mUIFacade), 0, 0));
    }

    /// <summary>
    /// ����ѡ�����ó���
    /// </summary>
    public void EnterSelectScene()
    {
        if (uiManager.mUIFacade.currentSceneState != null && uiManager.mUIFacade.currentSceneState is GameNormalSceneState)
            StartCoroutine(uiManager.mUIFacade.ChangeSceneState(new SelectSceneState(uiManager.mUIFacade), 240, 60));
        else
            StartCoroutine(uiManager.mUIFacade.ChangeSceneState(new SelectSceneState(uiManager.mUIFacade), 0, 0));
    }

    /// <summary>
    /// ������ҳ�泡��
    /// </summary>
    public void EnterMainScene()
    {
        if (uiManager.mUIFacade.currentSceneState != null && uiManager.mUIFacade.currentSceneState is GameNormalSceneState)
            StartCoroutine(uiManager.mUIFacade.ChangeSceneState(new MainSceneState(uiManager.mUIFacade), 240, 60));
        else
            StartCoroutine(uiManager.mUIFacade.ChangeSceneState(new MainSceneState(uiManager.mUIFacade), 0, 0));
    }

    public Canvas GetUICanvas()
    {
        return uiManager.mUIFacade.uiCanvas;
    }

    public GameObject CreateItem(GameObject itemGo)
    {
        GameObject go = Instantiate(itemGo);
        return go;
    }

    // ��ȡSprite��Դ
    public Sprite GetSprite(string resourcePath)
    {
        return factoryManager.spriteFactory.GetSingleResources(resourcePath);
    }

    public Sprite[] GetSprites(string resourcePath)
    {
        return Resources.LoadAll<Sprite>(resourcePath);
    }

    public AudioClip GetAudioClip(string resourcePath)
    {
        return factoryManager.audioClipFactory.GetSingleResources(resourcePath);
    }

    /// <summary>
    /// �첽�������ּ���
    /// </summary>
    /// <param name="resourcePath"></param>
    /// <returns></returns>
    public IEnumerator AsyncGetAudioClip(string resourcePath)
    {
        yield return StartCoroutine(factoryManager.audioClipFactory.AsyncLoadSingleResources(resourcePath));
    }

    public RuntimeAnimatorController GetRuntimeAnimatorController(string resourcePath)
    {
        return factoryManager.runtimeAnimatorControllerFactory.GetSingleResources(resourcePath);
    }

    // ��ȡ��Ϸ����
    public GameObject GetGameObjectResource(FactoryType factoryType, string resourcePath)
    {
        return factoryManager.factoryDict[factoryType].GetItem(resourcePath);
    }
    // ����Ϸ����Żض����
    public void PushGameObjectToFactory(FactoryType factoryType, string resourcePath, GameObject itemGo)
    {
        factoryManager.factoryDict[factoryType].PushItem(resourcePath, itemGo);
    }

    /// <summary>
    /// �����Ϸ���󹤳��Ķ���
    /// </summary>
    public void ClearGameObjectFactory(FactoryType factoryType)
    {
        factoryManager.factoryDict[factoryType].Clear();
    }

    // ������Ϸ���󹤳�����˵����һ������غ�һ��������ȡ����Ϸ����Ķ���أ������յ���Ϸ������Ƚ��뻺��أ���Ҫ�����������������صĶ����Ž�������Դ���
    public void PushGameObjectFromBufferToPool()
    {
        GameFactory f = (GameFactory)factoryManager.factoryDict[FactoryType.GameFactory];
        f.PushItemFromBuffer();
    }

    /// <summary>
    /// ��ȡ����
    /// </summary>
    /// <param name="resourcePath"></param>
    /// <returns></returns>
    public Material GetMaterial(string resourcePath)
    {
        return factoryManager.materialFactory.GetMaterial(resourcePath);
    }

    /// <summary>
    /// �첽���س���
    /// </summary>
    /// <returns></returns>
    public IEnumerator LoadSceneAsync(string name)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(name);
        while (!operation.isDone)
        {
            // ������û��û������
            startLoadPanel.AddFakeProgress();
            startLoadPanel.SetRealProgress(operation.progress / 9 * 10);
            startLoadPanel.transform.SetAsLastSibling();
            yield return null;
        }
    }

    /// <summary>
    /// ���뵽���ؽ���
    /// </summary>
    /// <returns></returns>
    public IEnumerator ChangeToStartLoadPanel(int time, Action OnStartLoadPanelAppearAction)
    {
        TopMask.gameObject.SetActive(true);
        for (int i = 0; i < time; i++)
        {
            TopMask.color = new Color(0, 0, 0, (float)i / (time-1));
            TopMask.transform.SetAsLastSibling();
            yield return null;
        }
        startLoadPanel.Appear();
        if (OnStartLoadPanelAppearAction != null)
            OnStartLoadPanelAppearAction();
        for (int i = 0; i < time; i++)
        {
            TopMask.color = new Color(0, 0, 0, 1 - (float)i / (time - 1));
            startLoadPanel.transform.SetAsLastSibling();
            TopMask.transform.SetAsLastSibling();
            yield return null;
        }
        TopMask.gameObject.SetActive(false);
    }

    /// <summary>
    /// �Ӽ��ؽ���ȥ
    /// </summary>
    /// <returns></returns>
    public IEnumerator StartLoadPanelDisappear(int time, Action OnStartLoadPanelDisappearAction)
    {
        TopMask.gameObject.SetActive(true);
        for (int i = 0; i < time; i++)
        {
            TopMask.color = new Color(0, 0, 0, (float)i / (time - 1));
            startLoadPanel.transform.SetAsLastSibling();
            TopMask.transform.SetAsLastSibling();
            yield return null;
        }
        startLoadPanel.Disappear();
        if (OnStartLoadPanelDisappearAction != null)
            OnStartLoadPanelDisappearAction();
        for (int i = 0; i < time; i++)
        {
            TopMask.color = new Color(0, 0, 0, 1 - (float)i / (time - 1));
            TopMask.transform.SetAsLastSibling();
            yield return null;
        }
        TopMask.gameObject.SetActive(false);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //GetFPs
        deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
        uiManager.mUIFacade.UpdatePanel();
        audioSourceManager.Update();
    }

    void OnGUI()
    {
        int w = Screen.width, h = Screen.height;
        GUIStyle style = new GUIStyle();
        

       {
            Rect rect = new Rect(0, h - h * 2 / 100, w, h * 2 / 100);
            style.alignment = TextAnchor.UpperLeft;
            style.fontSize = h * 2 / 100;
            style.normal.textColor = new Color(1, 1, 1, 1.0f);
            float msec = deltaTime * 1000.0f;
            float fps = 1.0f / deltaTime;
            string text = string.Format("{0:} ({1:0.} fps)", Application.version, fps);
            GUI.Label(rect, text, style);
        }

    }
}
