using System.Collections.Generic;

using UnityEngine;

public class MouseUnit : BaseUnit
{
    // 老鼠单位的属性
    [System.Serializable]
    new public struct Attribute
    {
        public BaseUnit.Attribute baseAttrbute;
        public double[] hertRateList;
    }

    // Awake里Find一次的组件
    public Rigidbody2D rigibody2D;
    public BoxCollider2D mBoxCollider2D;
    private SpriteRenderer spriteRenderer;
    protected Animator animator;
    public Transform spriteTrans;

    // 其他属性
    protected List<double> mHertRateList; // 切换贴图时的受伤比率（高->低)
    public int mHertIndex; // 受伤贴图阶段
    public int currentXIndex; // 当前地标X下标
    public int currentYIndex; // 当前地标Y下标
    public bool isBoss; // 是否是BOSS单位

    // 索敌相关
    protected bool isBlock { get; set; } // 是否被阻挡
    protected BaseUnit mBlockUnit; // 阻挡者

    // 换行相关
    // 相对权重表，默认情况下该表为空，可以插入特定值使得某些类型的敌人有特殊地形的趋向
    public Dictionary<GridType, int> GridDangerousWeightDict = new Dictionary<GridType, int>();


    /// <summary>
    /// 老鼠每次被投入战场时要做的初始化工作，要确定其各种属性
    /// </summary>
    public override void MInit()
    {
        base.MInit();

        // 动画控制器绑定animator
        animatorController.ChangeAnimator(animator);
        // 从Json中读取的属性以及相关的初始化
        MouseUnit.Attribute attr = GameController.Instance.GetMouseAttribute();
        mHertRateList = new List<double>();
        foreach (var item in attr.hertRateList)
        {
            mHertRateList.Add(item);
        }
        mHertIndex = 0;
        currentXIndex = 0;
        currentYIndex = 0;
        moveRotate = Vector2.left;
        isBoss = false;
        GridDangerousWeightDict.Clear();

        // 初始为移动状态
        SetActionState(new MoveState(this));


        // 添加几个行动点响应事件
        // 在受到伤害结算之后，更新受伤贴图状态
        AddActionPointListener(ActionPointType.PostReceiveDamage, delegate { UpdateHertMap(); });
        AddActionPointListener(ActionPointType.PostReceiveDamage, FlashWhenHited);
        AddActionPointListener(ActionPointType.PostReceiveReboundDamage, delegate { UpdateHertMap(); });
        AddActionPointListener(ActionPointType.PostReceiveReboundDamage, FlashWhenHited);
        // 在接收治疗结算之后，更新受伤贴图状态
        AddActionPointListener(ActionPointType.PostReceiveCure, delegate { UpdateHertMap(); });
        // 装上正常的受击材质
        spriteRenderer.material = GameManager.Instance.GetMaterial("Hit");
        UpdateRuntimeAnimatorController(); // 更新贴图
    }

    public override void SetUnitType()
    {
        mUnitType = UnitType.Mouse;
    }

    /// <summary>
    /// 设置判定参数
    /// </summary>
    public override void SetCollider2DParam()
    {
        mBoxCollider2D.offset = new Vector2(0, 0);
        mBoxCollider2D.size = new Vector2(0.49f*MapManager.gridWidth, 0.49f*MapManager.gridHeight);
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
        mBoxCollider2D = gameObject.GetComponent<BoxCollider2D>();
        spriteTrans = transform.Find("Ani_Mouse");
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
        return animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce();
    }

    /// <summary>
    /// 退出普通攻击后要做的事
    /// </summary>
    public override void AfterGeneralAttack()
    {
        mAttackFlag = true;
        UpdateBlockState(); // 更新阻挡状态
        // 如果有可以攻击的目标，则停下来等待下一次攻击，否则前进
        if (IsHasTarget())
            SetActionState(new IdleState(this));
        else
            SetActionState(new MoveState(this));
    }

    /// <summary>
    /// 判断是有有效的攻击目标
    /// </summary>
    /// <returns></returns>
    public virtual bool IsHasTarget()
    {
        return (isBlock && mBlockUnit.IsAlive());
    }

