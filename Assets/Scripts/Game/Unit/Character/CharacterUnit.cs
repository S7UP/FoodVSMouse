
using UnityEngine;
/// <summary>
/// 角色单位
/// </summary>
public class CharacterUnit : BaseUnit
{
    // Awake获取的组件
    protected Animator animator;
    private SpriteRenderer spriteRenderer;
    public Material defaultMaterial;
    public BoxCollider2D mBoxCollider2D;
    public Transform spriteTrans;

    // 其它组件
    public BaseWeapons weapons; // 持有武器

    // 其他
    public int typeAndShapeToLayer = 0; // 种类与变种对图层的加权等级

    private BaseGrid mGrid; // 卡片所在的格子（单格卡)


    public override void Awake()
    {
        base.Awake();
        spriteTrans = transform.Find("SpriteGo");
        animator = spriteTrans.gameObject.GetComponent<Animator>();
        spriteRenderer = spriteTrans.gameObject.GetComponent<SpriteRenderer>();
        mBoxCollider2D = transform.GetComponent<BoxCollider2D>();
        defaultMaterial = spriteRenderer.material;  // 装上正常的受击材质
    }

    // 单位被对象池回收时触发
    public override void OnDisable()
    {
        base.OnDisable();
        mGrid = null; // 所在的格子
        spriteRenderer.material = defaultMaterial; // 换回来
    }

    // 每次对象被创建时要做的初始化工作
    public override void MInit()
    {
        base.MInit();
        PlayerData data = PlayerData.GetInstance();
        animator.runtimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Character/" + data.GetCharacter());
        // 动画控制器绑定animator
        animatorController.ChangeAnimator(animator);
        // 受伤闪白
        AddActionPointListener(ActionPointType.PostReceiveDamage, FlashWhenHited);
        SetActionState(new IdleState(this));
        // 移除原本的武器引用（如果有
        if (weapons != null)
            weapons.DeathEvent();
        // 添加武器
        int type = data.GetWeapons();
        weapons = GameManager.Instance.GetGameObjectResource(FactoryType.GameFactory, "Weapons/"+type+"/0").GetComponent<BaseWeapons>();
        weapons.MInit();
        weapons.transform.SetParent(transform);
        weapons.master = this;
    }

    public override void SetUnitType()
    {
        mUnitType = UnitType.Character;
    }

    /// <summary>
    /// 设置判定参数
    /// </summary>
    public override void SetCollider2DParam()
    {
        mBoxCollider2D.offset = new Vector2(0, 0);
        mBoxCollider2D.size = new Vector2(0.49f * MapManager.gridWidth, 0.49f * MapManager.gridHeight);
    }

    /// <summary>
    /// 加载技能，此处仅加载普通攻击，具体技能加载实现请在子类中重写
    /// </summary>
    public override void LoadSkillAbility()
    {

    }

    /// <summary>
    /// 当被攻击时闪白
    /// </summary>
    /// <param name="action"></param>
    public void FlashWhenHited(CombatAction action)
    {
        // 当存在攻击来源时
        // if (action.Creator != null)
        {
            hitBox.OnHit();
        }
    }

    /// <summary>
    /// 判断是否有有效的攻击目标
    /// </summary>
    /// <returns></returns>
    protected virtual bool IsHasTarget()
    {
        return false;
    }

    /// <summary>
    /// 是否满足普通攻击的条件
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetGeneralAttackCondition()
    {
        return false;
    }

    /// <summary>
    /// 进入普通攻击动画状态
    /// </summary>
    public override void BeforeGeneralAttack()
    {

    }

    /// <summary>
    /// 普通攻击期间
    /// </summary>
    public override void OnGeneralAttack()
    {

    }

    /// <summary>
    /// 退出普通攻击的条件
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetEndGeneralAttackCondition()
    {
        return false;
    }

    /// <summary>
    /// 退出普通攻击后要做的事
    /// </summary>
    public override void AfterGeneralAttack()
    {

    }

    /// <summary>
    /// 是否为伤害判定时刻（近战攻击为打出实际伤害，远程攻击为确定发射弹体）
    /// </summary>
    /// <returns></returns>
    public virtual bool IsDamageJudgment()
    {
        return false;
    }

    /// <summary>
    /// 执行具体的攻击，位于伤害判定为真之后
    /// </summary>
    public virtual void ExecuteDamage()
    {

    }

    // 在待机状态时每帧要做的事
    public override void OnIdleState()
    {

    }

    // 在攻击状态时每帧要做的事
    public override void OnAttackState()
    {

    }

    public override void BeforeDeath()
    {
        base.BeforeDeath();
        // 武器也随之消亡
        if (weapons != null)
        {
            weapons.ExecuteDeath();
        }
    }

