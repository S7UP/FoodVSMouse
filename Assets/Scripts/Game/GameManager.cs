using System.Collections;
using System.Collections.Generic;
using System.IO;

using UnityEditor.SceneManagement;

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
        Debug.Log("GameManager Awake!");
        DontDestroyOnLoad(gameObject); // �г���ʱ������
        _instance = this;
        playerManager = new PlayerManager();
        factoryManager = new FactoryManager();
        audioSourceManager = new AudioSourceManager();
        // ����ConfigManager��Ŀǰ�����ý�������60֡
        configManager = new ConfigManager();
        abilityManager = AbilityManager.GetSingleton();

        uiManager = new UIManager();
        //EnterEditorScene();
        EnterComBatScene();
    }

    /// <summary>
    /// ����༭������
    /// </summary>
    public void EnterEditorScene()
    {
        uiManager.mUIFacade.currentSceneState = new EditorSceneState(uiManager.mUIFacade);
        uiManager.mUIFacade.currentSceneState.EnterScene();
    }

    /// <summary>
    /// ����ս������
    /// </summary>
    public void EnterComBatScene()
    {
        uiManager.mUIFacade.currentSceneState = new GameNormalSceneState(uiManager.mUIFacade);
        uiManager.mUIFacade.currentSceneState.EnterScene();
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
