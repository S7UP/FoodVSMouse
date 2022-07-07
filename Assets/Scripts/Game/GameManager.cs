
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public PlayerManager playerManager;
    public FactoryManager factoryManager;
    public AudioSourceManager audioSourceManager;
    public UIManager uiManager;
    public ConfigManager configManager;
    public AbilityManager abilityManager;

    private static GameManager _instance;

    public static GameManager Instance { get => _instance; }

    // public Stage currentStage;



    private void Awake()
    {
        DontDestroyOnLoad(gameObject); // 切场景时不销毁
        _instance = this;
        playerManager = new PlayerManager();
        factoryManager = new FactoryManager();
        audioSourceManager = new AudioSourceManager();
        // 加载ConfigManager，目前的作用仅仅是锁60帧
        configManager = new ConfigManager();
        abilityManager = AbilityManager.GetSingleton();
        uiManager = new UIManager();
        // 初始场景
        uiManager.mUIFacade.ChangeSceneState(new StartLoadSceneState(uiManager.mUIFacade));
        //Test
    }

    /// <summary>
    /// 进入编辑器场景
    /// </summary>
    public void EnterEditorScene()
    {
        uiManager.mUIFacade.ChangeSceneState(new EditorSceneState(uiManager.mUIFacade));
    }

    /// <summary>
    /// 进入战斗场景
    /// </summary>
    public void EnterComBatScene()
    {
        uiManager.mUIFacade.ChangeSceneState(new GameNormalSceneState(uiManager.mUIFacade));
    }

    /// <summary>
    /// 进入选择配置场景
    /// </summary>
    public void EnterSelectScene()
    {
        uiManager.mUIFacade.ChangeSceneState(new SelectSceneState(uiManager.mUIFacade));
    }

    /// <summary>
    /// 进入主页面场景
    /// </summary>
    public void EnterMainScene()
    {
        uiManager.mUIFacade.ChangeSceneState(new MainSceneState(uiManager.mUIFacade));
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

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
