using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 战斗场景中最基本的游戏单位
/// </summary>
public class BaseUnit : MonoBehaviour, IGameControllerMember, IBaseStateImplementor
{
    // 
    public bool isGameObjectValid; // 是否存活
    

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
    public int currentStateTimer = 0; // 当前状态的持续时间（切换状态时会重置）

    public virtual void Awake()
    {
        isGameObjectValid = true;
        currentStateTimer = -1;
    }

    // 切换动作状态
    public void SetActionState(IBaseActionState state)
    {
        if (mCurrentActionState != null)
        {
            mCurrentActionState.OnExit();
        }
        mCurrentActionState = state;
        currentStateTimer = -1; // 重置计数器，重置为-1是保证下一次执行到状态Update时，读取到的计数器值一定为0，详细实现见下述MUpdate()内容
        mCurrentActionState.OnEnter();
    }

    // 受到伤害
    public virtual void OnDamage(float dmg)
    {
        mCurrentHp -= dmg;
        if(mCurrentHp <= 0)
        {
            BeforeDeath();
        }
    }

    public virtual Vector3 GetPosition()
    {
        return gameObject.transform.position;
    }

    // 设置位置
    public virtual void SetPosition(Vector3 V3)
    {
        gameObject.transform.position = V3;
    }

    // 濒死（可能是用来给你抢救的）
    public virtual void BeforeDeath()
    {
        Debug.Log("BeforeDeath()");
        // 队友呢队友呢救一下啊
        // ....
        DuringDeath();
    }

    // 这下是真死了()
    public virtual void DuringDeath()
    {
        // 不知道要干啥了，反正这个地方肯定救不了了
        Debug.Log("DuringDeath()");
        GameManager.Instance.RecycleUnit(this, "Food/Pre_Food");
        AfterDeath();
    }

    // 还愣着干什么，人没了救不了了
    public virtual void AfterDeath()
    {
        Debug.Log("AfterDeath()");
        // 我死了也要化身为腻鬼！！
    }


    // 以下为 IGameControllerMember 接口方法的实现
    public virtual void MInit()
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
        SetActionState(new BaseActionState(this));
    }

    public virtual void MUpdate()
    {
        // 基础数据更新
        if (mAttackCDLeft > 0)
        {
            mAttackCDLeft--;
        }
        // 单位动作状态由状态机决定（如移动、攻击、待机、死亡）
        currentStateTimer += 1; 
        mCurrentActionState.OnUpdate();
    }

    public virtual void MDestory()
    {

    }

    public virtual void MPause()
    {

    }

    public virtual void MResume()
    {

    }

    // 以下为 IBaseStateImplementor 接口的方法实现
    public virtual void OnIdleState()
    {
        throw new System.NotImplementedException();
    }

    public virtual void OnMoveState()
    {
        throw new System.NotImplementedException();
    }

    public virtual void OnAttackState()
    {
        throw new System.NotImplementedException();
    }

    public virtual void OnStaticState()
    {
        throw new System.NotImplementedException();
    }

    // 继续
}
