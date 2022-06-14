using LitJson;

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using UnityEngine;
using UnityEngine.Profiling;

using static BaseEnemyGroup;
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
    public Camera mCamera;

    // 引用
    private GameObject[] enemyListGo; // 用于存放敌对单位的父对象
    private GameObject[] allyListGo; // 用于存放友方单位的父对象
    private GameObject[] itemListGo; // 用于存放地图物品单位的父对象

    public BaseStage mCurrentStage; //+ 当前关卡
    public BaseCostController mCostController; //+ 费用控制器
    public BaseCardController mCardController; //+ 卡片建造器
    public BaseSkillController mSkillController; //+ 技能控制器
    public BaseProgressController mProgressController; //+ 游戏进度控制器
    public MapController mMapController; // 格子控制器
    public List<BaseUnit>[] mEnemyList; //+ 存活的敌方单位表
    public List<BaseUnit>[] mAllyList; // 存活的友方单位表
    public List<BaseUnit>[] mItemList; // 存活的道具单位表
    public List<BaseBullet> mBulletList; // 存活的子弹表
    public List<AreaEffectExecution> areaEffectExecutionList; // 存活的能力执行体（对象）
    public List<BaseEffect> baseEffectList; // 存活的特效表
    public List<Tasker> taskerList; // 存活的任务执行器表
    //BaseRule[] mRuleList; //+ 规则表
    //KeyBoardSetting mKeyBoardSetting; //+ 键位控制接口
    //Recorder mRecorder; //+ 用户操作记录者
    public NumberManager numberManager;

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
        mMapController = GameObject.Find("MapController").GetComponent<MapController>();
        MMemberList.Add(mMapController);


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

        mItemList = new List<BaseUnit>[7];
        itemListGo = new GameObject[mItemList.Length];
        for (int i = 0; i < mItemList.Length; i++)
        {
            mItemList[i] = new List<BaseUnit>();
            GameObject go = new GameObject("i");
            itemListGo[i] = go;
            go.transform.SetParent(GameObject.Find("ItemList").transform);
        }

        mBulletList = new List<BaseBullet>();

        isPause = false;

        areaEffectExecutionList = new List<AreaEffectExecution>();
        baseEffectList = new List<BaseEffect>();
        taskerList = new List<Tasker>();

        // stage test
        mCurrentStage = new BaseStage();
        //mCurrentStage.Save();
        mCurrentStage.DemoLoad();
        mCurrentStage.Init();

        // 加载数值管理器
        numberManager = new NumberManager();

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
    /// 同上，不过是设置障碍的
    /// </summary>
    public void SetBarrierAttribute(BaseUnit.Attribute attr)
    {
        baseAttribute = attr;
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
        MouseUnit mouse = GameManager.Instance.GetGameObjectResource(FactoryType.GameFactory, "Mouse/"+enemyInfo.type).GetComponent<MouseUnit>();
        mouse.transform.SetParent(enemyListGo[yIndex].transform);
        mouse.MInit();
        //mMapController.GetGrid(xIndex, yIndex).SetMouseUnitInGrid(mouse, Vector2.right);
        mouse.transform.position = MapManager.GetGridLocalPosition(xIndex, yIndex) + new Vector3(Vector2.right.x * MapManager.gridWidth, Vector2.right.y * MapManager.gridHeight) / 2;
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
        return CreateMouseUnit(MapController.xColumn - 1, yIndex, enemyInfo);
    }

    /// <summary>
    /// 产生子弹单位
    /// </summary>
    /// <param name="master"></param>
    /// <param name="position"></param>
    /// <param name="rotate"></param>
    /// <param name="style"></param>
    /// <returns></returns>
    public BaseBullet CreateBullet(BaseUnit master, Vector3 position, Vector2 rotate, BulletStyle style)
    {
        GameObject instance = GameManager.Instance.GetGameObjectResource(FactoryType.GameFactory, "Bullet/"+((int)style));
        BaseBullet bullet = instance.GetComponent<BaseBullet>();
        bullet.mMasterBaseUnit = master;
        bullet.MInit();
        bullet.style = style;
        bullet.ChangeAnimatorWithoutChangeStyle(style); // 更新一下子弹样式，因为是从对象池取出来的
        bullet.SetRotate(rotate);
        bullet.transform.position = position;
        mBulletList.Add(bullet);
        bullet.UpdateRenderLayer(mBulletList.Count);
        return bullet;
    }

    /// <summary>
    /// 产生障碍单位
    /// </summary>
    /// <returns></returns>
    public BaseBarrier CreateBarrier(int xIndex, int yIndex, int type, int shape)
    {
        SetBarrierAttribute(JsonManager.Load<BaseUnit.Attribute>("Item/Barrier/" + type + "/" + shape + "")); // 准备先持有要创建实例的初始化信息
        BaseBarrier barrier = GameManager.Instance.GetGameObjectResource(FactoryType.GameFactory, "Item/Barrier/" + type).GetComponent<BaseBarrier>();
        barrier.MInit();
        AddBarrier(barrier, xIndex, yIndex);
        return barrier;
    }

    /// <summary>
    /// 把一个障碍添加至战场
    /// </summary>
    /// <param name="food"></param>
    /// <param name="yIndex"></param>
    /// <returns></returns>
    public BaseBarrier AddBarrier(BaseBarrier barrier, int xIndex, int yIndex)
    {
        barrier.transform.SetParent(itemListGo[yIndex].transform);
        mItemList[yIndex].Add(barrier);
        mMapController.GetGrid(xIndex, yIndex).SetBarrierUnitInGrid(barrier);
        barrier.UpdateRenderLayer(mItemList[yIndex].Count);
        return barrier;
    }

    // 把范围效果加入战场
    public void AddAreaEffectExecution(AreaEffectExecution areaEffectExecution)
    {
        areaEffectExecutionList.Add(areaEffectExecution);
    }

    /// <summary>
    /// 添加特效
    /// </summary>
    public void AddEffect(BaseEffect baseEffect)
    {
        baseEffectList.Add(baseEffect);
    }

    /// <summary>
    /// 添加一个任务执行器
    /// </summary>
    /// <param name="UpdateAction"></param>
    /// <param name="EndCondition"></param>
    /// <returns></returns>
    public Tasker AddTasker(Action InitAction, Action UpdateAction, Func<bool> EndCondition, Action EndEvent)
    {
        Tasker tasker = GameManager.Instance.GetGameObjectResource(FactoryType.GameFactory, "Tasker/Tasker").GetComponent<Tasker>();
        tasker.StartTask(InitAction, UpdateAction, EndCondition, EndEvent);
        taskerList.Add(tasker);
        return tasker;
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
    /// 更换友军行数
    /// </summary>
    public void ChangeAllyRow(int oldIndex, int newIndex, BaseUnit unit)
    {
        GetSpecificRowAllyList(oldIndex).Remove(unit);
        GetSpecificRowAllyList(newIndex).Add(unit);
        unit.transform.SetParent(allyListGo[newIndex].transform);
        unit.UpdateRenderLayer(GetSpecificRowAllyList(newIndex).Count);
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

    public NumberManager GetNumberManager()
    {
        return numberManager;
    }

    // Update is called once per frame
    void Update()
    {
        mFrameNum ++; // 先更新游戏帧，以保证Update里面与协程接收到的帧是相同的

        // test
        //if (mFrameNum == 120)
        //{
        //    InvincibilityBarrier b = CreateBarrier(4, 0, 0, 0) as InvincibilityBarrier;
        //    b.SetLeftTime(600);
        //}

        // 各种组件的Update()
        foreach (IGameControllerMember member in MMemberList)
        {
            member.MUpdate();
        }

        for (int i = 0; i < areaEffectExecutionList.Count; i++)
        {
            AreaEffectExecution e = areaEffectExecutionList[i];
            if (e.IsValid())
            {
                e.MUpdate();
            }
            else
            {
                i--;
                areaEffectExecutionList.Remove(e);
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
        {
            bool flag = false;
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
                    flag = true;
                }
            }
            if (flag)
            {
                for (int i = 0; i < mBulletList.Count; i++)
                {
                    BaseBullet unit = mBulletList[i];
                    unit.UpdateRenderLayer(i);
                }
            }
        }

        // 道具帧逻辑
        foreach (var item in mItemList)
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

        // 特效帧逻辑
        {
            
            for (int i = 0; i < baseEffectList.Count; i++)
            {
                BaseEffect unit = baseEffectList[i];
                if (unit.isActiveAndEnabled)
                {
                    unit.MUpdate();
                }
                else
                {
                    i--;
                    baseEffectList.Remove(unit);
                }
            }
        }

        // 任务执行器逻辑
        {
            for (int i = 0; i < taskerList.Count; i++)
            {
                Tasker tasker = taskerList[i];
                if (tasker.isActiveAndEnabled)
                {
                    tasker.MUpdate();
                }
                else
                {
                    i--;
                    taskerList.Remove(tasker);
                }
            }
        }

        // 将缓冲池的游戏对象放回到对象池
        GameManager.Instance.PushGameObjectFromBufferToPool();
    }
}
