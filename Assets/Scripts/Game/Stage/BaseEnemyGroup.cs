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
    public int mCount; // ��������

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
}