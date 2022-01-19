using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static UnityEngine.GraphicsBuffer;

public class FoodUnit : BaseUnit
{
    public override void Awake()
    {
        base.Awake();
    }

    public override void MInit()
    {
        base.MInit();
        SetActionState(new FoodIdleState(this));
        gameObject.transform.GetChild(0).gameObject.GetComponent<Animator>().runtimeAnimatorController = (RuntimeAnimatorController)Resources.Load("Animator/AnimatorController/Food/7/2");
    }
}

/// <summary>
/// 先暂时显式得把三线的状态方法写在这
/// </summary>
public class FoodAttackState : BaseActionState
{
    private FoodUnit mFoodUnit;
    private Animator animator;
    private bool canDamage;  // 是否能出伤害判定的标志
    private bool isFirstFrame; // 是否为切换时的第一帧 

    public FoodAttackState(FoodUnit foodUnit)
    {
        mFoodUnit = foodUnit;
        animator = mFoodUnit.gameObject.transform.GetChild(0).gameObject.GetComponent<Animator>();
        canDamage = true;
    }

    public override void OnEnter()
    {
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
            // Debug.Log("卡片攻击了！");
            for (int i = -1; i <= 1; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    GameController.Instance.CreateBullet(mFoodUnit.transform.position + Vector3.right*0.5f*j + Vector3.up*0.7f*i);
                }
            }
            
            canDamage = false;
        }else if (info.normalizedTime >= 1.0f)
        {
            mFoodUnit.SetActionState(new FoodIdleState(mFoodUnit));
        }
    }
}


public class FoodIdleState : BaseActionState
{
    private FoodUnit mFoodUnit;
    private Animator animator;

    public FoodIdleState(FoodUnit foodUnit)
    {
        mFoodUnit = foodUnit;
        animator = mFoodUnit.gameObject.transform.GetChild(0).gameObject.GetComponent<Animator>();
    }

    public override void OnEnter()
    {
        animator.Play("Idle");
    }

    public override void OnUpdate()
    {
        // 在射程内发现敌人 且 攻击计数器为零时才能发起攻击
        if (true && mFoodUnit.mAttackCDLeft <= 0)
        {
            mFoodUnit.SetActionState(new FoodAttackState(mFoodUnit));
            mFoodUnit.mAttackCDLeft += mFoodUnit.mAttackCD;
        }
    }
}