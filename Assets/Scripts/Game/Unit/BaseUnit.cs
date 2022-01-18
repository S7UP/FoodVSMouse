using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseUnit : IGameControllerMember
{
    // 由外部赋值的引用
    public GameObject mGameObject; // 该脚本所管理的游戏对象

    // 管理的变量
    public float mBaseHp; //+ 基础生命值
    public float mMaxHp; //+ 最大生命值
    public float mCurrentHp; //+ 当前生命值
    public float mBaseAttack; //+ 基础攻击力
    public float mCurrentAttack; //+ 当前攻击力
    public float mBaseAttackSpeed; //+ 基础攻击速度
    public float mCurrentAttackSpeed; //+ 当前攻击速度
    public int mAttackCD; //+ 当前攻击间隔
    public int mAttackCDLeft; //+ 攻击间隔计数器
    public int mHeight; //+ 高度
    public int mRow; // 当前所在行数
    public int mColumn; // 当前所在列数

    public string mName; // 当前单位的种类名称
    public int mType; // 当前单位的种类（如不同的卡，不同的老鼠）
    public int mShape; // 当前种类单位的外观（同一张卡的0、1、2转，老鼠的0、1、2转或者其他变种）

    public IBaseActionState mCurrentActionState; //+ 当前动作状态

    public BaseUnit(GameObject gameObject)
    {
        mGameObject = gameObject;
    }

    public virtual void Init()
    {
        // 血量
        mBaseHp = 100;
        mMaxHp = mBaseHp;
        mCurrentHp = mMaxHp;
        // 攻击力
        mBaseAttack = 10;
        mCurrentAttack = mBaseAttack;
        // 攻击速度与攻击间隔
        mBaseAttackSpeed = 0.5f;
        mCurrentAttackSpeed = mBaseAttackSpeed;
        mAttackCD = Mathf.FloorToInt(ConfigManager.fps / (mCurrentAttackSpeed));
        mAttackCDLeft = 0;
        // 高度
        mHeight = 0;

        // 初始化当前动作状态
        SetActionState(new BaseActionState());
    }

    // 切换动作状态
    public void SetActionState(IBaseActionState state)
    {
        if (mCurrentActionState != null)
        {
            mCurrentActionState.OnExit();
        }
        mCurrentActionState = state;
        mCurrentActionState.OnEnter();
    }

    public virtual void Update()
    {
        // 基础数据更新
        if (mAttackCDLeft > 0)
        {
            mAttackCDLeft--;
        }
        // 单位动作状态由状态机决定（如移动、攻击、待机、死亡）
        mCurrentActionState.OnUpdate();
    }

    public virtual void Destory()
    {

    }

    public virtual void Pause()
    {

    }

    public virtual void Resume()
    {

    }
}
