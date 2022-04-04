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

    // ����
    public static GameController Instance { get => _instance; } //+ ������
    public BaseGrid overGrid; // ��ǰ�����ͣ�ĸ���
    protected BaseUnit.Attribute baseAttribute;
    protected FoodUnit.Attribute foodAttribute;
    protected MouseUnit.Attribute mouseAttribute;


    // �ɱ༭������������
    public GameObject gridListGo; // ���ڴ�ŵ�ͼ���ӵĶ���

    // ����
    private GameObject[] enemyListGo; // ���ڴ�ŵжԵ�λ�ĸ�����
    private GameObject[] allyListGo; // ���ڴ���ѷ���λ�ĸ�����

    public BaseStage mCurrentStage; //+ ��ǰ�ؿ�
    public BaseCostController mCostController; //+ ���ÿ�����
    public BaseCardController mCardController; //+ ��Ƭ������
    public BaseSkillController mSkillController; //+ ���ܿ�����
    public BaseProgressController mProgressController; //+ ��Ϸ���ȿ�����
    public BaseGrid[,] mGridList; //+ ���ӱ�
    public List<BaseUnit>[] mEnemyList; //+ ���ĵз���λ��
    public List<BaseUnit>[] mAllyList; // �����ѷ���λ��
    public List<BaseBullet> mBulletList; // �����ӵ���
    public List<AbilityExecution> abilityExecutionList; // ��������ִ���壨�Ƕ���
    //BaseRule[] mRuleList; //+ �����
    //KeyBoardSetting mKeyBoardSetting; //+ ��λ���ƽӿ�
    //Recorder mRecorder; //+ �û�������¼��

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

        // ���ɳ��ظ���
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
        //MouseUnit.SaveNewMouseInfo(); // ���浱ǰ������Ϣ

        //FoodUnit.Attribute attr = new FoodUnit.Attribute()
        //{
        //    baseAttrbute = new BaseUnit.Attribute()
        //    {
        //        name = "�ս��߾Ƽ�", // ��λ�ľ�������
        //        type = 7, // ��λ���ڵķ���
        //        shape = 2, // ��λ�ڵ�ǰ����ı��ֱ��

        //        baseHP = 50, // ����Ѫ��
        //        baseAttack = 10, // ��������
        //        baseAttackSpeed = 0.85, // ���������ٶ�
        //        attackPercent = 0.5,
        //        baseHeight = 0, // �����߶�
        //    },
        //    foodType = FoodType.Shooter
        //};

        //Debug.Log("��ʼ�浵��ʳ��Ϣ��");
        //JsonManager.Save(attr, "Food/" + attr.baseAttrbute.type + "/" + attr.baseAttrbute.shape + "");
        //Debug.Log("��ʳ��Ϣ�浵��ɣ�");

        //Debug.Log("��ʼ��ȡ��ʳ��Ϣ��");
        //foodAttribute = JsonManager.Load<FoodUnit.Attribute>("Food/" + attr.baseAttrbute.type + "/" + attr.baseAttrbute.shape + "");
        //Debug.Log("name=" + foodAttribute.baseAttrbute.name);
        //Debug.Log("��ȡ��ʳ��Ϣ�ɹ���");

        // stage test
        mCurrentStage = new BaseStage();
        //mCurrentStage.Save();
        mCurrentStage.DemoLoad();
        mCurrentStage.Init();

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
    /// Ĭ�������Ҳ�ˢ����
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

    // ����������λ
    public void CreateOtherUnit()
    {

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

    // Update is called once per frame
    void Update()
    {
        mFrameNum ++; // �ȸ�����Ϸ֡���Ա�֤Update������Э�̽��յ���֡����ͬ��

        // test
        if (mFrameNum == 840)
        {
            Debug.Log("ȫ����������");
            foreach (var item in GetEachEnemy())
            {
                item.AddStatusAbility(new FrozenStatusAbility(item, 600));
            }
            //foreach (var item in GetEachAlly())
            //{
            //    item.AddStatusAbility(new FrozenStatusAbility(item, 240));
            //}
        }

        // ���������Update()
        foreach (IGameControllerMember member in MMemberList)
        {
            member.MUpdate();
        }
        // ��������ִ�������
        foreach (var item in abilityExecutionList)
        {
            item.Update();
        }

        // ���и���֡�߼�
        for (int i = 0; i < MapMaker.yRow; i++)
        {
            for (int j = 0; j < MapMaker.xColumn; j++)
            {
                mGridList[i, j].MUpdate();
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

        // ������ص���Ϸ����Żص������
        GameManager.Instance.PushGameObjectFromBufferToPool();
    }
}
