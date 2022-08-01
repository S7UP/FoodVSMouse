using System.Collections.Generic;

using UnityEngine;

/// <summary>
/// 基础武器类
/// </summary>
public class BaseWeapons : MonoBehaviour, IGameControllerMember
{
    protected string mPreFabPath
    {
        get
        {
            return "Weapons/"+mType+"/"+mShape;
        }
    } // 预制体类型/路径

    public Animator animator;
    public SpriteRenderer spriteRenderer;

    public BaseUnit master; // 持有武器者
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

    public SkillAbilityManager skillAbilityManager { get; set; } = new SkillAbilityManager(); // 技能管理器
    public AnimatorController animatorController = new AnimatorController(); // 动画播放控制器

    public virtual void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public virtual void MInit()
    {
        animatorController.Initialize();
        animatorController.ChangeAnimator(animator);
        master = null;
        isFrozenState = false;
        mAttackFlag = true;
        isDisableSkill = false;
        attackPercent = 0.5f; //test
        isDeathState = false;
        skillAbilityManager.Initialize();
        SetActionState(new WeaponsIdleState(this));
        LoadSkillAbility();
    }

    public virtual void MUpdate()
    {
        animatorController.Update();
        if (!isDisableSkill)
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
        animatorController.Pause();
    }

    public virtual void MResume()
    {
        animatorController.Resume();
    }

    /// <summary>
    /// 加载技能，此处仅加载普通攻击，具体技能加载实现请在子类中重写
    /// </summary>
    public virtual void LoadSkillAbility()
    {
        List<SkillAbility.SkillAbilityInfo> infoList = AbilityManager.Instance.GetSkillAbilityInfoList(UnitType.Weapons, mType, mShape);
        // 普通攻击
        if (infoList.Count > 0)
        {
            weaponsGeneralAttackSkillAbility = new WeaponsGeneralAttackSkillAbility(this, infoList[0]);
            skillAbilityManager.AddSkillAbility(weaponsGeneralAttackSkillAbility);
        }
    }

