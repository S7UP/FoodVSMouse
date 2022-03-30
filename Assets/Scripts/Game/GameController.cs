using LitJson;

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using UnityEngine;
using UnityEngine.Profiling;

using static UnityEditor.PlayerSettings;
using static UnityEngine.Networking.UnityWebRequest;
using static UnityEngine.UI.CanvasScaler;

public class GameController : MonoBehaviour
{
    private static GameController _instance;

    // 引用
    public static GameController Instance { get => _instance; } //+ 自身单例
    public BaseGrid overGrid; // 当前鼠标悬停的格子
    protected BaseUnit.Attribute baseAttribute;
    protected FoodUnit.Attribute foodAttribute;
    protected MouseUnit.Attribute mouseAttribute;


    // 由编辑器给定的引用
    public GameObject gridListGo; // 用于存放地图格子的对象

    // 引用
    private GameObject[] enemyListGo; // 用于存放敌对单位的父对象
    private GameObject[] allyListGo; // 用于存放友方单位的父对象

    public BaseStage mCurrentStage; //+ 当前关卡
    public BaseCostController mCostController; //+ 费用控制器
    public BaseCardController mCardController; //+ 卡片建造器
    public BaseSkillController mSkillController; //+ 技能控制器
    public BaseProgressController mProgressController; //+ 游戏进度控制器
    public BaseGrid[,] mGridList; //+ 格子表
    public List<BaseUnit>[] mEnemyList; //+ 存活的敌方单位表
    public List<BaseUnit>[] mAllyList; // 存活的友方单位表
    public List<BaseBullet> mBulletList; // 存活的子弹表
    public List<AbilityExecution> abilityExecutionList; // 存活的能力执行体（非对象）
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

        MMemberList = new List<IGameControllerMember>();
        // 获取当前场景的UIPanel
        GameNormalPanel panel = (GameNormalPanel)GameManager.Instance.uiManager.mUIFacade.currentScenePanelDict[StringManager.GameNormalPanel];
        // 从UIPanel中获取各种控制器脚本
        mCostController = panel.transform.Find("CostControllerUI").GetComponent<BaseCostController>();
        MMemberList.Add(mCostController);
        mCardController = panel.transform.Find("CardControllerUI").GetComponent<BaseCardController>();
        MMemberList.Add(mCardController);
        mProgressController = panel.transform.Find("ProgressControllerUI").GetComponent<BaseProgressController>();
        MMemberList.Add(mProgressController);


        _instance = this;
        mFrameNum = 0;
        mEnemyList = new List<BaseUnit>[7];
        enemyListGo = new GameObject[mEnemyList.Length];
        for (int i = 0; i < mEnemyList.Length; i++)
        {
            mEnemyList[i] = new List<BaseUnit>();
            GameObject go = new GameObject("i");
            enemyListGo[i] = go;
            go.transform.SetParent(GameObject.Find("EnemyList").transform);
        }

        mAllyList = new List<BaseUnit>[7];
        allyListGo = new GameObject[mAllyList.Length];
        for (int i = 0; i < mAllyList.Length; i++)
        {
            mAllyList[i] = new List<BaseUnit>();
            GameObject go = new GameObject("i");
            allyListGo[i] = go;
            go.transform.SetParent(GameObject.Find("AllyList").transform);
        }
        mBulletList = new List<BaseBullet>();
        isPause = false;

        // 生成场地格子
        mGridList = new BaseGrid[MapMaker.yRow, MapMaker.xColumn];
        for (int i = 0; i < MapMaker.yRow; i++)
        {
            for (int j = 0; j < MapMaker.xColumn; j++)
            {
                mGridList[i, j] = GameManager.Instance.GetGameObjectResource(FactoryType.GameFactory, "Grid/Grid").GetComponent<BaseGrid>();
                mGridList[i, j].transform.SetParent(gridListGo.transform);
                mGridList[i, j].InitGrid(j, i);
                mGridList[i, j].MInit();
            }
        }

        abilityExecutionList = new List<AbilityExecution>();
        //FoodUnit.SaveNewFoodInfo();
        //MouseUnit.SaveNewMouseInfo(); // 保存当前老鼠信息

        //FoodUnit.Attribute attr = new FoodUnit.Attribute()
        //{
        //    baseAttrbute = new BaseUnit.Attribute()
        //    {
        //        name = "终结者酒架", // 单位的具体名称
        //        type = 7, // 单位属于的分类
        //        shape = 2, // 单位在当前分类的变种编号

        //        baseHP = 50, // 基础血量
        //        baseAttack = 10, // 基础攻击
        //        baseAttackSpeed = 0.85, // 基础攻击速度
        //        attackPercent = 0.5,
        //        baseHeight = 0, // 基础高度
        //    },
        //    foodType = FoodType.Shooter
        //};

