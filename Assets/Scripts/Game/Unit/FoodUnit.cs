using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static UnityEngine.GraphicsBuffer;

public class FoodUnit : BaseUnit
{
    protected bool mAttackFlag; // 作用于一次攻击能否打出来的flag

    protected Animator animator;
    protected float attackPercent; // 攻击动画播放进度到attackPercent以上时允许出真正的攻击

    public override void Awake()
    {
        base.Awake();
        mAttackFlag = true;
        animator = gameObject.transform.GetChild(0).gameObject.GetComponent<Animator>();
        attackPercent = 0.65f;
    }

    // 判断能不能开火了，限定在OnAttackState()里调用，允许子类重写
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

    // 当真正开火时发出的子弹相关内容，允许且需要子类重写以达到多态性。
    protected virtual void Attack()
    {
        // Debug.Log("卡片攻击了！");
        for (int i = -1; i <= 1; i++)
        {
            for (int j = 0; j < 2; j++)
            {
                GameController.Instance.CreateBullet(transform.position + Vector3.right * 0.5f * j + Vector3.up * 0.7f * i);
            }
        }
    }

    // 
    public override void MInit()
    {
        base.MInit();
        SetActionState(new IdleState(this));
        gameObject.transform.GetChild(0).gameObject.GetComponent<Animator>().runtimeAnimatorController = (RuntimeAnimatorController)Resources.Load("Animator/AnimatorController/Food/7/2");
    }

    // 在待机状态时每帧要做的事
    public override void OnIdleState()
    {
        // 默认为攻击计数器为零时才能发起攻击
        if (mAttackCDLeft <= 0)
        {
            SetActionState(new AttackState(this));
            mAttackCDLeft += mAttackCD;
        }
    }

    // 在攻击状态时每帧要做的事
    public override void OnAttackState()
    {
        // 切换时的第一帧直接不执行update()，因为下述的info.normalizedTime的值还停留在上一个状态，逻辑会出问题！
        if (currentStateTimer<=0)
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
            mAttackFlag = true;
            SetActionState(new IdleState(this));
        }
    }
}