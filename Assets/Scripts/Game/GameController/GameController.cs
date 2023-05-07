using System;
using System.Collections.Generic;

using S7P.Numeric;

using UnityEngine;

public class GameController : MonoBehaviour
{
    private bool isStart = false;
    private bool isLoad = false;
    private static GameController _instance;

    // ����
    public static GameController Instance { get => _instance; } //+ ������
    public BaseGrid overGrid; // ��ǰ�����ͣ�ĸ���

    public MouseFactory mMouseFactory = new MouseFactory();

    // �ɱ༭������������
    public GameObject gridListGo; // ���ڴ�ŵ�ͼ���ӵĶ���
    public Camera mCamera;

    // ����
    private Transform enemyListTrans; // ���ڴ�ŵжԵ�λ�ĸ�����
    private GameObject[] allyListGo; // ���ڴ���ѷ���λ�ĸ�����
    private Transform effectListTrans; // ���ڴ����Ч��

    public BaseStage mCurrentStage; // ��ǰ�ؿ�
    public BaseCostController mCostController; // ���ÿ�����
    public BaseCardController mCardController; // ��Ƭ������
    public BaseJewelSkill[] mJewelSkillArray; // ��ʯ����
    public BaseProgressController mProgressController; // ��Ϸ���ȿ�����
    public MapController mMapController; // ���ӿ�����
    public ItemController mItemController; // ���߿�����
    public List<BaseUnit> mEnemyList; // ���ĵз���λ��
    public Dictionary<BaseUnit, int> mEnemyChangeRowDict; // ���˻����ֵ䣬intֵΪԭ���������������Զ�ȡkey��ǰ����
    public List<BaseUnit>[] mAllyList; // �����ѷ���λ��

    public BaseUnit focusTarget; // ��ѡΪ�����Ŀ��

    public CharacterController mCharacterController; // ��ǰ��ɫ������
    public List<BaseBullet> mBulletList; // �����ӵ���
    public List<BaseLaser> mLaserList; // ���ļ����
    public List<AreaEffectExecution> areaEffectExecutionList; // ��������ִ���壨����
    public List<BaseEffect> baseEffectList; // ������Ч��
    public List<Tasker> taskerList; // ��������ִ������
    //BaseRule[] mRuleList; //+ �����
    public KeyBoardSetting mKeyBoardSetting; // ��λ���ƽӿ�
    //Recorder mRecorder; //+ �û�������¼��
    //public NumberManager numberManager;

    private int mFrameNum; //+ ��ǰ��Ϸ֡
    public bool isPause;
    public bool isEnableNoTargetAttackMode{ get { return IsEnableNoTargetAttackModeNumeric.Value; } } // �Ƿ�����Ŀ��Ĺ���ģʽ
    private BoolNumeric IsEnableNoTargetAttackModeNumeric = new BoolNumeric();
    public System.Random rand;

    // ������Ҫ�ŵ�GameController����Init��Update�Ķ���
    protected List<IGameControllerMember> MMemberList;

    public GameNormalPanel mGameNormalPanel;


    // ��ͣ�ķ���
    public void Pause()
    {
        isPause = true;
        // �ѷ���λ��ͣ
        foreach (var item in mAllyList)
        {
            for (int i = 0; i < item.Count; i++)
            {
                item[i].MPause();
            }
        }
        // �з���λ��ͣ
        foreach (var item in mEnemyList)
        {
            item.MPause();
        }
        // �ӵ���ͣ
        foreach (var item in mBulletList)
        {
            item.MPause();
        }
        // ��Ч��ͣ
        foreach (var item in baseEffectList)
        {
            item.MPause();
        }
        // �������ͣ�¼�
        foreach (var item in MMemberList)
        {
            item.MPause();
        }
        // BGM��ͣ
        GameManager.Instance.audioSourceManager.PauseAllMusic();
    }