    /// <summary>
    /// 获取当前攻击目标，上述判断一并使用
    /// </summary>
    /// <returns></returns>
    public virtual BaseUnit GetCurrentTarget()
    {
        return mBlockUnit;
    }

    /// <summary>
    /// 更新阻挡的状态
    /// </summary>
    protected virtual void UpdateBlockState()
    {
        if (mBlockUnit != null && mBlockUnit.IsAlive())
            isBlock = true;
        else
            SetNoCollideAllyUnit();
    }

    /// <summary>
    /// 给予群攻型敌人一种可以选择多个目标的接口
    /// </summary>
    /// <returns></returns>
    public virtual List<BaseUnit> GetCurrentTargetList()
    {
        return null;
    }

    /// <summary>
    /// 是否为伤害判定时刻（近战攻击为打出实际伤害，远程攻击为确定发射弹体）
    /// </summary>
    /// <returns></returns>
    public virtual bool IsDamageJudgment()
    {
        return animatorController.GetCurrentAnimatorStateRecorder().GetNormalizedTime() >= attackPercent && mAttackFlag;
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
        UpdateBlockState();
    }

    public override void OnMoveState()
    {
        // 移动更新
        SetPosition((Vector2)GetPosition() + moveRotate * GetMoveSpeed());
    }

    public override void OnAttackState()
    {

    }

    /// <summary>
    /// 老鼠单位默认都是处在同行且同高度时被美食或者人物单位阻挡
    /// 提示，特殊类型老鼠可以重写这个方法而强制设为不可阻挡，如幽灵鼠
    /// </summary>
    public override bool CanBlock(BaseUnit unit)
    {
        return ((unit is FoodUnit || unit is CharacterUnit) && GetRowIndex() == unit.GetRowIndex() && mHeight == unit.mHeight);
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
    protected virtual void UpdateHertMap()
    {
        // 要是死了的话就免了吧
        if (isDeathState)
            return;

        // 是否要切换控制器的flag
        bool flag = false;
        // 恢复到上一个受伤贴图检测
        while (mHertIndex > 0 && GetHeathPercent() > mHertRateList[mHertIndex - 1])
        {
            mHertIndex--;
            flag = true;
        }
        // 下一个受伤贴图的检测
        while (mHertIndex < mHertRateList.Count && GetHeathPercent() <= mHertRateList[mHertIndex])
        {
            mHertIndex++;
            flag = true;
        }
        // 有切换通知时才切换
        if (flag)
            UpdateRuntimeAnimatorController();
    }

    /// <summary>
    /// 自动更新贴图
    /// </summary>
    /// <param name="collision"></param>
    public virtual void UpdateRuntimeAnimatorController()
    {
        AnimatorStateRecorder a = animatorController.GetCurrentAnimatorStateRecorder(); // 获取当前在播放的动画
        animator.runtimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Mouse/" + mType + "/" + mShape + "/" + mHertIndex);
        animatorController.ChangeAnimator(animator);
        // 保持当前动画播放
        if (a != null)
        {
            animatorController.Play(a.aniName, a.isCycle, a.GetNormalizedTime());
        }
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
    /// 取消接触友方单位
    /// </summary>
    /// <param name="unit"></param>
    public void SetNoCollideAllyUnit()
    {
        isBlock = false;
        mBlockUnit = null;
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

        if (collision.tag.Equals("Food") || collision.tag.Equals("Character"))
        {
            // 检测到友方单位碰撞了！
            OnAllyCollision(collision.GetComponent<BaseUnit>());
        }
        else if (collision.tag.Equals("Bullet"))
        {
            // 检测到子弹单位碰撞了
            OnBulletCollision(collision.GetComponent<BaseBullet>());
        }
    }

    /// <summary>
    /// 当与友方单位（美食、人物）发生刚体碰撞判定时
    /// </summary>
    public virtual void OnAllyCollision(BaseUnit unit)
    {
        // 检测本格美食最高受击优先级单位
        BaseUnit target = unit.GetGrid().GetHighestAttackPriorityUnit();
        if (!isBlock && UnitManager.CanBlock(this, target)) // 检测双方能否互相阻挡
        {
            isBlock = true;
            mBlockUnit = target;
        }
    }

    /// <summary>
    /// 当与子弹单位发生刚体碰撞判定时
    /// </summary>
    public virtual void OnBulletCollision(BaseBullet bullet)
    {
        if (UnitManager.CanBulletHit(this, bullet)) // 检测双方能否互相击中
        {
            bullet.TakeDamage(this);
        }
    }

    public virtual void OnTriggerEnter2D(Collider2D collision)
    {
        OnCollision(collision);
    }

    public virtual void OnTriggerStay2D(Collider2D collision)
    {
        OnCollision(collision);
    }

    public virtual void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag.Equals("Food") || collision.tag.Equals("Character"))
        {
            BaseUnit unit = collision.GetComponent<BaseUnit>();
            if (mBlockUnit == unit)
            {
                SetNoCollideAllyUnit();
            }
        }
    }

