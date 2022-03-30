using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static ActionPointManager;
/// <summary>
/// 战斗场景中最基本的游戏单位
/// </summary>
public class BaseUnit : MonoBehaviour, IGameControllerMember, IBaseStateImplementor
{
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
        public double baseMoveSpeed; // 基础移速
        public double baseDefense; // 基础减伤
        public double baseRange; // 基础射程
        public int baseHeight; // 基础高度
    }

    // 管理的变量
    public UnitType mUnitType;
    public float mBaseHp { get { return NumericBox.Hp.baseValue; } } //+ 基础生命值
    public float mMaxHp { get { return NumericBox.Hp.Value; } } //+ 最大生命值
    public float mCurrentHp; //+ 当前生命值
    public float mBaseAttack { get { return NumericBox.Attack.baseValue; } } //+ 基础攻击力
    public float mCurrentAttack { get { return NumericBox.Attack.Value; } } //+ 当前攻击力
    public float mBaseAttackSpeed { get { return NumericBox.AttackSpeed.baseValue; } } //+ 基础攻击速度
    public float mCurrentAttackSpeed { get { return NumericBox.AttackSpeed.Value; } } //+ 当前攻击速度
    public float mCurrentDefense { get { return NumericBox.Defense.Value; } } // 防御
    public float mCurrentRange { get { return NumericBox.Range.Value; } } //射程
    public float mBaseMoveSpeed { get { return NumericBox.MoveSpeed.baseValue; } } // 基础移动速度
    public float mCurrentMoveSpeed { get { return NumericBox.MoveSpeed.Value; } } // 当前移动速度
    [SerializeField]
    //public float mCurrentTotalShieldValue { get { return NumericBox.Shield.Value; } } // 当前护盾值之和
    public float mCurrentTotalShieldValue;

    protected float attackPercent; // 攻击动画播放进度到attackPercent以上时允许出真正的攻击
    protected bool mAttackFlag; // 作用于一次攻击能否打出来的flag
    public int mHeight; //+ 高度
    public bool isDeathState; // 是否在死亡状态

    //public HealthPoint CurrentHealth { get; private set; } = new HealthPoint();
    public CombatNumericBox NumericBox { get; private set; } = new CombatNumericBox(); // 存储单位当前属性的盒子
    public ActionPointManager actionPointManager { get; set; } = new ActionPointManager(); // 行动点管理器
    public SkillAbilityManager skillAbilityManager { get; set; } = new SkillAbilityManager(); // 技能管理器

    public string mName; // 当前单位的种类名称
    public int mType; // 当前单位的种类（如不同的卡，不同的老鼠）
    public int mShape; // 当前种类单位的外观（同一张卡的0、1、2转，老鼠的0、1、2转或者其他变种）

    public IBaseActionState mCurrentActionState; //+ 当前动作状态
    public int currentStateTimer = 0; // 当前状态的持续时间（切换状态时会重置）

    protected string jsonPath
    {
        get
        {
            switch (mUnitType)
            {
                case UnitType.Food: return "Food/";
                case UnitType.Mouse: return "Mouse/";
                default: return null;
            }
        }
    } // 存储JSON文件的相对路径

    protected string mPreFabPath 
    { 
        get
        {
            switch(mUnitType)
            {
                case UnitType.Food:return "Food/" + mType;
                case UnitType.Mouse:return "Mouse/" + mType;
                default:return null;
            }
        }
    } // 预制体类型/路径

    public virtual void Awake()
    {
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
        mUnitType = UnitType.Default;
        mCurrentHp = 0; //+ 当前生命值
        attackPercent = 0; // 攻击动画播放进度到attackPercent以上时允许出真正的攻击
        mAttackFlag = false; // 作用于一次攻击能否打出来的flag
        mHeight = 0; //+ 高度
        isDeathState = true;

        mName = null; // 当前单位的种类名称
        mType = 0; // 当前单位的种类（如不同的卡，不同的老鼠）
        mShape = 0; // 当前种类单位的外观（同一张卡的0、1、2转，老鼠的0、1、2转或者其他变种）

        mCurrentActionState = null; //+ 当前动作状态
        currentStateTimer = 0; // 当前状态的持续时间（切换状态时会重置）

        NumericBox.Initialize();
        actionPointManager.Initialize();
        skillAbilityManager.Initialize();
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

    /// <summary>
    /// 根据GameController的指示，系统做的初始化工作
    /// </summary>
    public virtual void MInit()
    {
        BaseUnit.Attribute attr = GameController.Instance.GetBaseAttribute();
        // 几个管理器初始化
        NumericBox.Initialize();
        actionPointManager.Initialize();
        // 种类
        mType = attr.type;
        mShape = attr.shape;
        SetUnitType();
        // 血量
        NumericBox.Hp.SetBase((float)attr.baseHP);
        mCurrentHp = mMaxHp;
        // 攻击力
        NumericBox.Attack.SetBase((float)attr.baseAttack);
        // 攻击速度与攻击间隔
        NumericBox.AttackSpeed.SetBase((float)attr.baseAttackSpeed);
        NumericBox.MoveSpeed.SetBase((float)attr.baseMoveSpeed);
        NumericBox.Defense.SetBase((float)attr.baseDefense);
        attackPercent = (float)attr.attackPercent; // 攻击动画播放进度到attackPercent以上时允许出真正的攻击
        mAttackFlag = true; // 作用于一次攻击能否打出来的flag
        // 高度
        mHeight = attr.baseHeight;
        // 死亡状态
        isDeathState = false;

        // 初始化当前动作状态
        SetActionState(new BaseActionState(this));
        // 设置动作点
        SetActionPointManager();
        // 读取技能信息
        LoadSkillAbility();
    }

    public virtual void MUpdate()
    {
        // 技能管理器更新
        skillAbilityManager.Update();
        // 单位动作状态由状态机决定（如移动、攻击、待机、死亡）
        currentStateTimer += 1; 
        mCurrentActionState.OnUpdate();
    }

    /// <summary>
    /// 由子类Override，设置大分类
    /// </summary>
    public virtual void SetUnitType()
    {

    }

    /// <summary>
    /// 由子类Override，加载目标的技能
    /// </summary>
    public virtual void LoadSkillAbility()
    {

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
    /// 判断有没有被回收
    /// </summary>
    public bool IsValid()
    {
        return isActiveAndEnabled;
    }

    /// <summary>
    /// 判断单位是否在战场上存活
    /// </summary>
    /// <returns></returns>
    public virtual bool IsAlive()
    {
        return IsValid();
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

    /// <summary>
    /// 设置渲染层级
    /// </summary>
    public virtual void UpdateRenderLayer(int arrayIndex)
    {

    }

    public void AddActionPointListener(ActionPointType actionPointType, Action<CombatAction> action)
    {
        actionPointManager.AddListener(actionPointType, action);
    }

    public void RemoveActionPointListener(ActionPointType actionPointType, Action<CombatAction> action)
    {
        actionPointManager.RemoveListener(actionPointType, action);
    }

    public void TriggerActionPoint(ActionPointType actionPointType, CombatAction action)
    {
        actionPointManager.TriggerActionPoint(actionPointType, action);
    }

    /// <summary>
    /// 受到伤害时结算伤害
    /// </summary>
    /// <param name="dmg"></param>
    public virtual void OnDamage(float dmg)
    {
        // 先计算抗性减免后的伤害
        dmg = Mathf.Max(0, dmg * (1 - mCurrentDefense));
        // 然后计算护盾吸收的伤害
        dmg = Mathf.Max(0, NumericBox.DamageShield(dmg));
        // 最后扣除本体生命值
        mCurrentHp -= dmg*(1-mCurrentDefense);
        if (mCurrentHp <= 0)
        {
            BeforeDeath();
        }
    }

    /// <summary>
    /// 受到无视护盾的伤害
    /// </summary>
    /// <param name="dmg"></param>
    public virtual void OnDamgeIgnoreShield(float dmg)
    {
        // 先计算抗性减免后的伤害
        dmg = Mathf.Max(0, dmg * (1 - mCurrentDefense));
        // 最后扣除本体生命值
        mCurrentHp -= dmg * (1 - mCurrentDefense);
        if (mCurrentHp <= 0)
        {
            BeforeDeath();
        }
    }

    /// <summary>
    /// 受到治疗时结算治疗
    /// </summary>
    /// <param name="cure"></param>
    public virtual void OnCure(float cure)
    {
        mCurrentHp += cure;
        if (mCurrentHp > mMaxHp)
            mCurrentHp = mMaxHp;
    }

    /// <summary>
    /// 受到护盾时结算护盾
    /// </summary>
    public virtual void OnAddedShield(float value)
    {
        NumericBox.AddDynamicShield(value);
    }

    /// <summary>
    /// 接收伤害
    /// </summary>
    /// <param name="combatAction"></param>
    public void ReceiveDamage(CombatAction combatAction)
    {
        var damageAction = combatAction as DamageAction;
        OnDamage(damageAction.DamageValue);
    }

    /// <summary>
    /// 接收治疗
    /// </summary>
    /// <param name="combatAction"></param>
    public void ReceiveCure(CombatAction combatAction)
    {
        var cureAction = combatAction as CureAction;
        OnCure(cureAction.CureValue);
    }

    /// <summary>
    /// 接收护盾
    /// </summary>
    /// <param name="combatAction"></param>
    public void ReceiveShield(CombatAction combatAction)
    {
        var shieldAction = combatAction as ShieldAction;
        OnAddedShield(shieldAction.Value);
    }

    /// <summary>
    /// 获取真实治疗值，即计算过治疗增益与减益后的治疗值
    /// </summary>
    public float GetRealCureValue(float oriValue)
    {
        return oriValue;
    }


    /// <summary>
    /// 由子类override,根据实际需求设置动作点监听
    /// </summary>
    public virtual void SetActionPointManager()
    {
        
    }

    public void AddSkillAbility(SkillAbility skillAbility)
    {
        skillAbilityManager.AddSkillAbility(skillAbility);
    }

    public void RemoveSkillAbility(SkillAbility skillAbility)
    {
        skillAbilityManager.RemoveSkillAbility(skillAbility);
    }

    /// <summary>
    /// 普通攻击的条件
    /// </summary>
    /// <returns></returns>
    public virtual bool IsMeetGeneralAttackCondition()
    {
        return false;
    }

    /// <summary>
    /// 在普通攻击之前要做的
    /// </summary>
    public virtual void BeforeGeneralAttack()
    {

    }

    /// <summary>
    /// 在普通攻击期间要做的
    /// </summary>
    public virtual void OnGeneralAttack()
    {

    }
     
    /// <summary>
    /// 结束普通攻击的条件
    /// </summary>
    public virtual bool IsMeetEndGeneralAttackCondition()
    {
        return true;
    }

    /// <summary>
    /// 在普通攻击结束时要做的
    /// </summary>
    public virtual void AfterGeneralAttack()
    {

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
    public virtual void OnCastStateEnter()
    {
    }
    public virtual void OnIdleState()
    {
    }

    public virtual void OnMoveState()
    {
    }

    public virtual void OnAttackState()
    {
    }

    public virtual void OnStaticState()
    {
    }

    public virtual void OnCastState()
    {
    }

    public virtual void OnTransitionStateEnter()
    {
        
    }

    public virtual void OnTransitionState()
    {
        
    }

    public virtual void OnIdleStateExit()
    {
        
    }

    public virtual void OnMoveStateExit()
    {
        
    }

    public virtual void OnAttackStateExit()
    {
        
    }

    public virtual void OnCastStateExit()
    {
        
    }

    public virtual void OnTransitionStateExit()
    {
        
    }
    // 继续
}
