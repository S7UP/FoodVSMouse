using System.Collections.Generic;

using UnityEngine;

public class BaseBullet : MonoBehaviour, IBaseBullet, IGameControllerMember
{
    // 来自外部引用
    public BaseUnit mMasterBaseUnit; // 发射它的主人单位

    // 引用
    public Animator animator;
    public CircleCollider2D mCircleCollider2D;
    private SpriteRenderer spriteRenderer;

    // 自身属性
    public float mVelocity;
    public Vector2 mRotate;
    public float mHeight;
    public float mDamage;
    public bool isDeathState;
    public BulletStyle style;
    // 外界给的标签
    public Dictionary<string, int> TagDict = new Dictionary<string, int>();

    public IBaseActionState mCurrentActionState; //+ 当前动作状态

    public AnimatorController animatorController = new AnimatorController(); // 动画播放控制器

    public virtual void Awake()
    {
        animator = transform.GetChild(0).GetComponent<Animator>();
        mCircleCollider2D = GetComponent<CircleCollider2D>();
        spriteRenderer = transform.Find("SpriteGo").GetComponent<SpriteRenderer>();
    }

    // 子弹对目标造成伤害，TakeDamage的调用时机是敌对单位碰到了这个子弹，然后过来调用这个子弹的伤害逻辑
    public virtual void TakeDamage(BaseUnit baseUnit)
    {
        if (baseUnit != null)
            new DamageAction(CombatAction.ActionType.CauseDamage, mMasterBaseUnit, baseUnit, mDamage).ApplyAction();
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
    /// 每次初始化都要做的事
    /// </summary>
    public virtual void MInit()
    {
        animatorController.Initialize();
        animatorController.ChangeAnimator(animator);
        mVelocity = 0.0f;
        mRotate = Vector2.zero;
        mDamage = 0;
        mHeight = 0;
        isDeathState = false;
        TagDict.Clear();
        SetCollision(true);
        SetActionState(new BulletFlyState(this));
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
    public void SetRotate(Vector2 v)
    {
        mRotate = v;
        transform.right = v;
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
        // 单位动作状态由状态机决定（如移动、攻击、待机、死亡）
        mCurrentActionState.OnUpdate();
        // 子弹若出屏了请自删
        if (!IsInView())
        {
            ExecuteRecycle();
        }
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
        Vector3 worldPos = transform.position;
        Vector2 viewPos = Camera.main.WorldToViewportPoint(worldPos); // 世界坐标转为屏幕坐标，然后判断是不是在(0,0)-(1,1)之间
        if (viewPos.x >= 0 && viewPos.x <= 1 && viewPos.y >= 0 && viewPos.y <= 1)
            return true;
        else
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
        return !isDeathState;
    }

    public virtual void OnFlyStateEnter()
    {
        animatorController.Play("Fly", true);
    }

    public virtual void OnFlyState()
    {
        transform.position += (Vector3)mRotate * mVelocity;
    }

    public virtual void OnFlyStateExit()
    {
        
    }

    public virtual void OnHitStateEnter()
    {
        animatorController.Play("Hit");
        SetCollision(false);
        isDeathState = true;
    }

    public virtual void OnHitState()
    {
        // 动画播放完一次后，转为移动状态
        AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
        //if (info.normalizedTime >= 1.0f)
        if(AnimatorManager.GetCurrentFrame(animator) == AnimatorManager.GetTotalFrame(animator))
        {
            SetActionState(new BaseBulletActionState(this));
        }
    }

    public virtual void OnHitStateExit()
    {
        ExecuteRecycle();
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
}