    public override void MUpdate()
    {
        base.MUpdate();
        int lastXIndex = currentXIndex;
        int lastYIndex = currentYIndex;
        currentXIndex = MapManager.GetXIndex(transform.position.x);
        currentYIndex = MapManager.GetYIndex(transform.position.y);
        // 当格子的判定坐标发生改变
        if (lastYIndex != currentYIndex)
        {
            // 换行
            GameController.Instance.ChangeEnemyRow(lastYIndex, this);
        }
        // 更新受击闪烁状态
        if (hitBox.GetPercent() > 0)
        {
            spriteRenderer.material.SetFloat("_FlashRate", 0.5f * hitBox.GetPercent());
        }
    }

    /// <summary>
    /// 根据攻击速度来更新攻击动画的速度
    /// </summary>
    private void UpdateAttackAnimationSpeed()
    {
        AnimatorStateRecorder a = animatorController.GetAnimatorStateRecorder("Attack");
        if (a != null)
        {
            float time = a.aniTime; // 一倍速下一次攻击动画的播放时间（帧）
            float interval = 1 / NumericBox.AttackSpeed.Value * 60;  // 攻击间隔（帧）
            float speed = Mathf.Max(1, time / interval); // 计算动画实际播放速度
            animatorController.SetSpeed("Attack", speed);
        }
    }

    /// <summary>
    /// 状态相关
    /// </summary>
    public override void OnIdleStateEnter()
    {
        animatorController.Play("Idle", true);
    }

    public override void OnMoveStateEnter()
    {
        animatorController.Play("Move", true);
    }

    public override void OnAttackStateEnter()
    {
        // 每次攻击时，最好根据攻速来计算一下播放速度，然后改变播放速度
        UpdateAttackAnimationSpeed();
        animatorController.Play("Attack");
    }

    public override void OnDieStateEnter()
    {
        animatorController.Play("Die");
    }

    public override void OnAttackStateContinue()
    {
        // 每次攻击时，最好根据攻速来计算一下播放速度，然后改变播放速度
        UpdateAttackAnimationSpeed();
    }

    public override void OnAttackStateExit()
    {
        // 恢复播放速度
        // ResumeAnimationSpeed();
    }

    public override void OnAttackStateInterrupt()
    {
        // 恢复播放速度
        // ResumeAnimationSpeed();
    }