    /// <summary>
    /// 死亡期间
    /// </summary>
    public override void DuringDeath()
    {
        DeathEvent();
    }

    /// <summary>
    /// 获取卡片所在的格子
    /// </summary>
    /// <returns></returns>
    public override BaseGrid GetGrid()
    {
        return mGrid;
    }

    public override void SetGrid(BaseGrid grid)
    {
        mGrid = grid;
    }

    /// <summary>
    /// 当与单位发生碰撞时能否阻挡的判定
    /// 默认是自身与老鼠同行时可以阻挡老鼠
    /// </summary>
    /// <param name="unit"></param>
    /// <returns></returns>
    public override bool CanBlock(BaseUnit unit)
    {
        if (unit is MouseUnit)
        {
            return GetRowIndex() == unit.GetRowIndex();
        }
        return false; // 别的单位暂时默认不能阻挡
    }

    /// <summary>
    /// 美食单位默认被处在同行的子弹击中
    /// </summary>
    /// <param name="bullet"></param>
    /// <returns></returns>
    public override bool CanHit(BaseBullet bullet)
    {
        return GetRowIndex() == bullet.GetRowIndex();
    }

    /// <summary>
    /// 状态相关
    /// </summary>
    public override void OnIdleStateEnter()
    {
        animatorController.Play("Idle", true);
    }

    public override void OnAttackStateEnter()
    {
        // 每次攻击时，最好根据攻速来计算一下播放速度，然后改变播放速度
        UpdateAttackAnimationSpeed();
        animatorController.Play("Attack");
    }

    public override void OnAttackStateExit()
    {
        // 攻击结束后播放速度改回来
        animator.speed = 1;
    }

    public override void OnAttackStateContinue()
    {
        // 每次攻击时，最好根据攻速来计算一下播放速度，然后改变播放速度
        UpdateAttackAnimationSpeed();
    }

    public override void OnDieStateEnter()
    {
        // 对于美食来说没有死亡动画的话，直接回收对象就行，在游戏里的体现就是直接消失，回收对象的事在duringDeath第一帧去做
    }

    public override void OnBurnStateEnter()
    {
        // 禁止播放动画
        // PauseCurrentAnimatorState(new BoolModifier(true));
    }

    public override void DuringBurn(float _Threshold)
    {
        DeathEvent();
    }

    private BoolModifier boolModifier = new BoolModifier(true);

    /// <summary>
    /// 摔落死亡瞬间
    /// </summary>
    public override void OnDropStateEnter()
    {
        // 禁止播放动画
        PauseCurrentAnimatorState(boolModifier);
    }

    /// <summary>
    /// 摔落死亡过程
    /// </summary>
    public override void OnDropState(float r)
    {
        SetAlpha(1-r);
        spriteRenderer.transform.localPosition = spriteRenderer.transform.localPosition + 0.25f * MapManager.gridHeight * r * Vector3.down;
        spriteRenderer.transform.localScale = Vector3.one * (1 - r);
        // 超过1就可以回收了
        if (r >= 1.0)
        {
            ResumeCurrentAnimatorState(boolModifier);
            SetAlpha(1);
            spriteRenderer.transform.localPosition = Vector3.zero;
            spriteRenderer.transform.localScale = Vector3.one;
            DeathEvent();
        }
    }

    /// <summary>
    /// 根据攻击速度来更新攻击动画的速度
    /// </summary>
    private void UpdateAttackAnimationSpeed()
    {
        AnimatorStateRecorder a = animatorController.GetAnimatorStateRecorder("Attack");
        float time = a.aniTime; // 一倍速下一次攻击动画的播放时间（帧）
        float interval = 1 / NumericBox.AttackSpeed.Value*60;  // 攻击间隔（帧）
        float speed = Mathf.Max(1, time / interval); // 计算动画实际播放速度
        animatorController.SetSpeed("Attack", speed);
    }

    /// <summary>
    /// 设置渲染层级
    /// </summary>
    /// <param name="arrayIndex"></param>
    public override void UpdateRenderLayer(int arrayIndex)
    {
        spriteRenderer.sortingOrder = LayerManager.CalculateSortingLayer(LayerManager.UnitType.Ally, GetRowIndex(), typeAndShapeToLayer, 2 * arrayIndex);
        UpdateSpecialRenderLayer();
        // 设置武器层级
        if (weapons != null)
            weapons.SetSpriteRenderSortingOrder(spriteRenderer.sortingOrder+1);
    }

    /// <summary>
    /// 由子类实现，更新子类特殊组件的层数
    /// </summary>
    public virtual void UpdateSpecialRenderLayer()
    {

    }

