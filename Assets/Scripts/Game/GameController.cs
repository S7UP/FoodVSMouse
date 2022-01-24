using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

using static UnityEngine.Networking.UnityWebRequest;
using static UnityEngine.UI.CanvasScaler;

public class GameController : MonoBehaviour
{
    private static GameController _instance;

    // 引用
    public static GameController Instance { get => _instance; } //+ 自身单例
    // 由编辑器给定的引用
    public GameObject enemyListGo; // 用于存放敌对单位的父对象
    public GameObject allyListGo; // 用于存放友方单位的父对象


    public BaseStage mCurrentStage; //+ 当前关卡
    public BaseCostController mCostController; //+ 费用控制器
    public BaseCardController mCardController; //+ 卡片建造器
    public BaseSkillController mSkillController; //+ 技能控制器
    public BaseProgressController mProgressController; //+ 游戏进度控制器
    public BaseGrid[,] mGridList; //+ 格子表
    public List<BaseUnit> mEnemyList; //+ 存活的敌方单位表
    public List<BaseUnit> mAllyList; // 存活的友方单位表
    public List<BaseBullet> mBulletList; // 存活的子弹表
    //BaseRule[] mRuleList; //+ 规则表
    //KeyBoardSetting mKeyBoardSetting; //+ 键位控制接口
    //Recorder mRecorder; //+ 用户操作记录者

    private int mFrameNum; //+ 当前游戏帧
    public bool isPause;

    // 所有需要放到GameController管理Init、Update的东西
    protected List<IGameControllerMember> MMemberList;

    // 场景内引用
    public GameObject uICanvasGo;

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
        uICanvasGo = GameObject.Find("UICanvas");
        Debug.Log(uICanvasGo);

        MMemberList = new List<IGameControllerMember>();
        mCardController = new BaseCardController();
        MMemberList.Add(mCardController);

        _instance = this;
        mFrameNum = 0;
        mEnemyList = new List<BaseUnit>();
        mAllyList = new List<BaseUnit>();
        mBulletList = new List<BaseBullet>();
        isPause = true;
    }


    // Start is called before the first frame update
    void Start()
    {
        // 
        foreach (IGameControllerMember member in MMemberList)
        {
            member.MInit();
        }

        // testing
        // 美食生成
        {
            BaseUnit unit = CreateFoodUnit(new Vector3(-1.42f, 0.94f, 0));
        }
        // 老鼠生成
        for (int i = 0; i < 2; i++)
        {
            MouseUnit unit = CreateMouseUnit(new Vector3(3.73f, 1.452f - 0.65f * 2 * i, 0));
        }

    }

    // 产生美食单位
    public FoodUnit CreateFoodUnit(Vector3 position)
    {
        GameObject instance = GameManager.Instance.GetGameObjectResource(FactoryType.GameFactory, "Food/Pre_Food");
        instance.transform.SetParent(allyListGo.transform);
        FoodUnit food = instance.GetComponent<FoodUnit>();
        food.MInit();
        mAllyList.Add(food);
        food.SetPosition(position);
        return food;
    }

    // 产生老鼠单位
    public MouseUnit CreateMouseUnit(Vector3 position)
    {
        GameObject instance = GameManager.Instance.GetGameObjectResource(FactoryType.GameFactory, "Mouse/Pre_HugeMouse");
        instance.transform.SetParent(enemyListGo.transform);
        MouseUnit mouse = instance.GetComponent<MouseUnit>();
        mouse.MInit();
        mEnemyList.Add(mouse);
        // 在更改游戏对象层级关系时，使用刚体设置位置会失效，即以下注释代码，就是上面那个SetParent，因此只能强改transform
        //unit.SetPosition(new Vector3(3.73f, 1.452f - 0.65f * 2 * i, 0));
        mouse.transform.position = position;
        return mouse;
    }

    public BaseBullet CreateBullet(Vector3 position)
    {
        GameObject instance = GameManager.Instance.GetGameObjectResource(FactoryType.GameFactory, "Bullet/Pre_Bullet");
        BaseBullet bullet = instance.GetComponent<BaseBullet>();
        bullet.MInit();
        bullet.transform.position = position;
        mBulletList.Add(bullet);
        return bullet;
    }

    // 产生其他单位
    public void CreateOtherUnit()
    {

    }


    // Update is called once per frame
    void Update()
    {
        mFrameNum++;
        // Debug.Log("Frame:"+mFrameNum);
        // 各种组件的Update()
        foreach(IGameControllerMember member in MMemberList)
        {
            member.MUpdate();
        }

        for (int i = 0; i < mEnemyList.Count; i++)
        {
            BaseUnit unit = mEnemyList[i];
            if (unit.isGameObjectValid)
            {
                unit.MUpdate();
            }
            else
            {
                i--;
                Debug.Log("remove enemy");
                mEnemyList.Remove(unit);
            }
        }
        for (int i = 0; i < mAllyList.Count; i++)
        {
            BaseUnit unit = mAllyList[i];
            if (unit.isGameObjectValid)
            {
                unit.MUpdate();
            }
            else
            {
                i--;
                Debug.Log("remove ally");
                mAllyList.Remove(unit);
            }
        }
        for (int i = 0; i < mBulletList.Count; i++)
        {
            BaseBullet unit = mBulletList[i];
            if (unit.isGameObjectValid)
            {
                unit.MUpdate();
            }
            else
            {
                i--;
                Debug.Log("remove bullet");
                mBulletList.Remove(unit);
            }
        }
        //foreach (BaseUnit unit in mEnemyList)
        //{
        //    if (unit.isGameObjectValid)
        //    {
        //        unit.MUpdate();
        //    }
        //    else
        //    {
        //        Debug.Log("remove enemy");
        //        mEnemyList.Remove(unit);
        //    }
        //}
        //foreach (BaseUnit unit in mAllyList)
        //{
        //    if (unit.isGameObjectValid)
        //    {
        //        unit.MUpdate();
        //    }
        //    else
        //    {
        //        Debug.Log("remove ally");
        //        mAllyList.Remove(unit);
        //    }
        //}
        // 
    }
}
