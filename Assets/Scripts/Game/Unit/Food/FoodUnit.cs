using System.Collections.Generic;

using UnityEngine;

public class FoodUnit : BaseUnit
{
    // 美食单位的属性
    [System.Serializable]
    new public struct Attribute
    {
        public BaseUnit.Attribute baseAttrbute;
        public float[] valueList; // 每级对应数值
        public FoodType foodType;
    }

    // Awake获取的组件
    protected Animator animator;
    protected Animator rankAnimator;
    protected FoodUnit.Attribute attr;
    private SpriteRenderer spriteRenderer1;
    private SpriteRenderer spriteRenderer2;
    public BoxCollider2D mBoxCollider2D;
    public Transform spriteTrans;

    // 其他
    public FoodType mFoodType; // 美食职业划分
    public BaseCardBuilder mBuilder; // 建造自身的建造器
    public int mLevel; //星级
    public int typeAndShapeToLayer = 0; // 种类与变种对图层的加权等级

    private BaseGrid mGrid; // 卡片所在的格子（单格卡)
    private List<BaseGrid> mGridList; // 卡片所在的格子（多格卡）
    public bool isUseSingleGrid; // 是否只占一格


    public override void Awake()
    {
        base.Awake();
        spriteTrans = transform.Find("Ani_Food");
        animator = spriteTrans.GetComponent<Animator>();
        rankAnimator = transform.Find("Ani_Rank").gameObject.GetComponent<Animator>();
        spriteRenderer1 = spriteTrans.GetComponent<SpriteRenderer>();
        spriteRenderer2 = transform.Find("Ani_Rank").gameObject.GetComponent<SpriteRenderer>();
        mBoxCollider2D = transform.GetComponent<BoxCollider2D>();
    }

    // 单位被对象池回收时触发
    public override void OnDisable()
    {
        base.OnDisable();
        mGrid = null; // 卡片所在的格子（单格卡)
        mGridList = null; // 卡片所在的格子（多格卡）
        isUseSingleGrid = false; // 是否只占一格

    }

    // 每次对象被创建时要做的初始化工作
    public override void MInit()
    {
        base.MInit();
        // 动画控制器绑定animator
        animatorController.ChangeAnimator(animator);
        mBuilder = null;
        attr = GameController.Instance.GetFoodAttribute();
        mFoodType = attr.foodType;

        mGridList = new List<BaseGrid>();
        isUseSingleGrid = true;

        animator.runtimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Food/"+mType+"/"+mShape);
        SetActionState(new IdleState(this));

        SetLevel(0);
        // 受伤闪白
        AddActionPointListener(ActionPointType.PostReceiveDamage, FlashWhenHited);
        spriteRenderer2.enabled = true; // 激活星级动画
        // 装上正常的受击材质
        spriteRenderer1.material = GameManager.Instance.GetMaterial("Hit");
    }

    /// <summary>
    /// 设置判定参数
    /// </summary>
    public override void SetCollider2DParam()
    {
        mBoxCollider2D.offset = new Vector2(0, 0);
        mBoxCollider2D.size = new Vector2(0.49f * MapManager.gridWidth, 0.49f * MapManager.gridHeight);
    }

    public override void SetUnitType()
    {
        mUnitType = UnitType.Food;
    }

    public void SetLevel(int level)
    {
        mLevel = level;
        UpdateAttributeByLevel();
        if (mLevel > 3)
        {
            rankAnimator.gameObject.SetActive(true);
            rankAnimator.Play(mLevel.ToString()); // 先播放星级的图标动画
        }else
            rankAnimator.gameObject.SetActive(false);
    }