    // 死亡后，将自身信息从对应格子移除，以腾出空间给后续其他同格子分类型卡片使用
    public override void AfterDeath()
    {
        RemoveFromGrid();
        if (weapons != null)
        {
            weapons.DeathEvent();
            weapons = null;
        }
        // 更新角色管理器的信息
        if (GameController.Instance.mCharacterController != null)
        {
            GameController.Instance.mCharacterController.AfterCharacterDeath();
        }
    }

    public override void MUpdate()
    {
        base.MUpdate();
        // 更新武器逻辑
        if (weapons != null)
            weapons.MUpdate();
        // 更新受击闪烁状态
        if (hitBox.GetPercent() > 0)
        {
            spriteRenderer.material.SetFloat("_FlashRate", 0.5f * hitBox.GetPercent());
        }
    }

    /// <summary>
    /// 启用冰冻减速效果
    /// </summary>
    /// <param name="enable"></param>
    public override void SetFrozeSlowEffectEnable(bool enable)
    {
        if (enable)
        {
            spriteRenderer.material.SetFloat("_IsSlow", 1);
        }
        else
        {
            spriteRenderer.material.SetFloat("_IsSlow", 0);
        }
    }

    /// <summary>
    /// 将自身移除出格子
    /// </summary>
    public virtual void RemoveFromGrid()
    {
        CharacterUnit c = mGrid.RemoveCharacterUnit();
        if (c != this)
            Debug.LogWarning("格子所移除角色非本角色！！");
    }

    /// <summary>
    /// 启动判定
    /// </summary>
    public override void OpenCollision()
    {
        mBoxCollider2D.enabled = true;
    }

    /// <summary>
    /// 关闭判定
    /// </summary>
    public override void CloseCollision()
    {
        mBoxCollider2D.enabled = false;
    }

    /// <summary>
    /// 设置透明度
    /// </summary>
    public override void SetAlpha(float a)
    {
        spriteRenderer.material.SetFloat("_Alpha", a);
    }

    /// <summary>
    /// 获取行下标
    /// </summary>
    /// <returns></returns>
    public override int GetRowIndex()
    {
        // 依附于格子则返回对应格子的下标
        if (GetGrid() != null)
        {
            return GetGrid().GetRowIndex();
        }
        return base.GetRowIndex();
    }

    /// <summary>
    /// 获取行下标
    /// </summary>
    /// <returns></returns>
    public override int GetColumnIndex()
    {
        // 依附于格子则返回对应格子的下标
        if (GetGrid() != null)
        {
            return GetGrid().GetColumnIndex();
        }
        return base.GetColumnIndex();
    }

    /// <summary>
    /// 设置贴图对象坐标
    /// </summary>
    public override void SetSpriteLocalPosition(Vector2 vector2)
    {
        spriteTrans.localPosition = vector2;
        if(weapons!=null)
            weapons.transform.localPosition = vector2;
    }

    /// <summary>
    /// 获取贴图
    /// </summary>
    public SpriteRenderer GetSpriteRender()
    {
        return spriteRenderer;
    }

    /// <summary>
    /// 可否被选择为目标
    /// </summary>
    /// <returns></returns>
    public override bool CanBeSelectedAsTarget(BaseUnit otherUnit)
    {
        return mBoxCollider2D.enabled && base.CanBeSelectedAsTarget(otherUnit);
    }

    public override void MPause()
    {
        base.MPause();
        // 暂停武器动作
        if(weapons!=null)
            weapons.MPause();
    }

    public override void MResume()
    {
        base.MResume();
        // 取消暂停武器动作
        if (weapons != null)
            weapons.MResume();
    }

    /// <summary>
    /// 获取贴图对象
    /// </summary>
    public override Sprite GetSpirte()
    {
        return spriteRenderer.sprite;
    }

    /// <summary>
    /// 获取SpriterRenderer
    /// </summary>
    /// <returns></returns>
    public override SpriteRenderer GetSpriteRenderer()
    {
        return spriteRenderer;
    }

    private WeaponsFrozenState frozenStatus;

    /// <summary>
    /// 被冻结时连带武器一起被冻结
    /// </summary>
    public override void OnFrozenStateEnter()
    {
        base.OnFrozenStateEnter();
        frozenStatus = new WeaponsFrozenState(weapons, weapons.mCurrentActionState);
        weapons.SetActionState(frozenStatus);
    }

    /// <summary>
    /// 解冻时连带武器一起解冻
    /// </summary>
    public override void OnFrozenStateExit()
    {
        base.OnFrozenStateExit();
        frozenStatus.TryExitCurrentState();
    }
}
