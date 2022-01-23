using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static UnityEngine.UI.CanvasScaler;

public class BaseBullet : MonoBehaviour, IBaseBullet, IGameControllerMember
{
    // 来自外部引用
    public bool isGameObjectValid;
    public BaseUnit mMasterBaseUnit; // 发射它的主人单位

    // 动画
    public Animator animator;


    // 自身属性
    public float mVelocity;
    public Vector2 mRotate;
    public float mDamage;

    public IBaseActionState mCurrentActionState; //+ 当前动作状态

    public virtual void Awake()
    {
        MInit();
        
        // 先暂时用三线的
        animator = transform.GetChild(0).GetComponent<Animator>();
        // animator.runtimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Food/7/bullet");
    }

    // 子弹对目标造成伤害，TakeDamage的调用时机是敌对单位碰到了这个子弹，然后过来调用这个子弹的伤害逻辑
    public virtual void TakeDamage(BaseUnit baseUnit)
    {
        baseUnit.OnDamage(mDamage);
        SetActionState(new BulletHitActionState(this));
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

    // 以下由GameController管
    public virtual void MDestory()
    {
        throw new System.NotImplementedException();
    }

    public virtual void MInit()
    {
        mVelocity = 1.0f;
        mRotate = Vector2.right;
        mDamage = 10;
        isGameObjectValid = true;
        SetActionState(new BulletFlyActionState(this));
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
    }
}



public class BulletFlyActionState : IBaseActionState
{
    private BaseBullet mBaseBullet;

    public BulletFlyActionState(BaseBullet baseBullet)
    {
        mBaseBullet = baseBullet;
    }

    // 当进入时
    public virtual void OnEnter()
    {
        Animator ani = mBaseBullet.transform.GetChild(0).gameObject.GetComponent<Animator>();
        ani.Play("Fly");
    }

    // 当退出时
    public virtual void OnExit()
    {

    }

    // 实现动作状态
    public virtual void OnUpdate()
    {
        mBaseBullet.transform.position += (Vector3)mBaseBullet.mRotate* mBaseBullet.mVelocity * 0.05f;
    }
}


public class BulletHitActionState : IBaseActionState
{
    private BaseBullet mBaseBullet;
    private bool isFirstFrame; // 是否为切换时的第一帧 

    public BulletHitActionState(BaseBullet baseBullet)
    {
        mBaseBullet = baseBullet;
    }

    // 当进入时
    public virtual void OnEnter()
    {
        isFirstFrame = true;
        Animator ani = mBaseBullet.transform.GetChild(0).gameObject.GetComponent<Animator>();
        ani.Play("Hit");
    }

    // 当退出时
    public virtual void OnExit()
    {

    }

    // 实现动作状态
    public virtual void OnUpdate()
    {
        // 切换时的第一帧直接不执行update()，因为下述的info.normalizedTime的值还停留在上一个状态，逻辑会出问题！
        if (isFirstFrame)
        {
            isFirstFrame = !isFirstFrame;
            return;
        }

        AnimatorStateInfo info = mBaseBullet.animator.GetCurrentAnimatorStateInfo(0);
        // Debug.Log(info.normalizedTime);
        if (info.normalizedTime >= 1.0f)
        {
            GameManager.Instance.RecycleBullet(mBaseBullet, "Bullet/Pre_Bullet");
            mBaseBullet.SetActionState(new BulletDefaultActionState(mBaseBullet));
        }
    }
}

public class BulletDefaultActionState : IBaseActionState
{
    private BaseBullet mBaseBullet;

    public BulletDefaultActionState(BaseBullet baseBullet)
    {
        mBaseBullet = baseBullet;
    }

    // 当进入时
    public virtual void OnEnter()
    {

    }

    // 当退出时
    public virtual void OnExit()
    {

    }

    // 实现动作状态
    public virtual void OnUpdate()
    {

    }
}