    /// <summary>
    /// 根据等级表和等级来更新对应数据
    /// </summary>
    public virtual void UpdateAttributeByLevel()
    {

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
                GeneralAttackSkillAbility s = new GeneralAttackSkillAbility(this, item);
                skillAbilityManager.AddSkillAbility(s);
                s.FullCurrentEnergy(); // 现在上来也可以直接攻击了，而非等第一轮攻击CD
            }
        }
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
        mAttackFlag = true;
        SetActionState(new IdleState(this));
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
    /// 对于多格卡片请使用这个
    /// </summary>
    /// <returns></returns>
    public List<BaseGrid> GetGridList()
    {
        return mGridList;
    }

    /// <summary>
    /// 当与单位发生碰撞时能否阻挡的判定
    /// 默认是自身与老鼠同行时可以阻挡老鼠
    /// </summary>
    /// <param name="unit"></param>
    /// <returns></returns>
    public override bool CanBlock(BaseUnit unit)
    {
        if(unit is MouseUnit)
        {
            return GetRowIndex() == unit.GetRowIndex() && IsAlive();
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
        return IsAlive();
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
        animatorController.Play("Attack", false);
    }

    //public override void OnAttackStateExit()
    //{
    //    // 攻击结束后播放速度改回来
    //    animator.speed = 1;
    //}

    public override void OnAttackStateContinue()
    {
        // 每次攻击时，最好根据攻速来计算一下播放速度，然后改变播放速度
        UpdateAttackAnimationSpeed();
        // animator.Play("Attack", -1, currentStateTimer/ConfigManager.fps);
    }

    public override void OnDieStateEnter()
    {
        // 对于美食来说没有死亡动画的话，直接回收对象就行，在游戏里的体现就是直接消失，回收对象的事在duringDeath第一帧去做

        // 从当前格子中移除引用
        RemoveFromGrid();
    }

    private BoolModifier boolModifier = new BoolModifier(true);
    public override void OnBurnStateEnter()
    {
        // 从当前格子中移除引用
        RemoveFromGrid();
        // 装上烧毁材质
        spriteRenderer1.material = GameManager.Instance.GetMaterial("Dissolve2");
        // 屏蔽星级特效
        spriteRenderer2.enabled = false;
        // 禁止播放动画
        PauseCurrentAnimatorState(boolModifier);
    }

    public override void DuringBurn(float _Threshold)
    {
        spriteRenderer1.material.SetFloat("_Threshold", _Threshold);
        // 超过1就可以回收了
        if (_Threshold >= 1.0)
        {
            ResumeCurrentAnimatorState(boolModifier);
            DeathEvent();
        }
    }

    /// <summary>
    /// 摔落死亡瞬间
    /// </summary>
    public override void OnDropStateEnter()
    {
        // 从当前格子中移除引用
        RemoveFromGrid();
        // 屏蔽星级特效
        spriteRenderer2.enabled = false;
        // 禁止播放动画
        PauseCurrentAnimatorState(boolModifier);
    }

    /// <summary>
    /// 摔落死亡过程
    /// </summary>
    public override void OnDropState(float r)
    {
        SetAlpha(1 - r);
        spriteRenderer1.transform.localPosition = spriteRenderer1.transform.localPosition + 0.25f*MapManager.gridHeight*r*Vector3.down;
        spriteRenderer1.transform.localScale = Vector3.one * (1-r);
        // 超过1就可以回收了
        if (r >= 1.0)
        {
            ResumeCurrentAnimatorState(boolModifier);
            SetAlpha(1);
            spriteRenderer1.transform.localPosition = Vector3.zero;
            spriteRenderer1.transform.localScale = Vector3.one;
            DeathEvent();
        }
    }

    /// <summary>
    /// 根据攻击速度来更新攻击动画的速度
    /// </summary>
    protected virtual void UpdateAttackAnimationSpeed()
    {
        UpdateAnimationSpeedByAttackSpeed("Attack");
    }

    protected void UpdateAnimationSpeedByAttackSpeed(string attackClipName)
    {
        AnimatorStateRecorder a = animatorController.GetAnimatorStateRecorder(attackClipName);
        float time = a.aniTime; // 一倍速下一次攻击动画的播放时间（帧）
        float interval = 1 / NumericBox.AttackSpeed.Value * 60;  // 攻击间隔（帧）
        float speed = Mathf.Max(1, time / interval); // 计算动画实际播放速度
        animatorController.SetSpeed(attackClipName, speed);
    }

    public static void SaveNewFoodInfo()
    {
        FoodUnit.Attribute attr = new FoodUnit.Attribute()
        {
            baseAttrbute = new BaseUnit.Attribute()
            {
                name = "终结者酒架", // 单位的具体名称
                type = 7, // 单位属于的分类
                shape = 2, // 单位在当前分类的变种编号

                baseHP = 50, // 基础血量
                baseAttack = 0, // 基础攻击
                baseAttackSpeed = 1.05, // 基础攻击速度
                attackPercent = 0.5,
                baseDefense = 0,
                baseMoveSpeed = 0,
                baseRange = 9,
                baseHeight = 0, // 基础高度
            },
            valueList = new float[] {10, 12, 14, 15, 18, 20, 22, 26, 32, 40, 55, 70, 85, 100, 115, 130, 145 },
            foodType = FoodType.Shooter
        };

        //Debug.Log("开始存档美食信息！");
        JsonManager.Save(attr, "Food/" + attr.baseAttrbute.type + "/" + attr.baseAttrbute.shape + "");
        //Debug.Log("美食信息存档完成！");
    }

    /// <summary>
    /// 设置渲染层级
    /// </summary>
    /// <param name="arrayIndex"></param>
    public override void UpdateRenderLayer(int arrayIndex)
    {
        spriteRenderer1.sortingOrder = LayerManager.CalculateSortingLayer(LayerManager.UnitType.Ally, GetRowIndex(), typeAndShapeToLayer, 2*arrayIndex);
        spriteRenderer2.sortingOrder = LayerManager.CalculateSortingLayer(LayerManager.UnitType.Ally, GetRowIndex(), typeAndShapeToLayer, 2*arrayIndex+1);
        UpdateSpecialRenderLayer();
    }

    /// <summary>
    /// 由子类实现，更新子类特殊组件的层数
    /// </summary>
    public virtual void UpdateSpecialRenderLayer()
    {

    }


    public override void MUpdate()
    {
        base.MUpdate();
        // 更新受击闪烁状态
        if (hitBox.GetPercent() > 0)
        {
            spriteRenderer1.material.SetFloat("_FlashRate", 0.5f * hitBox.GetPercent());
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
            spriteRenderer1.material.SetFloat("_IsSlow", 1);
        }
        else
        {
            spriteRenderer1.material.SetFloat("_IsSlow", 0);
        }
    }

    /// <summary>
    /// 将自身移除出格子
    /// </summary>
    public virtual void RemoveFromGrid()
    {
        mGrid.RemoveFoodUnit(this);
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
        spriteRenderer1.material.SetFloat("_Alpha", a);
    }

    /// <summary>
    /// 获取行下标
    /// </summary>
    /// <returns></returns>
    public override int GetRowIndex()
    {
        // 依附于格子则返回对应格子的下标
        if (GetGrid()!=null)
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
        rankAnimator.transform.localPosition = -0.15f*Vector2.up + vector2;
    }

    /// <summary>
    /// 执行该单位回收事件
    /// </summary>
    public override void ExecuteRecycle()
    {
        // 如果目标有其建造器，则走建造器的销毁流程，否则走默认回收流程
        if (mBuilder != null)
            mBuilder.Destructe(this);
        else
            base.ExecuteRecycle();
    }

    /// <summary>
    /// 获取其建造器
    /// </summary>
    public BaseCardBuilder GetCardBuilder()
    {
        return mBuilder;
    }

    /// <summary>
    /// 可否被选择为目标
    /// </summary>
    /// <returns></returns>
    public override bool CanBeSelectedAsTarget(BaseUnit otherUnit)
    {
        return mBoxCollider2D.enabled && IsAlive() && base.CanBeSelectedAsTarget(otherUnit);
    }

    public override void MPause()
    {
        base.MPause();
        // 暂停星级动画
        rankAnimator.speed = 0;
    }

    public override void MResume()
    {
        base.MResume();
        // 取消暂停武器动作
        rankAnimator.speed = 1;
    }

    /// <summary>
    /// 获取贴图对象
    /// </summary>
    public override Sprite GetSpirte()
    {
        return spriteRenderer1.sprite;
    }

    /// <summary>
    /// 获取SpriterRenderer
    /// </summary>
    /// <returns></returns>
    public override SpriteRenderer GetSpriteRenderer()
    {
        return spriteRenderer1;
    }

    public FoodInGridType GetFoodInGridType()
    {
        return BaseCardBuilder.GetFoodInGridType(mType);
    }
}