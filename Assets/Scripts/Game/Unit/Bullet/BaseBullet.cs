using System.Collections.Generic;
using System;
using UnityEngine;
using S7P.Numeric;
public class BaseBullet : MonoBehaviour, IBaseBullet, IGameControllerMember
{
    // 来自外部引用
    public BaseUnit mMasterBaseUnit; // 发射它的主人单位

    // 引用
    public Animator animator;
    public CircleCollider2D mCircleCollider2D;
    public SpriteRenderer spriteRenderer;
    private Transform Trans_Sprite;

    // 自身属性
    public float mVelocity;
    public float mAccelerate;
    public int mAccelerateTime;
    public int mMovetime;
    public Vector2 mRotate;
    public bool isIgnoreHeight; // 是否无视高度限制
    public float mHeight;
    public float mDamage;
    public bool isDeathState;
    public BulletStyle style;
    public List<Func<BaseBullet, BaseUnit, bool>> CanHitFuncList = new List<Func<BaseBullet, BaseUnit, bool>>(); // 子弹与单位能否相互碰撞的额外判断事件
    private List<Action<BaseBullet, BaseUnit>> HitActionList = new List<Action<BaseBullet, BaseUnit>>(); // 中弹后的事件（由外部添加）
    protected List<BaseUnit> unitList = new List<BaseUnit>();
    public bool isnKillSelf;
    public bool isnUseHitEffect; // 不采用击中动画
    public TaskController taskController = new TaskController();
    public int aliveTime;
    public bool isnDelOutOfBound;
    public float mAngle; // 当前子弹角度
    private float lastAngle; // 角度，上一帧
    public bool isNavi;
    // 以下三个为图片精灵偏移量
    public FloatNumeric SpriteOffsetX = new FloatNumeric();
    public FloatNumeric SpriteOffsetY = new FloatNumeric();
    public Vector2 SpriteOffset { get { return new Vector2(SpriteOffsetX.Value, SpriteOffsetY.Value); } }
    public string hitSoundsRefName = null;

    // 外界给的标签
    public Dictionary<string, int> TagDict = new Dictionary<string, int>();

    public IBaseActionState mCurrentActionState; //+ 当前动作状态

    public AnimatorController animatorController = new AnimatorController(); // 动画播放控制器

    public virtual void Awake()
    {
        animator = transform.Find("SpriteGo").GetComponent<Animator>();
        mCircleCollider2D = GetComponent<CircleCollider2D>();
        spriteRenderer = transform.Find("SpriteGo").GetComponent<SpriteRenderer>();
        Trans_Sprite = transform.Find("SpriteGo");
    }

    /// <summary>
    /// 每次初始化都要做的事
    /// </summary>
    public virtual void MInit()
    {
        animatorController.Initialize();
        animatorController.ChangeAnimator(animator);
        mVelocity = 0.0f;    
        mAccelerate = 0;
        mAccelerateTime = 0;
        mMovetime = 0;
        mRotate = Vector2.zero;
        mAngle = 0;
        lastAngle = 0;
        Trans_Sprite.transform.right = Vector2.right;
        transform.right = Vector2.right;
        mDamage = 0;
        mHeight = 0;
        isIgnoreHeight = false;
        aliveTime = 0;
        isDeathState = false;
        isnKillSelf = false;
        isnDelOutOfBound = false;
        isnUseHitEffect = false;
        SpriteOffsetX.Initialize(); SpriteOffsetY.Initialize();
        SetSpriteLocalPosition(Vector2.zero);
        SetSpriteRight(Vector2.right);
        HitActionList.Clear();
        CanHitFuncList.Clear();
        TagDict.Clear();
        taskController.Initial();
        unitList.Clear();
        SetCollision(true);
        SetActionState(new BulletFlyState(this));
        transform.localScale = Vector2.one;
        Trans_Sprite.transform.localRotation = new Quaternion(0, 0, 0, 0);
        isNavi = true; // 默认是同步方向
        Show(); // 显示自己
        hitSoundsRefName = null;
    }

    /// <summary>
    /// 设置速度变化事件
    /// </summary>
    public void SetVelocityChangeEvent(float v0, float v1, int t)
    {
        if (t < 1)
        {
            Debug.Log("速度变化时间不能小于1帧");
            return;
        }
        mAccelerate = (v1 - v0) / t;
        mVelocity = v0;
        mMovetime = 0;
        mAccelerateTime = t;
    }

