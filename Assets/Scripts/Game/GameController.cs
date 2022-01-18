using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

using static UnityEngine.Networking.UnityWebRequest;

public class GameController : MonoBehaviour
{
    private static GameController _instance;

    // 引用
    public static GameController Instance { get => _instance; } //+ 自身单例
    // 由编辑器给定的引用
    public GameObject enemyListGo; // 用于存放敌对单位的父对象
    public GameObject allyListGo; // 用于存放友方单位的父对象


    //BaseStage mCurrentStage; //+ 当前关卡
    //BaseCostController mCostController; //+ 费用控制器
    //BaseCardController mCardController; //+ 卡片建造器
    //BaseSkillController mSkillController; //+ 技能控制器
    //BaseProgressController mProgressController; //+ 游戏进度控制器
    //BaseGrid[] mGridList; //+ 格子表
    List<BaseUnit> mEnemyList; //+ 存活的敌方单位表
    //BaseRule[] mRuleList; //+ 规则表
    //KeyBoardSetting mKeyBoardSetting; //+ 键位控制接口
    //Recorder mRecorder; //+ 用户操作记录者

    private int mFrameNum; //+ 当前游戏帧

    // 定义私有构造函数，使外界不能创建该类实例
    private GameController()
    {

    }

    //+ 暂停的方法
    public void Pause()
    {

    }

    //+ 解除暂停的方法
    public void Resume()
    {

    }

    private void Awake()
    {
        _instance = this;
        mFrameNum = 0;
        mEnemyList = new List<BaseUnit>();

        // 加载ConfigManager，目前的作用仅仅是锁60帧
        new ConfigManager();
    }


    // Start is called before the first frame update
    void Start()
    {
        // testing
        // 美食生成
        {
            GameObject instance = (GameObject)Instantiate(Resources.Load("Prefabs/Food/Pre_Food"));
            instance.transform.SetParent(allyListGo.transform);
            instance.transform.position = new Vector3(-1.42f, 0.94f, 0);
            FoodUnit food = new FoodUnit(instance);
            food.Init();
            // 测试时借用一下敌对单位表处理更新~
            mEnemyList.Add(food);
        }
        // 老鼠生成
        for (int i = 0; i < 2; i++)
        {
            GameObject instance = (GameObject)Instantiate(Resources.Load("Prefabs/Mouse/Pre_HugeMouse"));
            instance.transform.SetParent(enemyListGo.transform);
            instance.transform.position = new Vector3(3.73f, 1.452f - 0.65f*2*i, 0);
            MouseUnit mouse = new MouseUnit(instance);
            mouse.Init();
            mEnemyList.Add(mouse);
            // 程序添加处理碰撞事件的组件(继承自MonoBehaviour)
            MouseCollision mouseCollision = instance.AddComponent<MouseCollision>();
            mouseCollision.mouseUnit = mouse; // 注入对应的MouseUnit
        }

    }

    // Update is called once per frame
    void Update()
    {
        mFrameNum++;
        // Debug.Log("Frame:"+mFrameNum);
        // 各种组件的Update()
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
