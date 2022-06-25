using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ����������
/// </summary>
public class BaseWeapons : MonoBehaviour, IGameControllerMember
{
    protected string mPreFabPath
    {
        get
        {
            return "Weapons/"+mType+"/"+mShape;
        }
    } // Ԥ��������/·��

    public Animator animator;
    public SpriteRenderer spriteRenderer;

    public BaseUnit master; // ����������
    public WeaponsActionState mCurrentActionState;
    public int currentStateTimer;
    public bool isFrozenState;
    public bool isDisableSkill;
    public bool mAttackFlag;
    public bool isDeathState;
    public float attackPercent;

    public int mType;
    public int mShape;

    public WeaponsGeneralAttackSkillAbility weaponsGeneralAttackSkillAbility;

    public SkillAbilityManager skillAbilityManager { get; set; } = new SkillAbilityManager(); // ���ܹ�����

    public virtual void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public virtual void MInit()
    {
        master = null;
        isFrozenState = false;
        mAttackFlag = false;
        isDisableSkill = false;
        attackPercent = 0.5f; //test
        isDeathState = false;
        skillAbilityManager.Initialize();
        SetActionState(new WeaponsIdleState(this));
        LoadSkillAbility();
    }

    public virtual void MUpdate()
    {
        if(!isDisableSkill)
            skillAbilityManager.Update();
        if (!isFrozenState)
            currentStateTimer += 1;
        mCurrentActionState.OnUpdate();
    }

    public virtual void MPauseUpdate()
    {

    }

    public virtual void MPause()
    {

    }

    public virtual void MResume()
    {

    }

    /// <summary>
    /// ���ؼ��ܣ��˴���������ͨ���������弼�ܼ���ʵ��������������д
    /// </summary>
    public virtual void LoadSkillAbility()
    {
        List<SkillAbility.SkillAbilityInfo> infoList = AbilityManager.Instance.GetSkillAbilityInfoList(UnitType.Weapons, mType, mShape);
        // ��ͨ����
        if (infoList.Count > 0)
        {
            weaponsGeneralAttackSkillAbility = new WeaponsGeneralAttackSkillAbility(this, infoList[0]);
            skillAbilityManager.AddSkillAbility(weaponsGeneralAttackSkillAbility);
        }
    }

    /// <summary>
    /// �ж��Ƿ�����Ч�Ĺ���Ŀ��
    /// </summary>
    /// <returns></returns>
    public virtual bool IsHasTarget()
    {
        List<BaseUnit>[] list = GameController.Instance.GetEnemyList();
        //int start = Mathf.Max(0, GetRowIndex() - 1);
        //int end = Mathf.Min(GetRowIndex() + 1, MapController.yRow - 1);
        int start = GetRowIndex();
        int end = start;
        for (int i = start; i <= end; i++)
        {
            if (list[i].Count > 0)
                return true;
        }
        return false;
    }

    /// <summary>
    /// �Ƿ�������ͨ����������
    /// </summary>
    /// <returns></returns>
    public virtual bool IsMeetGeneralAttackCondition()
    {
        // ����Ŀ�꼴��
        return IsHasTarget();
    }

    /// <summary>
    /// ������ͨ��������״̬
    /// </summary>
    public virtual void BeforeGeneralAttack()
    {
        // �л�Ϊ����������ͼ
        SetActionState(new WeaponsAttackState(this));
    }

    /// <summary>
    /// ��ͨ�����ڼ�
    /// </summary>
    public virtual void OnGeneralAttack()
    {
        // �л�ʱ�ĵ�һֱ֡�Ӳ�ִ��update()����Ϊ������info.normalizedTime��ֵ��ͣ������һ��״̬���߼�������⣡
        if (currentStateTimer <= 0)
        {
            return;
        }
        // �˺��ж�֡Ӧ��ִ���ж�
        if (IsDamageJudgment())
        {
            mAttackFlag = false;
            ExecuteDamage();
        }
    }

    /// <summary>
    /// �˳���ͨ����������
    /// </summary>
    /// <returns></returns>
    public virtual bool IsMeetEndGeneralAttackCondition()
    {
        return animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f; // ����������������һ�κ���Ϊ���ܽ���
    }

    /// <summary>
    /// �˳���ͨ������Ҫ������
    /// </summary>
    public virtual void AfterGeneralAttack()
    {
        mAttackFlag = true;
        SetActionState(new WeaponsIdleState(this));
    }

