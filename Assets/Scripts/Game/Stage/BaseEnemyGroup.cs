using System;
using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// 刷怪的基本单位：组
/// </summary>
[Serializable]
public class BaseEnemyGroup
{
    /// <summary>
    /// 敌人信息，只要知道敌人的种类和形态分支就知道要生成什么敌人了
    /// </summary>
    [System.Serializable]
    public struct EnemyInfo
    {
        public int type;
        public int shape;
    }

    /// <summary>
    /// 当前组敌人在游戏中实际出的行数表
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

    // 空的EnemyList
    public static RealEnemyList RealEnemyNullList = new RealEnemyList() {
        enemyInfo = new EnemyInfo() {
            type = 0,
            shape = 0
        },
        rowIndexList = new List<int>()
    };

    // 分路组表
    public int mApartIndex; // 处在关卡的第几分路组下标
    public int mStartIndex; // 在当前分路组下的第几下标对应的行开始作为怪的起始点
    public EnemyInfo mEnemyInfo; // 敌人种类
    public int mCount; // 敌人数量

    /// <summary>
    /// 初始化方法
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
    /// 获取一个已初始化好的默认的EnemyGroup实例
    /// </summary>
    /// <returns></returns>
    public static BaseEnemyGroup GetInitalEnemyGroupInfo()
    {
        BaseEnemyGroup enemyGroup = new BaseEnemyGroup();
        enemyGroup.Init();
        return enemyGroup;
    }

    /// <summary>
    /// 根据当前关卡的对应分路组来确定实际出怪行数表
    /// </summary>
    /// <param name="rowOffset">行偏移量，主要影响刷怪起始行</param>
    /// <returns></returns>    
    public RealEnemyList TransFormToRealEnemyGroup()
    {
        List<int> rowIndexList = new List<int>();
        List<List<int>> stageApartList = GameController.Instance.mCurrentStage.GetApartList();
        // 安全性检测
        if (stageApartList==null || stageApartList.Count<= mApartIndex)
        {
            Debug.LogWarning("当前组下标超过关卡最大组下标，越界了！因此当前组不执行刷怪！");
            return BaseEnemyGroup.RealEnemyNullList; // 返回一个空的
        }
        // 从所在关卡分路组中获取行映射表
        List<int> rowMap = stageApartList[mApartIndex];
        // 保证起始下标在行映射表范围之内
        int startIndex = (mStartIndex+ GameController.Instance.mCurrentStage.GetApartRowOffsetByIndex(mApartIndex)) % rowMap.Count;
        // 从起始下标起算至起初下标+mCount结束,开始填表
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