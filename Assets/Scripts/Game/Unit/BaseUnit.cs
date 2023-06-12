using System;
using System.Collections.Generic;

using S7P.Numeric;
using S7P.Component;
using S7P.State;
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
    public float mBaseHp { get { return NumericBox.Hp.baseValue; } } // ��������ֵ
    public float mMaxHp { get { return NumericBox.Hp.Value; } } // �������ֵ
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
    private float _mCurrentHp; // ��ǰ����ֵ
    private Func<BaseUnit, float> GetCurrentHpFunc;
    public float mBaseAttack { get { return NumericBox.Attack.baseValue; } } // ����������
    public float mCurrentAttack { get { return NumericBox.Attack.Value; } } // ��ǰ������
    public float mBaseAttackSpeed { get { return NumericBox.AttackSpeed.baseValue; } } // ���������ٶ�
    public float mCurrentAttackSpeed { get { return NumericBox.AttackSpeed.Value; } } // ��ǰ�����ٶ�
    public float mCurrentDefense { get { return NumericBox.Defense.Value; } } // ����
    public float mBaseMoveSpeed { get { return NumericBox.MoveSpeed.baseValue; } } // �����ƶ��ٶ�
    public float mCurrentMoveSpeed { get { return NumericBox.MoveSpeed.Value; } } // ��ǰ�ƶ��ٶ�
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
    } // �ҽ��˺�����
    private Func<BaseUnit, float> GetBurnRateFunc;
    public float mAoeRate
    {
        get
        {
            if (GetAoeRateFunc != null)
                return GetAoeRateFunc(this);
            return NumericBox.AoeRate.TotalValue;
        }
    } // ��Χ�˺�����
    private Func<BaseUnit, float> GetAoeRateFunc;

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
    public ActionPointController actionPointController { get; set; } // �ж��������
    public SkillAbilityManager skillAbilityManager { get; set; } = new SkillAbilityManager(); // ���ܹ�����
    public StatusAbilityManager statusAbilityManager { get; set; } = new StatusAbilityManager(); // ʱЧ״̬��BUFF��������

    public EffectController mEffectController;

    public AnimatorController animatorController = new AnimatorController(); // �������ſ�����
    public TaskController taskController = new TaskController();
    public HitBox hitBox = new HitBox();
    public RecordDamageComponent mRecordDamageComponent;
    public bool isIgnoreRecordDamage; // �Ƿ���������

    public string mName; // ��ǰ��λ����������
    public int mType; // ��ǰ��λ�����ࣨ�粻ͬ�Ŀ�����ͬ������
    public int mShape; // ��ǰ���൥λ����ۣ�ͬһ�ſ���0��1��2ת�������0��1��2ת�����������֣�
    public int typeAndShapeValue;

    public IBaseActionState mCurrentActionState; //+ ��ǰ����״̬
    public int currentStateTimer = 0; // ��ǰ״̬�ĳ���ʱ�䣨�л�״̬ʱ�����ã�
    public bool disableMove = false; // ��ֹ�ƶ�

    public int aliveTime = 0; // ��ǰ��������ɵ����ڵĴ��ʱ��
    public Vector2 lastPosition; // ��֡λ��
    public Vector2 DeltaPosition { get; private set; } // ��ǰ֡�������֡��λ�ñ仯��

    // �¼�
    private List<Action<BaseUnit>> BeforeDeathEventList = new List<Action<BaseUnit>>();
    private List<Action<BaseUnit>> AfterDeathEventList = new List<Action<BaseUnit>>();
    private List<Action<BaseUnit>> OnDestoryActionList = new List<Action<BaseUnit>>();

    public List<Func<BaseUnit, BaseUnit, bool>> CanBlockFuncList = new List<Func<BaseUnit, BaseUnit, bool>>(); // ������λ�ܷ����赲�Ķ����ж��¼�
    public List<Func<BaseUnit, BaseBullet, bool>> CanHitFuncList = new List<Func<BaseUnit, BaseBullet, bool>>(); // ��λ���ӵ��ܷ��໥��ײ�Ķ����ж��¼�
    public List<Func<BaseUnit, BaseUnit, bool>> CanBeSelectedAsTargetFuncList = new List<Func<BaseUnit, BaseUnit, bool>>(); // �ܷ�ѡȡ��ΪĿ�꣨��һ�����������Լ����ڶ��������������������ߣ�

    public Dictionary<string, BaseUnit> mAttachedUnitDict = new Dictionary<string, BaseUnit>(); // ���������ڸõ�λ�ϵĵ�λ����

    public StateController mStateController = new StateController();
    public ComponentController mComponentController = new ComponentController();


    private bool isUseDefaultRecieveDamageActionMethod = true; // �ڽ����˺�ʱ���Ƿ�ʹ��Ĭ�ϵĽ����˺��߼�

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
        actionPointController = new ActionPointController(this);
        mRecordDamageComponent = new RecordDamageComponent(this);
        mEffectController = new EffectController(this);
    }

    public virtual void MInit()
    {
        aliveTime = 0;
        lastPosition = Vector2.one;
        DeltaPosition = Vector2.one;

        // ������������ʼ��
        NumericBox.Initialize();
        actionPointController.Initialize();
        skillAbilityManager.Initialize();
        statusAbilityManager.Initialize();
        mEffectController.MInit();
        animatorController.Initialize();
        hitBox.Initialize();
        SpriteOffsetX.Initialize(); SpriteOffsetY.Initialize();
        SetSpriteLocalPosition(Vector2.zero);
        taskController.Initial();
        mRecordDamageComponent.Initilize();
        isIgnoreRecordDamage = false;

        typeAndShapeValue = 0; // ͼ��
        SetUnitType();

        mAttackFlag = true; // ������һ�ι����ܷ�������flag

        // ����״̬
        isDeathState = false;
        disableMove = false;

        // ��ʼ����ǰ����״̬
        mCurrentActionState = null;
        SetActionState(new BaseActionState(this));
        // ���ö�����
        SetActionPointManager();
        // ��ȡ������Ϣ
        LoadSkillAbility();
        // �����ж�
        OpenCollision();
        // ����Collider2D�Ĳ���
        SetCollider2DParam();
        // ��С��ʼ��
        SetLocalScale(Vector2.one);
        SetSpriteLocalScale(Vector2.one);
        transform.right = Vector2.right;

        BeforeDeathEventList.Clear();
        AfterDeathEventList.Clear();
        OnDestoryActionList.Clear();

        CanBlockFuncList.Clear();
        CanHitFuncList.Clear();
        CanBeSelectedAsTargetFuncList.Clear();

        mAttachedUnitDict.Clear();

        // ��ʼ��͸����
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
        // ���������λ�ֵ���Ч�ĵ�λ
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
        if (mCurrentActionState != null)
            mCurrentActionState.OnUpdate();
        // ��ǰλ��״̬����
        if (aliveTime <= 0)
        {
            lastPosition = transform.position;
            DeltaPosition = Vector2.zero;
        }
        else
        {
            DeltaPosition = (Vector2)transform.position - lastPosition;
        }

        // �ܻ����Ⱥ��Ӹ���
        hitBox.Update();
        // �����������
        if(!isDeathState)
            taskController.Update();
        // ��Ч�����
        mEffectController.MUpdate();
        // lastPosition���·���Task���棬����Ҳ�������������ȡ��֡����
        lastPosition = transform.position;

        mStateController.Update();
        mComponentController.Update();

        // �����´��ʱ��
        aliveTime++;
    }

    public virtual void MPause()
    {
        animatorController.Pause();
        mEffectController.MPause();
    }

    public virtual void MResume()
    {
        animatorController.Resume();
        mEffectController.MResume();
    }

    public virtual void MDestory()
    {
        foreach (var action in OnDestoryActionList)
        {
            action(this);
        }
        // ��ʼ��͸����
        SetAlpha(1);

        mEffectController.MDestory();
        mStateController.Destory();
        mComponentController.Destory();
        actionPointController.Initialize();

        if (GetSpriteRenderer() != null)
            GetSpriteRenderer().sprite = null;
        aliveTime = 0;
        ExecuteRecycle();
    }


    // �л�����״̬
    public void SetActionState(IBaseActionState state)
    {
        // ����ǰ״̬Ϊ����״̬������ͨ���˷������л��ɱ��״̬
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
            // �����ü�������������Update��ֹͣ����
        }
        else
        {
            if (mCurrentActionState != null && mCurrentActionState is FrozenState && !isFrozenState)
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
        return transform.position;
    }

    // ����λ��
    public virtual void SetPosition(Vector3 V3)
    {
        transform.position = V3;
    }


    /// <summary>
    /// ������֮ǰ
    /// </summary>
    public virtual void BeforeDeath()
    {
        // ֻ�ܴ���һ��
        if (isDeathState)
            return;
        // ������������״̬
        SetActionState(new DieState(this));
        isDeathState = true;
        // �����¼�
        foreach (var item in BeforeDeathEventList)
        {
            item(this);
        }
        // �������Ч��
        skillAbilityManager.TryEndAllSpellingSkillAbility();
        // ���BUFFЧ��
        statusAbilityManager.TryEndAllStatusAbility();
        // �����Чһ��
        mEffectController.MDestory();
        // �Ƴ����в��ö������ŵ�����
        animatorController.RemoveAllPauseModifier();
        taskController.Initial();
    }

    // �����������ˣ�����ʱ���м�֡״̬��Ҫ������
    public virtual void DuringDeath()
    {
        // ��֪��Ҫ��ɶ�ˣ���������ط��϶��Ȳ�����
    }

    /// <summary>
    /// �ҽ���ǰ
    /// </summary>
    public virtual void BeforeBurn()
    {       
        // ֻ�ܴ���һ��
        if (isDeathState)
            return;
        // ����һ���ս�����Ч��Ȼ��ɾ������
        {
            BaseEffect e = BaseEffect.CreateInstance(GetSpirte());
            e.transform.position = transform.position + (Vector3)GetSpriteLocalPosition();

            CustomizationTask t = new CustomizationTask();
            t.AddOnEnterAction(delegate {
                e.spriteRenderer.material = GameManager.Instance.GetMaterial("Dissolve2");
            });
            t.AddTimeTaskFunc(60, null, (left, total) => {
                e.spriteRenderer.material.SetFloat("_Threshold", 1 - (float)left / total);
            }, null);
            t.AddOnExitAction(delegate {
                e.ExecuteDeath();
            });
            e.AddTask(t);

            GameController.Instance.AddEffect(e);
        }
        BeforeDeath();
        DeathEvent();
    }

    /// <summary>
    /// ˤ��ǰ
    /// </summary>
    public virtual void BeforeDrop()
    {
        // ֻ�ܴ���һ��
        if (isDeathState)
            return;
        // ����һ��ˤ�����Ч��Ȼ��ɾ������
        {
            BaseEffect e = BaseEffect.CreateInstance(GetSpirte());
            e.transform.position = transform.position + (Vector3)GetSpriteLocalPosition();

            CustomizationTask t = new CustomizationTask();
            t.AddOnEnterAction(delegate {
            });
            t.AddTimeTaskFunc(60, null, (left, total) => {
                float r = (float)left / total;
                e.spriteRenderer.color = new Color(1, 1, 1, r);
                e.spriteRenderer.transform.position += 0.25f * MapManager.gridHeight * (1-r) * Vector3.down;
                e.spriteRenderer.transform.localScale = Vector3.one * r;
            }, null);
            t.AddOnExitAction(delegate {
                e.ExecuteDeath();
            });
            e.AddTask(t);

            GameController.Instance.AddEffect(e);
        }
        BeforeDeath();
        DeathEvent();
    }

    /// <summary>
    /// ֱ����Ŀ��������ֻ�о���AfterDeath()��������BeforeDeath()��DuringDeath()��ע����ExecuteDeath()��������)
    /// </summary>
    public void DeathEvent()
    {
        isDeathState = true;
        // ���������Ч��һ��
        skillAbilityManager.TryEndAllSpellingSkillAbility();
        // �����BUFFЧ��һ��
        statusAbilityManager.TryEndAllStatusAbility();
        // �������Чһ��
        mEffectController.MDestory();
        // �������Ч��
        taskController.Initial();
        // �Ƴ����в��ö������ŵ�����
        animatorController.RemoveAllPauseModifier();
        AfterDeath();
        foreach (var item in AfterDeathEventList)
        {
            item(this);
        }
        // Ȼ����ȥ����
        MDestory();
    }

    /// <summary>
    /// ��λ�������¼�
    /// </summary>
    public virtual void AfterDeath()
    {
        // ������ҲҪ����Ϊ�����
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
        return !isDeathState && isActiveAndEnabled;
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
    /// �Ƿ�ɱ�ѡȡΪĿ��
    /// </summary>
    /// <returns></returns>
    public virtual bool CanBeSelectedAsTarget(BaseUnit otherUnit)
    {
        return true;
    }

    /// <summary>
    /// ������Ⱦ�㼶
    /// </summary>
    public virtual void UpdateRenderLayer(int arrayIndex)
    {

    }

    /// <summary>
    /// ���Ի�ȡĿ��SpriteRenterer��Sorting��Ϣ
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
        actionPointController.AddListener(actionPointType, action);
    }

    public void RemoveActionPointListener(ActionPointType actionPointType, Action<CombatAction> action)
    {
        actionPointController.RemoveListener(actionPointType, action);
    }

    public void TriggerActionPoint(ActionPointType actionPointType, CombatAction action)
    {
        actionPointController.TriggerActionPoint(actionPointType, action);
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
    /// ִ��ˤ��
    /// </summary>
    public void ExecuteDrop()
    {
        BeforeDrop();
    }

    /// <summary>
    /// ��ȡ���������Ŀ�����յ����˱���
    /// </summary>
    public float GetFinalDamageRate()
    {
        return (1 - mCurrentDefense) * mDamgeRate;
    }

    /// <summary>
    /// �ܵ��˺�ʱ�����˺�
    /// </summary>
    /// <param name="dmg"></param>
    public virtual float OnDamage(float dmg)
    {
        // ���Ŀ�꺬���޵б�ǩ����ֱ�������˺��ж�
        if (NumericBox.GetBoolNumericValue(StringManager.Invincibility))
            return 0;

        // �ȼ��㿹�Լ������˺�
        dmg = Mathf.Max(0, dmg * GetFinalDamageRate());
        float recordDmg = dmg;
        // Ȼ����㻤�����յ��˺�
        dmg = Mathf.Max(0, NumericBox.DamageShield(dmg));
        dmg = Mathf.Min(dmg, _mCurrentHp);
        // ���۳���������ֵ
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
        // ���Ŀ�꺬���޵б�ǩ����ֱ�������˺��ж�
        if (NumericBox.GetBoolNumericValue(StringManager.Invincibility))
            return 0;

        // �ȼ��㿹�Լ������˺�
        dmg = Mathf.Max(0, dmg * GetFinalDamageRate()*NumericBox.AoeRate.TotalValue);
        float recordDmg = dmg;
        // Ȼ����㻤�����յ��˺�
        dmg = Mathf.Max(0, NumericBox.DamageShield(dmg));
        dmg = Mathf.Min(dmg, _mCurrentHp);
        // ���۳���������ֵ
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
    /// �ܵ����ӻ��ܵ��˺�
    /// </summary>
    /// <param name="dmg"></param>
    public virtual float OnDamgeIgnoreShield(float dmg)
    {
        // ���Ŀ�꺬���޵б�ǩ����ֱ�������˺��ж�
        if (NumericBox.GetBoolNumericValue(StringManager.Invincibility))
            return 0;

        // �ȼ��㿹�Լ������˺�
        dmg = Mathf.Max(0, dmg * GetFinalDamageRate());
        dmg = Mathf.Min(dmg, _mCurrentHp);
        // ֱ�ӿ۳���������ֵ
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
    /// �ܵ���ʵ�˺�
    /// </summary>
    public virtual float OnRealDamage(float dmg)
    {
        // ���Ŀ�꺬���޵б�ǩ����ֱ�������˺��ж�
        if (NumericBox.GetBoolNumericValue(StringManager.Invincibility))
            return 0;
        dmg = Mathf.Min(dmg, _mCurrentHp);
        // �۳���������ֵ
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
    /// �ܵ��ҽ��˺�
    /// </summary>
    /// <param name="dmg"></param>
    public virtual float OnBurnDamage(float dmg)
    {
        // ���Ŀ�꺬���޵б�ǩ����ֱ�������˺��ж�
        if (NumericBox.GetBoolNumericValue(StringManager.Invincibility))
            return 0;

        // �ҽ��˺�Ϊ��ʵ�˺�
        dmg = Mathf.Min(dmg, _mCurrentHp);
        // �۳���������ֵ
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
    /// �ܵ��ҽ�Ч����Դ�Ļҽ��˺�
    /// </summary>
    /// <param name="dmg"></param>
    public virtual float OnBombBurnDamage(float dmg)
    {
        // ���Ŀ�꺬���޵б�ǩ����ֱ�������˺��ж�
        if (NumericBox.GetBoolNumericValue(StringManager.Invincibility))
            return 0;

        // ��Ҫ����һ�λҽ�����
        dmg = Mathf.Min(dmg * mBurnRate, _mCurrentHp);
        // �۳���������ֵ
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
    /// �ܵ�����˺�ʱ�����˺�
    /// </summary>
    /// <param name="dmg"></param>
    public virtual float OnRecordDamage(float dmg)
    {
        // ���Ŀ�꺬���޵б�ǩ����ֱ�������˺��ж�
        if (NumericBox.GetBoolNumericValue(StringManager.Invincibility))
            return 0;
        dmg = Mathf.Min(dmg, _mCurrentHp);
        // ���۳���������ֵ
        _mCurrentHp -= dmg;
        if (_mCurrentHp <= 0)
        {
            _mCurrentHp = 0;
            ExecuteDeath();
        }
        return dmg;
    }

    /// <summary>
    /// �ܵ�����ʱ��������
    /// </summary>
    /// <param name="cure"></param>
    public virtual void OnCure(float cure)
    {
        _mCurrentHp += cure;
        if (_mCurrentHp > mMaxHp)
            _mCurrentHp = mMaxHp;
    }

    /// <summary>
    /// �ܵ�����ʱ���㻤��
    /// </summary>
    public virtual void OnAddedShield(float value)
    {
        NumericBox.AddDynamicShield(value);
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
        _mCurrentHp = NumericBox.Hp.Value;
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
        // ������ �� �����ƶ� �򷵻�0����
        if (NumericBox.GetBoolNumericValue(StringManager.Frozen) || disableMove)
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
    /// ��ȡ��ͼ�����������
    /// </summary>
    /// <returns></returns>
    public virtual Vector2 GetSpriteLocalPosition()
    {
        return Vector2.zero;
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
        taskController.AddUniqueTask(key, t);
    }

    /// <summary>
    /// ���һ������
    /// </summary>
    /// <param name="t"></param>
    public void AddTask(ITask t)
    {
        taskController.AddTask(t);
    }

    /// <summary>
    /// �Ƴ�Ψһ������
    /// </summary>
    public void RemoveUniqueTask(string key)
    {
        taskController.RemoveUniqueTask(key);
    }

    /// <summary>
    /// �Ƴ�һ������
    /// </summary>
    /// <param name="t"></param>
    public void RemoveTask(ITask t)
    {
        taskController.RemoveTask(t);
    }

    /// <summary>
    /// ��ȡĳ�����Ϊkey������
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public ITask GetTask(string key)
    {
        return taskController.GetTask(key);
    }

    /// <summary>
    /// ���ô�С
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
    /// u1һ��Ϊ�Լ���u2һ������ͼѡȡ�Լ�Ϊ����Ŀ��Ĺ�����
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
    /// �����ƶ���Ĭ��Ϊfalse,����Ϊtrueʱ�����ƶ�״̬ʱ���ٸ����ƶ����ı������
    /// </summary>
    /// <param name="disable"></param>
    public void DisableMove(bool disable)
    {
        disableMove = disable;
    }

    /// <summary>
    /// ���û�������
    /// </summary>
    public void SetBaseAttribute(float maxHp, float attack, float attackSpeed, float standardMoveSpeed, float defence, float attackPercent, int height)
    {
        // Ѫ��
        NumericBox.Hp.SetBase(maxHp);
        _mCurrentHp = mMaxHp;
        // ������
        NumericBox.Attack.SetBase(attack);
        // �����ٶ��빥�����
        NumericBox.AttackSpeed.SetBase(attackSpeed);
        NumericBox.MoveSpeed.SetBase(TransManager.TranToVelocity(standardMoveSpeed));
        NumericBox.Defense.SetBase(defence);
        this.attackPercent = attackPercent; // �����������Ž��ȵ�attackPercent����ʱ����������Ĺ���
        // �߶�
        mHeight = height;
    }

    /// <summary>
    /// �������
    /// </summary>
    /// <param name="value"></param>
    public virtual void AddRecordDamage(float value)
    {
        if(!isIgnoreRecordDamage)
            mRecordDamageComponent.AddRecordDamage(value);
    }

    /// <summary>
    /// ���unit����
    /// </summary>
    /// <param name="unit"></param>
    public void AddUnitToDict(string key, BaseUnit unit)
    {
        mAttachedUnitDict.Add(key, unit);
    }

    /// <summary>
    /// �Ƴ�unit����
    /// </summary>
    /// <param name="key"></param>
    public void RemoveUnitFromDict(string key)
    {
        mAttachedUnitDict.Remove(key);
    }

    /// <summary>
    /// ��ȡ���õĵ�λ�������
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
    /// �Ƿ���ĳ��Ϊkey�ĵ�λ����
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

    /// <summary>
    /// ִ�иõ�λ�����¼�
    /// </summary>
    protected virtual void ExecuteRecycle()
    {
        GameManager.Instance.PushGameObjectToFactory(FactoryType.GameFactory, mPreFabPath, this.gameObject);
    }
    // ����
}
