using System;
using System.Collections.Generic;

using S7P.Numeric;
using S7P.Component;
using S7P.State;
using UnityEngine;
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
    public float mBaseHp { get { return NumericBox.Hp.baseValue; } } // 基础生命值
    public float mMaxHp { get { return NumericBox.Hp.Value; } } // 最大生命值
    public float mCurrentHp
    {
        get
        {
            if (GetCurrentHpFunc != null)
                return GetCurrentHpFunc(this);
            return _mCurrentHp;
        }
        set { _mCurrentHp = value; }
    }
    private float _mCurrentHp; // 当前生命值
    private Func<BaseUnit, float> GetCurrentHpFunc;
    public float mBaseAttack { get { return NumericBox.Attack.baseValue; } } // 基础攻击力
    public float mCurrentAttack { get { return NumericBox.Attack.Value; } } // 当前攻击力
    public float mBaseAttackSpeed { get { return NumericBox.AttackSpeed.baseValue; } } // 基础攻击速度
    public float mCurrentAttackSpeed { get { return NumericBox.AttackSpeed.Value; } } // 当前攻击速度
    public float mCurrentDefense { get { return NumericBox.Defense.Value; } } // 防御
    public float mBaseMoveSpeed { get { return NumericBox.MoveSpeed.baseValue; } } // 基础移动速度
    public float mCurrentMoveSpeed { get { return NumericBox.MoveSpeed.Value; } } // 当前移动速度
    public float mDamgeRate
    {
        get
        {
            if (GetDamageRateFunc != null)
                return GetDamageRateFunc(this);
            return NumericBox.DamageRate.TotalValue;
        }
    } 
    private Func<BaseUnit, float> GetDamageRateFunc;
    public float mBurnRate
    {
        get
        {
            if (GetBurnRateFunc != null)
                return GetBurnRateFunc(this);
            return NumericBox.BurnRate.TotalValue;
        }
    } // 灰烬伤害比率
    private Func<BaseUnit, float> GetBurnRateFunc;
    public float mAoeRate
    {
        get
        {
            if (GetAoeRateFunc != null)
                return GetAoeRateFunc(this);
            return NumericBox.AoeRate.TotalValue;
        }
    } // 范围伤害比率
    private Func<BaseUnit, float> GetAoeRateFunc;

    public float mCurrentTotalShieldValue;

    protected float attackPercent; // 攻击动画播放进度到attackPercent以上时允许出真正的攻击
    protected bool mAttackFlag; // 作用于一次攻击能否打出来的flag
    public int mHeight; //+ 高度
    public Vector2 moveRotate; // 移动方向
    // 以下三个为图片精灵偏移量
    public FloatNumeric SpriteOffsetX = new FloatNumeric();
    public FloatNumeric SpriteOffsetY = new FloatNumeric();
    public Vector2 SpriteOffset { get { return new Vector2(SpriteOffsetX.Value, SpriteOffsetY.Value); } }
    public bool isFrozenState; // 是否在冻结状态
    public bool isDeathState{ get; set; } // 是否在死亡状态
    public bool isDisableSkill { get { return NumericBox.IsDisableSkill.Value; } } // 是否禁用主动技能

    public CombatNumericBox NumericBox { get; private set; } = new CombatNumericBox(); // 存储单位当前属性的盒子
    public ActionPointManager actionPointManager { get; set; } = new ActionPointManager(); // 行动点管理器
    public SkillAbilityManager skillAbilityManager { get; set; } = new SkillAbilityManager(); // 技能管理器
    public StatusAbilityManager statusAbilityManager { get; set; } = new StatusAbilityManager(); // 时效状态（BUFF）管理器
    public Dictionary<EffectType, BaseEffect> effectDict = new Dictionary<EffectType, BaseEffect>(); // 自身持有唯一性特效
    public Dictionary<string, BaseEffect> effectDict2 = new Dictionary<string, BaseEffect>(); // 自身持有唯一性特效（但是是用string作为key的）
    private bool isHideEffect = false; // 是否隐藏特效
    public AnimatorController animatorController = new AnimatorController(); // 动画播放控制器
    public TaskController taskController = new TaskController();
    public HitBox hitBox = new HitBox();
    public RecordDamageComponent mRecordDamageComponent;
    public bool isIgnoreRecordDamage; // 是否无视内伤

    public string mName; // 当前单位的种类名称
    public int mType; // 当前单位的种类（如不同的卡，不同的老鼠）
    public int mShape; // 当前种类单位的外观（同一张卡的0、1、2转，老鼠的0、1、2转或者其他变种）
    public int typeAndShapeValue;

    public IBaseActionState mCurrentActionState; //+ 当前动作状态
    public int currentStateTimer = 0; // 当前状态的持续时间（切换状态时会重置）
    public bool disableMove = false; // 禁止移动

    public int aliveTime = 0; // 当前对象从生成到现在的存活时间
    public Vector2 lastPosition; // 上帧位置
    public Vector2 DeltaPosition { get; private set; } // 当前帧相较于上帧的位置变化量

    // 事件
    private List<Action<BaseUnit>> BeforeDeathEventList = new List<Action<BaseUnit>>();
    private List<Action<BaseUnit>> BeforeBurnEventList = new List<Action<BaseUnit>>();
    private List<Action<BaseUnit>> AfterDeathEventList = new List<Action<BaseUnit>>();
    private List<Action<BaseUnit>> OnDestoryActionList = new List<Action<BaseUnit>>();

    public List<Func<BaseUnit, BaseUnit, bool>> CanBlockFuncList = new List<Func<BaseUnit, BaseUnit, bool>>(); // 两个单位能否互相阻挡的额外判断事件
    public List<Func<BaseUnit, BaseBullet, bool>> CanHitFuncList = new List<Func<BaseUnit, BaseBullet, bool>>(); // 单位与子弹能否相互碰撞的额外判断事件
    public List<Func<BaseUnit, BaseUnit, bool>> CanBeSelectedAsTargetFuncList = new List<Func<BaseUnit, BaseUnit, bool>>(); // 能否被选取作为目标（第一个参数代表自己，第二个参数代表其他传入者）

    public Dictionary<string, BaseUnit> mAttachedUnitDict = new Dictionary<string, BaseUnit>(); // 其他附加在该单位上的单位引用

    public StateController mStateController = new StateController();
    public ComponentController mComponentController = new ComponentController();


    private bool isUseDefaultRecieveDamageActionMethod = true; // 在接收伤害时，是否使过默认的接收伤害逻辑

    protected string jsonPath
    {
        get
        {
            switch (mUnitType)
            {
                case UnitType.Food: return "Food/";
                case UnitType.Mouse: return "Mouse/";
                case UnitType.Item: return "Item/";
                case UnitType.Character: return "Character/";
                case UnitType.Boss: return "Boss/";
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
                case UnitType.Item: return "Item/" + mType + "/"+mShape;
                case UnitType.Character: return "Character/"+mType + "/" + mShape;
                case UnitType.Boss: return "Boss/" + mType + "/" + mShape;
                default:return null;
            }
        }
    } // 预制体类型/路径

    public virtual void Awake()
    {
        mRecordDamageComponent = new RecordDamageComponent(this);
    }

    public virtual void MInit()
    {
        aliveTime = 0;
        lastPosition = Vector2.one;
        DeltaPosition = Vector2.one;

        // 几个管理器初始化
        NumericBox.Initialize();
        actionPointManager.Initialize();
        skillAbilityManager.Initialize();
        statusAbilityManager.Initialize();
        effectDict.Clear();
        effectDict2.Clear();
        isHideEffect = false;
        animatorController.Initialize();
        hitBox.Initialize();
        SpriteOffsetX.Initialize(); SpriteOffsetY.Initialize();
        SetSpriteLocalPosition(Vector2.zero);
        taskController.Initial();
        mRecordDamageComponent.Initilize();
        isIgnoreRecordDamage = false;

        typeAndShapeValue = 0; // 图层
        SetUnitType();

        mAttackFlag = true; // 作用于一次攻击能否打出来的flag

        // 死亡状态
        isDeathState = false;
        disableMove = false;

        // 初始化当前动作状态
        mCurrentActionState = null;
        SetActionState(new BaseActionState(this));
        // 设置动作点
        SetActionPointManager();
        // 读取技能信息
        LoadSkillAbility();
        // 启用判定
        OpenCollision();
        // 设置Collider2D的参数
        SetCollider2DParam();
        // 大小初始化
        SetLocalScale(Vector2.one);
        SetSpriteLocalScale(Vector2.one);
        transform.right = Vector2.right;

        BeforeDeathEventList.Clear();
        BeforeBurnEventList.Clear();
        AfterDeathEventList.Clear();
        OnDestoryActionList.Clear();

        CanBlockFuncList.Clear();
        CanHitFuncList.Clear();
        CanBeSelectedAsTargetFuncList.Clear();

        mAttachedUnitDict.Clear();

        // 初始化透明度
        SetAlpha(1);

        mStateController.Initial();
        mComponentController.Initial();

        isUseDefaultRecieveDamageActionMethod = true;
        GetCurrentHpFunc = null;
        GetDamageRateFunc = null;
        GetBurnRateFunc = null;
        GetAoeRateFunc = null;
    }

    public virtual void MUpdate()
    {
        // 清除依附单位字典无效的单位
        List<string> delList = new List<string>();
        foreach (var keyValuePair in mAttachedUnitDict)
        {
            if (keyValuePair.Value == null || !keyValuePair.Value.IsAlive())
                delList.Add(keyValuePair.Key);
        }
        foreach (var key in delList)
        {
            RemoveUnitFromDict(key);
        }
        // 动画控制器更新
        animatorController.Update();
        // 时效性状态管理器更新
        statusAbilityManager.Update();
        // 技能管理器更新
        if (!isDisableSkill)
            skillAbilityManager.Update();
        // 当不处于冻结状态时，当前状态计时器每帧都+1
        if (!isFrozenState)
            currentStateTimer += 1;
        // 单位动作状态由状态机决定（如移动、攻击、待机、冻结、死亡）
        if (mCurrentActionState != null)
            mCurrentActionState.OnUpdate();
        // 当前位置状态更新
        if (aliveTime <= 0)
        {
            lastPosition = transform.position;
            DeltaPosition = Vector2.zero;
        }
        else
        {
            DeltaPosition = (Vector2)transform.position - lastPosition;
        }

        // 受击进度盒子更新
        hitBox.Update();
        // 挂载任务更新
        if (!isDeathState)
            taskController.Update();
        // 特效存活检测
        CheckAndUpdateEffectDict();
        // lastPosition更新放在Task后面，这样也方便内置任务获取上帧坐标
        lastPosition = transform.position;

        mStateController.Update();
        mComponentController.Update();

        // 最后更新存活时间
        aliveTime++;
    }

    public virtual void MPause()
    {
        animatorController.Pause();
    }

    public virtual void MResume()
    {
        animatorController.Resume();
    }

    public virtual void MDestory()
    {
        foreach (var action in OnDestoryActionList)
        {
            action(this);
        }
        // 初始化透明度
        SetAlpha(1);

        mStateController.Destory();
        mComponentController.Destory();

        if (GetSpriteRenderer() != null)
            GetSpriteRenderer().sprite = null;
        ExecuteRecycle();
    }


    // 切换动作状态
    public void SetActionState(IBaseActionState state)
    {
        // 若当前状态为死亡状态，则不能通过此方法再切换成别的状态
        //if (mCurrentActionState!=null && (mCurrentActionState is DieState || mCurrentActionState is BurnState || mCurrentActionState is DropState))
        //    return;

        if (isDeathState)
            return;

        if (state is FrozenState)
        {
            if (isFrozenState)
                return;

            if (mCurrentActionState != null)
                mCurrentActionState.OnInterrupt();
            mCurrentActionState = state;
            mCurrentActionState.OnEnter();
            // 不重置计数器，并且在Update中停止计数
        }
        else
        {
            if (mCurrentActionState != null && mCurrentActionState is FrozenState && !isFrozenState)
            {
                // 如果切换前的状态是冰冻，则不能执行OnExit（），因为其OnExit（）会包括该方法，最后会导致无限递归
                mCurrentActionState = state;
                // 不重置计数器，同时启用解冻后状态的继续方法
                mCurrentActionState.OnContinue();
            }
            else
            {
                if (mCurrentActionState != null)
                {
                    mCurrentActionState.OnExit();
                }
                mCurrentActionState = state;
                currentStateTimer = -1; // 重置计数器，重置为-1是保证下一次执行到状态Update时，读取到的计数器值一定为0，详细实现见下述MUpdate()内容
                mCurrentActionState.OnEnter();
            }

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

    /// <summary>
    /// 获取当前生命值
    /// </summary>
    /// <returns></returns>
    public float GetCurrentHp()
    {
        return mCurrentHp;
    }

    /// <summary>
    /// 获取已损失生命值
    /// </summary>
    /// <returns></returns>
    public float GetLostHp()
    {
        return mMaxHp - mCurrentHp;
    }

    public virtual Vector3 GetPosition()
    {
        return transform.position;
    }

    // 设置位置
    public virtual void SetPosition(Vector3 V3)
    {
        transform.position = V3;
    }

    // 濒死（可能是用来给你抢救的）
    public virtual void BeforeDeath()
    {
        // 只能触发一次
        if (isDeathState)
            return;

        // 队友呢队友呢救一下啊
        // 子类override一下,再判断一下条件，大不了不调用该类下面的方法了，就能救的！
        // TNND,为什么不喝！？都不喝是吧！都怕死是吧！.wav

        // 死亡事件
        foreach (var item in BeforeDeathEventList)
        {
            item(this);
        }
        // 清除技能效果
        skillAbilityManager.TryEndAllSpellingSkillAbility();
        // 清除BUFF效果
        statusAbilityManager.TryEndAllStatusAbility();
        // 清除任务效果
        // taskController.Initial();
        // 清除特效一次
        RemoveAllEffect();
        // 移除所有不让动画播放的修饰
        animatorController.RemoveAllPauseModifier();
        // 进入死亡动画状态
        SetActionState(new DieState(this));
        isDeathState = true;
    }

    // 这下是真死了，死的时候还有几帧状态，要持续做
    public virtual void DuringDeath()
    {
        // 不知道要干啥了，反正这个地方肯定救不了了
    }

    /// <summary>
    /// 直接让目标死亡（只有经过AfterDeath()，不经过BeforeDeath()和DuringDeath()，注意与ExecuteDeath()有所区别)
    /// </summary>
    public void DeathEvent()
    {
        // 再清除技能效果一次
        skillAbilityManager.TryEndAllSpellingSkillAbility();
        // 再清除BUFF效果一次
        statusAbilityManager.TryEndAllStatusAbility();
        // 再清除特效一次
        RemoveAllEffect();
        // 清除任务效果
        taskController.Initial();
        // 移除所有不让动画播放的修饰
        animatorController.RemoveAllPauseModifier();
        AfterDeath();
        foreach (var item in AfterDeathEventList)
        {
            item(this);
        }
        // 然后安心去世吧
        MDestory();
    }

    /// <summary>
    /// 单位死亡后事件
    /// </summary>
    public virtual void AfterDeath()
    {
        // 我死了也要化身为腻鬼！！
    }

    /// <summary>
    /// 执行该单位回收事件
    /// </summary>
    protected virtual void ExecuteRecycle()
    {
        GameManager.Instance.PushGameObjectToFactory(FactoryType.GameFactory, mPreFabPath, this.gameObject);
    }


    /// <summary>
    /// 灰烬效果之前
    /// </summary>
    public virtual void BeforeBurn()
    {
        // 只能触发一次
        if (isDeathState)
            return;

        // 死亡事件
        foreach (var item in BeforeBurnEventList)
        {
            item(this);
        }
        // 清除技能效果
        skillAbilityManager.TryEndAllSpellingSkillAbility();
        // 清除BUFF效果
        statusAbilityManager.TryEndAllStatusAbility();
        // 清除任务效果
        // taskController.Initial();
        // 清除特效一次
        RemoveAllEffect();
        // 移除所有不让动画播放的修饰
        animatorController.RemoveAllPauseModifier();
        // 进入死亡动画状态
        SetActionState(new BurnState(this));
        isDeathState = true;
    }

    /// <summary>
    /// 进入灰烬状态后立即触发的事件
    /// </summary>
    public virtual void OnBurnStateEnter()
    {

    }
    /// <summary>
    /// 在灰烬状态下持续做的事（化灰）
    /// </summary>
    /// <param name="_Threshold"></param>
    public virtual void DuringBurn(float _Threshold)
    {

    }

    /// <summary>
    /// 摔落死亡瞬间
    /// </summary>
    public virtual void OnDropStateEnter()
    {

    }


    /// <summary>
    /// 掉落前
    /// </summary>
    public virtual void BeforeDrop()
    {
        // 只能触发一次
        if (isDeathState)
            return;

        // 死亡事件
        foreach (var item in BeforeBurnEventList)
        {
            item(this);
        }
        // 清除技能效果
        skillAbilityManager.TryEndAllSpellingSkillAbility();
        // 清除BUFF效果
        statusAbilityManager.TryEndAllStatusAbility();
        // 清除任务效果
        // taskController.Initial();
        // 清除特效一次
        RemoveAllEffect();
        // 进入死亡动画状态
        SetActionState(new DropState(this));
        isDeathState = true;
    }

    /// <summary>
    /// 摔落死亡过程
    /// </summary>
    public virtual void OnDropState(float r)
    {

    }

    /// <summary>
    /// 摔落死亡结束
    /// </summary>
    public virtual void OnDropStateExit()
    {

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
        return !isDeathState && isActiveAndEnabled;
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
    /// 是否可被选取为目标
    /// </summary>
    /// <returns></returns>
    public virtual bool CanBeSelectedAsTarget(BaseUnit otherUnit)
    {
        return true;
    }

    /// <summary>
    /// 设置渲染层级
    /// </summary>
    public virtual void UpdateRenderLayer(int arrayIndex)
    {

    }

    /// <summary>
    /// 尝试获取目标SpriteRenterer的Sorting信息
    /// </summary>
    /// <returns></returns>
    public virtual bool TryGetSpriteRenternerSorting(out string name, out int order)
    {
        name = null;
        order = 0;
        return false;
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
    /// 执行死亡操作（需要走BeforeDeath()、DuringDeath()、AfterDeath()的流程）
    /// </summary>
    public void ExecuteDeath()
    {
        BeforeDeath();
    }

    /// <summary>
    /// 执行灰烬操作
    /// </summary>
    public void ExecuteBurn()
    {
        BeforeBurn();
    }

    /// <summary>
    /// 执行摔落
    /// </summary>
    public void ExecuteDrop()
    {
        BeforeDrop();
    }

    /// <summary>
    /// 获取正常情况下目标最终的受伤倍率
    /// </summary>
    public float GetFinalDamageRate()
    {
        return (1 - mCurrentDefense) * mDamgeRate;
    }

    /// <summary>
    /// 受到伤害时结算伤害
    /// </summary>
    /// <param name="dmg"></param>
    public virtual float OnDamage(float dmg)
    {
        // 如果目标含有无敌标签，则直接跳过伤害判定
        if (NumericBox.GetBoolNumericValue(StringManager.Invincibility))
            return 0;

        // 先计算抗性减免后的伤害
        dmg = Mathf.Max(0, dmg * GetFinalDamageRate());
        float recordDmg = dmg;
        // 然后计算护盾吸收的伤害
        dmg = Mathf.Max(0, NumericBox.DamageShield(dmg));
        dmg = Mathf.Min(dmg, _mCurrentHp);
        // 最后扣除本体生命值
        _mCurrentHp -= dmg;
        if (_mCurrentHp <= 0)
        {
            _mCurrentHp = 0;
            ExecuteDeath();
        }
        else if (!isIgnoreRecordDamage)
        {
            mRecordDamageComponent.TriggerRecordDamage(recordDmg);
        }
        return dmg;
    }

    public virtual float OnAoeDamage(float dmg)
    {
        // 如果目标含有无敌标签，则直接跳过伤害判定
        if (NumericBox.GetBoolNumericValue(StringManager.Invincibility))
            return 0;

        // 先计算抗性减免后的伤害
        dmg = Mathf.Max(0, dmg * GetFinalDamageRate()*NumericBox.AoeRate.TotalValue);
        float recordDmg = dmg;
        // 然后计算护盾吸收的伤害
        dmg = Mathf.Max(0, NumericBox.DamageShield(dmg));
        dmg = Mathf.Min(dmg, _mCurrentHp);
        // 最后扣除本体生命值
        _mCurrentHp -= dmg;
        if (_mCurrentHp <= 0)
        {
            _mCurrentHp = 0;
            ExecuteDeath();
        }
        else if (!isIgnoreRecordDamage)
        {
            mRecordDamageComponent.TriggerRecordDamage(recordDmg);
        }
        return dmg;
    }

    /// <summary>
    /// 受到无视护盾的伤害
    /// </summary>
    /// <param name="dmg"></param>
    public virtual float OnDamgeIgnoreShield(float dmg)
    {
        // 如果目标含有无敌标签，则直接跳过伤害判定
        if (NumericBox.GetBoolNumericValue(StringManager.Invincibility))
            return 0;

        // 先计算抗性减免后的伤害
        dmg = Mathf.Max(0, dmg * GetFinalDamageRate());
        dmg = Mathf.Min(dmg, _mCurrentHp);
        // 直接扣除本体生命值
        _mCurrentHp -= dmg;
        if (_mCurrentHp <= 0)
        {
            _mCurrentHp = 0;
            ExecuteDeath();
        }
        else if (!isIgnoreRecordDamage)
        {
            mRecordDamageComponent.TriggerRecordDamage(dmg);
        }
        return dmg;
    }

    /// <summary>
    /// 受到真实伤害
    /// </summary>
    public virtual float OnRealDamage(float dmg)
    {
        // 如果目标含有无敌标签，则直接跳过伤害判定
        if (NumericBox.GetBoolNumericValue(StringManager.Invincibility))
            return 0;
        dmg = Mathf.Min(dmg, _mCurrentHp);
        // 扣除本体生命值
        _mCurrentHp -= dmg;
        if (_mCurrentHp <= 0)
        {
            _mCurrentHp = 0;
            ExecuteDeath();
        }
        else if (!isIgnoreRecordDamage)
        {
            mRecordDamageComponent.TriggerRecordDamage(dmg);
        }
        return dmg;
    }

    /// <summary>
    /// 受到灰烬伤害
    /// </summary>
    /// <param name="dmg"></param>
    public virtual float OnBurnDamage(float dmg)
    {
        // 如果目标含有无敌标签，则直接跳过伤害判定
        if (NumericBox.GetBoolNumericValue(StringManager.Invincibility))
            return 0;

        // 灰烬伤害为真实伤害
        dmg = Mathf.Min(dmg, _mCurrentHp);
        // 扣除本体生命值
        _mCurrentHp -= dmg;
        if (_mCurrentHp <= 0)
        {
            _mCurrentHp = 0;
            ExecuteBurn();
        }
        else if (!isIgnoreRecordDamage)
        {
            mRecordDamageComponent.TriggerRecordDamage(dmg);
        }
        return dmg;
    }

    /// <summary>
    /// 受到灰烬效果来源的灰烬伤害
    /// </summary>
    /// <param name="dmg"></param>
    public virtual float OnBombBurnDamage(float dmg)
    {
        // 如果目标含有无敌标签，则直接跳过伤害判定
        if (NumericBox.GetBoolNumericValue(StringManager.Invincibility))
            return 0;

        // 需要计算一次灰烬抗性
        dmg = Mathf.Min(dmg * mBurnRate, _mCurrentHp);
        // 扣除本体生命值
        _mCurrentHp -= dmg;
        if (_mCurrentHp <= 0)
        {
            _mCurrentHp = 0;
            ExecuteBurn();
        }
        else if (!isIgnoreRecordDamage)
        {
            mRecordDamageComponent.TriggerRecordDamage(dmg);
        }
        return dmg;
    }

    /// <summary>
    /// 受到标记伤害时结算伤害
    /// </summary>
    /// <param name="dmg"></param>
    public virtual float OnRecordDamage(float dmg)
    {
        // 如果目标含有无敌标签，则直接跳过伤害判定
        if (NumericBox.GetBoolNumericValue(StringManager.Invincibility))
            return 0;
        dmg = Mathf.Min(dmg, _mCurrentHp);
        // 最后扣除本体生命值
        _mCurrentHp -= dmg;
        if (_mCurrentHp <= 0)
        {
            _mCurrentHp = 0;
            ExecuteDeath();
        }
        return dmg;
    }

    /// <summary>
    /// 受到治疗时结算治疗
    /// </summary>
    /// <param name="cure"></param>
    public virtual void OnCure(float cure)
    {
        _mCurrentHp += cure;
        if (_mCurrentHp > mMaxHp)
            _mCurrentHp = mMaxHp;
    }

    /// <summary>
    /// 受到护盾时结算护盾
    /// </summary>
    public virtual void OnAddedShield(float value)
    {
        NumericBox.AddDynamicShield(value);
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

    public void AddStatusAbility(StatusAbility statusAbility)
    {
        statusAbilityManager.AddStatusAbility(statusAbility);
    }

    public void RemoveStatusAbility(StatusAbility statusAbility)
    {
        statusAbilityManager.RemoveStatusAbility(statusAbility);
    }

    public StatusAbility GetUniqueStatus(string statusName)
    {
        return statusAbilityManager.GetUniqueStatus(statusName);
    }

    public StatusAbility GetNoCountUniqueStatus(string statusName)
    {
        return statusAbilityManager.GetNoCountUniqueStatus(statusName);
    }

    public bool IsContainNoCountUniqueStatus(string statusName)
    {
        return statusAbilityManager.IsContainNoCountUniqueStatus(statusName);
    }

    public void AddUniqueStatusAbility(string statusName, StatusAbility statusAbility)
    {
        statusAbilityManager.AddUniqueStatusAbility(statusName, statusAbility);
    }

    public void RemoveUniqueStatusAbility(string statusName)
    {
        statusAbilityManager.RemoveUniqueStatusAbility(statusName);
    }

    public void AddNoCountUniqueStatusAbility(string statusName, StatusAbility statusAbility)
    {
        statusAbilityManager.AddNoCountUniqueStatusAbility(statusName, statusAbility);
    }

    public void RemoveNoCountUniqueStatusAbility(string statusName)
    {
        statusAbilityManager.RemoveNoCountUniqueStatusAbility(statusName);
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
    public virtual void OnIdleStateInterrupt()
    {
    }

    public virtual void OnMoveStateInterrupt()
    {
    }

    public virtual void OnAttackStateInterrupt()
    {
    }

    public virtual void OnCastStateInterrupt()
    {
    }

    public virtual void OnTransitionStateInterrupt()
    {
    }

    public virtual void OnIdleStateContinue()
    {
    }

    public virtual void OnMoveStateContinue()
    {
    }

    public virtual void OnAttackStateContinue()
    {
    }

    public virtual void OnCastStateContinue()
    {
    }

    public virtual void OnTransitionStateContinue()
    {
    }

    private BoolModifier FrozenModifier = new BoolModifier(true);
    public virtual void OnFrozenStateEnter()
    {
        animatorController.AddPauseModifier(FrozenModifier);
    }

    public virtual void OnFrozenState()
    {

    }

    public virtual void OnFrozenStateExit()
    {
        animatorController.RemovePauseModifier(FrozenModifier);
    }

    /// <summary>
    /// 暂停当前动画
    /// </summary>
    public virtual void PauseCurrentAnimatorState(BoolModifier boolModifier)
    {
        animatorController.AddPauseModifier(boolModifier);
    }

    /// <summary>
    /// 解除暂停当前动画
    /// </summary>
    public virtual void ResumeCurrentAnimatorState(BoolModifier boolModifier)
    {
        animatorController.RemovePauseModifier(boolModifier);
    }

    /// <summary>
    /// 启用冰冻减速效果
    /// </summary>
    public virtual void SetFrozeSlowEffectEnable(bool enable)
    {

    }

    public virtual BaseGrid GetGrid()
    {
        return null;
    }

    /// <summary>
    /// 把自身安放在格子上，由子类实现
    /// </summary>
    public virtual void SetGrid(BaseGrid grid)
    {

    }

    public void SetMaxHpAndCurrentHp(float hp)
    {
        NumericBox.Hp.SetBase(hp);
        _mCurrentHp = NumericBox.Hp.Value;
    }

    /// <summary>
    /// 启动判定
    /// </summary>
    public virtual void OpenCollision()
    {
        
    }

    /// <summary>
    /// 关闭判定
    /// </summary>
    public virtual void CloseCollision()
    {

    }

    /// <summary>
    /// 设置透明度
    /// </summary>
    public virtual void SetAlpha(float a)
    {

    }

    /// <summary>
    /// 获取当前移动速度
    /// </summary>
    /// <returns></returns>
    public virtual float GetMoveSpeed()
    {
        // 被冰冻 或 禁用移动 则返回0移速
        if (NumericBox.GetBoolNumericValue(StringManager.Frozen) || disableMove)
            return 0;
        // 否则返回默认移速
        return mCurrentMoveSpeed;
    }

    /// <summary>
    /// 获取贴图对象
    /// </summary>
    public virtual Sprite GetSpirte()
    {
        return null;
    }

    /// <summary>
    /// 如果有多个贴图对象则使用这个方法
    /// </summary>
    /// <returns></returns>s
    public virtual List<Sprite> GetSpriteList()
    {
        return null;
    }

    /// <summary>
    /// 获取SpriterRenderer
    /// </summary>
    /// <returns></returns>
    public virtual SpriteRenderer GetSpriteRenderer()
    {
        return null;
    }

    /// <summary>
    /// 如果有多个，则用这个方法
    /// </summary>
    /// <returns></returns>
    public virtual List<SpriteRenderer> GetSpriteRendererList()
    {
        return null;
    }

    /// <summary>
    /// 设置贴图对象坐标
    /// </summary>
    public virtual void SetSpriteLocalPosition(Vector2 vector2)
    {

    }

    /// <summary>
    /// 获取贴图对象相对坐标
    /// </summary>
    /// <returns></returns>
    public virtual Vector2 GetSpriteLocalPosition()
    {
        return Vector2.zero;
    }

    /// <summary>
    /// 获取高度
    /// </summary>
    /// <returns></returns>
    public virtual float GetHeight()
    {
        return mHeight;
    }

    public virtual void MPauseUpdate()
    {
        
    }

    /// <summary>
    /// 设置判定参数
    /// </summary>
    public virtual void SetCollider2DParam()
    {
        
    }


    /// <summary>
    /// 自身是否持有某种特效
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    public bool IsContainEffect(EffectType t)
    {
        if (effectDict.ContainsKey(t))
        {
            if (!effectDict[t].IsValid())
            {
                effectDict.Remove(t);
                return false;
            }
            else
            {
                return true;
            }
        }
        return false;
    }

    public bool IsContainEffect(string t)
    {
        if (effectDict2.ContainsKey(t))
        {
            if (!effectDict2[t].IsValid())
            {
                effectDict2.Remove(t);
                return false;
            }
            else
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 自动更新特效表，剔除不存活的子项
    /// </summary>
    public void CheckAndUpdateEffectDict()
    {
        List<EffectType> delList = new List<EffectType>();
        foreach (var keyValuePair in effectDict)
        {
            if (!keyValuePair.Value.IsValid())
                delList.Add(keyValuePair.Key);
        }
        foreach (var t in delList)
        {
            effectDict.Remove(t);
        }

        List<string> delList2 = new List<string>();
        foreach (var keyValuePair in effectDict2)
        {
            if (!keyValuePair.Value.IsValid())
                delList2.Add(keyValuePair.Key);
        }
        foreach (var t in delList2)
        {
            effectDict2.Remove(t);
        }
    }

    /// <summary>
    /// 获取某个特效引用
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    public BaseEffect GetEffect(EffectType t)
    {
        if (IsContainEffect(t))
            return effectDict[t];
        return null;
    }

    public BaseEffect GetEffect(string t)
    {
        if (IsContainEffect(t))
            return effectDict2[t];
        return null;
    }

    /// <summary>
    /// 为自身添加唯一性特效
    /// </summary>
    public void AddEffectToDict(EffectType t, BaseEffect eff, Vector2 localPosition)
    {
        effectDict.Add(t, eff);
        SpriteRenderer s = GetSpriteRenderer();
        Transform trans = null;
        if (s != null)
            trans = s.transform;
        else
            trans = transform;
        eff.transform.SetParent(trans);
        eff.transform.localPosition = localPosition;
        if (isHideEffect)
            eff.Hide(true);
        else
            eff.Hide(false);
    }

    public void AddEffectToDict(string t, BaseEffect eff, Vector2 localPosition)
    {
        effectDict2.Add(t, eff);
        SpriteRenderer s = GetSpriteRenderer();
        Transform trans = null;
        if (s != null)
            trans = s.transform;
        else
            trans = transform;
        eff.transform.SetParent(trans);
        eff.transform.localPosition = localPosition;
        if (isHideEffect)
            eff.Hide(true);
        else
            eff.Hide(false);
    }

    /// <summary>
    /// 移除某个特效引用
    /// </summary>
    public void RemoveEffectFromDict(EffectType t)
    {
        if (IsContainEffect(t))
        {
            BaseEffect eff = effectDict[t];
            effectDict.Remove(t);
            eff.ExecuteDeath();
        }
    }

    public void RemoveEffectFromDict(string t)
    {
        if (IsContainEffect(t))
        {
            BaseEffect eff = effectDict2[t];
            effectDict2.Remove(t);
            eff.ExecuteDeath();
        }
    }

    /// <summary>
    /// 移除自身所有特效引用（一般用于目标被回收时）
    /// </summary>
    public void RemoveAllEffect()
    {
        List<EffectType> l = new List<EffectType>();
        foreach (var item in effectDict)
        {
            l.Add(item.Key);
        }
        foreach (var t in l)
        {
            RemoveEffectFromDict(t);
        }

        List<string> l2 = new List<string>();
        foreach (var item in effectDict2)
        {
            l2.Add(item.Key);
        }
        foreach (var t in l2)
        {
            RemoveEffectFromDict(t);
        }
    }

    /// <summary>
    /// 隐藏全部特效
    /// </summary>
    public void HideEffect(bool enable)
    {
        isHideEffect = enable;
        if (enable)
        {
            foreach (var keyValuePair in effectDict)
            {
                keyValuePair.Value.Hide(true);
            }
            foreach (var keyValuePair in effectDict2)
            {
                keyValuePair.Value.Hide(true);
            }
        }
        else
        {
            foreach (var keyValuePair in effectDict)
            {
                keyValuePair.Value.Hide(false);
            }
            foreach (var keyValuePair in effectDict2)
            {
                keyValuePair.Value.Hide(false);
            }
        }
    }

    /// <summary>
    /// 改变贴图X偏移
    /// </summary>
    /// <param name="f"></param>
    public void AddSpriteOffsetX(FloatModifier f)
    {
        SpriteOffsetX.AddAddModifier(f);
        SetSpriteLocalPosition(SpriteOffset);
    }

    /// <summary>
    /// 移除改变贴图X偏移
    /// </summary>
    /// <param name="f"></param>
    public void RemoveSpriteOffsetX(FloatModifier f)
    {
        SpriteOffsetX.RemoveAddModifier(f);
        SetSpriteLocalPosition(SpriteOffset);
    }

    /// <summary>
    /// 移除改变贴图X偏移
    /// </summary>
    /// <param name="f"></param>
    public void AddSpriteOffsetY(FloatModifier f)
    {
        SpriteOffsetY.AddAddModifier(f);
        SetSpriteLocalPosition(SpriteOffset);
    }

    /// <summary>
    /// 移除改变贴图Y偏移
    /// </summary>
    /// <param name="f"></param>
    public void RemoveSpriteOffsetY(FloatModifier f)
    {
        SpriteOffsetY.RemoveAddModifier(f);
        SetSpriteLocalPosition(SpriteOffset);
    }

    /// <summary>
    /// 添加唯一性任务
    /// </summary>
    public void AddUniqueTask(string key, ITask t)
    {
        taskController.AddUniqueTask(key, t);
    }

    /// <summary>
    /// 添加一个任务
    /// </summary>
    /// <param name="t"></param>
    public void AddTask(ITask t)
    {
        taskController.AddTask(t);
    }

    /// <summary>
    /// 移除唯一性任务
    /// </summary>
    public void RemoveUniqueTask(string key)
    {
        taskController.RemoveUniqueTask(key);
    }

    /// <summary>
    /// 移除一个任务
    /// </summary>
    /// <param name="t"></param>
    public void RemoveTask(ITask t)
    {
        taskController.RemoveTask(t);
    }

    /// <summary>
    /// 获取某个标记为key的任务
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public ITask GetTask(string key)
    {
        return taskController.GetTask(key);
    }

    /// <summary>
    /// 设置大小
    /// </summary>
    public virtual void SetLocalScale(Vector2 scale)
    {
        transform.localScale = scale;
    }

    public virtual void SetSpriteLocalScale(Vector2 scale)
    {
        if (GetSpriteRenderer() != null)
            GetSpriteRenderer().transform.localScale = scale;
    }


    public void AddBeforeDeathEvent(Action<BaseUnit> action)
    {
        BeforeDeathEventList.Add(action);
    }

    public void RemoveBeforeDeathEvent(Action<BaseUnit> action)
    {
        BeforeDeathEventList.Remove(action);
    }

    public void AddBeforeBurnEvent(Action<BaseUnit> action)
    {
        BeforeBurnEventList.Add(action);
    }

    public void RemoveBeforeBurnEvent(Action<BaseUnit> action)
    {
        BeforeBurnEventList.Remove(action);
    }

    public void AddAfterDeathEvent(Action<BaseUnit> action)
    {
        AfterDeathEventList.Add(action);
    }

    public void RemoveAfterDeathEvent(Action<BaseUnit> action)
    {
        AfterDeathEventList.Remove(action);
    }

    public void AddOnDestoryAction(Action<BaseUnit> action)
    {
        OnDestoryActionList.Add(action);
    }

    public void RemoveOnDestoryAction(Action<BaseUnit> action)
    {
        OnDestoryActionList.Remove(action);
    }

    public void AddCanBlockFunc(Func<BaseUnit, BaseUnit, bool> action)
    {
        CanBlockFuncList.Add(action);
    }

    public void RemoveCanBlockFunc(Func<BaseUnit, BaseUnit, bool> action)
    {
        CanBlockFuncList.Remove(action);
    }

    public void AddCanHitFunc(Func<BaseUnit, BaseBullet, bool> action)
    {
        CanHitFuncList.Add(action);
    }

    public void RemoveCanHitFunc(Func<BaseUnit, BaseBullet, bool> action)
    {
        CanHitFuncList.Remove(action);
    }

    /// <summary>
    /// u1一般为自己，u2一般是试图选取自己为攻击目标的攻击者
    /// </summary>
    /// <param name="action"></param>
    public void AddCanBeSelectedAsTargetFunc(Func<BaseUnit, BaseUnit, bool> action)
    {
        CanBeSelectedAsTargetFuncList.Add(action);
    }

    public void RemoveCanBeSelectedAsTargetFunc(Func<BaseUnit, BaseUnit, bool> action)
    {
        CanBeSelectedAsTargetFuncList.Remove(action);
    }

    /// <summary>
    /// 禁用移动，默认为false,当设为true时处于移动状态时不再更新移动所改变的坐标
    /// </summary>
    /// <param name="disable"></param>
    public void DisableMove(bool disable)
    {
        disableMove = disable;
    }

    /// <summary>
    /// 设置基础属性
    /// </summary>
    public void SetBaseAttribute(float maxHp, float attack, float attackSpeed, float standardMoveSpeed, float defence, float attackPercent, int height)
    {
        // 血量
        NumericBox.Hp.SetBase(maxHp);
        _mCurrentHp = mMaxHp;
        // 攻击力
        NumericBox.Attack.SetBase(attack);
        // 攻击速度与攻击间隔
        NumericBox.AttackSpeed.SetBase(attackSpeed);
        NumericBox.MoveSpeed.SetBase(TransManager.TranToVelocity(standardMoveSpeed));
        NumericBox.Defense.SetBase(defence);
        this.attackPercent = attackPercent; // 攻击动画播放进度到attackPercent以上时允许出真正的攻击
        // 高度
        mHeight = height;
    }

    /// <summary>
    /// 添加内伤
    /// </summary>
    /// <param name="value"></param>
    public virtual void AddRecordDamage(float value)
    {
        if(!isIgnoreRecordDamage)
            mRecordDamageComponent.AddRecordDamage(value);
    }

    /// <summary>
    /// 添加unit引用
    /// </summary>
    /// <param name="unit"></param>
    public void AddUnitToDict(string key, BaseUnit unit)
    {
        mAttachedUnitDict.Add(key, unit);
    }

    /// <summary>
    /// 移除unit引用
    /// </summary>
    /// <param name="key"></param>
    public void RemoveUnitFromDict(string key)
    {
        mAttachedUnitDict.Remove(key);
    }

    /// <summary>
    /// 获取引用的单位（必须存活）
    /// </summary>
    /// <param name="key"></param>
    public BaseUnit GetUnitFromDict(string key)
    {
        if (IsContainUnit(key))
            return mAttachedUnitDict[key];
        else
            return null;
    }

    /// <summary>
    /// 是否含有某个为key的单位引用
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public bool IsContainUnit(string key)
    {
        if (mAttachedUnitDict.ContainsKey(key))
        {
            BaseUnit unit = mAttachedUnitDict[key];
            if (unit.IsAlive())
                return true;
            else
            {
                mAttachedUnitDict.Remove(key);
                return false;
            }
        }
        return false;
    }

    public void SetUseDefaultRecieveDamageActionMethod(bool isUse)
    {
        isUseDefaultRecieveDamageActionMethod = isUse;
    }

    public bool IsUseDefaultRecieveDamageActionMethod()
    {
        return isUseDefaultRecieveDamageActionMethod;
    }
    
    public void SetGetCurrentHpFunc(Func<BaseUnit, float> func)
    {
        GetCurrentHpFunc = func;
    }

    public void SetGetDamageRateFunc(Func<BaseUnit, float> func)
    {
        GetDamageRateFunc = func;
    }

    public void SetGetBurnRateFunc(Func<BaseUnit, float> func)
    {
        GetBurnRateFunc = func;
    }
    public void SetGetAoeRateFunc(Func<BaseUnit, float> func)
    {
        GetAoeRateFunc = func;
    }

    // 继续
}
