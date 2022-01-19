using System.Collections;
using System.Collections.Generic;
using UnityEditor.SceneManagement;

using UnityEngine;

public class GameManager : MonoBehaviour
{
    public PlayerManager playerManager;
    public FactoryManager factoryManager;
    public AudioSourceManager audioSourceManager;
    public UIManager uiManager;

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
        //uiManager = new UIManager();
        //uiManager.mUIFacade.currentSceneState.EnterScene();
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


    public void RecycleUnit(BaseUnit unit, string resourcePath)
    {
        PushGameObjectToFactory(FactoryType.GameFactory, resourcePath, unit.gameObject);
        unit.isGameObjectValid = false;
    }

    public void RecycleBullet(BaseBullet unit, string resourcePath)
    {
        PushGameObjectToFactory(FactoryType.GameFactory, resourcePath, unit.gameObject);
        unit.isGameObjectValid = false;
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
