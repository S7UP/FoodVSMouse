using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseUnit : BaseUnit
{

    public float mBaseMoveSpeed; // 基础移动速度
    public float mCurrentMoveSpeed; // 当前移动速度

    public Rigidbody2D rigibody2D;

    public override void Awake()
    {
        base.Awake();
        rigibody2D = gameObject.GetComponent<Rigidbody2D>();
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

    public override void MInit()
    {
        base.MInit();

        mBaseMoveSpeed = 0.5f; // 移动速度值为格/秒，默认设为0.5格/秒
        mCurrentMoveSpeed = mBaseMoveSpeed;
        // 移动状态test
        SetActionState(new MouseMoveState(this));
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log(collision.tag);
        
        if (collision.tag.Equals("Food"))
        {
            // 被美食挡着了！
            Debug.Log("切换为攻击模式！");
            MouseAttackState state = new MouseAttackState(this);
            SetActionState(state);
            state.mTargetUnit = collision.GetComponent<BaseUnit>();
        }else if (collision.tag.Equals("Bullet"))
        {
            // 被子弹打中了
            BaseBullet bullet = collision.GetComponent<BaseBullet>();
            bullet.TakeDamage(this);
        }
    }
}
