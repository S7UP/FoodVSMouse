using System.Collections.Generic;

using UnityEngine;

/// <summary>
/// 火盆
/// </summary>
public class HeaterFoodUnit : FoodUnit
{
    /// <summary>
    /// 可以通过火盆的子弹类型表
    /// </summary>
    public static List<BulletStyle> canThroughtBulletStyleList = new List<BulletStyle>() { 
        BulletStyle.Wine, BulletStyle.Water
    };


    private float mulRate; //伤害倍化倍率
    private Rigidbody2D rigibody2D;

    public override void Awake()
    {
        base.Awake();
        rigibody2D = GetComponent<Rigidbody2D>();
    }

    public override void MInit()
    {
        base.MInit();
    }

    /// <summary>
    /// 根据等级表和等级来更新对应数据
    /// </summary>
    public override void UpdateAttributeByLevel()
    {
        mulRate = (float)(attr.baseAttrbute.baseAttack + attr.valueList[mLevel]);
    }

    /// <summary>
    /// 该单位装了rigibody2d，因此需要重写此方法
    /// </summary>
    /// <param name="V3"></param>
    public override void SetPosition(Vector3 V3)
    {
        rigibody2D.MovePosition(V3);
    }

    /// <summary>
    /// 检测传入子弹是否能穿过
    /// </summary>
    /// <param name="baseBullet"></param>
    /// <returns></returns>
    public bool CanThrought(BaseBullet baseBullet)
    {
        // 判断子弹是否已被增强
        if (baseBullet.GetTagCount(StringManager.BulletDamgeIncreasement) > 0)
            return false;
        // 判断子弹类型能否过盆
        foreach (var item in canThroughtBulletStyleList)
        {
            if (item == baseBullet.style)
                return true;
        }
        return false;
    }

    public void OnCollision(Collider2D collision)
    {
        // 死亡动画时不接受任何碰撞事件
        if (isDeathState)
        {
            return;
        }


        if (collision.tag.Equals("Bullet"))
        {
            // 检测到子弹单位碰撞了
            BaseBullet bullet = collision.GetComponent<BaseBullet>();
            if (CanThrought(bullet)) // 检测子弹能否穿过
            {
                // 强制把子弹贴图改为火弹（但是不改变原始style值）
                bullet.ChangeAnimatorWithoutChangeStyle(BulletStyle.Fire);
                bullet.SetDamage(bullet.GetDamage() * mulRate); // 倍化伤害
                bullet.SetVelocity(bullet.GetVelocity() * 1.5f); // 加速
                bullet.AddTag(StringManager.BulletDamgeIncreasement); // 为子弹打上已增幅的标记，防止多次过火
            }
        }
    }

    // rigibody相关
    private void OnTriggerEnter2D(Collider2D collision)
    {
        OnCollision(collision);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        OnCollision(collision);
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
