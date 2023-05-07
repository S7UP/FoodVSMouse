using System;
using System.Collections.Generic;

using S7P.Numeric;

using UnityEngine;

public class GameController : MonoBehaviour
{
    private bool isStart = false;
    private bool isLoad = false;
    private static GameController _instance;

    // 引用
    public static GameController Instance { get => _instance; } //+ 自身单例
    public BaseGrid overGrid; // 当前鼠标悬停的格子

    public MouseFactory mMouseFactory = new MouseFactory();

    // 由编辑器给定的引用
    public GameObject gridListGo; // 用于存放地图格子的对象
    public Camera mCamera;

    // 引用
    private Transform enemyListTrans; // 用于存放敌对单位的父对象
    private GameObject[] allyListGo; // 用于存放友方单位的父对象
    private Transform effectListTrans; // 用于存放特效的

    public BaseStage mCurrentStage; // 当前关卡
    public BaseCostController mCostController; // 费用控制器
    public BaseCardController mCardController; // 卡片建造器
    public BaseJewelSkill[] mJewelSkillArray; // 宝石技能
    public BaseProgressController mProgressController; // 游戏进度控制器
    public MapController mMapController; // 格子控制器
    public ItemController mItemController; // 道具控制器
    public List<BaseUnit> mEnemyList; // 存活的敌方单位表
    public Dictionary<BaseUnit, int> mEnemyChangeRowDict; // 敌人换行字典，int值为原行数，新行数可以读取key当前行数
    public List<BaseUnit>[] mAllyList; // 存活的友方单位表

    public BaseUnit focusTarget; // 被选为焦点的目标

    public CharacterController mCharacterController; // 当前角色控制器
    public List<BaseBullet> mBulletList; // 存活的子弹表
    public List<BaseLaser> mLaserList; // 存活的激光表
    public List<AreaEffectExecution> areaEffectExecutionList; // 存活的能力执行体（对象）
    public List<BaseEffect> baseEffectList; // 存活的特效表
    public List<Tasker> taskerList; // 存活的任务执行器表
    //BaseRule[] mRuleList; //+ 规则表
    public KeyBoardSetting mKeyBoardSetting; // 键位控制接口
    //Recorder mRecorder; //+ 用户操作记录者
    //public NumberManager numberManager;

    private int mFrameNum; //+ 当前游戏帧
    public bool isPause;
    public bool isEnableNoTargetAttackMode{ get { return IsEnableNoTargetAttackModeNumeric.Value; } } // 是否开启无目标的攻击模式
    private BoolNumeric IsEnableNoTargetAttackModeNumeric = new BoolNumeric();
    public System.Random rand;

    // 所有需要放到GameController管理Init、Update的东西
    protected List<IGameControllerMember> MMemberList;

    public GameNormalPanel mGameNormalPanel;


    // 暂停的方法
    public void Pause()
    {
        isPause = true;
        // 友方单位暂停
        foreach (var item in mAllyList)
        {
            for (int i = 0; i < item.Count; i++)
            {
                item[i].MPause();
            }
        }
        // 敌方单位暂停
        foreach (var item in mEnemyList)
        {
            item.MPause();
        }
        // 子弹暂停
        foreach (var item in mBulletList)
        {
            item.MPause();
        }
        // 特效暂停
        foreach (var item in baseEffectList)
        {
            item.MPause();
        }
        // 各组件暂停事件
        foreach (var item in MMemberList)
        {
            item.MPause();
        }
        // BGM暂停
        GameManager.Instance.audioSourceManager.PauseAllMusic();
    }

    // 解除暂停的方法
    public void Resume()
    {
        isPause = false;
        // 友方单位解除暂停
        foreach (var item in mAllyList)
        {
            for (int i = 0; i < item.Count; i++)
            {
                item[i].MResume();
            }
        }
        // 敌方单位解除暂停
        foreach (var item in mEnemyList)
        {
            item.MResume();
        }
        // 特效解除暂停
        foreach (var item in baseEffectList)
        {
            item.MResume();
        }
        // 子弹解除暂停
        foreach (var item in mBulletList)
        {
            item.MResume();
        }
        // 各组件解除暂停
        foreach (var item in MMemberList)
        {
            item.MResume();
        }
        // BGM停止暂停
        GameManager.Instance.audioSourceManager.ResumeAllMusic();
    }