    // 子弹对目标造成伤害，TakeDamage的调用时机是敌对单位碰到了这个子弹，然后过来调用这个子弹的伤害逻辑
    public virtual void TakeDamage(BaseUnit baseUnit)
    {
        // 子弹在死亡状态下不生效TakeDamage，且每个单位只能被生效一次
        if (isDeathState || unitList.Contains(baseUnit))
            return;
        if (baseUnit != null)
            new DamageAction(CombatAction.ActionType.CauseDamage, mMasterBaseUnit, baseUnit, mDamage).ApplyAction();
        ExecuteHitAction(baseUnit);
        if(hitSoundsRefName != null)
            GameManager.Instance.audioSourceController.PlayEffectMusic(hitSoundsRefName);
        if(!isnKillSelf)
            KillThis();
    }

    /// <summary>
    /// 子弹自爆
    /// </summary>
    public virtual void KillThis()
    {
        SetActionState(new BulletHitState(this));
    }

    public void SetActionState(IBaseActionState state)
    {
        if (mCurrentActionState != null)
        {
            mCurrentActionState.OnExit();
        }
        mCurrentActionState = state;
        mCurrentActionState.OnEnter();
    }

    /// <summary>
    /// 开关判定
    /// </summary>
    public void SetCollision(bool colli)
    {
        mCircleCollider2D.enabled = colli;
    }

    // 以下由GameController管
    public virtual void MDestory()
    {
        throw new System.NotImplementedException();
    }

    /// <summary>
    /// 仅改变子弹外观（Ani)并且不改变样式(style)
    /// </summary>
    /// <param name="style"></param>
    public void ChangeAnimatorWithoutChangeStyle(BulletStyle target_style)
    {
        animator.runtimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Bullet/" + ((int)target_style) + "/0");
    }

    public Vector2 GetRotate()
    {
        return mRotate;
    }

    /// <summary>
    /// 改变方向
    /// </summary>
    /// <param name="v"></param>
    public virtual void SetRotate(Vector2 v)
    {
        mRotate = v;
        // transform.right = v;
        if(isNavi)
            Trans_Sprite.transform.right = v;
    }

