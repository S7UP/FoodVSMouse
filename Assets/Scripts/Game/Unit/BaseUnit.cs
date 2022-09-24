using System;
using System.Collections.Generic;


using UnityEngine;
/// <summary>
/// ս�����������������Ϸ��λ
/// </summary>
public class BaseUnit : MonoBehaviour, IGameControllerMember, IBaseStateImplementor
{
    // ��Ҫ���ش洢�ı���
    [System.Serializable]
    public struct Attribute
    {
        public string name; // ��λ�ľ�������
        public int type; // ��λ���ڵķ���
        public int shape; // ��λ�ڵ�ǰ����ı��ֱ��

        public double baseHP; // ����Ѫ��
        public double baseAttack; // ��������
        public double baseAttackSpeed; // ���������ٶ�
        public double attackPercent; // �������ж�ʱ�������ŵİٷֱ�
        public double baseMoveSpeed; // ��������
        public double baseDefense; // ��������
        public double baseRange; // �������
        public int baseHeight; // �����߶�
    }

    // ����ı���
    public UnitType mUnitType;
    public float mBaseHp { get { return NumericBox.Hp.baseValue; } } //+ ��������ֵ
    public float mMaxHp { get { return NumericBox.Hp.Value; } } //+ �������ֵ
    public float mCurrentHp; //+ ��ǰ����ֵ
    public float mBaseAttack { get { return NumericBox.Attack.baseValue; } } //+ ����������
    public float mCurrentAttack { get { return NumericBox.Attack.Value; } } //+ ��ǰ������
    public float mBaseAttackSpeed { get { return NumericBox.AttackSpeed.baseValue; } } //+ ���������ٶ�
    public float mCurrentAttackSpeed { get { return NumericBox.AttackSpeed.Value; } } //+ ��ǰ�����ٶ�
    public float mCurrentDefense { get { return NumericBox.Defense.Value; } } // ����
    public float mCurrentRange { get { return NumericBox.Range.Value; } } //���
    public float mBaseMoveSpeed { get { return NumericBox.MoveSpeed.baseValue; } } // �����ƶ��ٶ�
    public float mCurrentMoveSpeed { get { return NumericBox.MoveSpeed.Value; } } // ��ǰ�ƶ��ٶ�

    //public float mCurrentTotalShieldValue { get { return NumericBox.Shield.Value; } } // ��ǰ����ֵ֮��
    public float mCurrentTotalShieldValue;

    protected float attackPercent; // �����������Ž��ȵ�attackPercent����ʱ����������Ĺ���
    protected bool mAttackFlag; // ������һ�ι����ܷ�������flag
    public int mHeight; //+ �߶�
    public Vector2 moveRotate; // �ƶ�����
    // ��������ΪͼƬ����ƫ����
    public FloatNumeric SpriteOffsetX = new FloatNumeric();
    public FloatNumeric SpriteOffsetY = new FloatNumeric();
    public Vector2 SpriteOffset { get { return new Vector2(SpriteOffsetX.Value, SpriteOffsetY.Value); } }
    public bool isFrozenState; // �Ƿ��ڶ���״̬
    public bool isDeathState{ get; set; } // �Ƿ�������״̬
    public bool isDisableSkill { get { return NumericBox.IsDisableSkill.Value; } } // �Ƿ������������

    public CombatNumericBox NumericBox { get; private set; } = new CombatNumericBox(); // �洢��λ��ǰ���Եĺ���
    public ActionPointManager actionPointManager { get; set; } = new ActionPointManager(); // �ж��������
    public SkillAbilityManager skillAbilityManager { get; set; } = new SkillAbilityManager(); // ���ܹ�����
    public StatusAbilityManager statusAbilityManager { get; set; } = new StatusAbilityManager(); // ʱЧ״̬��BUFF��������
    public Dictionary<EffectType, BaseEffect> effectDict = new Dictionary<EffectType, BaseEffect>(); // �������Ψһ����Ч
    private bool isHideEffect = false; // �Ƿ�������Ч
    public AnimatorController animatorController = new AnimatorController(); // �������ſ�����
    public List<ITask> TaskList = new List<ITask>(); // ������������
    public Dictionary<string, ITask> TaskDict = new Dictionary<string, ITask>(); // �����ֵ䣨����¼���ò�ʵ��ִ���߼���ִ���߼���������У�
    public HitBox hitBox = new HitBox();

