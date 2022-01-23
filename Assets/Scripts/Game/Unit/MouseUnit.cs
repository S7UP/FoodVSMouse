using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseUnit : BaseUnit
{
    protected Animator animator;
    protected float attackPercent; // 攻击动画播放进度到attackPercent以上时允许出真正的攻击

    protected bool mAttackFlag; // 作用于一次攻击能否打出来的flag

    public float mBaseMoveSpeed; // 基础移动速度
    public float mCurrentMoveSpeed; // 当前移动速度

    // 索敌相关
    protected bool isBlock; // 是否被阻挡
    protected BaseUnit mBlockUnit; // 阻挡者

    public Rigidbody2D rigibody2D;

    public override void Awake()
    {
        base.Awake();
        rigibody2D = gameObject.GetComponent<Rigidbody2D>();
        animator = gameObject.transform.GetChild(0).gameObject.GetComponent<Animator>();
        attackPercent = 0.60f;
    }


    // 至少大部分老鼠应该有一个能直接攻击的手段
    public void TakeDamage(BaseUnit unit)
    {
        unit.OnDamage(mCurrentAttack);
    }


    // 注：一个Collider2D不应该直接使用Transform或者其offset属性来移动它，而是应该使用Rigidbody2D的移动代替之。这样会得到最好的表现和正确的碰撞检测。
    // 因此，操作老鼠移动不应该用上面的transform,而是用下面的rigibody2D
    public override void SetPosition(Vector3 V3)
    {
        rigibody2D.MovePosition(V3);
    }

    // 判断是否发现攻击目标
    protected virtual void SearchTarget()
    {
        // 被阻挡了
        if (isBlock)
        {
            // 阻挡对象是否有效
            if (mBlockUnit.isGameObjectValid)
            {
                SetActionState(new AttackState(this));
            }
            else
            {
                isBlock = false;
            }
        }
    } 

    // 判断能不能攻击了，限定在OnAttackState()里调用，允许子类重写
    protected virtual bool CanAttack()
    {
        // 获取Attack的动作信息，使得伤害判定与动画显示尽可能同步
        AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
        // 这个normalizedTime的小数可以近似表示一个动画播放进度的百分比，个位数则可以表示已循环的次数。
        int c = Mathf.FloorToInt(info.normalizedTime);

        // 动画进度在到一定时启动伤害判定
        float percent = info.normalizedTime - c;
        if (percent >= attackPercent && mAttackFlag)
        {
            return true;
        }
        return false;
    }

    // 当真正攻击时做出的具体操作。
    protected virtual void Attack()
    {
        if (mBlockUnit != null && mBlockUnit.isGameObjectValid)
        {
            TakeDamage(mBlockUnit);
        }
        Debug.Log("打出伤害判定了！！");
        mAttackFlag = false;
    }


    // 以下为 IGameControllerMember 接口方法的实现
    public override void MInit()
    {
        base.MInit();

        mBaseMoveSpeed = 0.5f; // 移动速度值为格/秒，默认设为0.5格/秒
        mCurrentMoveSpeed = mBaseMoveSpeed;
        // 移动状态test
        SetActionState(new MoveState(this));
    }

    // 以下为 IBaseStateImplementor 接口的方法实现
    public override void OnIdleState()
    {
        // 索敌逻辑
        SearchTarget();
    }

    public override void OnMoveState()
    {
        // 移动更新
        SetPosition((Vector2)GetPosition() + Vector2.left * mCurrentMoveSpeed * 0.005f);
        // 索敌逻辑
        SearchTarget();
    }

    public override void OnAttackState()
    {
        // 切换时的第一帧直接不执行update()，因为下述的info.normalizedTime的值还停留在上一个状态，逻辑会出问题！
        if (currentStateTimer <= 0)
        {
            return;
        }
        // 获取Attack的动作信息，使得伤害判定与动画显示尽可能同步
        AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
        if (CanAttack())
        {
            Attack();
            mAttackFlag = false;
        }
        else if (info.normalizedTime >= 1.0f) // 攻击动画播放完一次后转为待机状态
        {
            SetActionState(new IdleState(this));
        }
    }



    // rigibody相关

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log(collision.tag);
        
        if (collision.tag.Equals("Food"))
        {
            // 被美食挡着了！
            Debug.Log("检测到被阻挡！");
            //MouseAttackState state = new MouseAttackState(this);
            //SetActionState(state);
            //state.mTargetUnit = collision.GetComponent<BaseUnit>();
            isBlock = true;
            mBlockUnit = collision.GetComponent<BaseUnit>();
        }
        else if (collision.tag.Equals("Bullet"))
        {
            // 被子弹打中了
            BaseBullet bullet = collision.GetComponent<BaseBullet>();
            bullet.TakeDamage(this);
        }
    }
}