    /// <summary>
    /// 判断是否有有效的攻击目标
    /// </summary>
    /// <returns></returns>
    public virtual bool IsHasTarget()
    {
        List<BaseUnit>[] list = GameController.Instance.GetEnemyList();
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
    /// 是否满足普通攻击的条件
    /// </summary>
    /// <returns></returns>
    public virtual bool IsMeetGeneralAttackCondition()
    {
        // 发现目标即可
        return IsHasTarget();
    }

    /// <summary>
    /// 进入普通攻击动画状态
    /// </summary>
    public virtual void BeforeGeneralAttack()
    {
        // 切换为攻击动画贴图
        SetActionState(new WeaponsAttackState(this));
    }

    /// <summary>
    /// 普通攻击期间
    /// </summary>
    public virtual void OnGeneralAttack()
    {
        // 切换时的第一帧直接不执行update()，因为下述的info.normalizedTime的值还停留在上一个状态，逻辑会出问题！
        if (currentStateTimer <= 0)
        {
            return;
        }
        // 伤害判定帧应当执行判定
        if (IsDamageJudgment())
        {
            mAttackFlag = false;
            ExecuteDamage();
        }
    }

    /// <summary>
    /// 退出普通攻击的条件
    /// </summary>
    /// <returns></returns>
    public virtual bool IsMeetEndGeneralAttackCondition()
    {
        return animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce();
    }

    /// <summary>
    /// 退出普通攻击后要做的事
    /// </summary>
    public virtual void AfterGeneralAttack()
    {
        mAttackFlag = true;
        SetActionState(new WeaponsIdleState(this));
    }

    /// <summary>
    /// 是否为伤害判定时刻（近战攻击为打出实际伤害，远程攻击为确定发射弹体）
    /// </summary>
    /// <returns></returns>
    public virtual bool IsDamageJudgment()
    {
        return (animatorController.GetCurrentAnimatorStateRecorder().GetNormalizedTime() >= attackPercent && mAttackFlag);
    }

    /// <summary>
    /// 执行具体的攻击，位于伤害判定为真之后
    /// </summary>
    public virtual void ExecuteDamage()
    {

    }

    public virtual void OnIdleStateEnter()
    {
        animatorController.Play("Idle", true);
    }

    public virtual void OnIdleState()
    {

    }

    public virtual void OnIdleStateExit()
    {

    }

    public virtual void OnAttackStateEnter()
    {
        animatorController.Play("Attack");
    }

    public virtual void OnAttackState()
    {

    }

    public virtual void OnAttackStateExit()
    {

    }

    public virtual void OnDieStateEnter()
    {
        animatorController.Play("Die");
    }

    public virtual void OnDieStateExit()
    {
        Debug.LogWarning("警告：有武器对象从死亡状态转换到其他状态了！！！");
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

    public virtual void PauseCurrentAnimatorState()
    {
        if (animatorController.currentTask != null)
        {
            animatorController.Pause(animatorController.currentTask.aniName);
        }
    }

    public virtual void ResumeCurrentAnimatorState()
    {
        if (animatorController.currentTask != null)
        {
            animatorController.Resume(animatorController.currentTask.aniName);
        }
    }

    /// <summary>
    /// 切换动作状态
    /// </summary>
    /// <param name="state"></param>
    public void SetActionState(WeaponsActionState state)
    {
        // 若当前状态为死亡状态，则不能通过此方法再切换成别的状态
        if (mCurrentActionState is WeaponsDieState)
            return;

        if (state is WeaponsFrozenState)
        {
            if (mCurrentActionState != null)
                mCurrentActionState.OnInterrupt();
            mCurrentActionState = state;
            mCurrentActionState.OnEnter();
            // 不重置计数器，并且在Update中停止计数
        }
        else
        {
            if (mCurrentActionState is WeaponsFrozenState)
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

    public BaseUnit GetMaster()
    {
        return master;
    }

    /// <summary>
    /// 获取行下标
    /// </summary>
    /// <returns></returns>
    public virtual int GetRowIndex()
    {
        // 依附于单位则返回对应单位的下标
        if (GetMaster() != null)
        {
            return GetMaster().GetRowIndex();
        }
        return MapManager.GetXIndex(transform.position.x);
    }

    /// <summary>
    /// 获取行下标
    /// </summary>
    /// <returns></returns>
    public virtual int GetColumnIndex()
    {
        // 依附于单位则返回对应单位的下标
        if (GetMaster() != null)
        {
            return GetMaster().GetColumnIndex();
        }
        return MapManager.GetYIndex(transform.position.y);
    }

    /// <summary>
    /// 执行死亡操作（需要走BeforeDeath()、DuringDeath()、AfterDeath()的流程）
    /// </summary>
    public void ExecuteDeath()
    {
        BeforeDeath();
    }

    public virtual void BeforeDeath()
    {
        // 进入死亡动画状态
        isDeathState = true;
        SetActionState(new WeaponsDieState(this));
        // 清除技能效果
        skillAbilityManager.TryEndAllSpellingSkillAbility();
    }

    public virtual void OnDieState()
    {
        if (currentStateTimer == 0)
            return;
        if(animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
        {
            DeathEvent();
        }
    }

    // 还愣着干什么，人没了救不了了
    public void DeathEvent()
    {
        // 我死了也要化身为腻鬼！！
        AfterDeath();
        // 再清除技能效果
        skillAbilityManager.TryEndAllSpellingSkillAbility();
        // 然后安心去世吧
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
    /// 设置渲染层级
    /// </summary>
    /// <param name="arrayIndex"></param>
    public virtual void SetSpriteRenderSortingOrder(int layer)
    {
        spriteRenderer.sortingOrder = layer;
    }
}
