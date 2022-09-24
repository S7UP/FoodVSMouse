using System;
using System.Collections.Generic;


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

    //public float mCurrentTotalShieldValue { get { return NumericBox.Shield.Value; } } // 当前护盾值之和
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
    private bool isHideEffect = false; // 是否隐藏特效
    public AnimatorController animatorController = new AnimatorController(); // 动画播放控制器
    public List<ITask> TaskList = new List<ITask>(); // 自身挂载任务表
    public Dictionary<string, ITask> TaskDict = new Dictionary<string, ITask>(); // 任务字典（仅记录引用不实际执行逻辑，执行逻辑在任务表中）
    public HitBox hitBox = new HitBox();

    public string mName; // 当前单位的种类名称
    public int mType; // 当前单位的种类（如不同的卡，不同的老鼠）
    public int mShape; // 当前种类单位的外观（同一张卡的0、1、2转，老鼠的0、1、2转或者其他变种）
    public int typeAndShapeValue;

    public IBaseActionState mCurrentActionState; //+ 当前动作状态
    public int currentStateTimer = 0; // 当前状态的持续时间（切换状态时会重置）

    // 事件
    public List<Action<BaseUnit>> BeforeDeathEventList = new List<Action<BaseUnit>>();
    public List<Action<BaseUnit>> BeforeBurnEventList = new List<Action<BaseUnit>>();
    public List<Action<BaseUnit>> AfterDeathEventList = new List<Action<BaseUnit>>();
    

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
        mCurrentHp = 0; //+ 当前生命值
        attackPercent = 0; // 攻击动画播放进度到attackPercent以上时允许出真正的攻击
        mAttackFlag = false; // 作用于一次攻击能否打出来的flag
        mHeight = 0; //+ 高度
        isDeathState = true;
        isFrozenState = false;

        mName = null; // 当前单位的种类名称

        mCurrentActionState = null; //+ 当前动作状态
        currentStateTimer = 0; // 当前状态的持续时间（切换状态时会重置）

        NumericBox.Initialize();
        actionPointManager.Initialize();
        skillAbilityManager.Initialize();
        statusAbilityManager.Initialize();
        effectDict.Clear();
        isHideEffect = false;
        animatorController.Initialize();
        hitBox.Initialize();
        SpriteOffsetX.Initialize(); SpriteOffsetY.Initialize();
        SetSpriteLocalPosition(Vector2.zero);
        TaskList.Clear();
        TaskDict.Clear();

        CloseCollision();

        BeforeDeathEventList.Clear();
        BeforeBurnEventList.Clear();
        AfterDeathEventList.Clear();
    }

    // 切换动作状态
    public void SetActionState(IBaseActionState state)
    {
        // 若当前状态为死亡状态，则不能通过此方法再切换成别的状态
        if (mCurrentActionState!=null && (mCurrentActionState is DieState || mCurrentActionState is BurnState))
            return;

        if(state is FrozenState)
        {
            if (mCurrentActionState != null)
                mCurrentActionState.OnInterrupt();
            mCurrentActionState = state;
            mCurrentActionState.OnEnter();
            // 不重置计数器，并且在Update中停止计数
        }
        else
        {
            if (mCurrentActionState != null && mCurrentActionState is FrozenState)
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

        // 死亡事件
        foreach (var item in BeforeDeathEventList)
        {
            item(this);
        }
        // 进入死亡动画状态
        isDeathState = true;
        // 清除技能效果
        skillAbilityManager.TryEndAllSpellingSkillAbility();
        // 清除BUFF效果
        statusAbilityManager.TryEndAllStatusAbility();
        SetActionState(new DieState(this));
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
        AfterDeath();
        foreach (var item in AfterDeathEventList)
        {
            item(this);
        }
        // 然后安心去世吧
        ExecuteRecycle();
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
    public virtual void ExecuteRecycle()
    {
        GameManager.Instance.PushGameObjectToFactory(FactoryType.GameFactory, mPreFabPath, this.gameObject);
    }


    /// <summary>
    /// 灰烬效果之前
    /// </summary>
    public virtual void BeforeBurn()
    {
        // 死亡事件
        foreach (var item in BeforeBurnEventList)
        {
            item(this);
        }

        // 进入死亡动画状态
        isDeathState = true;
        // 清除技能效果
        skillAbilityManager.TryEndAllSpellingSkillAbility();
        // 清除BUFF效果
        statusAbilityManager.TryEndAllStatusAbility();
        SetActionState(new BurnState(this));
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
    /// 根据GameController的指示，系统做的初始化工作
    /// </summary>
    public virtual void MInit()
    {
        BaseUnit.Attribute attr = GameController.Instance.GetBaseAttribute();
        if (jsonPath == null)
        {
            attr = new BaseUnit.Attribute();
        }
        else
        {
            attr = GameController.Instance.GetBaseAttribute();
        }
        // 几个管理器初始化
        NumericBox.Initialize();
        actionPointManager.Initialize();
        skillAbilityManager.Initialize();
        statusAbilityManager.Initialize();
        effectDict.Clear();
        isHideEffect = false;
        animatorController.Initialize();
        hitBox.Initialize();
        SpriteOffsetX.Initialize(); SpriteOffsetY.Initialize();
        SetSpriteLocalPosition(Vector2.zero);
        TaskList.Clear();
        TaskDict.Clear();
        // 种类
        mType = attr.type;
        mShape = attr.shape;
        typeAndShapeValue = 0; // 图层
        SetUnitType();
        // 血量
        NumericBox.Hp.SetBase((float)attr.baseHP);
        mCurrentHp = mMaxHp;
        // 攻击力
        NumericBox.Attack.SetBase((float)attr.baseAttack);
        // 攻击速度与攻击间隔
        NumericBox.AttackSpeed.SetBase((float)attr.baseAttackSpeed);
        NumericBox.MoveSpeed.SetBase(TransManager.TranToVelocity((float)attr.baseMoveSpeed));
        NumericBox.Defense.SetBase((float)attr.baseDefense);
        attackPercent = (float)attr.attackPercent; // 攻击动画播放进度到attackPercent以上时允许出真正的攻击
        mAttackFlag = true; // 作用于一次攻击能否打出来的flag
        // 高度
        mHeight = attr.baseHeight;
        // 死亡状态
        isDeathState = false;

        // 初始化当前动作状态
        mCurrentActionState = null;
        SetActionState(new BaseActionState(this));
        // 设置动作点
        SetActionPointManager();
        // 读取技能信息
        LoadSkillAbility();
        // 启用判定
        OpenCollision();
        // 透明度默认先写到1
        SetAlpha(1.0f);
        // 设置Collider2D的参数
        SetCollider2DParam();
        // 大小初始化
        SetLocalScale(Vector2.one);

        BeforeDeathEventList.Clear();
        BeforeBurnEventList.Clear();
        AfterDeathEventList.Clear();
    }

    public virtual void MUpdate()
    {
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
        mCurrentActionState.OnUpdate();
        // 受击进度盒子更新
        hitBox.Update();
        // 挂载任务更新
        OnTaskUpdate();
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

    /// <summary>
    /// 当游戏暂停时
    /// </summary>
    public virtual void MPause()
    {
        animatorController.Pause();
    }

    /// <summary>
    /// 当游戏取消暂停时
    /// </summary>
    public virtual void MResume()
    {
        animatorController.Resume();
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
        return !isDeathState;
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
    /// 受到伤害时结算伤害
    /// </summary>
    /// <param name="dmg"></param>
    public virtual void OnDamage(float dmg)
    {
        // 如果目标含有无敌标签，则直接跳过伤害判定
        if (NumericBox.GetBoolNumericValue(StringManager.Invincibility))
            return;

        // 先计算抗性减免后的伤害
        dmg = Mathf.Max(0, dmg * (1 - mCurrentDefense));
        // 然后计算护盾吸收的伤害
        dmg = Mathf.Max(0, NumericBox.DamageShield(dmg));
        // 最后扣除本体生命值
        mCurrentHp -= dmg;
        if (mCurrentHp <= 0)
        {
            ExecuteDeath();
        }
    }

    /// <summary>
    /// 受到无视护盾的伤害
    /// </summary>
    /// <param name="dmg"></param>
    public virtual void OnDamgeIgnoreShield(float dmg)
    {
        // 如果目标含有无敌标签，则直接跳过伤害判定
        if (NumericBox.GetBoolNumericValue(StringManager.Invincibility))
            return;

        // 先计算抗性减免后的伤害
        dmg = Mathf.Max(0, dmg * (1 - mCurrentDefense));
        // 最后扣除本体生命值
        mCurrentHp -= dmg;
        if (mCurrentHp <= 0)
        {
            ExecuteDeath();
        }
    }

    /// <summary>
    /// 受到灰烬伤害
    /// </summary>
    /// <param name="dmg"></param>
    public virtual void OnBurnDamage(float dmg)
    {
        // 如果目标含有无敌标签，则直接跳过伤害判定
        if (NumericBox.GetBoolNumericValue(StringManager.Invincibility))
            return;

        // 灰烬伤害为真实伤害
        // 扣除本体生命值
        mCurrentHp -= dmg;
        if (mCurrentHp <= 0)
        {
            ExecuteBurn();
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
    /// 接收灰烬伤害
    /// </summary>
    /// <param name="combatAction"></param>
    public void ReceiveBurnDamage(CombatAction combatAction)
    {
        var damageAction = combatAction as BurnDamageAction;
        OnBurnDamage(damageAction.DamageValue);
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
    /// 添加禁用技能效果（沉默）
    /// </summary>
    /// <returns></returns>
    public BoolModifier AddDisAbleSkillModifier()
    {
        return NumericBox.AddDisAbleSkillModifier();
    }

    /// <summary>
    /// 添加免疫禁用技能效果（沉默免疫）
    /// </summary>
    /// <returns></returns>
    public BoolModifier AddImmuneDisAbleSkillModifier()
    {
        return NumericBox.AddImmuneDisAbleSkillModifier();
    }

    /// <summary>
    /// 移除禁用技能效果（沉默）
    /// </summary>
    /// <returns></returns>
    public void RemoveDisAbleSkillModifier(BoolModifier boolModifier)
    {
        NumericBox.RemoveDisAbleSkillModifier(boolModifier);
    }

    /// <summary>
    /// 移除免疫禁用技能效果（沉默免疫）
    /// </summary>
    /// <returns></returns>
    public void RemoveImmuneDisAbleSkillModifier(BoolModifier boolModifier)
    {
        NumericBox.RemoveImmuneDisAbleSkillModifier(boolModifier);
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
        mCurrentHp = NumericBox.Hp.Value;
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
        // 被冰冻则返回0移速
        if (NumericBox.GetBoolNumericValue(StringManager.Frozen))
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
    /// 是否可被选取为目标
    /// </summary>
    /// <returns></returns>
    public virtual bool CanBeSelectedAsTarget()
    {
        return true;
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

    /// <summary>
    /// 为自身添加唯一性特效
    /// </summary>
    public void AddEffectToDict(EffectType t, BaseEffect eff, Vector2 localPosition)
    {
        effectDict.Add(t, eff);
        eff.transform.SetParent(transform);
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
            //GameController.Instance.SetEffectDefaultParentTrans(eff);
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
        }
        else
        {
            foreach (var keyValuePair in effectDict)
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
        if (!TaskDict.ContainsKey(key))
        {
            TaskDict.Add(key, t);
            AddTask(t);
        }
    }

    /// <summary>
    /// 添加一个任务
    /// </summary>
    /// <param name="t"></param>
    public void AddTask(ITask t)
    {
        TaskList.Add(t);
        t.OnEnter();
    }

    /// <summary>
    /// 移除唯一性任务
    /// </summary>
    public void RemoveUniqueTask(string key)
    {
        if (TaskDict.ContainsKey(key))
        {
            RemoveTask(TaskDict[key]);
            TaskDict.Remove(key);
        }
    }

    /// <summary>
    /// 移除一个任务
    /// </summary>
    /// <param name="t"></param>
    public void RemoveTask(ITask t)
    {
        TaskList.Remove(t);
        t.OnExit();
    }

    /// <summary>
    /// 获取某个标记为key的任务
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public ITask GetTask(string key)
    {
        if (TaskDict.ContainsKey(key))
            return TaskDict[key];
        return null;
    }

    /// <summary>
    /// Task组更新
    /// </summary>
    private void OnTaskUpdate()
    {
        List<string> deleteKeyList = new List<string>();
        foreach (var keyValuePair in TaskDict)
        {
            ITask t = keyValuePair.Value;
            if (t.IsMeetingExitCondition())
                deleteKeyList.Add(keyValuePair.Key);
        }
        foreach (var key in deleteKeyList)
        {
            RemoveUniqueTask(key);
        }
        List<ITask> deleteTask = new List<ITask>();
        foreach (var t in TaskList)
        {
            if (t.IsMeetingExitCondition())
                deleteTask.Add(t);
            else
                t.OnUpdate();
        }
        foreach (var t in deleteTask)
        {
            RemoveTask(t);
        }
    }

    /// <summary>
    /// 设置大小
    /// </summary>
    public virtual void SetLocalScale(Vector2 scale)
    {
        transform.localScale = scale;
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
    // 继续
}
