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

    // ����
    public static GameController Instance { get => _instance; } //+ ������
    public BaseGrid overGrid; // ��ǰ�����ͣ�ĸ���
    protected BaseUnit.Attribute baseAttribute;
    protected FoodUnit.Attribute foodAttribute;
    protected MouseUnit.Attribute mouseAttribute;


    // �ɱ༭������������
    public GameObject gridListGo; // ���ڴ�ŵ�ͼ���ӵĶ���
    public Camera mCamera;

    // ����
    private GameObject[] enemyListGo; // ���ڴ�ŵжԵ�λ�ĸ�����
    private GameObject[] allyListGo; // ���ڴ���ѷ���λ�ĸ�����
    private GameObject[] itemListGo; // ���ڴ�ŵ�ͼ��Ʒ��λ�ĸ�����

    public BaseStage mCurrentStage; //+ ��ǰ�ؿ�
    public BaseCostController mCostController; //+ ���ÿ�����
    public BaseCardController mCardController; //+ ��Ƭ������
    public BaseSkillController mSkillController; //+ ���ܿ�����
    public BaseProgressController mProgressController; //+ ��Ϸ���ȿ�����
    public MapController mMapController; // ���ӿ�����
    public List<BaseUnit>[] mEnemyList; //+ ���ĵз���λ��
    public List<BaseUnit>[] mAllyList; // �����ѷ���λ��
    public List<BaseUnit>[] mItemList; // ���ĵ��ߵ�λ��
    public List<BaseBullet> mBulletList; // �����ӵ���
    public List<AreaEffectExecution> areaEffectExecutionList; // ��������ִ���壨����
    public List<BaseEffect> baseEffectList; // ������Ч��
    public List<Tasker> taskerList; // ��������ִ������
    //BaseRule[] mRuleList; //+ �����
    //KeyBoardSetting mKeyBoardSetting; //+ ��λ���ƽӿ�
    //Recorder mRecorder; //+ �û�������¼��
    public NumberManager numberManager;

    private int mFrameNum; //+ ��ǰ��Ϸ֡
    public bool isPause;

    // ������Ҫ�ŵ�GameController����Init��Update�Ķ���
    protected List<IGameControllerMember> MMemberList;

    // ����������
    public GameObject uICanvasGo;

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
        uICanvasGo = GameObject.Find("UICanvas");

        MMemberList = new List<IGameControllerMember>();
        // ��ȡ��ǰ������UIPanel
        GameNormalPanel panel = (GameNormalPanel)GameManager.Instance.uiManager.mUIFacade.currentScenePanelDict[StringManager.GameNormalPanel];
        // ��UIPanel�л�ȡ���ֿ������ű�
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

        // ������ֵ������
        numberManager = new NumberManager();

        Test.OnGameControllerAwake();
    }

    /// <summary>
    /// ��֪ͨ��Ƭ������������Ƭ֮ǰ������ϵͳ֪����һ��Ҫ���쿨Ƭ������
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
    /// ͬ�ϣ������������ϰ���
    /// </summary>
    public void SetBarrierAttribute(BaseUnit.Attribute attr)
    {
        baseAttribute = attr;
    }

    /// <summary>
    /// ��ȡ�������ԣ�Ȼ��������ʼ������
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

    // Start is called before the first frame update
    void Start()
    {
        // 
        foreach (IGameControllerMember member in MMemberList)
        {
            member.MInit();
        }

        // �ؿ���ˢ���߼�ʹ��Э��
        StartCoroutine(mCurrentStage.Start());
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
    /// ��������λ
    /// </summary>
    /// <param name="xIndex">���Ӻ�����</param>
    /// <param name="yIndex">����������</param>
    /// <param name="enemyInfo">������������̬�����Ϣ��������Ҫ����ʲô����</param>
    /// <returns></returns>
    public MouseUnit CreateMouseUnit(int xIndex, int yIndex, BaseEnemyGroup.EnemyInfo enemyInfo)
    {
        SetMouseAttribute(JsonManager.Load<MouseUnit.Attribute>("Mouse/"+enemyInfo.type+"/"+enemyInfo.shape+"")); // ׼���ȳ���Ҫ����ʵ���ĳ�ʼ����Ϣ
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
        mBulletList.Add(bullet);
        bullet.UpdateRenderLayer(mBulletList.Count);
        return bullet;
    }

    /// <summary>
    /// �����ϰ���λ
    /// </summary>
    /// <returns></returns>
    public BaseBarrier CreateBarrier(int xIndex, int yIndex, int type, int shape)
    {
        SetBarrierAttribute(JsonManager.Load<BaseUnit.Attribute>("Item/Barrier/" + type + "/" + shape + "")); // ׼���ȳ���Ҫ����ʵ���ĳ�ʼ����Ϣ
        BaseBarrier barrier = GameManager.Instance.GetGameObjectResource(FactoryType.GameFactory, "Item/Barrier/" + type).GetComponent<BaseBarrier>();
        barrier.MInit();
        AddBarrier(barrier, xIndex, yIndex);
        return barrier;
    }

    /// <summary>
    /// ��һ���ϰ������ս��
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

    // �ѷ�ΧЧ������ս��
    public void AddAreaEffectExecution(AreaEffectExecution areaEffectExecution)
    {
        areaEffectExecutionList.Add(areaEffectExecution);
    }

    /// <summary>
    /// �����Ч
    /// </summary>
    public void AddEffect(BaseEffect baseEffect)
    {
        baseEffectList.Add(baseEffect);
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

    /// <summary>
    /// ��GameControllerЭ���У�����һ���ȴ�����֡��ָ��
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
    /// ��ȡ��ǰ�ؿ�����֡
    /// </summary>
    public int GetCurrentStageFrame()
    {
        return mFrameNum;
    }

    /// <summary>
    /// ��ȡ��ǰ�������˱�
    /// </summary>
    /// <returns></returns>
    public List<BaseUnit>[] GetEnemyList()
    {
        return mEnemyList;
    }


    /// <summary>
    /// ��ȡ�ض��еĵ���
    /// </summary>
    /// <returns></returns>
    public List<BaseUnit> GetSpecificRowEnemyList(int i)
    {
        return mEnemyList[i];
    }

    /// <summary>
    /// ��ȡȫ���ĵ���
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

    public NumberManager GetNumberManager()
    {
        return numberManager;
    }

    // Update is called once per frame
    void Update()
    {
        mFrameNum ++; // �ȸ�����Ϸ֡���Ա�֤Update������Э�̽��յ���֡����ͬ��

        // test
        //if (mFrameNum == 120)
        //{
        //    InvincibilityBarrier b = CreateBarrier(4, 0, 0, 0) as InvincibilityBarrier;
        //    b.SetLeftTime(600);
        //}

        // ���������Update()
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


        // ����֡�߼�
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

        // ������ص���Ϸ����Żص������
        GameManager.Instance.PushGameObjectFromBufferToPool();
    }
}
