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

    // ����
    public static GameController Instance { get => _instance; } //+ ������
    // �ɱ༭������������
    public GameObject enemyListGo; // ���ڴ�ŵжԵ�λ�ĸ�����
    public GameObject allyListGo; // ���ڴ���ѷ���λ�ĸ�����


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
        // ��ʳ����
        {
            BaseUnit unit = CreateFoodUnit(new Vector3(-1.42f, 0.94f, 0));
        }
        // ��������
        for (int i = 0; i < 2; i++)
        {
            MouseUnit unit = CreateMouseUnit(new Vector3(3.73f, 1.452f - 0.65f * 2 * i, 0));
        }

    }

    // ������ʳ��λ
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

    // ��������λ
    public MouseUnit CreateMouseUnit(Vector3 position)
    {
        GameObject instance = GameManager.Instance.GetGameObjectResource(FactoryType.GameFactory, "Mouse/Pre_HugeMouse");
        instance.transform.SetParent(enemyListGo.transform);
        MouseUnit mouse = instance.GetComponent<MouseUnit>();
        mouse.MInit();
        mEnemyList.Add(mouse);
        // �ڸ�����Ϸ����㼶��ϵʱ��ʹ�ø�������λ�û�ʧЧ��������ע�ʹ��룬���������Ǹ�SetParent�����ֻ��ǿ��transform
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

    // ����������λ
    public void CreateOtherUnit()
    {

    }


    // Update is called once per frame
    void Update()
    {
        mFrameNum++;
        // Debug.Log("Frame:"+mFrameNum);
        // ���������Update()
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
