using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

using static System.Collections.Specialized.BitVector32;
using static UnityEngine.UI.CanvasScaler;

public class MouseUnit : BaseUnit
{
    // 老鼠单位的属性
    [System.Serializable]
    new public struct Attribute
    {
        public BaseUnit.Attribute baseAttrbute;
        // public double baseMoveSpeed;
        public double[] hertRateList;
    }

    // Awake里Find一次的组件
    public Rigidbody2D rigibody2D;
    private SpriteRenderer spriteRenderer;
    protected Animator animator;

    // 其他属性

    protected List<double> mHertRateList; // 切换贴图时的受伤比率（高->低)
    public int mHertIndex; // 受伤贴图阶段
    

    // 索敌相关
    protected bool isBlock { get; set; } // 是否被阻挡
    protected BaseUnit mBlockUnit; // 阻挡者

    /// <summary>
    /// 老鼠每次被投入战场时要做的初始化工作，要确定其各种属性
    /// </summary>
    public override void MInit()
    {
        base.MInit();
        
        // 从Json中读取的属性以及相关的初始化
        MouseUnit.Attribute attr = GameController.Instance.GetMouseAttribute();
        mHertRateList = new List<double>();
        foreach (var item in attr.hertRateList)
        {
            mHertRateList.Add(item);
        }
        mHertIndex = 0;

        // 初始为移动状态
        SetActionState(new MoveState(this));
        // 更新贴图
        UpdateRuntimeAnimatorController();

        renderManager.AddSpriteRenderer("mainSprite", spriteRenderer);
        // 添加几个行动点响应事件
        // 在受到伤害结算之后，更新受伤贴图状态
        AddActionPointListener(ActionPointType.PostReceiveDamage, delegate { UpdateHertMap(); });
        AddActionPointListener(ActionPointType.PostReceiveDamage, FlashWhenHited);
        // 在接收治疗结算之后，更新受伤贴图状态
        AddActionPointListener(ActionPointType.PostReceiveCure, delegate { UpdateHertMap(); });
        // 装上正常的受击材质
        spriteRenderer.material = GameManager.Instance.GetMaterial("Hit");
        AnimatorContinue(); // 恢复动画
    }

    public override void SetUnitType()
    {
        mUnitType = UnitType.Mouse;
    }

    /// <summary>
    /// 只有对象被创建时做一次，主要是用来获取各组件的引用
    /// </summary>
    public override void Awake()
    {
        base.Awake();
        // 组件获取
        rigibody2D = gameObject.GetComponent<Rigidbody2D>();
        animator = gameObject.transform.Find("Ani_Mouse").gameObject.GetComponent<Animator>();
        spriteRenderer = gameObject.transform.Find("Ani_Mouse").gameObject.GetComponent<SpriteRenderer>();
    }

    /// <summary>
    /// 单位被对象池回收时触发，主要是用来将各种属性再初始化回去
    /// </summary>
    public override void OnDisable()
    {
        base.OnDisable();
        // 其他属性
        mHertRateList = null;

        // 索敌相关
        isBlock = false; // 是否被阻挡
        mBlockUnit = null; // 阻挡者
    }

    /// <summary>
    /// 加载技能，此处仅加载普通攻击，具体技能加载实现请在子类中重写
    /// </summary>
    public override void LoadSkillAbility()
    {
        foreach (var item in AbilityManager.Instance.GetSkillAbilityInfoList(mUnitType, mType, mShape))
        {
            if (item.skillType == SkillAbility.Type.GeneralAttack)
            {
                skillAbilityManager.AddSkillAbility(new GeneralAttackSkillAbility(this, item));
            }
        }
    }

    /// <summary>
    /// 至少大部分老鼠应该有一个能直接攻击的手段
    /// </summary>
    /// <param name="unit"></param>
    public void TakeDamage(BaseUnit unit)
    {
        new DamageAction(CombatAction.ActionType.CauseDamage, this, unit, mCurrentAttack).ApplyAction();
    }


    // 注：一个Collider2D不应该直接使用Transform或者其offset属性来移动它，而是应该使用Rigidbody2D的移动代替之。这样会得到最好的表现和正确的碰撞检测。
    // 因此，操作老鼠移动不应该用上面的transform,而是用下面的rigibody2D
    public override void SetPosition(Vector3 V3)
    {
        rigibody2D.MovePosition(V3);
    }

    /// <summary>
    /// 是否满足普通攻击的条件
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetGeneralAttackCondition()
    {
        // 被阻挡了 且 阻挡对象是有效的
        return IsHasTarget();
    }

    /// <summary>
    /// 进入普通攻击动画状态
    /// </summary>
    public override void BeforeGeneralAttack()
    {
        // 切换为攻击动画贴图
        SetActionState(new AttackState(this));
    }

    /// <summary>
    /// 普通攻击期间
    /// </summary>
    public override void OnGeneralAttack()
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
    public override bool IsMeetEndGeneralAttackCondition()
    {
        return animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f; // 攻击动画播放完整一次后视为技能结束
    }

