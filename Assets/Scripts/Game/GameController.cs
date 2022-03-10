using LitJson;

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

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
    public GameObject enemyListGo; // ���ڴ�ŵжԵ�λ�ĸ�����
    public GameObject allyListGo; // ���ڴ���ѷ���λ�ĸ�����
    public GameObject gridListGo; // ���ڴ�ŵ�ͼ���ӵĶ���


    public BaseStage mCurrentStage; //+ ��ǰ�ؿ�
    public BaseCostController mCostController; //+ ���ÿ�����
    public BaseCardController mCardController; //+ ��Ƭ������
    public BaseSkillController mSkillController; //+ ���ܿ�����
    public BaseProgressController mProgressController; //+ ��Ϸ���ȿ�����
    public BaseGrid[,] mGridList; //+ ���ӱ�
    public List<BaseUnit> mEnemyList; //+ ���ĵз���λ��
    public List<BaseUnit> mAllyList; // �����ѷ���λ��
    public List<BaseBullet> mBulletList; // �����ӵ���
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
        mEnemyList = new List<BaseUnit>();
        mAllyList = new List<BaseUnit>();
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

        //MouseUnit.Attribute attr = new MouseUnit.Attribute()
        //{
        //    baseAttrbute = new BaseUnit.Attribute()
        //    {
        //        name = "ƽ����", // ��λ�ľ�������
        //        type = 0, // ��λ���ڵķ���
        //        shape = 0, // ��λ�ڵ�ǰ����ı��ֱ��

        //        baseHP = 100, // ����Ѫ��
        //        baseAttack = 10, // ��������
        //        baseAttackSpeed = 1.0, // ���������ٶ�
        //        attackPercent = 0.6,
        //        baseHeight = 0, // �����߶�
        //    },
        //    baseMoveSpeed = 1.0,
        //    hertRateList = new double[] { 0.5}
        //};

        //Debug.Log("��ʼ�浵������Ϣ��");
        //JsonManager.Save(attr, "Mouse/" + attr.baseAttrbute.type + "/" + attr.baseAttrbute.shape + "");
        //Debug.Log("������Ϣ�浵��ɣ�");

        //Debug.Log("��ʼ��ȡ������Ϣ��");
        //mouseAttribute = JsonManager.Load<MouseUnit.Attribute>("Mouse/" + attr.baseAttrbute.type + "/" + attr.baseAttrbute.shape + "");
        //Debug.Log("name=" + mouseAttribute.baseAttrbute.name);
        //Debug.Log("��ȡ������Ϣ�ɹ���");


        MouseUnit.SaveNewMouseInfo(); // ���浱ǰ������Ϣ

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

        // test
        mCurrentStage = new BaseStage();
        mCurrentStage.Save();
        mCurrentStage.DemoLoad();
        mCurrentStage.Init();
        
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

        // testing
        // ��ʳ����
        {
            // BaseUnit unit = CreateFoodUnit(new Vector3(-1.42f, 0.94f, 0));
        }
        // ��������
        for (int i = 6; i < 7; i++)
        {
            //MouseUnit unit = CreateMouseUnit(MapMaker.xColumn - 1, i);

            //// TEST
            //if (i == 6)
            //{
            //    mProgressController.mBossHpBar.SetTarget(unit);
            //}
        }

        // �ؿ���ˢ���߼�ʹ��Э��
        StartCoroutine(mCurrentStage.Start());
    }

    // ���һ����ʳ��λ��ս���У�����
    public FoodUnit AddFoodUnit(FoodUnit food)
    {
        food.transform.SetParent(allyListGo.transform);
        mAllyList.Add(food);
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
        GameObject instance = GameManager.Instance.GetGameObjectResource(FactoryType.GameFactory, "Mouse/Pre_Mouse");
        instance.transform.SetParent(enemyListGo.transform);
        MouseUnit mouse = instance.GetComponent<MouseUnit>();
        mouse.MInit();
        mEnemyList.Add(mouse);
        mGridList[yIndex, xIndex].SetMouseUnitInGrid(mouse, Vector2.right);
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

    public BaseBullet CreateBullet(Vector3 position)
    {
        GameObject instance = GameManager.Instance.GetGameObjectResource(FactoryType.GameFactory, "Bullet/Pre_Bullet");
        BaseBullet bullet = instance.GetComponent<BaseBullet>();
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

    // Update is called once per frame
    void Update()
    {
        mFrameNum ++; // �ȸ�����Ϸ֡���Ա�֤Update������Э�̽��յ���֡����ͬ��
        // ���������Update()
        foreach (IGameControllerMember member in MMemberList)
        {
            member.MUpdate();
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
        for (int i = 0; i < mEnemyList.Count; i++)
        {
            BaseUnit unit = mEnemyList[i];
            if (unit.IsValid())
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
        // �ѷ�֡�߼�
        for (int i = 0; i < mAllyList.Count; i++)
        {
            BaseUnit unit = mAllyList[i];
            if (unit.IsValid())
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