    public string mName; // ��ǰ��λ����������
    public int mType; // ��ǰ��λ�����ࣨ�粻ͬ�Ŀ�����ͬ������
    public int mShape; // ��ǰ���൥λ����ۣ�ͬһ�ſ���0��1��2ת�������0��1��2ת�����������֣�
    public int typeAndShapeValue;

    public IBaseActionState mCurrentActionState; //+ ��ǰ����״̬
    public int currentStateTimer = 0; // ��ǰ״̬�ĳ���ʱ�䣨�л�״̬ʱ�����ã�

    // �¼�
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
    } // �洢JSON�ļ������·��

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
    } // Ԥ��������/·��

    public virtual void Awake()
    {
    }

    // ��λ�����������ȡ�����߸մ���ʱ����
    public virtual void OnEnable()
    {
        // MInit();
    }

    // ��λ������ػ���ʱ����
    public virtual void OnDisable()
    {
        // ����ı���
        mCurrentHp = 0; //+ ��ǰ����ֵ
        attackPercent = 0; // �����������Ž��ȵ�attackPercent����ʱ����������Ĺ���
        mAttackFlag = false; // ������һ�ι����ܷ�������flag
        mHeight = 0; //+ �߶�
        isDeathState = true;
        isFrozenState = false;

        mName = null; // ��ǰ��λ����������

        mCurrentActionState = null; //+ ��ǰ����״̬
        currentStateTimer = 0; // ��ǰ״̬�ĳ���ʱ�䣨�л�״̬ʱ�����ã�

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

    // �л�����״̬
    public void SetActionState(IBaseActionState state)
    {
        // ����ǰ״̬Ϊ����״̬������ͨ���˷������л��ɱ��״̬
        if (mCurrentActionState!=null && (mCurrentActionState is DieState || mCurrentActionState is BurnState))
            return;

        if(state is FrozenState)
        {
            if (mCurrentActionState != null)
                mCurrentActionState.OnInterrupt();
            mCurrentActionState = state;
            mCurrentActionState.OnEnter();
            // �����ü�������������Update��ֹͣ����
        }
        else
        {
            if (mCurrentActionState != null && mCurrentActionState is FrozenState)
            {
                // ����л�ǰ��״̬�Ǳ���������ִ��OnExit��������Ϊ��OnExit����������÷��������ᵼ�����޵ݹ�
                mCurrentActionState = state;
                // �����ü�������ͬʱ���ýⶳ��״̬�ļ�������
                mCurrentActionState.OnContinue();
            }
            else
            {
                if (mCurrentActionState != null)
                {
                    mCurrentActionState.OnExit();
                }
                mCurrentActionState = state;
                currentStateTimer = -1; // ���ü�����������Ϊ-1�Ǳ�֤��һ��ִ�е�״̬Updateʱ����ȡ���ļ�����ֵһ��Ϊ0����ϸʵ�ּ�����MUpdate()����
                mCurrentActionState.OnEnter();
            }

        }
    }



    /// <summary>
    /// ���㵱ǰѪ���ٷֱ�
    /// </summary>
    /// <returns></returns>
    public float GetHeathPercent()
    {
        return mCurrentHp / mMaxHp;
    }

    /// <summary>
    /// ��ȡ��ǰ����ֵ
    /// </summary>
    /// <returns></returns>
    public float GetCurrentHp()
    {
        return mCurrentHp;
    }

    /// <summary>
    /// ��ȡ����ʧ����ֵ
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

    // ����λ��
    public virtual void SetPosition(Vector3 V3)
    {
        gameObject.transform.position = V3;
    }

    // �����������������������ȵģ�
    public virtual void BeforeDeath()
    {
        // �����ض����ؾ�һ�°�
        // ����overrideһ��,���ж�һ�����������˲����ø�������ķ����ˣ����ܾȵģ�
        // TNND,Ϊʲô���ȣ����������ǰɣ��������ǰɣ�.wav

        // �����¼�
        foreach (var item in BeforeDeathEventList)
        {
            item(this);
        }
        // ������������״̬
        isDeathState = true;
        // �������Ч��
        skillAbilityManager.TryEndAllSpellingSkillAbility();
        // ���BUFFЧ��
        statusAbilityManager.TryEndAllStatusAbility();
        SetActionState(new DieState(this));
    }

    // �����������ˣ�����ʱ���м�֡״̬��Ҫ������
    public virtual void DuringDeath()
    {
        // ��֪��Ҫ��ɶ�ˣ���������ط��϶��Ȳ�����
    }

    /// <summary>
    /// ֱ����Ŀ��������ֻ�о���AfterDeath()��������BeforeDeath()��DuringDeath()��ע����ExecuteDeath()��������)
    /// </summary>
    public void DeathEvent()
    {
        // ���������Ч��һ��
        skillAbilityManager.TryEndAllSpellingSkillAbility();
        // �����BUFFЧ��һ��
        statusAbilityManager.TryEndAllStatusAbility();
        // �������Чһ��
        RemoveAllEffect();
        AfterDeath();
        foreach (var item in AfterDeathEventList)
        {
            item(this);
        }
        // Ȼ����ȥ����
        ExecuteRecycle();
    }

    /// <summary>
    /// ��λ�������¼�
    /// </summary>
    public virtual void AfterDeath()
    {
        // ������ҲҪ����Ϊ�����
    }

    /// <summary>
    /// ִ�иõ�λ�����¼�
    /// </summary>
    public virtual void ExecuteRecycle()
    {
        GameManager.Instance.PushGameObjectToFactory(FactoryType.GameFactory, mPreFabPath, this.gameObject);
    }


    /// <summary>
    /// �ҽ�Ч��֮ǰ
    /// </summary>
    public virtual void BeforeBurn()
    {
        // �����¼�
        foreach (var item in BeforeBurnEventList)
        {
            item(this);
        }

        // ������������״̬
        isDeathState = true;
        // �������Ч��
        skillAbilityManager.TryEndAllSpellingSkillAbility();
        // ���BUFFЧ��
        statusAbilityManager.TryEndAllStatusAbility();
        SetActionState(new BurnState(this));
    }

    /// <summary>
    /// ����ҽ�״̬�������������¼�
    /// </summary>
    public virtual void OnBurnStateEnter()
    {

    }
    /// <summary>
    /// �ڻҽ�״̬�³��������£����ң�
    /// </summary>
    /// <param name="_Threshold"></param>
    public virtual void DuringBurn(float _Threshold)
    {

    }

    /// <summary>
    /// ����GameController��ָʾ��ϵͳ���ĳ�ʼ������
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
        // ������������ʼ��
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
        // ����
        mType = attr.type;
        mShape = attr.shape;
        typeAndShapeValue = 0; // ͼ��
        SetUnitType();
        // Ѫ��
        NumericBox.Hp.SetBase((float)attr.baseHP);
        mCurrentHp = mMaxHp;
        // ������
        NumericBox.Attack.SetBase((float)attr.baseAttack);
        // �����ٶ��빥�����
        NumericBox.AttackSpeed.SetBase((float)attr.baseAttackSpeed);
        NumericBox.MoveSpeed.SetBase(TransManager.TranToVelocity((float)attr.baseMoveSpeed));
        NumericBox.Defense.SetBase((float)attr.baseDefense);
        attackPercent = (float)attr.attackPercent; // �����������Ž��ȵ�attackPercent����ʱ����������Ĺ���
        mAttackFlag = true; // ������һ�ι����ܷ�������flag
        // �߶�
        mHeight = attr.baseHeight;
        // ����״̬
        isDeathState = false;

        // ��ʼ����ǰ����״̬
        mCurrentActionState = null;
        SetActionState(new BaseActionState(this));
        // ���ö�����
        SetActionPointManager();
        // ��ȡ������Ϣ
        LoadSkillAbility();
        // �����ж�
        OpenCollision();
        // ͸����Ĭ����д��1
        SetAlpha(1.0f);
        // ����Collider2D�Ĳ���
        SetCollider2DParam();
        // ��С��ʼ��
        SetLocalScale(Vector2.one);

        BeforeDeathEventList.Clear();
        BeforeBurnEventList.Clear();
        AfterDeathEventList.Clear();
    }

    public virtual void MUpdate()
    {
        // ��������������
        animatorController.Update();
        // ʱЧ��״̬����������
        statusAbilityManager.Update();
        // ���ܹ���������
        if (!isDisableSkill)
            skillAbilityManager.Update();
        // �������ڶ���״̬ʱ����ǰ״̬��ʱ��ÿ֡��+1
        if (!isFrozenState)
            currentStateTimer += 1;
        // ��λ����״̬��״̬�����������ƶ������������������ᡢ������
        mCurrentActionState.OnUpdate();
        // �ܻ����Ⱥ��Ӹ���
        hitBox.Update();
        // �����������
        OnTaskUpdate();
    }

    /// <summary>
    /// ������Override�����ô����
    /// </summary>
    public virtual void SetUnitType()
    {

    }

    /// <summary>
    /// ������Override������Ŀ��ļ���
    /// </summary>
    public virtual void LoadSkillAbility()
    {

    }

    public virtual void MDestory()
    {

    }

    /// <summary>
    /// ����Ϸ��ͣʱ
    /// </summary>
    public virtual void MPause()
    {
        animatorController.Pause();
    }

    /// <summary>
    /// ����Ϸȡ����ͣʱ
    /// </summary>
    public virtual void MResume()
    {
        animatorController.Resume();
    }

    /// <summary>
    /// �ж���û�б�����
    /// </summary>
    public bool IsValid()
    {
        return isActiveAndEnabled;
    }

    /// <summary>
    /// �жϵ�λ�Ƿ���ս���ϴ��
    /// </summary>
    /// <returns></returns>
    public virtual bool IsAlive()
    {
        return !isDeathState;
    }

    /// <summary>
    /// ��ȡ��ǰ��λ�������±�
    /// </summary>
    /// <returns></returns>
    public virtual int GetRowIndex()
    {
        return MapManager.GetYIndex(transform.position.y);
    }

    /// <summary>
    /// ��ȡ��ǰ��λ�������±�
    /// </summary>
    /// <returns></returns>
    public virtual int GetColumnIndex()
    {
        return MapManager.GetXIndex(transform.position.x);
    }

    /// <summary>
    /// �����Ƿ����赲��������unit��������������д���������ʵ�ֶ�̬
    /// </summary>
    /// <returns></returns>
    public virtual bool CanBlock(BaseUnit unit)
    {
        return false;
    }

    /// <summary>
    /// �����ܷ񱻴�������bullet���У�������������д���������ʵ�ֶ�̬
    /// </summary>
    /// <returns></returns>
    public virtual bool CanHit(BaseBullet bullet)
    {
        return false;
    }

    /// <summary>
    /// ������Ⱦ�㼶
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
    /// ִ��������������Ҫ��BeforeDeath()��DuringDeath()��AfterDeath()�����̣�
    /// </summary>
    public void ExecuteDeath()
    {
        BeforeDeath();
    }

    /// <summary>
    /// ִ�лҽ�����
    /// </summary>
    public void ExecuteBurn()
    {
        BeforeBurn();
    }

    /// <summary>
    /// �ܵ��˺�ʱ�����˺�
    /// </summary>
    /// <param name="dmg"></param>
    public virtual void OnDamage(float dmg)
    {
        // ���Ŀ�꺬���޵б�ǩ����ֱ�������˺��ж�
        if (NumericBox.GetBoolNumericValue(StringManager.Invincibility))
            return;

        // �ȼ��㿹�Լ������˺�
        dmg = Mathf.Max(0, dmg * (1 - mCurrentDefense));
        // Ȼ����㻤�����յ��˺�
        dmg = Mathf.Max(0, NumericBox.DamageShield(dmg));
        // ���۳���������ֵ
        mCurrentHp -= dmg;
        if (mCurrentHp <= 0)
        {
            ExecuteDeath();
        }
    }

    /// <summary>
    /// �ܵ����ӻ��ܵ��˺�
    /// </summary>
    /// <param name="dmg"></param>
    public virtual void OnDamgeIgnoreShield(float dmg)
    {
        // ���Ŀ�꺬���޵б�ǩ����ֱ�������˺��ж�
        if (NumericBox.GetBoolNumericValue(StringManager.Invincibility))
            return;

        // �ȼ��㿹�Լ������˺�
        dmg = Mathf.Max(0, dmg * (1 - mCurrentDefense));
        // ���۳���������ֵ
        mCurrentHp -= dmg;
        if (mCurrentHp <= 0)
        {
            ExecuteDeath();
        }
    }

    /// <summary>
    /// �ܵ��ҽ��˺�
    /// </summary>
    /// <param name="dmg"></param>
    public virtual void OnBurnDamage(float dmg)
    {
        // ���Ŀ�꺬���޵б�ǩ����ֱ�������˺��ж�
        if (NumericBox.GetBoolNumericValue(StringManager.Invincibility))
            return;

        // �ҽ��˺�Ϊ��ʵ�˺�
        // �۳���������ֵ
        mCurrentHp -= dmg;
        if (mCurrentHp <= 0)
        {
            ExecuteBurn();
        }
    }

    /// <summary>
    /// �ܵ�����ʱ��������
    /// </summary>
    /// <param name="cure"></param>
    public virtual void OnCure(float cure)
    {
        mCurrentHp += cure;
        if (mCurrentHp > mMaxHp)
            mCurrentHp = mMaxHp;
    }

    /// <summary>
    /// �ܵ�����ʱ���㻤��
    /// </summary>
    public virtual void OnAddedShield(float value)
    {
        NumericBox.AddDynamicShield(value);
    }

    /// <summary>
    /// �����˺�
    /// </summary>
    /// <param name="combatAction"></param>
    public void ReceiveDamage(CombatAction combatAction)
    {
        var damageAction = combatAction as DamageAction;
        OnDamage(damageAction.DamageValue);
    }

    /// <summary>
    /// ���ջҽ��˺�
    /// </summary>
    /// <param name="combatAction"></param>
    public void ReceiveBurnDamage(CombatAction combatAction)
    {
        var damageAction = combatAction as BurnDamageAction;
        OnBurnDamage(damageAction.DamageValue);
    }

    /// <summary>
    /// ��������
    /// </summary>
    /// <param name="combatAction"></param>
    public void ReceiveCure(CombatAction combatAction)
    {
        var cureAction = combatAction as CureAction;
        OnCure(cureAction.CureValue);
    }

    /// <summary>
    /// ���ջ���
    /// </summary>
    /// <param name="combatAction"></param>
    public void ReceiveShield(CombatAction combatAction)
    {
        var shieldAction = combatAction as ShieldAction;
        OnAddedShield(shieldAction.Value);
    }

    /// <summary>
    /// ��ȡ��ʵ����ֵ����������������������������ֵ
    /// </summary>
    public float GetRealCureValue(float oriValue)
    {
        return oriValue;
    }


    /// <summary>
    /// ������override,����ʵ���������ö��������
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
    /// ��ӽ��ü���Ч������Ĭ��
    /// </summary>
    /// <returns></returns>
    public BoolModifier AddDisAbleSkillModifier()
    {
        return NumericBox.AddDisAbleSkillModifier();
    }

    /// <summary>
    /// ������߽��ü���Ч������Ĭ���ߣ�
    /// </summary>
    /// <returns></returns>
    public BoolModifier AddImmuneDisAbleSkillModifier()
    {
        return NumericBox.AddImmuneDisAbleSkillModifier();
    }

    /// <summary>
    /// �Ƴ����ü���Ч������Ĭ��
    /// </summary>
    /// <returns></returns>
    public void RemoveDisAbleSkillModifier(BoolModifier boolModifier)
    {
        NumericBox.RemoveDisAbleSkillModifier(boolModifier);
    }

    /// <summary>
    /// �Ƴ����߽��ü���Ч������Ĭ���ߣ�
    /// </summary>
    /// <returns></returns>
    public void RemoveImmuneDisAbleSkillModifier(BoolModifier boolModifier)
    {
        NumericBox.RemoveImmuneDisAbleSkillModifier(boolModifier);
    }

    /// <summary>
    /// ��ͨ����������
    /// </summary>
    /// <returns></returns>
    public virtual bool IsMeetGeneralAttackCondition()
    {
        return false;
    }

    /// <summary>
    /// ����ͨ����֮ǰҪ����
    /// </summary>
    public virtual void BeforeGeneralAttack()
    {

    }

    /// <summary>
    /// ����ͨ�����ڼ�Ҫ����
    /// </summary>
    public virtual void OnGeneralAttack()
    {

    }
     
    /// <summary>
    /// ������ͨ����������
    /// </summary>
    public virtual bool IsMeetEndGeneralAttackCondition()
    {
        return true;
    }

    /// <summary>
    /// ����ͨ��������ʱҪ����
    /// </summary>
    public virtual void AfterGeneralAttack()
    {

    }

    /// <summary>
    /// ״̬���
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
    /// ��ͣ��ǰ����
    /// </summary>
    public virtual void PauseCurrentAnimatorState(BoolModifier boolModifier)
    {
        animatorController.AddPauseModifier(boolModifier);
    }

    /// <summary>
    /// �����ͣ��ǰ����
    /// </summary>
    public virtual void ResumeCurrentAnimatorState(BoolModifier boolModifier)
    {
        animatorController.RemovePauseModifier(boolModifier);
    }

    /// <summary>
    /// ���ñ�������Ч��
    /// </summary>
    public virtual void SetFrozeSlowEffectEnable(bool enable)
    {

    }

    public virtual BaseGrid GetGrid()
    {
        return null;
    }

    /// <summary>
    /// ���������ڸ����ϣ�������ʵ��
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
    /// �����ж�
    /// </summary>
    public virtual void OpenCollision()
    {
        
    }

    /// <summary>
    /// �ر��ж�
    /// </summary>
    public virtual void CloseCollision()
    {

    }

    /// <summary>
    /// ����͸����
    /// </summary>
    public virtual void SetAlpha(float a)
    {

    }

    /// <summary>
    /// ��ȡ��ǰ�ƶ��ٶ�
    /// </summary>
    /// <returns></returns>
    public virtual float GetMoveSpeed()
    {
        // �������򷵻�0����
        if (NumericBox.GetBoolNumericValue(StringManager.Frozen))
            return 0;
        // ���򷵻�Ĭ������
        return mCurrentMoveSpeed;
    }

    /// <summary>
    /// ��ȡ��ͼ����
    /// </summary>
    public virtual Sprite GetSpirte()
    {
        return null;
    }

    /// <summary>
    /// ����ж����ͼ������ʹ���������
    /// </summary>
    /// <returns></returns>s
    public virtual List<Sprite> GetSpriteList()
    {
        return null;
    }

    /// <summary>
    /// ��ȡSpriterRenderer
    /// </summary>
    /// <returns></returns>
    public virtual SpriteRenderer GetSpriteRenderer()
    {
        return null;
    }

    /// <summary>
    /// ����ж���������������
    /// </summary>
    /// <returns></returns>
    public virtual List<SpriteRenderer> GetSpriteRendererList()
    {
        return null;
    }

    /// <summary>
    /// ������ͼ��������
    /// </summary>
    public virtual void SetSpriteLocalPosition(Vector2 vector2)
    {

    }

    /// <summary>
    /// ��ȡ�߶�
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
    /// �����ж�����
    /// </summary>
    public virtual void SetCollider2DParam()
    {
        
    }

    /// <summary>
    /// �Ƿ�ɱ�ѡȡΪĿ��
    /// </summary>
    /// <returns></returns>
    public virtual bool CanBeSelectedAsTarget()
    {
        return true;
    }


    /// <summary>
    /// �����Ƿ����ĳ����Ч
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
    /// Ϊ�������Ψһ����Ч
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
    /// �Ƴ�ĳ����Ч����
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
    /// �Ƴ�����������Ч���ã�һ������Ŀ�걻����ʱ��
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
    /// ����ȫ����Ч
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
    /// �ı���ͼXƫ��
    /// </summary>
    /// <param name="f"></param>
    public void AddSpriteOffsetX(FloatModifier f)
    {
        SpriteOffsetX.AddAddModifier(f);
        SetSpriteLocalPosition(SpriteOffset);
    }

    /// <summary>
    /// �Ƴ��ı���ͼXƫ��
    /// </summary>
    /// <param name="f"></param>
    public void RemoveSpriteOffsetX(FloatModifier f)
    {
        SpriteOffsetX.RemoveAddModifier(f);
        SetSpriteLocalPosition(SpriteOffset);
    }

    /// <summary>
    /// �Ƴ��ı���ͼXƫ��
    /// </summary>
    /// <param name="f"></param>
    public void AddSpriteOffsetY(FloatModifier f)
    {
        SpriteOffsetY.AddAddModifier(f);
        SetSpriteLocalPosition(SpriteOffset);
    }

    /// <summary>
    /// �Ƴ��ı���ͼYƫ��
    /// </summary>
    /// <param name="f"></param>
    public void RemoveSpriteOffsetY(FloatModifier f)
    {
        SpriteOffsetY.RemoveAddModifier(f);
        SetSpriteLocalPosition(SpriteOffset);
    }

    /// <summary>
    /// ���Ψһ������
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
    /// ���һ������
    /// </summary>
    /// <param name="t"></param>
    public void AddTask(ITask t)
    {
        TaskList.Add(t);
        t.OnEnter();
    }

    /// <summary>
    /// �Ƴ�Ψһ������
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
    /// �Ƴ�һ������
    /// </summary>
    /// <param name="t"></param>
    public void RemoveTask(ITask t)
    {
        TaskList.Remove(t);
        t.OnExit();
    }

    /// <summary>
    /// ��ȡĳ�����Ϊkey������
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
    /// Task�����
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
    /// ���ô�С
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
    // ����
}