        //Debug.Log("开始存档美食信息！");
        //JsonManager.Save(attr, "Food/" + attr.baseAttrbute.type + "/" + attr.baseAttrbute.shape + "");
        //Debug.Log("美食信息存档完成！");

        //Debug.Log("开始读取美食信息！");
        //foodAttribute = JsonManager.Load<FoodUnit.Attribute>("Food/" + attr.baseAttrbute.type + "/" + attr.baseAttrbute.shape + "");
        //Debug.Log("name=" + foodAttribute.baseAttrbute.name);
        //Debug.Log("读取美食信息成功！");

        // stage test
        mCurrentStage = new BaseStage();
        //mCurrentStage.Save();
        mCurrentStage.DemoLoad();
        mCurrentStage.Init();

        Test.OnGameControllerAwake();
    }

    /// <summary>
    /// 在通知卡片控制器创建卡片之前，先让系统知道下一个要建造卡片的属性
    /// </summary>
    public void SetFoodAttribute(FoodUnit.Attribute attr)
    {
        foodAttribute = attr;
        baseAttribute = foodAttribute.baseAttrbute;
    }

    public void SetMouseAttribute(MouseUnit.Attribute attr)
    {
        mouseAttribute = attr;
        baseAttribute = mouseAttribute.baseAttrbute;
    }

    /// <summary>
    /// 获取基础属性，然后用来初始化对象
    /// </summary>
    /// <returns></returns>
    public BaseUnit.Attribute GetBaseAttribute()
    {
        return baseAttribute;
    }

    public FoodUnit.Attribute GetFoodAttribute()
    {
        return foodAttribute;
    }

    public MouseUnit.Attribute GetMouseAttribute()
    {
        return mouseAttribute;
    }

    /// <summary>
    /// 获取当前火苗数
    /// </summary>
    /// <returns></returns>
    public float GetFire()
    {
        return mCostController.GetCost("Fire");
    }


    /// <summary>
    /// 获取当前鼠标悬停的格子
    /// </summary>
    public BaseGrid GetOverGrid()
    {
        return overGrid;
    }

    // Start is called before the first frame update
    void Start()
    {
        // 
        foreach (IGameControllerMember member in MMemberList)
        {
            member.MInit();
        }

        // 关卡的刷怪逻辑使用协程
        StartCoroutine(mCurrentStage.Start());
    }

    // 添加一个美食单位至战场中（管理）
    public FoodUnit AddFoodUnit(FoodUnit food, int yIndex)
    {
        food.transform.SetParent(allyListGo[yIndex].transform);
        food.UpdateRenderLayer(mAllyList[yIndex].Count);
        mAllyList[yIndex].Add(food);
        return food;
    }

    /// <summary>
    /// 产生老鼠单位
    /// </summary>
    /// <param name="xIndex">格子横坐标</param>
    /// <param name="yIndex">格子纵坐标</param>
    /// <param name="enemyInfo">老鼠种类与形态编号信息表，即决定要产生什么老鼠</param>
    /// <returns></returns>
    public MouseUnit CreateMouseUnit(int xIndex, int yIndex, BaseEnemyGroup.EnemyInfo enemyInfo)
    {
        SetMouseAttribute(JsonManager.Load<MouseUnit.Attribute>("Mouse/"+enemyInfo.type+"/"+enemyInfo.shape+"")); // 准备先持有要创建实例的初始化信息
        //MouseUnit mouse = GameManager.Instance.GetGameObjectResource(FactoryType.GameFactory, "Mouse/Pre_Mouse").GetComponent<MouseUnit>();
        MouseUnit mouse = GameManager.Instance.GetGameObjectResource(FactoryType.GameFactory, "Mouse/"+enemyInfo.type).GetComponent<MouseUnit>();
        mouse.transform.SetParent(enemyListGo[yIndex].transform);
        mouse.MInit();
        mGridList[yIndex, xIndex].SetMouseUnitInGrid(mouse, Vector2.right);
        mouse.UpdateRenderLayer(mEnemyList[yIndex].Count);
        mEnemyList[yIndex].Add(mouse);
        return mouse;
    }

    /// <summary>
    /// 默认在最右侧刷老鼠
    /// </summary>
    /// <param name="yIndex"></param>
    /// <returns></returns>
    public MouseUnit CreateMouseUnit(int yIndex, BaseEnemyGroup.EnemyInfo enemyInfo)
    {
        return CreateMouseUnit(MapMaker.xColumn - 1, yIndex, enemyInfo);
    }

    public BaseBullet CreateBullet(BaseUnit master, Vector3 position)
    {
        GameObject instance = GameManager.Instance.GetGameObjectResource(FactoryType.GameFactory, "Bullet/Pre_Bullet");
        BaseBullet bullet = instance.GetComponent<BaseBullet>();
        bullet.mMasterBaseUnit = master;
        bullet.MInit();
        bullet.transform.position = position;
        mBulletList.Add(bullet);
        return bullet;
    }

    // 产生其他单位
    public void CreateOtherUnit()
    {

    }

    /// <summary>
    /// 在GameController协程中，产生一个等待若干帧的指令
    /// </summary>
    /// <returns></returns>
    public IEnumerator WaitForIEnumerator(int time)
    {
        for (int i = 0; i < time; i++)
        {
            yield return null;
        }
    }

    /// <summary>
    /// 获取当前关卡进行帧
    /// </summary>
    public int GetCurrentStageFrame()
    {
        return mFrameNum;
    }

    /// <summary>
    /// 获取当前场景敌人表
    /// </summary>
    /// <returns></returns>
    public List<BaseUnit>[] GetEnemyList()
    {
        return mEnemyList;
    }


    /// <summary>
    /// 获取特定行的敌人
    /// </summary>
    /// <returns></returns>
    public List<BaseUnit> GetSpecificRowEnemyList(int i)
    {
        return mEnemyList[i];
    }

    /// <summary>
    /// 获取全部的敌人
    /// </summary>
    /// <returns></returns>
    public List<BaseUnit> GetEachEnemy()
    {
        List<BaseUnit> list = new List<BaseUnit>();
        for (int i = 0; i < mEnemyList.Length; i++)
        {
            foreach (var item in GetSpecificRowEnemyList(i))
            {
                list.Add(item);
            }
        }
        return list;
    }

    /// <summary>
    /// 获取当前场景友军表
    /// </summary>
    /// <returns></returns>
    public List<BaseUnit>[] GetAllyList()
    {
        return mAllyList;
    }


    /// <summary>
    /// 获取特定行的友军
    /// </summary>
    /// <returns></returns>
    public List<BaseUnit> GetSpecificRowAllyList(int i)
    {
        return mAllyList[i];
    }

    /// <summary>
    /// 获取全部的友军
    /// </summary>
    /// <returns></returns>
    public List<BaseUnit> GetEachAlly()
    {
        List<BaseUnit> list = new List<BaseUnit>();
        for (int i = 0; i < mAllyList.Length; i++)
        {
            foreach (var item in GetSpecificRowAllyList(i))
            {
                list.Add(item);
            }
        }
        return list;
    }

    // Update is called once per frame
    void Update()
    {
        mFrameNum ++; // 先更新游戏帧，以保证Update里面与协程接收到的帧是相同的
        // 各种组件的Update()
        foreach (IGameControllerMember member in MMemberList)
        {
            member.MUpdate();
        }
        // 各大能力执行体更新
        foreach (var item in abilityExecutionList)
        {
            item.Update();
        }

        // 所有格子帧逻辑
        for (int i = 0; i < MapMaker.yRow; i++)
        {
            for (int j = 0; j < MapMaker.xColumn; j++)
            {
                mGridList[i, j].MUpdate();
            }
        }
        // 敌人帧逻辑
        foreach (var item in mEnemyList)
        {
            bool flag = false;
            for (int i = 0; i < item.Count; i++)
            {
                BaseUnit unit = item[i];
                if (unit.IsValid())
                {
                    unit.MUpdate();
                }
                else
                {
                    i--;
                    item.Remove(unit);
                    flag = true;
                }
            }
            if(flag)
                for (int i = 0; i < item.Count; i++)
                {
                    BaseUnit unit = item[i];
                    unit.UpdateRenderLayer(i);
                }
        }
        // 友方帧逻辑
        foreach (var item in mAllyList)
        {
            bool flag = false;
            for (int i = 0; i < item.Count; i++)
            {
                BaseUnit unit = item[i];
                if (unit.IsValid())
                {
                    unit.MUpdate();
                }
                else
                {
                    i--;
                    item.Remove(unit);
                    flag = true;
                }
            }
            if (flag)
                for (int i = 0; i < item.Count; i++)
                {
                    BaseUnit unit = item[i];
                    unit.UpdateRenderLayer(i);
                }
        }

        // 子弹帧逻辑
        for (int i = 0; i < mBulletList.Count; i++)
        {
            BaseBullet unit = mBulletList[i];
            if (unit.isActiveAndEnabled)
            {
                unit.MUpdate();
            }
            else
            {
                i--;
                mBulletList.Remove(unit);
            }
        }

        // 将缓冲池的游戏对象放回到对象池
        GameManager.Instance.PushGameObjectFromBufferToPool();
    }
}