    /// <summary>
    /// 退出普通攻击后要做的事
    /// </summary>
    public override void AfterGeneralAttack()
    {
        mAttackFlag = true;
        // 如果有可以攻击的目标，则停下来等待下一次攻击，否则前进
        if (IsHasTarget())
            SetActionState(new IdleState(this));
        else
            SetActionState(new MoveState(this));
        UpdateBlockState(); // 更新阻挡状态
    }

    /// <summary>
    /// 判断是有有效的攻击目标
    /// </summary>
    /// <returns></returns>
    protected virtual bool IsHasTarget()
    {
        return (isBlock && mBlockUnit.IsAlive());
    }

    /// <summary>
    /// 获取当前攻击目标，上述判断一并使用
    /// </summary>
    /// <returns></returns>
    protected virtual BaseUnit GetCurrentTarget()
    {
        return mBlockUnit;
    }

    /// <summary>
    /// 更新阻挡的状态
    /// </summary>
    protected virtual void UpdateBlockState()
    {
        if (mBlockUnit.IsAlive())
            isBlock = true;
        else
            isBlock = false;
    }

    /// <summary>
    /// 给予群攻型敌人一种可以选择多个目标的接口
    /// </summary>
    /// <returns></returns>
    protected virtual List<BaseUnit> GetCurrentTargetList()
    {
        return null;
    }

    /// <summary>
    /// 是否为伤害判定时刻（近战攻击为打出实际伤害，远程攻击为确定发射弹体）
    /// </summary>
    /// <returns></returns>
    public virtual bool IsDamageJudgment()
    {
        AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
        return (info.normalizedTime - Mathf.FloorToInt(info.normalizedTime) >= attackPercent && mAttackFlag);
    }

    /// <summary>
    /// 执行具体的攻击，位于伤害判定为真之后
    /// </summary>
    public virtual void ExecuteDamage()
    {
        if (IsHasTarget())
            TakeDamage(GetCurrentTarget());
    }

    // 以下为 IBaseStateImplementor 接口的方法实现
    public override void OnIdleState()
    {

    }

    public override void OnMoveState()
    {
        // 移动更新
        SetPosition((Vector2)GetPosition() + Vector2.left * mCurrentMoveSpeed * 0.003f);
    }

    public override void OnAttackState()
    {

    }

    /// <summary>
    /// 老鼠单位默认都是处在同行且同高度时被美食阻挡
    /// 提示，特殊类型老鼠可以重写这个方法而强制设为不可阻挡，如幽灵鼠
    /// </summary>
    public override bool CanBlock(BaseUnit unit)
    {
        return (unit is FoodUnit && GetRowIndex() == unit.GetRowIndex() && mHeight == unit.mHeight);
    }
    
    /// <summary>
    /// 老鼠单位默认被处在同行的子弹击中
    /// </summary>
    /// <param name="bullet"></param>
    /// <returns></returns>
    public override bool CanHit(BaseBullet bullet)
    {
        return (GetRowIndex() == bullet.GetRowIndex() && mHeight == bullet.mHeight);
    }

    /// <summary>
    /// 当受伤或者被治疗时，更新单位贴图状态
    /// </summary>
    protected void UpdateHertMap()
    {
        // 要是死了的话就免了吧
        if (isDeathState)
            return;

        // 是否要切换控制器的flag
        bool flag = false;
        // 恢复到上一个受伤贴图检测
        while(mHertIndex > 0 && GetHeathPercent() > mHertRateList[mHertIndex - 1])
        {
            mHertIndex --;
            flag = true;
        }
        // 下一个受伤贴图的检测
        while(mHertIndex < mHertRateList.Count && GetHeathPercent() <= mHertRateList[mHertIndex])
        {
            mHertIndex ++;
            flag = true;
        }
        // 有切换通知时才切换
        if(flag)
            UpdateRuntimeAnimatorController();
    }