    public void Awake()
    {
        _instance = this;
    }

    private void Load()
    {
        if (!isLoad)
        {
            //Debug.Log("GameController Awake!");
            // 随机数生成器
            rand = new System.Random();

            //AbilityManager.Instance.LoadAll();

            MMemberList = new List<IGameControllerMember>();
            // 获取当前场景的UIPanel
            // mGameNormalPanel = (GameNormalPanel)GameManager.Instance.uiManager.mUIFacade.currentScenePanelDict[StringManager.GameNormalPanel];
            mGameNormalPanel = GameNormalPanel.Instance;
            // 从UIPanel中获取各种控制器脚本
            mCostController = mGameNormalPanel.transform.Find("CostControllerUI").GetComponent<BaseCostController>();
            MMemberList.Add(mCostController);
            mCardController = mGameNormalPanel.transform.Find("CardControllerUI").GetComponent<BaseCardController>();
            MMemberList.Add(mCardController);
            mProgressController = mGameNormalPanel.transform.Find("ProgressControllerUI").GetComponent<BaseProgressController>();
            MMemberList.Add(mProgressController);
            mMapController = GameObject.Find("MapController").GetComponent<MapController>();
            MMemberList.Add(mMapController);
            mItemController = GameObject.Find("ItemController").GetComponent<ItemController>();
            MMemberList.Add(mItemController);
            // 获取引用
            effectListTrans = GameObject.Find("EffectList").transform;

            // 敌方表相关
            mEnemyList = new List<BaseUnit>();
            enemyListTrans = GameObject.Find("EnemyList").transform;
            mEnemyChangeRowDict = new Dictionary<BaseUnit, int>();
            // 友方表相关
            mAllyList = new List<BaseUnit>[MapController.yRow];
            allyListGo = new GameObject[mAllyList.Length];
            for (int i = 0; i < mAllyList.Length; i++)
            {
                mAllyList[i] = new List<BaseUnit>();
                GameObject go = new GameObject("i");
                allyListGo[i] = go;
                go.transform.SetParent(GameObject.Find("AllyList").transform);
            }
            // 宝石相关
            PlayerData data = PlayerData.GetInstance();
            mJewelSkillArray = new BaseJewelSkill[3] { JewelManager.GetSkillInstance(data.GetJewel(0)),
            JewelManager.GetSkillInstance(data.GetJewel(1)),
            JewelManager.GetSkillInstance(data.GetJewel(2))};
            // 焦点目标重置
            focusTarget = null;
            // 子弹表相关
            mBulletList = new List<BaseBullet>();
            // 激光相关
            mLaserList = new List<BaseLaser>();
            // 范围效果表相关
            areaEffectExecutionList = new List<AreaEffectExecution>();
            // 特效表相关
            baseEffectList = new List<BaseEffect>();
            // 任务执行者表相关
            taskerList = new List<Tasker>();

            // 加载数值管理器
            //numberManager = new NumberManager();

            // 角色控制器
            mCharacterController = new CharacterController();
            MMemberList.Add(mCharacterController);

            // 键位控制器
            mKeyBoardSetting = new KeyBoardSetting();
            MMemberList.Add(mKeyBoardSetting);

            mGameNormalPanel.InitInGameController();
            Test.OnGameControllerAwake();

            isLoad = true;
        }
    }

    /// <summary>
    /// 变量初始化
    /// </summary>
    public void Init()
    {
        RecycleAndDestoryAllInstance();
        // 当前关卡对象创建
        if (mCurrentStage != null)
            Destroy(mCurrentStage);
        mCurrentStage = GameManager.Instance.GetGameObjectResource(FactoryType.GameFactory, "Stage/Stage").GetComponent<BaseStage>();
        mCurrentStage.Load();
        mCurrentStage.Init();

        // 自身变量初始化
        mFrameNum = 0;
        isPause = false;
        IsEnableNoTargetAttackModeNumeric.Initialize();
        // 焦点目标重置
        focusTarget = null;
        // 宝石技能重置
        foreach (var item in mJewelSkillArray)
        {
            item.Initial();
        }


        // 自身携带控制器初始化（要写到初始化的最后，不建议后面再加其他初始化）
        foreach (IGameControllerMember member in MMemberList)
        {
            member.MInit();
        }
    }

