using System.Collections.Generic;

using S7P.Numeric;

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
    public SpriteRenderer spriteRenderer;
    protected Animator animator;
    public Transform spriteTrans;
    
    // 其他属性
    protected List<double> mHertRateList = new List<double>(); // 切换贴图时的受伤比率（高->低)
    public int mHertIndex; // 受伤贴图阶段
    public int currentXIndex; // 当前地标X下标
    public int currentYIndex; // 当前地标Y下标
    public bool isBoss; // 是否是BOSS单位
    public bool canDrivenAway; // 能否被强制换行

    public string AttackClipName;
    public string IdleClipName;
    public string MoveClipName;
    public string DieClipName;

    // 索敌相关
    protected bool isBlock { get; set; } // 是否被阻挡
    protected BaseUnit mBlockUnit; // 阻挡者

    // 换行相关
    // 地形危险度权重表，可以修改特定值使得某些类型的敌人有特殊地形的趋向
    public Dictionary<GridType, int> GridDangerousWeightDict;

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
    /// 老鼠每次被投入战场时要做的初始化工作，要确定其各种属性
    /// </summary>
    public override void MInit()
    {
        isBlock = false; // 是否被阻挡
        mBlockUnit = null; // 阻挡者
        isBoss = false;

        AttackClipName = "Attack";
        IdleClipName = "Idle";
        MoveClipName = "Move";
        DieClipName = "Die";

        base.MInit();

        // 动画控制器绑定animator
        animatorController.ChangeAnimator(animator);

        mHertRateList.Clear();
        mHertIndex = 0;
        currentXIndex = 0;
        currentYIndex = 0;
        moveRotate = Vector2.left;
        canDrivenAway = true;

        // 初始化
        GridDangerousWeightDict = new Dictionary<GridType, int>()
        {
            { GridType.Default, 10},
            { GridType.NotBuilt, 10},
            { GridType.Water, 12}, // 众所周知水是剧毒的
            { GridType.Lava, 11},
            { GridType.Sky, 13}, // 老鼠更恐高
            { GridType.Teleport, 10 }, // 传送装置？太cool了！！
        };

        // 初始为移动状态
        SetActionState(new MoveState(this));

        // 添加几个行动点响应事件
        // 在受到伤害结算之后，更新受伤贴图状态
        AddActionPointListener(ActionPointType.PostReceiveDamage, delegate { UpdateHertMap(); });
        AddActionPointListener(ActionPointType.PostReceiveDamage, FlashWhenHited);
        // 在接收治疗结算之后，更新受伤贴图状态
        AddActionPointListener(ActionPointType.PostReceiveCure, delegate { UpdateHertMap(); });
        // 装上正常的受击材质
        spriteRenderer.material = GameManager.Instance.GetMaterial("Hit");
        OnHertStageChanged(); // 更新贴图
        // 修改地形危险度权值表
        SetGridDangerousWeightDict();
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
            spriteRenderer.material.SetFloat("_FlashRate", 0.75f * hitBox.GetPercent());
        }
        // 进家判定
        if (CanTriggerLoseWhenEnterLoseLine() && transform.position.x < MapManager.GetColumnX(-1.5f))
        {
            GameController.Instance.Lose();
        }
        else
        {
            // 出屏判定
            if (IsOutOfBound())
            {
                DeathEvent();
            }
        }

    }

    public override void MDestory()
    {
        NumericBox.Initialize();
        base.MDestory();
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
        if (IsBlock())
        {
            // 若目标依附于格子，则将目标切换为目标所在格的最高攻击优先级目标
            BaseGrid g = mBlockUnit.GetGrid();
            if (g != null)
            {
                mBlockUnit = g.GetHighestAttackPriorityUnit(this);
                UpdateBlockState();
            }
            return IsBlock();
        }
        return false;
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
        if (mBlockUnit != null && mBlockUnit.IsAlive() && UnitManager.CanBeSelectedAsTarget(this, mBlockUnit) && UnitManager.CanBlock(this, mBlockUnit))
        {
            isBlock = true;
        }
        else
            SetNoCollideAllyUnit();
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
        GameManager.Instance.audioSourceManager.PlayEffectMusic("Chomp" + GameManager.Instance.rand.Next(0, 3));
    }


    public override void OnIdleState()
    {
        UpdateBlockState();
        if (!isBlock)
            SetActionState(new MoveState(this));
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
        return ((unit is FoodUnit || unit is CharacterUnit) && UnitManager.CanBeSelectedAsTarget(this, unit) && GetRowIndex() == unit.GetRowIndex() && mHeight == unit.mHeight);
    }

    /// <summary>
    /// 老鼠单位默认被处在同行的子弹击中
    /// </summary>
    /// <param name="bullet"></param>
    /// <returns></returns>
    public override bool CanHit(BaseBullet bullet)
    {
        return IsAlive();
    }

    /// <summary>
    /// 当受伤或者被治疗时，更新单位贴图状态
    /// </summary>
    public virtual void UpdateHertMap()
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
            OnHertStageChanged();
    }

    /// <summary>
    /// 当受伤阶段点切换时
    /// </summary>
    /// <param name="collision"></param>
    public virtual void OnHertStageChanged()
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
        hitBox.OnHit();
    }

    /// <summary>
    /// 当被攻击时闪白
    /// </summary>
    public void FlashWhenHited()
    {
        {
            hitBox.OnHit();
        }
    }

    /// <summary>
    /// 当贴图更新时要做的事，由子类override
    /// </summary>
    public virtual void OnUpdateRuntimeAnimatorController()
    {

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
    /// 是否被阻挡
    /// </summary>
    /// <returns></returns>
    public bool IsBlock()
    {
        return isBlock && mBlockUnit!=null && mBlockUnit.IsAlive() && UnitManager.CanBeSelectedAsTarget(this, mBlockUnit);
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
        BaseUnit target = unit.GetGrid().GetHighestAttackPriorityUnit(this);
        if (target!=null && !isBlock && UnitManager.CanBlock(this, target)) // 检测双方能否互相阻挡，且老鼠能否将该目标视为攻击目标(这个在GetHighestAttackPriorityUnit已经做了）
        {
            isBlock = true;
            mBlockUnit = target;
        }
    }

    /// <summary>
    /// 当友方单位离开时
    /// </summary>
    /// <param name="collision"></param>
    public virtual void OnAllyTriggerExit(Collider2D collision)
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
        OnAllyTriggerExit(collision);
    }

    /// <summary>
    /// 是否出屏判定
    /// </summary>
    /// <returns></returns>
    public virtual bool IsOutOfBound()
    {
        return !isBoss && (GetColumnIndex() > MapController.xColumn + 2 || GetColumnIndex() <= -2 || GetRowIndex() <= -1 || GetRowIndex() >= 7);
    }

    /// <summary>
    /// 根据攻击速度来更新攻击动画的速度
    /// </summary>
    protected void UpdateAttackAnimationSpeed()
    {
        AnimatorStateRecorder a = animatorController.GetAnimatorStateRecorder(AttackClipName);
        if (a != null)
        {
            float time = a.aniTime; // 一倍速下一次攻击动画的播放时间（帧）
            float interval = 1 / NumericBox.AttackSpeed.Value * 60;  // 攻击间隔（帧）
            float speed = Mathf.Max(1, time / interval); // 计算动画实际播放速度
            animatorController.SetSpeed(AttackClipName, speed);
        }
    }

    /// <summary>
    /// 状态相关
    /// </summary>
    public override void OnIdleStateEnter()
    {
        animatorController.Play(IdleClipName, true);
    }

    public override void OnMoveStateEnter()
    {
        animatorController.Play(MoveClipName, true);
    }

    public override void OnAttackStateEnter()
    {
        // 每次攻击时，最好根据攻速来计算一下播放速度，然后改变播放速度
        UpdateAttackAnimationSpeed();
        animatorController.Play(AttackClipName);
    }

    public override void OnDieStateEnter()
    {
        animatorController.Play(DieClipName);
        animatorController.SetNoPlayOtherClip(true); // 不许切成其他动画
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
        // 获取Die的动作信息，使得回收时机与动画显示尽可能同步
        AnimatorStateRecorder r = animatorController.GetCurrentAnimatorStateRecorder();
        if (r==null || r.IsFinishOnce())
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
        animatorController.SetNoPlayOtherClip(true);
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

    /// <summary>
    /// 摔落死亡瞬间
    /// </summary>
    public override void OnDropStateEnter()
    {
        // 禁止播放动画
        PauseCurrentAnimatorState(boolModifier);
        animatorController.SetNoPlayOtherClip(true);
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
    /// 设置在同种类敌人的渲染层级
    /// </summary>
    /// <param name="arrayIndex"></param>
    public override void UpdateRenderLayer(int arrayIndex)
    {
        spriteRenderer.sortingOrder = LayerManager.CalculateSortingLayer(LayerManager.UnitType.Enemy, GetRowIndex(), typeAndShapeValue, arrayIndex);
    }

    public override bool TryGetSpriteRenternerSorting(out string name, out int order)
    {
        if (spriteRenderer == null)
            return base.TryGetSpriteRenternerSorting(out name, out order);
        name = spriteRenderer.sortingLayerName;
        order = spriteRenderer.sortingOrder;
        return true;
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

    /// <summary>
    /// 获取贴图对象相对坐标
    /// </summary>
    /// <returns></returns>
    public override Vector2 GetSpriteLocalPosition()
    {
        return spriteTrans.localPosition;
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
    public override bool CanBeSelectedAsTarget(BaseUnit otherUnit)
    {
        return mBoxCollider2D.enabled && IsAlive() && base.CanBeSelectedAsTarget(otherUnit);
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
        return canDrivenAway;
    }

    /// <summary>
    /// 被驱使更换行数
    /// </summary>
    public void DrivenAway()
    {
        if (GetTask("DrivenAway") != null)
            return;

        // 计算上中下格的危险权重，然后取危险权重小的换行
        int currentRowIndex = GetRowIndex();
        int currentColumnIndex = GetColumnIndex();
        int startIndex = Mathf.Max(0, currentRowIndex - 1);
        int endIndex = Mathf.Min(6, currentRowIndex + 1);
        List<int> rowIndexList = new List<int>();
        // 这步是取出权重最小的几个格子
        int min = int.MaxValue;
        for (int i = startIndex; i <= endIndex; i++)
        {
            int weight;
            BaseGrid grid = GameController.Instance.mMapController.GetGrid(currentColumnIndex, i);
            if(grid != null)
            {
                weight = GetDangerousWeight(grid);
            }
            else
            {
                // 如果相邻没有有效格子，那么视为默认格子类型计算
                weight = GridDangerousWeightDict[GridType.Default];
            }
            if(weight < min)
            {
                min = weight;
                rowIndexList.Clear();
                rowIndexList.Add(i);
            }
            else if(weight == min)
            {
                rowIndexList.Add(i);
            }
        }
        // 超过一个结果时需要优先移除本路，此目的旨在尽可能不让老鼠停留在本路
        if (rowIndexList.Count > 1)
        {
            rowIndexList.Remove(currentRowIndex);
        }
        // 然后从剩下的里面随机抽取一名幸运观众作为最终移动行
        int selectedIndex = Random.Range(0, rowIndexList.Count); // 还是注意，生成整形时不包括最大值
        // 纵向位移
        AddUniqueTask("DrivenAway", new StraightMovePresetTask(transform, MapManager.gridHeight / 60 * (currentRowIndex - rowIndexList[selectedIndex]), Vector3.up, 60));
        // AddTask(new StraightMovePresetTask(transform, MapManager.gridHeight / 60 * (currentRowIndex - rowIndexList[selectedIndex]), Vector3.up, 60));
        // GameController.Instance.AddTasker(new StraightMovePresetTasker(this, MapManager.gridHeight / 60 * (currentRowIndex - rowIndexList[selectedIndex]), Vector3.up, 60));
    }

    /// <summary>
    /// 获取该单位在某格上的地形危险权重
    /// </summary>
    /// <param name="g"></param>
    /// <returns></returns>
    public int GetDangerousWeight(BaseGrid g)
    {
        int maxDangerous = int.MinValue; // 最大危险权重
        foreach (var t in g.GetAllGridType())
        {
            if (GridDangerousWeightDict.ContainsKey(t))
            {
                if (GridDangerousWeightDict[t] > maxDangerous)
                    maxDangerous = GridDangerousWeightDict[t];
            }
        }
        return maxDangerous;
    }

    /// <summary>
    /// 在与猫判定时是否能触发猫
    /// </summary>
    /// <returns></returns>
    public virtual bool CanTriggerCat()
    {
        return true;
    }

    /// <summary>
    /// 在越过失败判定线后是否会触发游戏失败判定
    /// </summary>
    /// <returns></returns>
    public virtual bool CanTriggerLoseWhenEnterLoseLine()
    {
        return true;
    }

    /// <summary>
    /// 修改地形权值表
    /// </summary>
    public virtual void SetGridDangerousWeightDict()
    {

    }

    /// <summary>
    /// 对于BOSS单位来说，内伤就是普通伤害
    /// </summary>
    /// <param name="value"></param>
    public override void AddRecordDamage(float value)
    {
        if (!isIgnoreRecordDamage && IsBoss())
            OnDamage(value);
        mRecordDamageComponent.AddRecordDamage(value);
    }

    /// <summary>
    /// 设置怪的属性（会完全刷新目标的战斗盒子数据）
    /// </summary>
    /// <param name="attr"></param>
    public void SetAttribute(MouseManager.MouseAttribute attr)
    {
        // 血量
        NumericBox.Hp.SetBase(attr.hp);
        mCurrentHp = mMaxHp;
        // 攻击力
        NumericBox.Attack.SetBase(attr.attack);
        // 攻击速度与攻击间隔
        NumericBox.AttackSpeed.SetBase(attr.attackSpeed);
        NumericBox.MoveSpeed.SetBase(TransManager.TranToVelocity(attr.moveSpeed));
        attackPercent = attr.attackPercent; // 攻击动画播放进度到attackPercent以上时允许出真正的攻击

        if (attr.burnDefence != 0)
            NumericBox.BurnRate.AddModifier(new FloatModifier(1-attr.burnDefence));
        if(attr.aoeDefence != 0)
            NumericBox.AoeRate.AddModifier(new FloatModifier(1 - attr.aoeDefence));
        mHertRateList.Clear();
        foreach (var item in attr.mHertRateList)
        {
            mHertRateList.Add(item);
        }
    }
}
