using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static UnityEditor.Experimental.GraphView.GraphView;
/// <summary>
/// 角色单位
/// </summary>
public class CharacterUnit : BaseUnit
{
    // Awake获取的组件
    protected Animator animator;
    private SpriteRenderer spriteRenderer;
    public Material defaultMaterial;
    public Collider2D mCollider2D;
    public Transform spriteTrans;

    // 其它组件
    public BaseWeapons weapons; // 持有武器

    // 其他
    // type: 0 = female；1 = male
    public int typeAndShapeToLayer = 0; // 种类与变种对图层的加权等级

    private BaseGrid mGrid; // 卡片所在的格子（单格卡)


    public override void Awake()
    {
        base.Awake();
        spriteTrans = transform.Find("SpriteGo");
        animator = spriteTrans.gameObject.GetComponent<Animator>();
        spriteRenderer = spriteTrans.gameObject.GetComponent<SpriteRenderer>();
        mCollider2D = transform.GetComponent<Collider2D>();
        defaultMaterial = spriteRenderer.material;  // 装上正常的受击材质
        AnimatorContinue(); // 恢复动画
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
        animator.runtimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Character/" + mType + "/" + mShape);
        // 受伤闪白
        AddActionPointListener(ActionPointType.PostReceiveDamage, FlashWhenHited);
        AnimatorContinue(); // 恢复播放动画
        SetActionState(new IdleState(this));
        // 移除原本的武器引用（如果有
        if (weapons != null)
            weapons.DeathEvent();
        // 添加武器test
        weapons = GameManager.Instance.GetGameObjectResource(FactoryType.GameFactory, "Weapons/0/0").GetComponent<BaseWeapons>();
        weapons.MInit();
        weapons.transform.SetParent(transform);
        weapons.master = this;
    }

    public override void SetUnitType()
    {
        mUnitType = UnitType.Character;
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
        if (action.Creator != null)
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
        animator.Play("Idle");
    }

    public override void OnAttackStateEnter()
    {
        // 每次攻击时，最好根据攻速来计算一下播放速度，然后改变播放速度
        UpdateAttackAnimationSpeed();
        animator.Play("Attack");
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
        AnimatorStop();
    }

    public override void DuringBurn(float _Threshold)
    {
        DeathEvent();
    }

    /// <summary>
    /// 根据攻击速度来更新攻击动画的速度
    /// </summary>
    private void UpdateAttackAnimationSpeed()
    {
        float time = AnimatorManager.GetClipTime(animator, "Attack"); // 1倍情况下，一次攻击的默认时间 秒
        float interval = 1 / NumericBox.AttackSpeed.Value; // 攻击间隔  秒
        float rate = Mathf.Max(1, time / interval);
        AnimatorManager.SetClipSpeed(animator, "Attack", rate);
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

    public override void AnimatorStop()
    {
        animator.speed = 0;
    }

    public override void AnimatorContinue()
    {
        animator.speed = 1;
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
        mCollider2D.enabled = true;
    }

    /// <summary>
    /// 关闭判定
    /// </summary>
    public override void CloseCollision()
    {
        mCollider2D.enabled = false;
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
    }

    /// <summary>
    /// 获取贴图
    /// </summary>
    public SpriteRenderer GetSpriteRender()
    {
        return spriteRenderer;
    }
}
