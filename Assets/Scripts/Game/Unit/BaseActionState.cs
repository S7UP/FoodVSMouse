using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IBaseActionState
{
    public void OnEnter();
    public void OnExit();
    public void OnUpdate();
}

public class BaseActionState: IBaseActionState
{
    // 持有的Unit引用

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

/// <summary>
/// 老鼠单位移动状态
/// </summary>
public class MouseMoveState : BaseActionState
{
    private MouseUnit mMouseUnit;
    private Rigidbody2D rigidbody2D;

    public MouseMoveState(MouseUnit mouseUnit)
    {
        mMouseUnit = mouseUnit;
        rigidbody2D = mouseUnit.mGameObject.GetComponent<Rigidbody2D>();
    }

    public override void OnEnter()
    {
        // 获取sprite的obj，并修改动画播放
        GameObject go = mMouseUnit.mGameObject.transform.GetChild(0).gameObject;
        Animator ani = go.GetComponent<Animator>();
        ani.Play("Move");
    }

    public override void OnUpdate()
    {
        // 移动更新
        // mMouseUnit.mGameObject.transform.localPosition += new Vector3(-mMouseUnit.mCurrentMoveSpeed * 0.005f, 0, 0);
        // 注：一个Collider2D不应该直接使用Transform或者其offset属性来移动它，而是应该使用Rigidbody2D的移动代替之。这样会得到最好的表现和正确的碰撞检测。
        // 因此，操作老鼠移动不应该用上面的transform,而是用下面的rigibody2D
        rigidbody2D.MovePosition(rigidbody2D.position + Vector2.left * mMouseUnit.mCurrentMoveSpeed * 0.005f);
        // Debug.Log(rigidbody2D.position);
    }
}

/// <summary>
/// 老鼠单位攻击状态
/// </summary>
public class MouseAttackState : BaseActionState
{
    // Target由外部给定或者临时计算给定，并且允许为空；
    public GameObject mTarget;

    private MouseUnit mMouseUnit; // 持有此状态的MouseUnit引用
    private Animator animator;
    private bool canDamage;  // 是否能出伤害判定的标志
    private bool isFirstFrame; // 是否为切换时的第一帧 
    private int attackCount; // 从这个状态开始至今，Attack动画完成了几次

    public MouseAttackState(MouseUnit mouseUnit)
    {
        mMouseUnit = mouseUnit;
        animator = mMouseUnit.mGameObject.transform.GetChild(0).gameObject.GetComponent<Animator>();
        attackCount = 0;
        isFirstFrame = true;
        canDamage = true;
    }

    public override void OnEnter()
    {
        // 修改动画播放
        animator.Play("Attack");
    }

    public override void OnUpdate()
    {
        // 切换时的第一帧直接不执行update()，因为下述的info.normalizedTime的值还停留在上一个状态，逻辑会出问题！
        if (isFirstFrame)
        {
            isFirstFrame = !isFirstFrame;
            return;
        }
        // 获取Attack的动作信息，使得伤害判定与动画显示尽可能同步
        AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
        // 这个normalizedTime的小数可以近似表示一个动画播放进度的百分比，个位数则可以表示已循环的次数。
        // Debug.Log(info.normalizedTime);
        int c = Mathf.FloorToInt(info.normalizedTime);
        // Debug.Log("个位数为："+c);

        // 动画进度在到一定时启动伤害判定
        float percent = info.normalizedTime - c;
        if (percent >= 0.65 && canDamage)
        {
            // TakeDamage()
            if (mTarget != null)
            {
                GameController.Destroy(mTarget);
            }
            Debug.Log("打出伤害判定了！！");
            canDamage = false;
        }

        // 下述情况下表明开始新的一轮攻击循环了，伤害判定应当放开
        if (attackCount < c)
        {
            Debug.Log("新一轮攻击了吗？");
            attackCount++;
            canDamage = true;
            // 检查一下还有没有被阻挡
            // IsBlocked()
            if (mTarget == null)
            {
                mMouseUnit.SetActionState(new MouseMoveState(mMouseUnit));
                Debug.Log("切换为走路模式");
                // return;
            }
        }
    }
}