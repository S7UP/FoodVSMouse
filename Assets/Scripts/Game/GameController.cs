using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

using static UnityEngine.Networking.UnityWebRequest;

public class GameController : MonoBehaviour
{
    private static GameController _instance;

    // ����
    public static GameController Instance { get => _instance; } //+ ������
    // �ɱ༭������������
    public GameObject enemyListGo; // ���ڴ�ŵжԵ�λ�ĸ�����
    public GameObject allyListGo; // ���ڴ���ѷ���λ�ĸ�����


    //BaseStage mCurrentStage; //+ ��ǰ�ؿ�
    //BaseCostController mCostController; //+ ���ÿ�����
    //BaseCardController mCardController; //+ ��Ƭ������
    //BaseSkillController mSkillController; //+ ���ܿ�����
    //BaseProgressController mProgressController; //+ ��Ϸ���ȿ�����
    //BaseGrid[] mGridList; //+ ���ӱ�
    List<BaseUnit> mEnemyList; //+ ���ĵз���λ��
    //BaseRule[] mRuleList; //+ �����
    //KeyBoardSetting mKeyBoardSetting; //+ ��λ���ƽӿ�
    //Recorder mRecorder; //+ �û�������¼��

    private int mFrameNum; //+ ��ǰ��Ϸ֡

    // ����˽�й��캯����ʹ��粻�ܴ�������ʵ��
    private GameController()
    {

    }

    //+ ��ͣ�ķ���
    public void Pause()
    {

    }

    //+ �����ͣ�ķ���
    public void Resume()
    {

    }

    private void Awake()
    {
        _instance = this;
        mFrameNum = 0;
        mEnemyList = new List<BaseUnit>();

        // ����ConfigManager��Ŀǰ�����ý�������60֡
        new ConfigManager();
    }


    // Start is called before the first frame update
    void Start()
    {
        // testing
        // ��ʳ����
        {
            GameObject instance = (GameObject)Instantiate(Resources.Load("Prefabs/Food/Pre_Food"));
            instance.transform.SetParent(allyListGo.transform);
            instance.transform.position = new Vector3(-1.42f, 0.94f, 0);
            FoodUnit food = new FoodUnit(instance);
            food.Init();
            // ����ʱ����һ�µжԵ�λ�������~
            mEnemyList.Add(food);
        }
        // ��������
        for (int i = 0; i < 2; i++)
        {
            GameObject instance = (GameObject)Instantiate(Resources.Load("Prefabs/Mouse/Pre_HugeMouse"));
            instance.transform.SetParent(enemyListGo.transform);
            instance.transform.position = new Vector3(3.73f, 1.452f - 0.65f*2*i, 0);
            MouseUnit mouse = new MouseUnit(instance);
            mouse.Init();
            mEnemyList.Add(mouse);
            // ������Ӵ�����ײ�¼������(�̳���MonoBehaviour)
            MouseCollision mouseCollision = instance.AddComponent<MouseCollision>();
            mouseCollision.mouseUnit = mouse; // ע���Ӧ��MouseUnit
        }

    }

    // Update is called once per frame
    void Update()
    {
        mFrameNum++;
        // Debug.Log("Frame:"+mFrameNum);
        // ���������Update()
        foreach (BaseUnit unit in mEnemyList)
        {
            unit.Update();
        }
        // 
    }

    public void OnTriggerEnter(Collider other)
    {
        
    }
}