    /// <summary>
    /// 改变方向（传进来的是角度制的参数）
    /// </summary>
    /// <param name="angle"></param>
    public void SetRotate(float angle)
    {
        mAngle = angle;
        lastAngle = mAngle;
        angle = angle / 180 * Mathf.PI;
        SetRotate(new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)));
    }

    public float GetDamage()
    {
        return mDamage;
    }

    public void SetDamage(float dmg)
    {
        mDamage = dmg;
    }

    public float GetVelocity()
    {
        return mVelocity;
    }

    /// <summary>
    /// 直接设置移动速度值（比较抽象，推荐使用SetStandardVelocity())方法
    /// </summary>
    /// <param name="v"></param>
    public void SetVelocity(float v)
    {
        mVelocity = v;
    }

    /// <summary>
    /// 设置标准的移动速度，1单位标准移动速度为 6秒内走完1格 的速度
    /// </summary>
    public void SetStandardVelocity(float standardVelocity)
    {
        mVelocity = TransManager.TranToVelocity(standardVelocity);
    }

    public virtual void MPause()
    {
        animatorController.Pause();
    }

    public virtual void MResume()
    {
        animatorController.Resume();
    }

    public virtual void MUpdate()
    {
        if (mAngle != lastAngle)
        {
            SetRotate(mAngle);
        }

        List<BaseUnit> delList = new List<BaseUnit>();
        foreach (var u in unitList)
        {
            if (!u.IsAlive())
                delList.Add(u);
        }
        foreach (var u in delList)
        {
            unitList.Remove(u);
        }

        taskController.Update();

        // 单位动作状态由状态机决定（如移动、攻击、待机、死亡）
        mCurrentActionState.OnUpdate();
        animatorController.Update();
        // 子弹若出屏了请自删
        if (!isnDelOutOfBound && !IsInView())
        {
            ExecuteRecycle();
        }

        aliveTime++;
    }

    /// <summary>
    /// 执行回收
    /// </summary>
    public virtual void ExecuteRecycle()
    {
        GameManager.Instance.PushGameObjectToFactory(FactoryType.GameFactory, "Bullet/" + ((int)style), gameObject);
    }

    /// <summary>
    /// 判断子弹在不在屏幕内
    /// </summary>
    public bool IsInView()
    {
        Vector3 p = transform.position;
        if (p.x > MapManager.GetColumnX(-2) && p.x < MapManager.GetColumnX(9) && p.y > MapManager.GetRowY(9) && p.y < MapManager.GetRowY(-3))
            return true;
        return false;
    }

    /// <summary>
    /// 获取当前单位所在行下标
    /// </summary>
    /// <returns></returns>
    public virtual int GetRowIndex()
    {
        return MapManager.GetYIndex(transform.position.y);
    }

    /// <summary>
    /// 获取当前单位所在列下标
    /// </summary>
    /// <returns></returns>
    public virtual int GetColumnIndex()
    {
        return MapManager.GetXIndex(transform.position.x);
    }

    /// <summary>
    /// 判定子弹能否击中单位 
    /// </summary>
    /// <returns></returns>
    public virtual bool CanHit(BaseUnit unit)
    {
        return IsAlive() && (isIgnoreHeight || mHeight == unit.mHeight);
    }

    public virtual void OnFlyStateEnter()
    {
        if(animatorController.animator.runtimeAnimatorController != null)
            animatorController.Play("Fly", true);
    }

    public virtual void OnFlyState()
    {
        transform.position += (Vector3)mRotate * mVelocity;
        if (mMovetime < mAccelerateTime)
            mVelocity += mAccelerate;
        mMovetime ++;
    }

    public virtual void OnFlyStateExit()
    {
        
    }

    public virtual void OnHitStateEnter()
    {
        if (animatorController.animator.runtimeAnimatorController != null && !isnUseHitEffect)
        {
            animatorController.Play("Hit", false);
            SetCollision(false);
            isDeathState = true;
        }
        else
            ExecuteRecycle();
    }

    public virtual void OnHitState()
    {
        // 动画播放完一次后，转为移动状态
        if(isnUseHitEffect || animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
        {
            ExecuteRecycle();
        }
    }

    public virtual void OnHitStateExit()
    {
        // ExecuteRecycle();
        //GameManager.Instance.PushGameObjectToFactory(FactoryType.GameFactory, "Bullet/"+((int)style), gameObject);
    }

    /// <summary>
    /// 设置渲染层级
    /// </summary>
    /// <param name="arrayIndex"></param>
    public virtual void UpdateRenderLayer(int arrayIndex)
    {
        spriteRenderer.sortingOrder = LayerManager.CalculateSortingLayer(LayerManager.UnitType.Bullet, GetRowIndex(), 0, arrayIndex);
    }

    /// <summary>
    /// 获取某个标签对应的数值
    /// </summary>
    /// <returns></returns>
    public int GetTagCount(string tagName)
    {
        if (TagDict.ContainsKey(tagName))
            return TagDict[tagName];
        return 0;
    }

    public void AddTag(string tagName)
    {
        AddTagCount(tagName, 1);
    }

    public void RemoveTag(string tagName)
    {
        TagDict.Remove(tagName);
    }

    public void AddTagCount(string tagName, int count)
    {
        if (TagDict.ContainsKey(tagName))
            TagDict[tagName] += count;
        else
        {
            TagDict.Add(tagName, count);
        }
    }

    /// <summary>
    /// 启动判定
    /// </summary>
    public virtual void OpenCollision()
    {
        mCircleCollider2D.enabled = true;
    }

    /// <summary>
    /// 关闭判定
    /// </summary>
    public virtual void CloseCollision()
    {
        mCircleCollider2D.enabled = false;
    }

    /// <summary>
    /// 设置透明度
    /// </summary>
    public virtual void SetAlpha(float a)
    {
        spriteRenderer.color = new UnityEngine.Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, a);
    }

    public virtual void OnTriggerEnter2D(Collider2D collision)
    {

    }

    public virtual void OnTriggerStay2D(Collider2D collision)
    {

    }

    public virtual void OnTriggerExit2D(Collider2D collision)
    {

    }

    public virtual void MPauseUpdate()
    {
        
    }

    public void SetHeight(float height)
    {
        mHeight = height;
    }

    /// <summary>
    /// 添加被弹事件
    /// </summary>
    /// <param name="HitAction"></param>
    public void AddHitAction(Action<BaseBullet, BaseUnit> HitAction)
    {
        HitActionList.Add(HitAction);
    }

    /// <summary>
    /// 移除被弹事件
    /// </summary>
    /// <param name="HitAction"></param>
    public void RemoveHitAction(Action<BaseBullet, BaseUnit> HitAction)
    {
        HitActionList.Remove(HitAction);
    }

    /// <summary>
    /// 执行被弹事件
    /// </summary>
    /// <param name="hitedUnit"></param>
    public void ExecuteHitAction(BaseUnit hitedUnit)
    {
        foreach (var action in HitActionList)
        {
            action(this, hitedUnit);
        }
    }

    /// <summary>
    /// 添加一个任务
    /// </summary>
    /// <param name="t"></param>
    public void AddTask(ITask t)
    {
        taskController.AddTask(t);
    }

    /// <summary>
    /// 移除一个任务
    /// </summary>
    /// <param name="t"></param>
    public void RemoveTask(ITask t)
    {
        taskController.RemoveTask(t);
    }

    public bool IsAlive()
    {
        return !isDeathState && isActiveAndEnabled;
    }

    public void AddCanHitFunc(Func<BaseBullet, BaseUnit, bool> action)
    {
        CanHitFuncList.Add(action);
    }

    public void RemoveCanHitFunc(Func<BaseBullet, BaseUnit, bool> action)
    {
        CanHitFuncList.Remove(action);
    }

    /// <summary>
    /// 改变贴图X偏移
    /// </summary>
    /// <param name="f"></param>
    public void AddSpriteOffsetX(FloatModifier f)
    {
        SpriteOffsetX.AddAddModifier(f);
        SetSpriteLocalPosition(SpriteOffset);
    }

    /// <summary>
    /// 移除改变贴图X偏移
    /// </summary>
    /// <param name="f"></param>
    public void RemoveSpriteOffsetX(FloatModifier f)
    {
        SpriteOffsetX.RemoveAddModifier(f);
        SetSpriteLocalPosition(SpriteOffset);
    }

    /// <summary>
    /// 移除改变贴图X偏移
    /// </summary>
    /// <param name="f"></param>
    public void AddSpriteOffsetY(FloatModifier f)
    {
        SpriteOffsetY.AddAddModifier(f);
        SetSpriteLocalPosition(SpriteOffset);
    }

    /// <summary>
    /// 移除改变贴图Y偏移
    /// </summary>
    /// <param name="f"></param>
    public void RemoveSpriteOffsetY(FloatModifier f)
    {
        SpriteOffsetY.RemoveAddModifier(f);
        SetSpriteLocalPosition(SpriteOffset);
    }

    /// <summary>
    /// 设置贴图对象坐标
    /// </summary>
    public virtual void SetSpriteLocalPosition(Vector2 vector2)
    {
        spriteRenderer.transform.localPosition = vector2;
    }

    /// <summary>
    /// 获取贴图对象相对坐标
    /// </summary>
    /// <returns></returns>
    public virtual Vector2 GetSpriteLocalPosition()
    {
        return spriteRenderer.transform.localPosition;
    }

    public virtual void SetSpriteRight(Vector2 vector2)
    {
        spriteRenderer.transform.right = vector2;
    }

    /// <summary>
    /// 设置贴图的角度
    /// </summary>
    /// <param name="angle"></param>
    public void SetSpriteRotate(float angle)
    {
        //Trans_Sprite.transform.localRotation = new Quaternion(0, 0, angle, 0);
        angle = angle * Mathf.PI / 180;
        Trans_Sprite.transform.right = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
    }

    /// <summary>
    /// 隐藏自己
    /// </summary>
    public void Hide()
    {
        spriteRenderer.enabled = false;
    }

    /// <summary>
    /// 显示自己
    /// </summary>
    public void Show()
    {
        spriteRenderer.enabled = true;
    }

    /// <summary>
    /// 设置击中音效（填null即为默认无）
    /// </summary>
    /// <param name="refName"></param>
    public void SetHitSoundEffect(string refName)
    {
        hitSoundsRefName = refName;
    }
}