    /// <summary>
    /// �Ƿ�Ϊ�˺��ж�ʱ�̣���ս����Ϊ���ʵ���˺���Զ�̹���Ϊȷ�����䵯�壩
    /// </summary>
    /// <returns></returns>
    public virtual bool IsDamageJudgment()
    {
        AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
        return (info.normalizedTime - Mathf.FloorToInt(info.normalizedTime) >= attackPercent && mAttackFlag);
    }

    /// <summary>
    /// ִ�о���Ĺ�����λ���˺��ж�Ϊ��֮��
    /// </summary>
    public virtual void ExecuteDamage()
    {

    }

    public virtual void OnIdleStateEnter()
    {
        animator.Play("Idle");
    }

    public virtual void OnIdleState()
    {

    }

    public virtual void OnIdleStateExit()
    {

    }

    public virtual void OnAttackStateEnter()
    {
        animator.Play("Attack");
    }

    public virtual void OnAttackState()
    {

    }

    public virtual void OnAttackStateExit()
    {

    }

    public virtual void OnDieStateEnter()
    {
        animator.Play("Die");
    }

    public virtual void OnDieStateExit()
    {
        Debug.LogWarning("���棺���������������״̬ת��������״̬�ˣ�����");
    }

    public virtual void OnFrozenStateEnter()
    {
        
    }

    public virtual void OnFrozenState()
    {

    }

    public virtual void OnFrozenStateExit()
    {

    }

    public virtual void MDestory()
    {

    }

    public virtual void AnimatorStop()
    {
        animator.speed = 0;
    }

    public virtual void AnimatorContinue()
    {
        animator.speed = 1;
    }

    /// <summary>
    /// �л�����״̬
    /// </summary>
    /// <param name="state"></param>
    public void SetActionState(WeaponsActionState state)
    {
        // ����ǰ״̬Ϊ����״̬������ͨ���˷������л��ɱ��״̬
        if (mCurrentActionState is WeaponsDieState)
            return;

        if (state is WeaponsFrozenState)
        {
            if (mCurrentActionState != null)
                mCurrentActionState.OnInterrupt();
            mCurrentActionState = state;
            mCurrentActionState.OnEnter();
            // �����ü�������������Update��ֹͣ����
        }
        else
        {
            if (mCurrentActionState is WeaponsFrozenState)
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

    public BaseUnit GetMaster()
    {
        return master;
    }

    /// <summary>
    /// ��ȡ���±�
    /// </summary>
    /// <returns></returns>
    public virtual int GetRowIndex()
    {
        // �����ڵ�λ�򷵻ض�Ӧ��λ���±�
        if (GetMaster() != null)
        {
            return GetMaster().GetRowIndex();
        }
        return MapManager.GetXIndex(transform.position.x);
    }

    /// <summary>
    /// ��ȡ���±�
    /// </summary>
    /// <returns></returns>
    public virtual int GetColumnIndex()
    {
        // �����ڵ�λ�򷵻ض�Ӧ��λ���±�
        if (GetMaster() != null)
        {
            return GetMaster().GetColumnIndex();
        }
        return MapManager.GetYIndex(transform.position.y);
    }

    /// <summary>
    /// ִ��������������Ҫ��BeforeDeath()��DuringDeath()��AfterDeath()�����̣�
    /// </summary>
    public void ExecuteDeath()
    {
        BeforeDeath();
    }

    public virtual void BeforeDeath()
    {
        // ������������״̬
        isDeathState = true;
        SetActionState(new WeaponsDieState(this));
    }

    public virtual void OnDieState()
    {
        if (currentStateTimer == 0)
            return;
        if (AnimatorManager.GetNormalizedTime(animator)>1.0)
        {
            DeathEvent();
        }
    }

    // ����Ÿ�ʲô����û�˾Ȳ�����
    public void DeathEvent()
    {
        // ������ҲҪ����Ϊ�����
        AfterDeath();
        // Ȼ����ȥ����
        GameManager.Instance.PushGameObjectToFactory(FactoryType.GameFactory, mPreFabPath, this.gameObject);
    }

    public virtual void AfterDeath()
    {

    }

    public bool IsAlive()
    {
        return !isDeathState;
    }

    /// <summary>
    /// ������Ⱦ�㼶
    /// </summary>
    /// <param name="arrayIndex"></param>
    public virtual void SetSpriteRenderSortingOrder(int layer)
    {
        spriteRenderer.sortingOrder = layer;
    }
}