    /// <summary>
    /// 重开本场游戏
    /// </summary>
    public void Restart()
    {
        // 自身属性初始化
        Init();
        // 关卡的刷怪逻辑使用协程
        mCurrentStage.StartStage();
    }

    /// <summary>
    /// 获取默认的经验值奖励值（时间相关）
    /// </summary>
    public float GetDefaultExpReward()
    {
        return (float)GetCurrentStageFrame()/3600*10; // 每分钟10点经验值
    }

    /// <summary>
    /// 赢下这把游戏
    /// </summary>
    public void Win()
    {
        PlayerData data = PlayerData.GetInstance();
        if (data.GetCurrentStageSuccessRewardFunc() != null && data.GetCurrentStageSuccessRewardFunc()())
        {

        }
        else
        {
            mGameNormalPanel.SetExpTips("胜利啦！获得"+ GetDefaultExpReward().ToString("#0")+"点经验值！");
            data.AddExp(GetDefaultExpReward());
        }
        mGameNormalPanel.EnterWinPanel();
        Pause();
    }

    /// <summary>
    /// 输掉这把游戏
    /// </summary>
    public void Lose()
    {
        PlayerData data = PlayerData.GetInstance();
        mGameNormalPanel.SetExpTips("失败啦，没关系，再加把劲就可以啦！给你" + GetDefaultExpReward().ToString("#0") + "点经验值！");
        data.AddExp(GetDefaultExpReward());
        mGameNormalPanel.EnterLosePanel();
        Pause();
    }

