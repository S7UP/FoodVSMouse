using System.Collections.Generic;

using UnityEngine;

/// <summary>
/// 火盆
/// </summary>
public class HeaterFoodUnit : FoodUnit
{
    private static RuntimeAnimatorController Bullet_RuntimeAnimatorController;

    /// <summary>
    /// 可以通过火盆的子弹类型表
    /// </summary>
    public static List<BulletStyle> canThroughtBulletStyleList = new List<BulletStyle>() {
        BulletStyle.Normal
    };


    private float mulRate; //伤害倍化倍率

    public override void Awake()
    {
        if (Bullet_RuntimeAnimatorController == null)
            Bullet_RuntimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Food/8/Bullet");
        base.Awake();
    }

    public override void MInit()
    {
        base.MInit();
        CreateCheckArea();
    }

    /// <summary>
    /// 根据等级表和等级来更新对应数据
    /// </summary>
    public override void UpdateAttributeByLevel()
    {
        mulRate = (float)attr.valueList[mLevel];
    }

    /// <summary>
    /// 检测传入子弹是否能穿过
    /// </summary>
    /// <param name="b"></param>
    /// <returns></returns>
    public bool CanThrought(BaseBullet b)
    {
        // 自身不被控制
        if (isFrozenState)
            return false;
        // 子弹得是友方子弹
        if (!(b is AllyBullet))
            return false;
        // 判断子弹是否已被增强
        if (b.GetTagCount(StringManager.BulletDamgeIncreasement) > 0)
            return false;
        // 判断子弹类型能否过盆
        foreach (var item in canThroughtBulletStyleList)
        {
            if (item == b.style)
                return true;
        }
        return false;
    }

    /// <summary>
    /// 获取倍率
    /// </summary>
    /// <returns></returns>
    public float GetDamageRate()
    {
        return 1 + Mathf.Min(1, mCurrentAttackSpeed) * Mathf.Min(1, mCurrentAttack / 10) * (mulRate - 1);
    }

    /// <summary>
    /// 创建过火检测圈
    /// </summary>
    private void CreateCheckArea()
    {
        RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(transform.position, 1.0f, 1.0f, "ItemCollideBullet");
        r.isAffectBullet = true;
        r.SetOnBulletEnterAction(OnCollision);
        r.SetOnBulletStayAction(OnCollision);
        {
            CustomizationTask t = new CustomizationTask();
            t.AddTaskFunc(delegate {
                if (this.IsAlive())
                {
                    r.transform.position = transform.position;
                    return false;
                }
                else
                {
                    return true;
                }
            });
            t.OnExitFunc = delegate {
                r.MDestory();
            };
            r.AddTask(t);
        }
        GameController.Instance.AddAreaEffectExecution(r);
    }

    public void OnCollision(BaseBullet b)
    {
        if (CanThrought(b)) // 检测子弹能否穿过
        {
            // 强制把子弹贴图改为火弹（但是不改变原始style值）
            b.animator.runtimeAnimatorController = Bullet_RuntimeAnimatorController;
            b.SetDamage(b.GetDamage() * GetDamageRate()); // 倍化伤害
            b.SetVelocity(b.GetVelocity() * 1.5f); // 加速
            b.AddTag(StringManager.BulletDamgeIncreasement); // 为子弹打上已增幅的标记，防止多次过火
        }
    }

    /////////////////////////////////以下功能均失效，不需要往下翻看/////////////////////////////////////

    /// <summary>
    /// 判断是否有有效的攻击目标
    /// </summary>
    /// <returns></returns>
    protected override bool IsHasTarget()
    {
        // 功能型卡片不需要
        return false;
    }

    /// <summary>
    /// 是否满足普通攻击的条件
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetGeneralAttackCondition()
    {
        // 功能型卡片不需要
        return true;
    }

    /// <summary>
    /// 进入普通攻击动画状态
    /// </summary>
    public override void BeforeGeneralAttack()
    {
        // 功能型卡片无攻击状态
    }

    /// <summary>
    /// 普通攻击期间
    /// </summary>
    public override void OnGeneralAttack()
    {
        // 功能型卡片无
    }

    /// <summary>
    /// 退出普通攻击的条件
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetEndGeneralAttackCondition()
    {
        // 功能型卡片无
        return true;
    }

    /// <summary>
    /// 退出普通攻击后要做的事
    /// </summary>
    public override void AfterGeneralAttack()
    {
        // 功能型卡片无
    }

    /// <summary>
    /// 是否为伤害判定时刻（近战攻击为打出实际伤害，远程攻击为确定发射弹体）
    /// </summary>
    /// <returns></returns>
    public override bool IsDamageJudgment()
    {
        // 功能型卡片无
        return false;
    }

    /// <summary>
    /// 执行具体的攻击，位于伤害判定为真之后
    /// </summary>
    public override void ExecuteDamage()
    {
        // 功能型卡片无
    }
}
