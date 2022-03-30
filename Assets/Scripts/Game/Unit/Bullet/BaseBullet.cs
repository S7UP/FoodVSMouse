using System.Collections;
using System.Collections.Generic;
using System.Drawing;

using UnityEngine;

using static UnityEngine.UI.CanvasScaler;

public class BaseBullet : MonoBehaviour, IBaseBullet, IGameControllerMember
{
    // 来自外部引用
    public BaseUnit mMasterBaseUnit; // 发射它的主人单位

    // 引用
    public Animator animator;
    public CircleCollider2D mCircleCollider2D;

    // 自身属性
    public float mVelocity;
    public Vector2 mRotate;
    public float mHeight;
    public float mDamage;
    public bool isDeathState;

    public IBaseActionState mCurrentActionState; //+ 当前动作状态

    public virtual void Awake()
    {
        // 先暂时用三线的
        animator = transform.GetChild(0).GetComponent<Animator>();
        mCircleCollider2D = GetComponent<CircleCollider2D>();
        // animator.runtimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Food/7/bullet");
    }

    // 子弹对目标造成伤害，TakeDamage的调用时机是敌对单位碰到了这个子弹，然后过来调用这个子弹的伤害逻辑
    public virtual void TakeDamage(BaseUnit baseUnit)
    {
        new DamageAction(CombatAction.ActionType.CauseDamage, mMasterBaseUnit, baseUnit, mDamage).ApplyAction();
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
        mVelocity = 1.0f;
        mRotate = Vector2.right;
        mDamage = 10;
        isDeathState = false;
        SetCollision(true);
        SetActionState(new BulletFlyState(this));
    }

    public void SetDamage(float dmg)
    {
        mDamage = dmg;
    }

    public virtual void MPause()
    {
        throw new System.NotImplementedException();
    }

    public virtual void MResume()
    {
        throw new System.NotImplementedException();
    }

    public virtual void MUpdate()
    {
        // 单位动作状态由状态机决定（如移动、攻击、待机、死亡）
        mCurrentActionState.OnUpdate();
        // 子弹若出屏了请自删
        if (!IsInView())
        {
            GameManager.Instance.PushGameObjectToFactory(FactoryType.GameFactory, "Bullet/Pre_Bullet", gameObject);
        }
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
        animator.Play("Fly");
    }

    public virtual void OnFlyState()
    {
        transform.position += (Vector3)mRotate * mVelocity * 0.05f;
    }

    public virtual void OnFlyStateExit()
    {
        
    }

    public virtual void OnHitStateEnter()
    {
        animator.Play("Hit");
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
        GameManager.Instance.PushGameObjectToFactory(FactoryType.GameFactory, "Bullet/Pre_Bullet", gameObject);
    }
}