    // Start is called before the first frame update
    void Start()
    {
        // Restart();
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

    // 添加一个美食单位至战场中（管理）
    public FoodUnit AddFoodUnit(FoodUnit food, int yIndex)
    {
        food.transform.SetParent(allyListGo[yIndex].transform);
        food.UpdateRenderLayer(mAllyList[yIndex].Count);
        mAllyList[yIndex].Add(food);
        return food;
    }

    /// <summary>
    /// 手动产生美食单位（非玩家操作）
    /// </summary>
    /// <returns></returns>
    public FoodUnit CreateFoodUnit(FoodNameTypeMap type, int shape, int level, BaseGrid grid)
    {
        if(grid!=null && grid.isActiveAndEnabled)
        {
            FoodUnit unit = GameManager.Instance.GetGameObjectResource(FactoryType.GameFactory, "Food/" + (int)type).GetComponent<FoodUnit>();
            BaseCardBuilder.InitInstance(unit, (int)type, shape, grid, level, null);
            return unit;
        }
        return null;
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
        MouseUnit mouse = mMouseFactory.GetMouse(enemyInfo.type, enemyInfo.shape);
        mouse.transform.position = MapManager.GetGridLocalPosition(xIndex, yIndex) + new Vector3(Vector2.right.x * MapManager.gridWidth, Vector2.right.y * MapManager.gridHeight) / 2;
        mouse.currentXIndex = xIndex;
        mouse.currentYIndex = yIndex;
        AddMouseUnit(mouse);
        return mouse;
    }

    /// <summary>
    /// 把一个老鼠单位加入战场
    /// </summary>
    public void AddMouseUnit(MouseUnit mouse)
    {
        mouse.UpdateRenderLayer(mEnemyList.Count);
        mouse.transform.SetParent(enemyListTrans);
        mEnemyList.Add(mouse);
    }

    /// <summary>
    /// 产生BOSS单位
    /// </summary>
    /// <param name="xIndex">格子横坐标</param>
    /// <param name="yIndex">格子纵坐标</param>
    /// <param name="enemyInfo">BOSS种类与形态编号信息表，即决定要产生什么BOSS</param>
    /// <returns></returns>
    public BossUnit CreateBossUnit(int firstColumn, int firstRow, BaseEnemyGroup.EnemyInfo enemyInfo, float hp)
    {
        BossUnit boss = GameManager.Instance.GetGameObjectResource(FactoryType.GameFactory, "Boss/" + enemyInfo.type + "/" + enemyInfo.shape).GetComponent<BossUnit>();
        boss.mType = enemyInfo.type;
        boss.mShape = enemyInfo.shape;
        boss.transform.SetParent(enemyListTrans);
        boss.MInit();
        boss.SetMaxHpAndCurrentHp(hp);
        boss.LoadSeedDict(); // 读取BOSS的种子表
        boss.SetRandSeedByRowIndex(firstRow); // 设置BOSS的种子生成器
        boss.transform.position = MapManager.GetGridLocalPosition(firstColumn, firstRow) + new Vector3(Vector2.right.x * MapManager.gridWidth, Vector2.right.y * MapManager.gridHeight) / 2;
        boss.currentXIndex = firstColumn;
        boss.currentYIndex = firstRow;
        boss.UpdateRenderLayer(mEnemyList.Count);
        mEnemyList.Add(boss);
        return boss;
    }

    public BossUnit CreateBossUnit(int firstRow, BaseEnemyGroup.EnemyInfo enemyInfo, float hp)
    {
        return CreateBossUnit(8, firstRow, enemyInfo, hp);
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
        AddBullet(bullet);
        return bullet;
    }

    /// <summary>
    /// 把一个子弹加入战场
    /// </summary>
    /// <param name="bullet"></param>
    public void AddBullet(BaseBullet bullet)
    {
        mBulletList.Add(bullet);
        bullet.UpdateRenderLayer(mBulletList.Count);
    }

    public void RemoveBullet(BaseBullet bullet)
    {
        mBulletList.Remove(bullet);
    }

    public void AddLaser(BaseLaser laser)
    {
        mLaserList.Add(laser);
        laser.laserRenderer.UpdateRenderLayer(mLaserList.Count);
    }

    public void RemoveLaser(BaseLaser laser)
    {
        mLaserList.Remove(laser);
    }

    /// <summary>
    /// 产生道具单位（依附于格子）
    /// </summary>
    /// <returns></returns>
    public BaseUnit CreateItem(int xIndex, int yIndex, int type, int shape)
    {
        // SetItemAttribute(JsonManager.Load<BaseUnit.Attribute>("Item/" + type + "/" + shape + "")); // 准备先持有要创建实例的初始化信息
        // SetItemAttribute(GameManager.Instance.attributeManager.GetItemUnitAttribute(type, shape)); // 准备先持有要创建实例的初始化信息
        BaseUnit.Attribute attr = GameManager.Instance.attributeManager.GetItemUnitAttribute(type, shape);

        BaseUnit item = GameManager.Instance.GetGameObjectResource(FactoryType.GameFactory, "Item/" + type + "/"+shape).GetComponent<BaseUnit>();
        item.mType = type;
        item.mShape = shape;
        item.MInit();
        item.SetBaseAttribute((float)attr.baseHP, (float)attr.baseAttack, (float)attr.baseAttackSpeed, (float)attr.baseMoveSpeed, (float)attr.baseDefense, (float)attr.attackPercent, attr.baseHeight);
        AddItem(item, xIndex, yIndex);
        return item;
    }

    /// <summary>
    /// 产生道具单位（不依附于格子）
    /// </summary>
    /// <param name="position"></param>
    /// <param name="type"></param>
    /// <param name="shape"></param>
    /// <returns></returns>
    public BaseUnit CreateItem(Vector2 position, int type, int shape)
    {
        //SetItemAttribute(JsonManager.Load<BaseUnit.Attribute>("Item/" + type + "/" + shape + "")); // 准备先持有要创建实例的初始化信息
        //SetItemAttribute(GameManager.Instance.attributeManager.GetItemUnitAttribute(type, shape)); // 准备先持有要创建实例的初始化信息
        BaseUnit.Attribute attr = GameManager.Instance.attributeManager.GetItemUnitAttribute(type, shape);

        BaseUnit item = GameManager.Instance.GetGameObjectResource(FactoryType.GameFactory, "Item/" + type + "/" + shape).GetComponent<BaseUnit>();
        item.mType = type;
        item.mShape = shape;
        item.MInit();
        item.SetBaseAttribute((float)attr.baseHP, (float)attr.baseAttack, (float)attr.baseAttackSpeed, (float)attr.baseMoveSpeed, (float)attr.baseDefense, (float)attr.attackPercent, attr.baseHeight);
        item.transform.position = position;
        AddItem(item);
        return item;
    }

    /// <summary>
    /// 把一个道具添加至战场（依附于格子）
    /// </summary>
    /// <returns></returns>
    public BaseUnit AddItem(BaseUnit item, int xIndex, int yIndex)
    {
        item.transform.SetParent(allyListGo[yIndex].transform);
        //mItemList[yIndex].Add(item);
        mItemController.GetSpecificRowItemList(yIndex).Add(item);
        BaseGrid g = mMapController.GetGrid(xIndex, yIndex);
        if (g != null)
        {
            // 设置新的道具在格上的同时，要把同样tag的旧道具强制消亡（如果有）
            BaseUnit old = g.SetItemUnitInGrid(item);
            if (old != null)
                old.ExecuteDeath();
            item.UpdateRenderLayer(mItemController.GetSpecificRowItemList(yIndex).Count);
        }
        else
        {
            // 如果依附于格子的物品找不到对应格子，则直接消亡
            item.transform.position = MapManager.GetGridLocalPosition(xIndex, yIndex);
            item.ExecuteDeath();
        }
        return item;
    }

    /// <summary>
    /// 把一个道具添加至战场（不依附于格子）
    /// </summary>
    /// <returns></returns>
    public BaseUnit AddItem(BaseUnit item)
    {
        int yIndex = item.GetRowIndex();
        mItemController.SetItemRowParent(item, yIndex);
        mItemController.GetSpecificRowItemList(yIndex).Add(item);
        item.UpdateRenderLayer(mItemController.GetSpecificRowItemList(yIndex).Count);
        return item;
    }

    /// <summary>
    /// 产生角色单位（依附于格子）
    /// </summary>
    /// <returns></returns>
    public CharacterUnit CreateCharacter(int type)
    {
        CharacterUnit c = GameManager.Instance.GetGameObjectResource(FactoryType.GameFactory, "Character/CharacterModel").GetComponent<CharacterUnit>();
        c.mType = type;
        c.mShape = 0;
        c.MInit();
        return c;
    }

    /// <summary>
    /// 把一个角色添加至战场（依附于格子）
    /// </summary>
    /// <returns></returns>
    public CharacterUnit AddCharacter(CharacterUnit c, int xIndex, int yIndex)
    {
        c.transform.SetParent(allyListGo[yIndex].transform);
        mAllyList[yIndex].Add(c);
        mMapController.GetGrid(xIndex, yIndex).SetCharacterUnitInGrid(c);
        c.UpdateRenderLayer(mItemController.GetSpecificRowItemList(yIndex).Count);
        return c;
    }

    // 把范围效果加入战场
    public void AddAreaEffectExecution(AreaEffectExecution areaEffectExecution)
    {
        areaEffectExecutionList.Add(areaEffectExecution);
    }

    /// <summary>
    /// 添加特效，默认父转换为effectListTrans
    /// </summary>
    public void AddEffect(BaseEffect baseEffect)
    {
        baseEffectList.Add(baseEffect);
        SetEffectDefaultParentTrans(baseEffect);
    }

    /// <summary>
    /// 设置特效位于默认的父变换
    /// </summary>
    public void SetEffectDefaultParentTrans(BaseEffect baseEffect)
    {
        baseEffect.transform.SetParent(effectListTrans);
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

    public Tasker AddTasker(PresetTasker presetTasker)
    {
        Tasker tasker = GameManager.Instance.GetGameObjectResource(FactoryType.GameFactory, "Tasker/Tasker").GetComponent<Tasker>();
        tasker.StartTask(presetTasker);
        taskerList.Add(tasker);
        return tasker;
    }

    /// <summary>
    /// 获取当前关卡进行帧
    /// </summary>
    public int GetCurrentStageFrame()
    {
        return mFrameNum;
    }


    /// <summary>
    /// 获取特定行的敌人
    /// </summary>
    /// <returns></returns>
    public List<BaseUnit> GetSpecificRowEnemyList(int i)
    {
        if (i < 0)
            i = 0;
        if (i >= 7)
            i = 6;
        List<BaseUnit> list = new List<BaseUnit>();
        foreach (var unit in mEnemyList)
        {
            if (unit.GetRowIndex() == i)
                list.Add(unit);
        }
        return list;
    }

    /// <summary>
    /// 获取全部的敌人
    /// </summary>
    /// <returns></returns>
    public List<BaseUnit> GetEachEnemy()
    {
        return mEnemyList;
    }

    /// <summary>
    /// 当前场上还存在敌人吗
    /// </summary>
    /// <returns></returns>
    public bool IsHasEnemyInScene()
    {
        return mEnemyList.Count > 0;
    }

    /// <summary>
    /// 更换敌人所在行
    /// </summary>
    /// <param name="oldIndex"></param>
    /// <param name="newIndex"></param>
    /// <param name="unit"></param>
    public void ChangeEnemyRow(int oldIndex, BaseUnit unit)
    {
        mEnemyChangeRowDict.Add(unit, oldIndex);
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
        if (i < 0)
            i = 0;
        else if (i >= mAllyList.Length)
            i = mAllyList.Length - 1;
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

    /// <summary>
    /// 增加火苗
    /// </summary>
    public void AddFireResource(float add)
    {
        mCostController.AddCost("Fire", add);
    }

    /// <summary>
    /// 添加资源变化修饰
    /// </summary>
    public void AddCostResourceModifier(string name, FloatModifier floatModifier)
    {
        mCostController.AddCostResourceModifier(name, floatModifier);
    }

    public void RemoveCostResourceModifier(string name, FloatModifier floatModifier)
    {
        mCostController.RemoveCostResourceModifier(name, floatModifier);
    }

    /// <summary>
    /// 清理所有敌人
    /// </summary>
    public void ClearAllEnemy()
    {
        foreach (var item in mEnemyList)
        {
            item.MDestory();
        }
        mEnemyList.Clear();
        mEnemyChangeRowDict.Clear();
    }

    /// <summary>
    /// 清理所有友方
    /// </summary>
    public void ClearAllAlly()
    {
        for (int i = 0; i < mAllyList.Length; i++)
        {
            foreach (var item in mAllyList[i])
            {
                item.MDestory();
            }
            mAllyList[i].Clear();
        }
    }

    /// <summary>
    /// 清理所有子弹
    /// </summary>
    public void ClearAllBullet()
    {
        foreach (var item in mBulletList)
        {
            item.ExecuteRecycle();
        }
        mBulletList.Clear();
    }

    /// <summary>
    /// 清理所有激光
    /// </summary>
    public void ClearAllLaser()
    {
        foreach (var item in mLaserList)
        {
            item.ExecuteRecycle();
        }
        mLaserList.Clear();
    }

    /// <summary>
    /// 清理所有范围效果
    /// </summary>
    public void ClearAllAreaEffectExecution()
    {
        foreach (var item in areaEffectExecutionList)
        {
            item.ExecuteRecycle();
        }
        areaEffectExecutionList.Clear();
    }

    /// <summary>
    /// 清理所有特效
    /// </summary>
    public void ClearAllEffect()
    {
        foreach (var item in baseEffectList)
        {
            item.Recycle();
        }
        baseEffectList.Clear();
    }

    /// <summary>
    /// 清理所有任务执行器
    /// </summary>
    public void ClearAllTasker()
    {
        foreach (var item in taskerList)
        {
            item.ExecuteRecycle();
        }
        taskerList.Clear();
    }

    /// <summary>
    /// 是否还存在BOSS在场上
    /// </summary>
    /// <returns></returns>
    public bool IsHasBoss()
    {
        foreach (var unit in mEnemyList)
        {
            if (unit is BossUnit)
                return true;
        }
        return false;
    }

    //public NumberManager GetNumberManager()
    //{
    //    return numberManager;
    //}

    // Update is called once per frame
    void Update()
    {
        if (!isStart)
            return;

        if (isPause)
        {
            foreach (IGameControllerMember member in MMemberList)
            {
                member.MPauseUpdate();
            }
            return;
        }

        mFrameNum++; // 先更新游戏帧，以保证Update里面与协程接收到的帧是相同的

        // 各种组件的Update()
        foreach (IGameControllerMember member in MMemberList)
        {
            member.MUpdate();
        }

        // 宝石帧逻辑
        for (int i = 0; i < mJewelSkillArray.Length; i++)
        {
            mJewelSkillArray[i].Update();
        }
        // 范围效果帧逻辑
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

        List<BaseUnit> enemyUpdateList = new List<BaseUnit>();
        // 敌人帧逻辑
        for (int i = 0; i < mEnemyList.Count; i++)
        {
            BaseUnit unit = mEnemyList[i];
            if (unit.IsValid())
            {
                enemyUpdateList.Add(unit);
            }
            else
            {
                i--;
                mEnemyList.Remove(unit);
            }
        }
        foreach (var unit in enemyUpdateList)
        {
            unit.MUpdate();
        }
        //List<BaseUnit> delList = new List<BaseUnit>();
        //foreach (var u in mEnemyList)
        //{
        //    if (!u.IsAlive())
        //        delList.Add(u);
        //}
        //foreach (var u in delList)
        //{
        //    mEnemyList.Remove(u);
        //}


        // 敌人换行更新图层
        foreach (var item in mEnemyChangeRowDict)
        {
            BaseUnit unit = item.Key;
            unit.UpdateRenderLayer(GetSpecificRowEnemyList(unit.GetRowIndex()).Count);
        }
        mEnemyChangeRowDict.Clear();
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

        // 寻找焦点目标
        {
            // 如果玩家点击了鼠标，就这么做
            if (Input.GetMouseButton(0))
            {
                LayerMask mask = LayerMask.GetMask("Ally", "Enemy");
                Ray r = GameController.Instance.mCamera.ScreenPointToRay(Input.mousePosition);
                RaycastHit2D hit = Physics2D.Raycast(new Vector2(r.origin.x, r.origin.y), Vector2.zero, Mathf.Infinity, mask);
                if (hit.collider != null)
                {
                    if (hit.collider.tag.Equals("Food"))
                    {
                        GameController.Instance.focusTarget = hit.collider.GetComponent<FoodUnit>();
                        mGameNormalPanel.DisplayFoodInfo(GameController.Instance.focusTarget);
                    }
                    else if (hit.collider.tag.Equals("Mouse"))
                    {
                        GameController.Instance.focusTarget = hit.collider.GetComponent<MouseUnit>();
                        mGameNormalPanel.DisplayMouseInfo(GameController.Instance.focusTarget);
                    }
                    else if (hit.collider.tag.Equals("Character"))
                    {
                        GameController.Instance.focusTarget = hit.collider.GetComponent<CharacterUnit>();
                        mGameNormalPanel.DisplayCharacterInfo(GameController.Instance.focusTarget);
                    }
                }
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

        // 激光帧逻辑
        {
            bool flag = false;
            for (int i = 0; i < mLaserList.Count; i++)
            {
                BaseLaser unit = mLaserList[i];
                if (unit.isActiveAndEnabled)
                {
                    unit.MUpdate();
                }
                else
                {
                    i--;
                    mLaserList.Remove(unit);
                    flag = true;
                }
            }
            if (flag)
            {
                for (int i = 0; i < mLaserList.Count; i++)
                {
                    BaseLaser unit = mLaserList[i];
                    unit.laserRenderer.UpdateRenderLayer(i);
                }
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

        if (mCurrentStage.isWinWhenClearAllBoss && mCurrentStage.bossLeft <= 0)
        {
            // 胜利判定（消灭了最终BOSS）
            Win();
        }
        else if(mCurrentStage.isEndRound && mProgressController.IsPathEnd() && !IsHasEnemyInScene())
        {
            // 胜利判定（当前道中已完成且场上不存在敌人）
            Win();
        }else if (mProgressController.IsTimeOut())
        {
            // 超时判定
            if (mCurrentStage.isWinWhenClearAllBoss)
            {
                // 击败BOSS的情况
                int currentSenceBossCount = 0;
                foreach (var unit in GetEachEnemy())
                {
                    if(unit is MouseUnit)
                    {
                        MouseUnit m = unit as MouseUnit;
                        if (m.IsBoss())
                        {
                            // boss还活着就输了
                            if (m.IsAlive())
                                Lose();
                            currentSenceBossCount++;
                        }
                    }
                }
                // 比较剩余BOSS与场上的BOSS数，如果一致且它们都处于死亡状态，则不会判输
                if (currentSenceBossCount < mCurrentStage.bossLeft)
                    Lose();
            }
            else
            {
                // 击败全部小兵的情况
                foreach (var unit in GetEachEnemy())
                {
                    if (unit.IsAlive())
                        Lose();
                }
            }
        }

        // 将缓冲池的游戏对象放回到对象池
        GameManager.Instance.PushGameObjectFromBufferToPool();
    }


    /// <summary>
    /// 回收战斗场景中的所有相关对象并且销毁
    /// </summary>
    public void RecycleAndDestoryAllInstance()
    {
        // 停用BGM
        GameManager.Instance.audioSourceManager.StopAllMusic();
        // 回收场上所有对象 && 自身表引用初始化
        ClearAllEnemy();
        ClearAllAlly();
        ClearAllBullet();
        ClearAllLaser();
        ClearAllAreaEffectExecution();
        ClearAllEffect();
        ClearAllTasker();
        mCharacterController.RecycleCurrentCharacter(); // 回收角色对象
        mItemController.RecycleAll(); // 回收所有道具对象
        mMapController.RecycleAllGridAndGroup(); // 回收所有格子与格子组对象
        // 清空游戏对象工厂的所有对象（清空的是上一步回收的东西）
        GameManager.Instance.ClearGameObjectFactory(FactoryType.GameFactory);
    }

    /// <summary>
    /// 加载当前关的键控表并且应用效果于键位控制器LoadAndFixKeyBoardSetting()
    /// </summary>
    public void LoadAndFixKeyBoardSetting()
    {
        List<char> c_list = GameManager.Instance.playerData.GetCurrentCardKeyList();
        List<AvailableCardInfo> a_list = GameManager.Instance.playerData.GetCurrentSelectedCardInfoList();
        for (int i = 0; i < a_list.Count; i++)
        {
            int j = i;
            if (c_list[i] != '\0')
            {
                char c = c_list[i];
                if (c >= 'A' && c <= 'Z')
                {
                    c -= 'A';
                    c += 'a';
                }
                mKeyBoardSetting.AddAction((KeyCode)c, delegate { 
                    mCardController.mCardBuilderList[j].OnClick();
                    if (GameController.Instance.mCardController.isSelectCard && GameManager.Instance.configManager.mConfig.isEnableQuickReleaseCard)
                        GameController.Instance.mCardController.OnMouseLeftDownWhenSelectedCard();
                });
            }
        }
    }


    /// <summary>
    /// 检测某行能否触发攻击（用于友方射手攻击判定）
    /// </summary>
    /// <returns></returns>
    public bool CheckRowCanAttack(BaseUnit unit, int rowIndex)
    {
        if (isEnableNoTargetAttackMode)
            return true;
        foreach (var m in GetSpecificRowEnemyList(rowIndex))
        {
            if (UnitManager.CanBeSelectedAsTarget(unit, m))
                return true;
        }
        return false;
    }

    /// <summary>
    /// 添加无目标也能攻击的tag
    /// </summary>
    /// <param name="boolModifier"></param>
    public void AddNoTargetAttackModeModifier(BoolModifier boolModifier)
    {
        IsEnableNoTargetAttackModeNumeric.AddModifier(boolModifier);
    }

    /// <summary>
    /// 移除无目标也能攻击的tag
    /// </summary>
    /// <param name="boolModifier"></param>
    public void RemoveNoTargetAttackModeModifier(BoolModifier boolModifier)
    {
        IsEnableNoTargetAttackModeNumeric.RemoveModifier(boolModifier);
    }

    /// <summary>
    /// 生成随机浮点数（左开右闭）
    /// </summary>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    public float GetRandomFloat(float min, float max)
    {
        return min + (float)rand.NextDouble()*(max-min);
    }

    /// <summary>
    /// 生成随机整数（左开右闭）
    /// </summary>
    /// <returns></returns>
    public int GetRandomInt(int min, int max)
    {
        return rand.Next(min, max);
    }

    /// <summary>
    /// 设置为开始运作状态，仅GameNormalSceneState里可用
    /// </summary>
    public void SetStart()
    {
        isStart = true;
        Load();
        Restart();
    }
}
