using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 战斗场景中最基本的游戏单位
/// </summary>
public class BaseUnit : MonoBehaviour, IGameControllerMember, IBaseStateImplementor
{
    protected string jsonPath; // 存储JSON文件的相对路径

    // 需要本地存储的变量
    [System.Serializable]
    public struct Attribute
    {
        public string name; // 单位的具体名称
        public int type; // 单位属于的分类
        public int shape; // 单位在当前分类的变种编号

        public double baseHP; // 基础血量
        public double baseAttack; // 基础攻击
        public double baseAttackSpeed; // 基础攻击速度
        public double attackPercent; // 出攻击判定时动画播放的百分比
        public int baseHeight; // 基础高度
    }

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
    protected float attackPercent; // 攻击动画播放进度到attackPercent以上时允许出真正的攻击
    protected bool mAttackFlag; // 作用于一次攻击能否打出来的flag
    public int mHeight; //+ 高度
    public bool isDeathState; // 是否在死亡状态


    public string mName; // 当前单位的种类名称
    public int mType; // 当前单位的种类（如不同的卡，不同的老鼠）
    public int mShape; // 当前种类单位的外观（同一张卡的0、1、2转，老鼠的0、1、2转或者其他变种）

    public IBaseActionState mCurrentActionState; //+ 当前动作状态
    public int currentStateTimer = 0; // 当前状态的持续时间（切换状态时会重置）

    protected string mPreFabPath; // 预制体类型/路径

    public virtual void Awake()
    {
        jsonPath = "";
    }

    // 单位被对象池重新取出或者刚创建时触发
    public virtual void OnEnable()
    {
        // MInit();
    }

    // 单位被对象池回收时触发
    public virtual void OnDisable()
    {
        // 管理的变量
        mBaseHp = 0; //+ 基础生命值
        mMaxHp = 0; //+ 最大生命值
        mCurrentHp = 0; //+ 当前生命值
        mBaseAttack = 0; //+ 基础攻击力
        mCurrentAttack = 0; //+ 当前攻击力
        mBaseAttackSpeed = 0; //+ 基础攻击速度
        mCurrentAttackSpeed = 0; //+ 当前攻击速度
        mAttackCD = 0; //+ 当前攻击间隔
        mAttackCDLeft = 0; //+ 攻击间隔计数器
        attackPercent = 0; // 攻击动画播放进度到attackPercent以上时允许出真正的攻击
        mAttackFlag = false; // 作用于一次攻击能否打出来的flag
        mHeight = 0; //+ 高度
        isDeathState = true;

        mName = null; // 当前单位的种类名称
        mType = 0; // 当前单位的种类（如不同的卡，不同的老鼠）
        mShape = 0; // 当前种类单位的外观（同一张卡的0、1、2转，老鼠的0、1、2转或者其他变种）

        mCurrentActionState = null; //+ 当前动作状态
        currentStateTimer = 0; // 当前状态的持续时间（切换状态时会重置）
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

    /// <summary>
    /// 计算当前血量百分比
    /// </summary>
    /// <returns></returns>
    public float GetHeathPercent()
    {
        return mCurrentHp / mMaxHp;
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
        // 队友呢队友呢救一下啊
        // 子类override一下,再判断一下条件，大不了不调用该类下面的方法了，就能救的！
        // TNND,为什么不喝！？都不喝是吧！都怕死是吧！.wav

        // 进入死亡动画状态
        isDeathState = true;
        SetActionState(new DieState(this));
    }

    // 这下是真死了，死的时候还有几帧状态，要持续做
    public virtual void DuringDeath()
    {
        // 不知道要干啥了，反正这个地方肯定救不了了
    }

    // 还愣着干什么，人没了救不了了
    public virtual void AfterDeath()
    {
        // 我死了也要化身为腻鬼！！
        // override
        // 然后安心去世吧

        GameManager.Instance.PushGameObjectToFactory(FactoryType.GameFactory, mPreFabPath, this.gameObject);
    }


    // 以下为 IGameControllerMember 接口方法的实现
    public virtual void MInit()
    {
        //// 血量
        //mBaseHp = 100;
        //mMaxHp = mBaseHp;
        //mCurrentHp = mMaxHp;
        //// 攻击力
        //mBaseAttack = 10;
        //mCurrentAttack = mBaseAttack;
        //// 攻击速度与攻击间隔
        //mBaseAttackSpeed = 0.5f;
        //mCurrentAttackSpeed = mBaseAttackSpeed;
        //mAttackCD = Mathf.FloorToInt(ConfigManager.fps / (mCurrentAttackSpeed));
        //mAttackCDLeft = 0;
        //attackPercent = 0.60f; // 攻击动画播放进度到attackPercent以上时允许出真正的攻击
        //mAttackFlag = true; // 作用于一次攻击能否打出来的flag
        //// 高度
        //mHeight = 0;

        BaseUnit.Attribute attr = GameController.Instance.GetBaseAttribute();
        // 血量
        mBaseHp = (float)attr.baseHP;
        mMaxHp = mBaseHp;
        mCurrentHp = mMaxHp;
        // 攻击力
        mBaseAttack = (float)attr.baseAttack;
        mCurrentAttack = mBaseAttack;
        // 攻击速度与攻击间隔
        mBaseAttackSpeed = (float)attr.baseAttackSpeed;
        mCurrentAttackSpeed = mBaseAttackSpeed;
        mAttackCD = Mathf.FloorToInt(ConfigManager.fps / (mCurrentAttackSpeed));
        mAttackCDLeft = 0;
        attackPercent = (float)attr.attackPercent; // 攻击动画播放进度到attackPercent以上时允许出真正的攻击
        mAttackFlag = true; // 作用于一次攻击能否打出来的flag
        // 高度
        mHeight = attr.baseHeight;
        // 死亡状态
        isDeathState = false;

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

    /// <summary>
    /// 这个单位是否还在战场上存活
    /// </summary>
    public bool IsValid()
    {
        return isActiveAndEnabled;
    }

    /// <summary>
    /// 获取当前单位所在行下标
    /// </summary>
    /// <returns></returns>
    public virtual int GetRowIndex()
    {
        return MapManager.GetYIndex(transform.position.y);
    }

    /// <summary>
    /// 获取当前单位所在列下标
    /// </summary>
    /// <returns></returns>
    public virtual int GetColumnIndex()
    {
        return MapManager.GetXIndex(transform.position.x);
    }

    /// <summary>
    /// 自身是否能阻挡传进来的unit，请在子类中重写这个方法以实现多态
    /// </summary>
    /// <returns></returns>
    public virtual bool CanBlock(BaseUnit unit)
    {
        return false;
    }

    /// <summary>
    /// 自身能否被传进来的bullet命中，请在子类中重写这个方法以实现多态
    /// </summary>
    /// <returns></returns>
    public virtual bool CanHit(BaseBullet bullet)
    {
        return false;
    }

    /// <summary>
    /// 状态相关
    /// </summary>
    public virtual void OnIdleStateEnter()
    {

    }
    public virtual void OnMoveStateEnter()
    {

    }
    public virtual void OnAttackStateEnter()
    {

    }
    public virtual void OnDieStateEnter()
    {

    }
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


    /// <summary>
    /// 当进入静止状态时
    /// </summary>
    public virtual void OnStaticStateEnter()
    {

    }

    /// <summary>
    /// 当退出静止状态时
    /// </summary>
    public virtual void OnStaticStateExit()
    {

    }

    // 继续
}
