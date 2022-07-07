
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
        DontDestroyOnLoad(gameObject); // �г���ʱ������
        _instance = this;
        playerManager = new PlayerManager();
        factoryManager = new FactoryManager();
        audioSourceManager = new AudioSourceManager();
        // ����ConfigManager��Ŀǰ�����ý�������60֡
        configManager = new ConfigManager();
        abilityManager = AbilityManager.GetSingleton();
        uiManager = new UIManager();
        // ��ʼ����
        uiManager.mUIFacade.ChangeSceneState(new StartLoadSceneState(uiManager.mUIFacade));
        //Test
    }

    /// <summary>
    /// ����༭������
    /// </summary>
    public void EnterEditorScene()
    {
        uiManager.mUIFacade.ChangeSceneState(new EditorSceneState(uiManager.mUIFacade));
    }

    /// <summary>
    /// ����ս������
    /// </summary>
    public void EnterComBatScene()
    {
        uiManager.mUIFacade.ChangeSceneState(new GameNormalSceneState(uiManager.mUIFacade));
    }

    /// <summary>
    /// ����ѡ�����ó���
    /// </summary>
    public void EnterSelectScene()
    {
        uiManager.mUIFacade.ChangeSceneState(new SelectSceneState(uiManager.mUIFacade));
    }

    /// <summary>
    /// ������ҳ�泡��
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

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