    public override void DuringDeath()
    {
        // 切换时的第一帧直接不执行update()，因为下述的info.normalizedTime的值还停留在上一个状态，逻辑会出问题！
        if (currentStateTimer <= 0)
        {
            return;
        }
        // 获取Die的动作信息，使得回收时机与动画显示尽可能同步
        if(animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
        {
            DeathEvent();
        }
    }

    private BoolModifier boolModifier = new BoolModifier(true);

    public override void OnBurnStateEnter()
    {
        // 装上烧毁材质
        spriteRenderer.material = GameManager.Instance.GetMaterial("Dissolve2");
        // 禁止播放动画
        PauseCurrentAnimatorState(boolModifier);
    }

    public override void DuringBurn(float _Threshold)
    {
        spriteRenderer.material.SetFloat("_Threshold", _Threshold);
        // 超过1就可以回收了
        if (_Threshold >= 1.0)
        {
            ResumeCurrentAnimatorState(boolModifier);
            DeathEvent();
        }
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
            hertRateList = new double[] { }
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
        spriteRenderer.sortingOrder = LayerManager.CalculateSortingLayer(LayerManager.UnitType.Enemy, GetRowIndex(), typeAndShapeValue, arrayIndex);
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
    /// 设置移动方向
    /// </summary>
    public void SetMoveRoate(Vector2 v2)
    {
        moveRotate = v2;
    }

    /// <summary>
    /// 设置贴图对象坐标
    /// </summary>
    public override void SetSpriteLocalPosition(Vector2 vector2)
    {
        spriteTrans.localPosition = vector2;
    }

    public override int GetColumnIndex()
    {
        return currentXIndex;
    }

    public override int GetRowIndex()
    {
        return currentYIndex;
    }

    /// <summary>
    /// 可否被选择为目标
    /// </summary>
    /// <returns></returns>
    public override bool CanBeSelectedAsTarget()
    {
        return mBoxCollider2D.enabled;
    }

    /// <summary>
    /// 目标是否定是BOSS单位
    /// </summary>
    /// <returns></returns>
    public bool IsBoss()
    {
        return isBoss;
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

    /// <summary>
    /// 能否被驱使换行
    /// </summary>
    /// <returns></returns>
    public virtual bool CanDrivenAway()
    {
        return true;
    }

    /// <summary>
    /// 被驱使更换行数
    /// </summary>
    public virtual void DrivenAway()
    {
        // 计算上中下格的危险权重，然后取危险权重小的换行
        int currentRowIndex = GetRowIndex();
        int currentColumnIndex = GetColumnIndex();
        int startIndex = Mathf.Max(0, currentRowIndex - 1);
        int endIndex = Mathf.Min(6, currentRowIndex + 1);
        Dictionary<int, int> rowIndex_WeightMap = new Dictionary<int, int>();
        // 这步是取出权重最小的几个格子
        int min = int.MaxValue;
        for (int i = startIndex; i <= endIndex; i++)
        {
            int weight = 1;
            BaseGrid grid = GameController.Instance.mMapController.GetGrid(currentColumnIndex, i);
            if(grid != null)
            {
                weight *= grid.GetDefaultDangerousWeight()* GetDangerousWeight(grid.GetGridType());
            }
            // 如果是中格，则将危险权重翻10倍，此目的旨在尽可能不让老鼠停留在本路
            if (i == currentRowIndex)
                weight *= 10;
            if(weight < min)
            {
                min = weight;
                rowIndex_WeightMap.Clear();
                rowIndex_WeightMap.Add(i, weight);
            }
            else if(weight == min)
            {
                rowIndex_WeightMap.Add(i, weight);
            }
        }
        // 超过一个结果时需要优先移除本路，此目的旨在尽可能不让老鼠停留在本路
        if (rowIndex_WeightMap.Count > 1)
        {
            rowIndex_WeightMap.Remove(currentRowIndex);
        }
        // 然后从剩下的里面随机抽取一名幸运观众作为最终移动行
        int selectedIndex = Random.Range(0, rowIndex_WeightMap.Count); // 还是注意，生成整形时不包括最大值
        int j = 0;
        foreach (var keyValuePair in rowIndex_WeightMap)
        {
            if (j == selectedIndex)
            {
                // 纵向位移
                GameController.Instance.AddTasker(new StraightMovePresetTasker(this, MapManager.gridHeight/60, Vector3.up * (currentRowIndex - keyValuePair.Key), 60));
                break;
            }
            j++;
        }
    }

    /// <summary>
    /// 获取相对权重
    /// </summary>
    /// <param name="gridType"></param>
    /// <returns></returns>
    public int GetDangerousWeight(GridType gridType)
    {
        if (GridDangerousWeightDict.ContainsKey(gridType))
        {
            return GridDangerousWeightDict[gridType];
        }
        else
            return 1;
    }
}
