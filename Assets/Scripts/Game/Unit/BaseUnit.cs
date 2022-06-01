using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static ActionPointManager;
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
    [SerializeField]
    //public float mCurrentTotalShieldValue { get { return NumericBox.Shield.Value; } } // ��ǰ����ֵ֮��
    public float mCurrentTotalShieldValue;

    protected float attackPercent; // �����������Ž��ȵ�attackPercent����ʱ����������Ĺ���
    protected bool mAttackFlag; // ������һ�ι����ܷ�������flag
    public int mHeight; //+ �߶�
    public bool isFrozenState; // �Ƿ��ڶ���״̬
    public bool isDeathState{ get; set; } // �Ƿ�������״̬
    public bool isDisableSkill { get { return NumericBox.IsDisableSkill.Value; } } // �Ƿ������������

    public CombatNumericBox NumericBox { get; private set; } = new CombatNumericBox(); // �洢��λ��ǰ���Եĺ���
    public ActionPointManager actionPointManager { get; set; } = new ActionPointManager(); // �ж��������
    public SkillAbilityManager skillAbilityManager { get; set; } = new SkillAbilityManager(); // ���ܹ�����
    public StatusAbilityManager statusAbilityManager { get; set; } = new StatusAbilityManager(); // ʱЧ״̬��BUFF��������
    public RenderManager renderManager { get; set; } = new RenderManager(); // ����Ⱦ�йصĹ�����

    public HitBox hitBox = new HitBox();

    public string mName; // ��ǰ��λ����������
    public int mType; // ��ǰ��λ�����ࣨ�粻ͬ�Ŀ�����ͬ������
    public int mShape; // ��ǰ���൥λ����ۣ�ͬһ�ſ���0��1��2ת�������0��1��2ת�����������֣�

    public IBaseActionState mCurrentActionState; //+ ��ǰ����״̬
    public int currentStateTimer = 0; // ��ǰ״̬�ĳ���ʱ�䣨�л�״̬ʱ�����ã�

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
    } // �洢JSON�ļ������·��

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
        mUnitType = UnitType.Default;
        mCurrentHp = 0; //+ ��ǰ����ֵ
        attackPercent = 0; // �����������Ž��ȵ�attackPercent����ʱ����������Ĺ���
        mAttackFlag = false; // ������һ�ι����ܷ�������flag
        mHeight = 0; //+ �߶�
        isDeathState = true;
        isFrozenState = false;

        mName = null; // ��ǰ��λ����������
        mType = 0; // ��ǰ��λ�����ࣨ�粻ͬ�Ŀ�����ͬ������
        mShape = 0; // ��ǰ���൥λ����ۣ�ͬһ�ſ���0��1��2ת�������0��1��2ת�����������֣�

        mCurrentActionState = null; //+ ��ǰ����״̬
        currentStateTimer = 0; // ��ǰ״̬�ĳ���ʱ�䣨�л�״̬ʱ�����ã�

        NumericBox.Initialize();
        actionPointManager.Initialize();
        skillAbilityManager.Initialize();
        statusAbilityManager.Initialize();
        renderManager.Initialize();
        hitBox.Initialize();
    }

    // �л�����״̬
    public void SetActionState(IBaseActionState state)
    {
        // ����ǰ״̬Ϊ����״̬������ͨ���˷������л��ɱ��״̬
        if (mCurrentActionState is DieState)
            return;

        if(state is FrozenState)
        {
            Debug.Log("Enter FrozenState!");
            if (mCurrentActionState != null)
                mCurrentActionState.OnInterrupt();
            mCurrentActionState = state;
            mCurrentActionState.OnEnter();
            // �����ü�������������Update��ֹͣ����
        }
        else
        {
            if (mCurrentActionState is FrozenState)
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

        // ������������״̬
        isDeathState = true;
        // ���BUFFЧ��
        statusAbilityManager.TryEndAllStatusAbility();
        SetActionState(new DieState(this));
    }

    // �����������ˣ�����ʱ���м�֡״̬��Ҫ������
    public virtual void DuringDeath()
    {
        // ��֪��Ҫ��ɶ�ˣ���������ط��϶��Ȳ�����
    }

    // ����Ÿ�ʲô����û�˾Ȳ�����
    public void DeathEvent()
    {
        // ������ҲҪ����Ϊ�����
        // override
        AfterDeath();
        // Ȼ����ȥ����
        GameManager.Instance.PushGameObjectToFactory(FactoryType.GameFactory, mPreFabPath, this.gameObject);
    }

    public virtual void AfterDeath()
    {
        // ������ҲҪ����Ϊ�����
        // override
        // Ȼ����ȥ����
    }


    /// <summary>
    /// �ҽ�Ч��֮ǰ
    /// </summary>
    public virtual void BeforeBurn()
    {
        // ������������״̬
        isDeathState = true;
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
        // ������������ʼ��
        NumericBox.Initialize();
        actionPointManager.Initialize();
        skillAbilityManager.Initialize();
        statusAbilityManager.Initialize();
        renderManager.Initialize();
        hitBox.Initialize();
        // ����
        mType = attr.type;
        mShape = attr.shape;
        SetUnitType();
        // Ѫ��
        NumericBox.Hp.SetBase((float)attr.baseHP);
        mCurrentHp = mMaxHp;
        // ������
        NumericBox.Attack.SetBase((float)attr.baseAttack);
        // �����ٶ��빥�����
        NumericBox.AttackSpeed.SetBase((float)attr.baseAttackSpeed);
        NumericBox.MoveSpeed.SetBase((float)attr.baseMoveSpeed);
        NumericBox.Defense.SetBase((float)attr.baseDefense);
        attackPercent = (float)attr.attackPercent; // �����������Ž��ȵ�attackPercent����ʱ����������Ĺ���
        mAttackFlag = true; // ������һ�ι����ܷ�������flag
        // �߶�
        mHeight = attr.baseHeight;
        // ����״̬
        isDeathState = false;

        // ��ʼ����ǰ����״̬
        SetActionState(new BaseActionState(this));
        // ���ö�����
        SetActionPointManager();
        // ��ȡ������Ϣ
        LoadSkillAbility();
    }

    public virtual void MUpdate()
    {
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

    public virtual void MPause()
    {

    }

    public virtual void MResume()
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
        return IsValid();
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
    /// ִ����������
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
        // �ȼ��㿹�Լ������˺�
        dmg = Mathf.Max(0, dmg * (1 - mCurrentDefense));
        // Ȼ����㻤�����յ��˺�
        dmg = Mathf.Max(0, NumericBox.DamageShield(dmg));
        // ���۳���������ֵ
        mCurrentHp -= dmg*(1-mCurrentDefense);
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
        // �ȼ��㿹�Լ������˺�
        dmg = Mathf.Max(0, dmg * (1 - mCurrentDefense));
        // ���۳���������ֵ
        mCurrentHp -= dmg * (1 - mCurrentDefense);
        if (mCurrentHp <= 0)
        {
            ExecuteDeath();
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

    /// <summary>
    /// �ɱ�����ʱ���ã���ͣ��ǰ����
    /// </summary>
    public virtual void AnimatorStop()
    {

    }

    /// <summary>
    /// �ɽ������ʱ���ã������ͣ��ǰ����
    /// </summary>
    public virtual void AnimatorContinue()
    {

    }

    /// <summary>
    /// ���ñ�������Ч��
    /// </summary>
    public virtual void SetFrozeSlowEffectEnable(bool enable)
    {

    }

    public void SetMaxHpAndCurrentHp(float hp)
    {
        NumericBox.Hp.SetBase(hp);
        mCurrentHp = NumericBox.Hp.Value;
    }
    // ����
}