    // �����ͣ�ķ���
    public void Resume()
    {
        isPause = false;
        // �ѷ���λ�����ͣ
        foreach (var item in mAllyList)
        {
            for (int i = 0; i < item.Count; i++)
            {
                item[i].MResume();
            }
        }
        // �з���λ�����ͣ
        foreach (var item in mEnemyList)
        {
            item.MResume();
        }
        // ��Ч�����ͣ
        foreach (var item in baseEffectList)
        {
            item.MResume();
        }
        // �ӵ������ͣ
        foreach (var item in mBulletList)
        {
            item.MResume();
        }
        // ����������ͣ
        foreach (var item in MMemberList)
        {
            item.MResume();
        }
        // BGMֹͣ��ͣ
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
            // �����������
            rand = new System.Random();

            //AbilityManager.Instance.LoadAll();

            MMemberList = new List<IGameControllerMember>();
            // ��ȡ��ǰ������UIPanel
            // mGameNormalPanel = (GameNormalPanel)GameManager.Instance.uiManager.mUIFacade.currentScenePanelDict[StringManager.GameNormalPanel];
            mGameNormalPanel = GameNormalPanel.Instance;
            // ��UIPanel�л�ȡ���ֿ������ű�
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
            // ��ȡ����
            effectListTrans = GameObject.Find("EffectList").transform;

            // �з������
            mEnemyList = new List<BaseUnit>();
            enemyListTrans = GameObject.Find("EnemyList").transform;
            mEnemyChangeRowDict = new Dictionary<BaseUnit, int>();
            // �ѷ������
            mAllyList = new List<BaseUnit>[MapController.yRow];
            allyListGo = new GameObject[mAllyList.Length];
            for (int i = 0; i < mAllyList.Length; i++)
            {
                mAllyList[i] = new List<BaseUnit>();
                GameObject go = new GameObject("i");
                allyListGo[i] = go;
                go.transform.SetParent(GameObject.Find("AllyList").transform);
            }
            // ��ʯ���
            PlayerData data = PlayerData.GetInstance();
            mJewelSkillArray = new BaseJewelSkill[3] { JewelManager.GetSkillInstance(data.GetJewel(0)),
            JewelManager.GetSkillInstance(data.GetJewel(1)),
            JewelManager.GetSkillInstance(data.GetJewel(2))};
            // ����Ŀ������
            focusTarget = null;
            // �ӵ������
            mBulletList = new List<BaseBullet>();
            // �������
            mLaserList = new List<BaseLaser>();
            // ��ΧЧ�������
            areaEffectExecutionList = new List<AreaEffectExecution>();
            // ��Ч�����
            baseEffectList = new List<BaseEffect>();
            // ����ִ���߱����
            taskerList = new List<Tasker>();

            // ������ֵ������
            //numberManager = new NumberManager();

            // ��ɫ������
            mCharacterController = new CharacterController();
            MMemberList.Add(mCharacterController);

            // ��λ������
            mKeyBoardSetting = new KeyBoardSetting();
            MMemberList.Add(mKeyBoardSetting);

            mGameNormalPanel.InitInGameController();
            Test.OnGameControllerAwake();

            isLoad = true;
        }
    }

    /// <summary>
    /// ������ʼ��
    /// </summary>
    public void Init()
    {
        RecycleAndDestoryAllInstance();
        // ��ǰ�ؿ����󴴽�
        if (mCurrentStage != null)
            Destroy(mCurrentStage);
        mCurrentStage = GameManager.Instance.GetGameObjectResource(FactoryType.GameFactory, "Stage/Stage").GetComponent<BaseStage>();
        mCurrentStage.Load();
        mCurrentStage.Init();

        // ���������ʼ��
        mFrameNum = 0;
        isPause = false;
        IsEnableNoTargetAttackModeNumeric.Initialize();
        // ����Ŀ������
        focusTarget = null;
        // ��ʯ��������
        foreach (var item in mJewelSkillArray)
        {
            item.Initial();
        }


        // ����Я����������ʼ����Ҫд����ʼ������󣬲���������ټ�������ʼ����
        foreach (IGameControllerMember member in MMemberList)
        {
            member.MInit();
        }
    }

    /// <summary>
    /// �ؿ�������Ϸ
    /// </summary>
    public void Restart()
    {
        // �������Գ�ʼ��
        Init();
        // �ؿ���ˢ���߼�ʹ��Э��
        mCurrentStage.StartStage();
    }

    /// <summary>
    /// ��ȡĬ�ϵľ���ֵ����ֵ��ʱ����أ�
    /// </summary>
    public float GetDefaultExpReward()
    {
        return (float)GetCurrentStageFrame()/3600*10; // ÿ����10�㾭��ֵ
    }

    /// <summary>
    /// Ӯ�������Ϸ
    /// </summary>
    public void Win()
    {
        PlayerData data = PlayerData.GetInstance();
        if (data.GetCurrentStageSuccessRewardFunc() != null && data.GetCurrentStageSuccessRewardFunc()())
        {

        }
        else
        {
            mGameNormalPanel.SetExpTips("ʤ���������"+ GetDefaultExpReward().ToString("#0")+"�㾭��ֵ��");
            data.AddExp(GetDefaultExpReward());
        }
        mGameNormalPanel.EnterWinPanel();
        Pause();
    }

    /// <summary>
    /// ��������Ϸ
    /// </summary>
    public void Lose()
    {
        PlayerData data = PlayerData.GetInstance();
        mGameNormalPanel.SetExpTips("ʧ������û��ϵ���ټӰѾ��Ϳ�����������" + GetDefaultExpReward().ToString("#0") + "�㾭��ֵ��");
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
    /// ��ȡ��ǰ������
    /// </summary>
    /// <returns></returns>
    public float GetFire()
    {
        return mCostController.GetCost("Fire");
    }

    /// <summary>
    /// ��ȡ��ǰ�����ͣ�ĸ���
    /// </summary>
    public BaseGrid GetOverGrid()
    {
        return overGrid;
    }

    // ���һ����ʳ��λ��ս���У�����
    public FoodUnit AddFoodUnit(FoodUnit food, int yIndex)
    {
        food.transform.SetParent(allyListGo[yIndex].transform);
        food.UpdateRenderLayer(mAllyList[yIndex].Count);
        mAllyList[yIndex].Add(food);
        return food;
    }

    /// <summary>
    /// �ֶ�������ʳ��λ������Ҳ�����
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
    /// ��������λ
    /// </summary>
    /// <param name="xIndex">���Ӻ�����</param>
    /// <param name="yIndex">����������</param>
    /// <param name="enemyInfo">������������̬�����Ϣ��������Ҫ����ʲô����</param>
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
    /// ��һ������λ����ս��
    /// </summary>
    public void AddMouseUnit(MouseUnit mouse)
    {
        mouse.UpdateRenderLayer(mEnemyList.Count);
        mouse.transform.SetParent(enemyListTrans);
        mEnemyList.Add(mouse);
    }

    /// <summary>
    /// ����BOSS��λ
    /// </summary>
    /// <param name="xIndex">���Ӻ�����</param>
    /// <param name="yIndex">����������</param>
    /// <param name="enemyInfo">BOSS��������̬�����Ϣ��������Ҫ����ʲôBOSS</param>
    /// <returns></returns>
    public BossUnit CreateBossUnit(int firstColumn, int firstRow, BaseEnemyGroup.EnemyInfo enemyInfo, float hp)
    {
        BossUnit boss = GameManager.Instance.GetGameObjectResource(FactoryType.GameFactory, "Boss/" + enemyInfo.type + "/" + enemyInfo.shape).GetComponent<BossUnit>();
        boss.mType = enemyInfo.type;
        boss.mShape = enemyInfo.shape;
        boss.transform.SetParent(enemyListTrans);
        boss.MInit();
        boss.SetMaxHpAndCurrentHp(hp);
        boss.LoadSeedDict(); // ��ȡBOSS�����ӱ�
        boss.SetRandSeedByRowIndex(firstRow); // ����BOSS������������
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
    /// Ĭ�������Ҳ�ˢ����
    /// </summary>
    /// <param name="yIndex"></param>
    /// <returns></returns>
    public MouseUnit CreateMouseUnit(int yIndex, BaseEnemyGroup.EnemyInfo enemyInfo)
    {
        return CreateMouseUnit(MapController.xColumn - 1, yIndex, enemyInfo);
    }

    /// <summary>
    /// �����ӵ���λ
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
        bullet.ChangeAnimatorWithoutChangeStyle(style); // ����һ���ӵ���ʽ����Ϊ�ǴӶ����ȡ������
        bullet.SetRotate(rotate);
        bullet.transform.position = position;
        AddBullet(bullet);
        return bullet;
    }

    /// <summary>
    /// ��һ���ӵ�����ս��
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
    /// �������ߵ�λ�������ڸ��ӣ�
    /// </summary>
    /// <returns></returns>
    public BaseUnit CreateItem(int xIndex, int yIndex, int type, int shape)
    {
        // SetItemAttribute(JsonManager.Load<BaseUnit.Attribute>("Item/" + type + "/" + shape + "")); // ׼���ȳ���Ҫ����ʵ���ĳ�ʼ����Ϣ
        // SetItemAttribute(GameManager.Instance.attributeManager.GetItemUnitAttribute(type, shape)); // ׼���ȳ���Ҫ����ʵ���ĳ�ʼ����Ϣ
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
    /// �������ߵ�λ���������ڸ��ӣ�
    /// </summary>
    /// <param name="position"></param>
    /// <param name="type"></param>
    /// <param name="shape"></param>
    /// <returns></returns>
    public BaseUnit CreateItem(Vector2 position, int type, int shape)
    {
        //SetItemAttribute(JsonManager.Load<BaseUnit.Attribute>("Item/" + type + "/" + shape + "")); // ׼���ȳ���Ҫ����ʵ���ĳ�ʼ����Ϣ
        //SetItemAttribute(GameManager.Instance.attributeManager.GetItemUnitAttribute(type, shape)); // ׼���ȳ���Ҫ����ʵ���ĳ�ʼ����Ϣ
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
    /// ��һ�����������ս���������ڸ��ӣ�
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
            // �����µĵ����ڸ��ϵ�ͬʱ��Ҫ��ͬ��tag�ľɵ���ǿ������������У�
            BaseUnit old = g.SetItemUnitInGrid(item);
            if (old != null)
                old.ExecuteDeath();
            item.UpdateRenderLayer(mItemController.GetSpecificRowItemList(yIndex).Count);
        }
        else
        {
            // ��������ڸ��ӵ���Ʒ�Ҳ�����Ӧ���ӣ���ֱ������
            item.transform.position = MapManager.GetGridLocalPosition(xIndex, yIndex);
            item.ExecuteDeath();
        }
        return item;
    }

    /// <summary>
    /// ��һ�����������ս�����������ڸ��ӣ�
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
    /// ������ɫ��λ�������ڸ��ӣ�
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
    /// ��һ����ɫ�����ս���������ڸ��ӣ�
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

    // �ѷ�ΧЧ������ս��
    public void AddAreaEffectExecution(AreaEffectExecution areaEffectExecution)
    {
        areaEffectExecutionList.Add(areaEffectExecution);
    }

    /// <summary>
    /// �����Ч��Ĭ�ϸ�ת��ΪeffectListTrans
    /// </summary>
    public void AddEffect(BaseEffect baseEffect)
    {
        baseEffectList.Add(baseEffect);
        SetEffectDefaultParentTrans(baseEffect);
    }

    /// <summary>
    /// ������Чλ��Ĭ�ϵĸ��任
    /// </summary>
    public void SetEffectDefaultParentTrans(BaseEffect baseEffect)
    {
        baseEffect.transform.SetParent(effectListTrans);
    }

    /// <summary>
    /// ���һ������ִ����
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
    /// ��ȡ��ǰ�ؿ�����֡
    /// </summary>
    public int GetCurrentStageFrame()
    {
        return mFrameNum;
    }


    /// <summary>
    /// ��ȡ�ض��еĵ���
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
    /// ��ȡȫ���ĵ���
    /// </summary>
    /// <returns></returns>
    public List<BaseUnit> GetEachEnemy()
    {
        return mEnemyList;
    }

    /// <summary>
    /// ��ǰ���ϻ����ڵ�����
    /// </summary>
    /// <returns></returns>
    public bool IsHasEnemyInScene()
    {
        return mEnemyList.Count > 0;
    }

    /// <summary>
    /// ��������������
    /// </summary>
    /// <param name="oldIndex"></param>
    /// <param name="newIndex"></param>
    /// <param name="unit"></param>
    public void ChangeEnemyRow(int oldIndex, BaseUnit unit)
    {
        mEnemyChangeRowDict.Add(unit, oldIndex);
    }

    /// <summary>
    /// ��ȡ��ǰ�����Ѿ���
    /// </summary>
    /// <returns></returns>
    public List<BaseUnit>[] GetAllyList()
    {
        return mAllyList;
    }


    /// <summary>
    /// ��ȡ�ض��е��Ѿ�
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
    /// �����Ѿ�����
    /// </summary>
    public void ChangeAllyRow(int oldIndex, int newIndex, BaseUnit unit)
    {
        GetSpecificRowAllyList(oldIndex).Remove(unit);
        GetSpecificRowAllyList(newIndex).Add(unit);
        unit.transform.SetParent(allyListGo[newIndex].transform);
        unit.UpdateRenderLayer(GetSpecificRowAllyList(newIndex).Count);
    }


    /// <summary>
    /// ��ȡȫ�����Ѿ�
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
    /// ���ӻ���
    /// </summary>
    public void AddFireResource(float add)
    {
        mCostController.AddCost("Fire", add);
    }

    /// <summary>
    /// �����Դ�仯����
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
    /// �������е���
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
    /// ���������ѷ�
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
    /// ���������ӵ�
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
    /// �������м���
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
    /// �������з�ΧЧ��
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
    /// ����������Ч
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
    /// ������������ִ����
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
    /// �Ƿ񻹴���BOSS�ڳ���
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

        mFrameNum++; // �ȸ�����Ϸ֡���Ա�֤Update������Э�̽��յ���֡����ͬ��

        // ���������Update()
        foreach (IGameControllerMember member in MMemberList)
        {
            member.MUpdate();
        }

        // ��ʯ֡�߼�
        for (int i = 0; i < mJewelSkillArray.Length; i++)
        {
            mJewelSkillArray[i].Update();
        }
        // ��ΧЧ��֡�߼�
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
        // ����֡�߼�
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


        // ���˻��и���ͼ��
        foreach (var item in mEnemyChangeRowDict)
        {
            BaseUnit unit = item.Key;
            unit.UpdateRenderLayer(GetSpecificRowEnemyList(unit.GetRowIndex()).Count);
        }
        mEnemyChangeRowDict.Clear();
        // �ѷ�֡�߼�
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

        // Ѱ�ҽ���Ŀ��
        {
            // �����ҵ������꣬����ô��
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

        // �ӵ�֡�߼�
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

        // ����֡�߼�
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


        // ��Ч֡�߼�
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

        // ����ִ�����߼�
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
            // ʤ���ж�������������BOSS��
            Win();
        }
        else if(mCurrentStage.isEndRound && mProgressController.IsPathEnd() && !IsHasEnemyInScene())
        {
            // ʤ���ж�����ǰ����������ҳ��ϲ����ڵ��ˣ�
            Win();
        }else if (mProgressController.IsTimeOut())
        {
            // ��ʱ�ж�
            if (mCurrentStage.isWinWhenClearAllBoss)
            {
                // ����BOSS�����
                int currentSenceBossCount = 0;
                foreach (var unit in GetEachEnemy())
                {
                    if(unit is MouseUnit)
                    {
                        MouseUnit m = unit as MouseUnit;
                        if (m.IsBoss())
                        {
                            // boss�����ž�����
                            if (m.IsAlive())
                                Lose();
                            currentSenceBossCount++;
                        }
                    }
                }
                // �Ƚ�ʣ��BOSS�볡�ϵ�BOSS�������һ�������Ƕ���������״̬���򲻻�����
                if (currentSenceBossCount < mCurrentStage.bossLeft)
                    Lose();
            }
            else
            {
                // ����ȫ��С�������
                foreach (var unit in GetEachEnemy())
                {
                    if (unit.IsAlive())
                        Lose();
                }
            }
        }

        // ������ص���Ϸ����Żص������
        GameManager.Instance.PushGameObjectFromBufferToPool();
    }


    /// <summary>
    /// ����ս�������е�������ض���������
    /// </summary>
    public void RecycleAndDestoryAllInstance()
    {
        // ͣ��BGM
        GameManager.Instance.audioSourceManager.StopAllMusic();
        // ���ճ������ж��� && ��������ó�ʼ��
        ClearAllEnemy();
        ClearAllAlly();
        ClearAllBullet();
        ClearAllLaser();
        ClearAllAreaEffectExecution();
        ClearAllEffect();
        ClearAllTasker();
        mCharacterController.RecycleCurrentCharacter(); // ���ս�ɫ����
        mItemController.RecycleAll(); // �������е��߶���
        mMapController.RecycleAllGridAndGroup(); // �������и�������������
        // �����Ϸ���󹤳������ж�����յ�����һ�����յĶ�����
        GameManager.Instance.ClearGameObjectFactory(FactoryType.GameFactory);
    }

    /// <summary>
    /// ���ص�ǰ�صļ��ر���Ӧ��Ч���ڼ�λ������LoadAndFixKeyBoardSetting()
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
    /// ���ĳ���ܷ񴥷������������ѷ����ֹ����ж���
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
    /// �����Ŀ��Ҳ�ܹ�����tag
    /// </summary>
    /// <param name="boolModifier"></param>
    public void AddNoTargetAttackModeModifier(BoolModifier boolModifier)
    {
        IsEnableNoTargetAttackModeNumeric.AddModifier(boolModifier);
    }

    /// <summary>
    /// �Ƴ���Ŀ��Ҳ�ܹ�����tag
    /// </summary>
    /// <param name="boolModifier"></param>
    public void RemoveNoTargetAttackModeModifier(BoolModifier boolModifier)
    {
        IsEnableNoTargetAttackModeNumeric.RemoveModifier(boolModifier);
    }

    /// <summary>
    /// ������������������ұգ�
    /// </summary>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    public float GetRandomFloat(float min, float max)
    {
        return min + (float)rand.NextDouble()*(max-min);
    }

    /// <summary>
    /// ����������������ұգ�
    /// </summary>
    /// <returns></returns>
    public int GetRandomInt(int min, int max)
    {
        return rand.Next(min, max);
    }

    /// <summary>
    /// ����Ϊ��ʼ����״̬����GameNormalSceneState�����
    /// </summary>
    public void SetStart()
    {
        isStart = true;
        Load();
        Restart();
    }
}
