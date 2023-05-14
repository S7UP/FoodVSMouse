
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
    public StartLoadPanel startLoadPanel; // 由编辑器赋值吧

    private static GameManager _instance;

    public static GameManager Instance { get => _instance; }

    private float deltaTime = 0.0f;

    private Image TopMask; // 最上层黑色遮罩

    private void Awake()
    {
        DontDestroyOnLoad(gameObject); // 切场景时不销毁
        _instance = this;
        rand = new System.Random();
        playerManager = new PlayerManager();
        factoryManager = new FactoryManager();
        audioSourceManager = new AudioSourceManager();
        configManager = new ConfigManager(); // 加载ConfigManager，导入玩家的设置
        abilityManager = AbilityManager.GetSingleton();
        uiManager = new UIManager();
        attributeManager = new AttributeManager();
        attributeManager.Initial();
        startLoadPanel = GameObject.Find("Canvas").transform.Find("StartLoadPanel").GetComponent<StartLoadPanel>();
        startLoadPanel.gameObject.SetActive(false);
        TopMask = GameObject.Find("Canvas").transform.Find("TopMask").GetComponent<Image>();
        TopMask.gameObject.SetActive(false);

        // 加载玩家数据(静态)
        playerData = PlayerData.GetInstance();
        // 初始场景
        // StartCoroutine(uiManager.mUIFacade.ChangeSceneState(new StartLoadSceneState(uiManager.mUIFacade)));
        Load();
        //Test
    }

    /// <summary>
    /// 在Loading界面加载的东西
    /// </summary>
    public void Load()
    {
        MusicManager.Load();
        MouseManager.LoadAll();
        TipsManager.Load();
        TagsManager.Load();
    }

    /// <summary>
    /// 第一次进入游戏主界面前就要加载好的内容
    /// </summary>
    /// <returns></returns>
    public IEnumerator LoadWhenBeforeEnterMainScene()
    {
        yield return SoundsManager.Load();
    }

    /// <summary>
    /// 进入编辑器场景
    /// </summary>
    public void EnterEditorScene()
    {
        StartCoroutine(uiManager.mUIFacade.ChangeSceneState(new EditorSceneState(uiManager.mUIFacade), 0, 0));
    }

    /// <summary>
    /// 进入战斗场景
    /// </summary>
    public void EnterComBatScene()
    {
        StartCoroutine(uiManager.mUIFacade.ChangeSceneState(new GameNormalSceneState(uiManager.mUIFacade), 240, 60));
    }

    /// <summary>
    /// 进入美味镇场景
    /// </summary>
    public void EnterTownScene()
    {
        if(uiManager.mUIFacade.currentSceneState != null && uiManager.mUIFacade.currentSceneState is GameNormalSceneState)
            StartCoroutine(uiManager.mUIFacade.ChangeSceneState(new TownSceneState(uiManager.mUIFacade), 240, 60));
        else
            StartCoroutine(uiManager.mUIFacade.ChangeSceneState(new TownSceneState(uiManager.mUIFacade), 0, 0));
    }

    /// <summary>
    /// 进入选择配置场景
    /// </summary>
    public void EnterSelectScene()
    {
        if (uiManager.mUIFacade.currentSceneState != null && uiManager.mUIFacade.currentSceneState is GameNormalSceneState)
            StartCoroutine(uiManager.mUIFacade.ChangeSceneState(new SelectSceneState(uiManager.mUIFacade), 240, 60));
        else
            StartCoroutine(uiManager.mUIFacade.ChangeSceneState(new SelectSceneState(uiManager.mUIFacade), 0, 0));
    }

    /// <summary>
    /// 进入主页面场景
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

    // 获取Sprite资源
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
    /// 异步加载音乐剪辑
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

    // 获取游戏物体
    public GameObject GetGameObjectResource(FactoryType factoryType, string resourcePath)
    {
        return factoryManager.factoryDict[factoryType].GetItem(resourcePath);
    }
    // 将游戏物体放回对象池
    public void PushGameObjectToFactory(FactoryType factoryType, string resourcePath, GameObject itemGo)
    {
        factoryManager.factoryDict[factoryType].PushItem(resourcePath, itemGo);
    }

    /// <summary>
    /// 清空游戏对象工厂的对象
    /// </summary>
    public void ClearGameObjectFactory(FactoryType factoryType)
    {
        factoryManager.factoryDict[factoryType].Clear();
    }

    // 对于游戏对象工厂的来说，有一个缓冲池和一个真正的取回游戏对象的对象池，被回收的游戏对象会先进入缓冲池，需要调用这个方法将缓冲池的东西放进对象池以待命
    public void PushGameObjectFromBufferToPool()
    {
        GameFactory f = (GameFactory)factoryManager.factoryDict[FactoryType.GameFactory];
        f.PushItemFromBuffer();
    }

    /// <summary>
    /// 获取材质
    /// </summary>
    /// <param name="resourcePath"></param>
    /// <returns></returns>
    public Material GetMaterial(string resourcePath)
    {
        return factoryManager.materialFactory.GetMaterial(resourcePath);
    }

    /// <summary>
    /// 异步加载场景
    /// </summary>
    /// <returns></returns>
    public IEnumerator LoadSceneAsync(string name)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(name);
        while (!operation.isDone)
        {
            // 当场景没还没加载完
            startLoadPanel.AddFakeProgress();
            startLoadPanel.SetRealProgress(operation.progress / 9 * 10);
            startLoadPanel.transform.SetAsLastSibling();
            yield return null;
        }
    }

    /// <summary>
    /// 渐入到加载界面
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
    /// 从加载渐出去
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