    /// <summary>
    /// 自动更新贴图
    /// </summary>
    /// <param name="collision"></param>
    public void UpdateRuntimeAnimatorController()
    {
        string name = animator.GetCurrentAnimatorClipInfo(0)[0].clip.name;
        float time = AnimatorManager.GetNormalizedTime(animator);
        animator.runtimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Mouse/" + mType + "/" + mShape + "/" + mHertIndex);
        // 保持当前动画播放
        animator.Play(name, -1, time);
        OnUpdateRuntimeAnimatorController();
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
            //SpriteRendererManager srm = renderManager.GetSpriteRender("mainSprite");
            //srm.spriteRenderer.material.SetInt("BeAttack",1);
            //spriteRenderer.material.SetFloat("_FlashRate", 1);
            hitBox.OnHit();
        }
    }

    /// <summary>
    /// 当贴图更新时要做的事，由子类override
    /// </summary>
    public virtual void OnUpdateRuntimeAnimatorController()
    {
        // demo...
        //switch (mHertIndex)
        //{
        //    case 0:
        //        break;
        //    default:
        //        break;
        //}
    }

    /// <summary>
    /// 当确定碰撞到美食单位
    /// </summary>
    public virtual void OnCollideFoodUnit(FoodUnit unit)
    {
        isBlock = true;
        mBlockUnit = unit;
    }

    /// <summary>
    /// 碰撞事件
    /// </summary>
    /// <param name="collision"></param>
    public void OnCollision(Collider2D collision)
    {
        // 死亡动画时不接受任何碰撞事件
        if (isDeathState)
        {
            return;
        }

        if (collision.tag.Equals("Food"))
        {
            // 检测到美食单位碰撞了！
            FoodUnit food = collision.GetComponent<FoodUnit>();
            if (UnitManager.CanBlock(this, food)) // 检测双方能否互相阻挡
            {
                OnCollideFoodUnit(food);
            }
        }
        else if (collision.tag.Equals("Bullet"))
        {
            // 检测到子弹单位碰撞了
            BaseBullet bullet = collision.GetComponent<BaseBullet>();
            if (UnitManager.CanBulletHit(this, bullet)) // 检测双方能否互相击中
            {
                bullet.TakeDamage(this);
            }
        }
    }

    // rigibody相关
    private void OnTriggerEnter2D(Collider2D collision)
    {
        OnCollision(collision);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        OnCollision(collision);
    }

    public override void MUpdate()
    {
        base.MUpdate();
        // 更新贴图控制器状态
        // UpdateHertMap();
        // 更新受击闪烁状态
        if (hitBox.GetPercent() > 0)
        {
            spriteRenderer.material.SetFloat("_FlashRate", 0.5f * hitBox.GetPercent());
        }
    }

    /// <summary>
    /// 状态相关
    /// </summary>
    public override void OnIdleStateEnter()
    {
        animator.Play("Idle");
    }

    public override void OnMoveStateEnter()
    {
        animator.Play("Move");
    }

    public override void OnAttackStateEnter()
    {
        animator.Play("Attack");
    }

    public override void OnDieStateEnter()
    {
        animator.Play("Die");
    }

    public override void DuringDeath()
    {
        // 切换时的第一帧直接不执行update()，因为下述的info.normalizedTime的值还停留在上一个状态，逻辑会出问题！
        if (currentStateTimer <= 0)
        {
            return;
        }
        // 获取Die的动作信息，使得回收时机与动画显示尽可能同步
        int currentFrame = AnimatorManager.GetCurrentFrame(animator);
        int totalFrame = AnimatorManager.GetTotalFrame(animator);
        if (currentFrame>totalFrame && currentFrame%totalFrame == 1) // 动画播放完毕后调用DeathEvent()
        {
            DeathEvent();
        }
    }

    public override void OnBurnStateEnter()
    {
        // 装上烧毁材质
        spriteRenderer.material = GameManager.Instance.GetMaterial("Dissolve2");
        // 禁止播放动画
        AnimatorStop();
    }

    public override void DuringBurn(float _Threshold)
    {
        spriteRenderer.material.SetFloat("_Threshold", _Threshold);
        // 超过1就可以回收了
        if (_Threshold >= 1.0)
        {
            DeathEvent();
        }
    }

    /// <summary>
    /// 是否存活
    /// </summary>
    /// <returns></returns>
    public override bool IsAlive()
    {
        return (!isDeathState && base.IsAlive());
    }


    public static void SaveNewMouseInfo()
    {
        MouseUnit.Attribute attr = new MouseUnit.Attribute()
        {
            baseAttrbute = new BaseUnit.Attribute()
            {
                name = "僵尸魔笛鼠", // 单位的具体名称
                type = 5, // 单位属于的分类
                shape = 3, // 单位在当前分类的变种编号

                baseHP = 1700, // 基础血量
                baseAttack = 10, // 基础攻击
                baseAttackSpeed = 1.0, // 基础攻击速度
                attackPercent = 0.6,
                baseDefense = 0,
                baseMoveSpeed = 1.0,
                baseHeight = 0, // 基础高度
            },
            hertRateList = new double[] {  }
        };

        Debug.Log("开始存档老鼠信息！");
        JsonManager.Save(attr, "Mouse/" + attr.baseAttrbute.type + "/" + attr.baseAttrbute.shape + "");
        Debug.Log("老鼠信息存档完成！");
    }

    /// <summary>
    /// 设置在同种类敌人的渲染层级
    /// </summary>
    /// <param name="arrayIndex"></param>
    public override void UpdateRenderLayer(int arrayIndex)
    {
        spriteRenderer.sortingOrder = LayerManager.CalculateSortingLayer(LayerManager.UnitType.Enemy, GetRowIndex(), 0, arrayIndex);
    }

    public override void AnimatorStop()
    {
        animator.speed = 0;
    }

    public override void AnimatorContinue()
    {
        animator.speed = 1;
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
}
