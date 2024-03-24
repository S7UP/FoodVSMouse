using System;
using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// ˢ�ֵĻ�����λ����
/// </summary>
[Serializable]
public class BaseEnemyGroup
{
    /// <summary>
    /// ������Ϣ��ֻҪ֪�����˵��������̬��֧��֪��Ҫ����ʲô������
    /// </summary>
    [System.Serializable]
    public struct EnemyInfo
    {
        public int type;
        public int shape;
    }

    /// <summary>
    /// ���˸������ԣ���v0.26�汾��ӣ����ڣ�������Զ����˵�������
    /// </summary>
    [System.Serializable]
    public class EnemyAttribute
    {
        public float HpRate; // ����ֵ���ʣ�Ĭ��Ϊ1.0
        public float AttackRate; // ���������ʣ�Ĭ��Ϊ1.0
        public float MoveSpeedRate; // �����ƶ��ٶȱ��ʣ�Ĭ��Ϊ1.0
        public float Defence; // ��ʼ���ˣ�Ĭ��Ϊ0��ȡֵ��ΧΪ0~100��100ʱ����100%���ˣ���ǹ���룩

        public Dictionary<string, List<float>> ParamDict; // �����ֵ䣬���ڸ���ԭ�й��ض�����

        public EnemyAttribute()
        {
            HpRate = 1.0f;
            AttackRate = 1.0f;
            MoveSpeedRate = 1.0f;
            Defence = 0.0f;
            ParamDict = new Dictionary<string, List<float>>();
        }

        public EnemyAttribute DeepCopy()
        {
            EnemyAttribute new_attr = new EnemyAttribute();
            new_attr.HpRate = HpRate;
            new_attr.AttackRate = AttackRate;
            new_attr.MoveSpeedRate = MoveSpeedRate;
            new_attr.Defence = Defence;
            foreach (var keyValuePair in ParamDict)
            {
                List<float> ls = new List<float>();
                foreach (var val in keyValuePair.Value)
                    ls.Add(val);
                new_attr.ParamDict.Add(keyValuePair.Key, ls);
            }
            return new_attr;
        }
    }

    /// <summary>
    /// ��ǰ���������Ϸ��ʵ�ʳ���������
    /// </summary>
    public struct RealEnemyList
    {
        public EnemyInfo enemyInfo;
        public List<int> rowIndexList;

        public EnemyInfo GetEnemyInfo()
        {
            return enemyInfo;
        }

        public int Get(int i)
        {
            return rowIndexList[i];
        }

        public int GetSize()
        {
            return rowIndexList.Count;
        }
    }

    // �յ�EnemyList
    public static RealEnemyList RealEnemyNullList = new RealEnemyList() {
        enemyInfo = new EnemyInfo() {
            type = 0,
            shape = 0
        },
        rowIndexList = new List<int>()
    };

    // ��·���
    public int mApartIndex; // ���ڹؿ��ĵڼ���·���±�
    public int mStartIndex; // �ڵ�ǰ��·���µĵڼ��±��Ӧ���п�ʼ��Ϊ�ֵ���ʼ��
    public EnemyInfo mEnemyInfo; // ��������
    public int mCount; // ��С�ֿ��ã���������
    public float mHp; // ��BOSS���ã�BOSSѪ��
    public EnemyAttribute mEnemyAttribute;

    /// <summary>
    /// ��ʼ������
    /// </summary>
    public void Init(int apartIndex, int startIndex, EnemyInfo enemyInfo, int count)
    {
        mApartIndex = apartIndex;
        mStartIndex = startIndex;
        mEnemyInfo = enemyInfo;
        mCount = count;
    }

    public void Init()
    {
        mApartIndex = 0;
        mStartIndex = 0;
        mEnemyInfo = new EnemyInfo() { 
            type = 0,
            shape = 0
        };
        mCount = 1;
    }

    /// <summary>
    /// ��ȡһ���ѳ�ʼ���õ�Ĭ�ϵ�EnemyGroupʵ��
    /// </summary>
    /// <returns></returns>
    public static BaseEnemyGroup GetInitalEnemyGroupInfo()
    {
        BaseEnemyGroup enemyGroup = new BaseEnemyGroup();
        enemyGroup.Init();
        return enemyGroup;
    }

    /// <summary>
    /// ���ݵ�ǰ�ؿ��Ķ�Ӧ��·����ȷ��ʵ�ʳ���������
    /// </summary>
    /// <param name="rowOffset">��ƫ��������ҪӰ��ˢ����ʼ��</param>
    /// <returns></returns>    
    public RealEnemyList TransFormToRealEnemyGroup()
    {
        List<int> rowIndexList = new List<int>();
        List<List<int>> stageApartList = GameController.Instance.mCurrentStage.GetApartList();
        // ��ȫ�Լ��
        if (stageApartList==null || stageApartList.Count<= mApartIndex)
        {
            Debug.LogWarning("��ǰ���±곬���ؿ�������±꣬Խ���ˣ���˵�ǰ�鲻ִ��ˢ�֣�");
            return BaseEnemyGroup.RealEnemyNullList; // ����һ���յ�
        }
        // �����ڹؿ���·���л�ȡ��ӳ���
        List<int> rowMap = stageApartList[mApartIndex];
        // ��֤��ʼ�±�����ӳ���Χ֮��
        int startIndex = (mStartIndex+ GameController.Instance.mCurrentStage.GetApartRowOffsetByIndex(mApartIndex)) % rowMap.Count;
        // ����ʼ�±�����������±�+mCount����,��ʼ���
        for (int i = 0; i < mCount; i++)
        {
            int index = (startIndex + i) % rowMap.Count;
            rowIndexList.Add(rowMap[index]);
        }
        return new RealEnemyList()
        {
            enemyInfo = mEnemyInfo,
            rowIndexList = rowIndexList
        };
    }

    /// <summary>
    /// ���Ⱪ¶��Ψһ��ȡ���ԵĽӿ�
    /// </summary>
    public EnemyAttribute GetEnemyAttribute()
    {
        if (mEnemyAttribute == null)
            mEnemyAttribute = new EnemyAttribute();
        return mEnemyAttribute;
    }

    public BaseEnemyGroup DeepCopy()
    {
        BaseEnemyGroup new_group = new BaseEnemyGroup();
        new_group.mApartIndex = mApartIndex;
        new_group.mStartIndex = mStartIndex;
        new_group.mEnemyInfo = mEnemyInfo;
        new_group.mCount = mCount;
        new_group.mHp = mHp;
        new_group.mEnemyAttribute = mEnemyAttribute.DeepCopy();
        return new_group;
    }
}