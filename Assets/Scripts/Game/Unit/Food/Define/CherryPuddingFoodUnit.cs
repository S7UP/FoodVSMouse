using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// 樱桃反弹布丁
/// </summary>
public class CherryPuddingFoodUnit : FoodUnit
{
    /// <summary>
    /// 可以被反弹的子弹类型表
    /// </summary>
    public static List<BulletStyle> canReboundBulletStyleList = new List<BulletStyle>() {
        BulletStyle.Wine, BulletStyle.Water, BulletStyle.Fire
    };


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
        if(mShape >= 1)
            SetMaxHpAndCurrentHp((float)(attr.baseAttrbute.baseHP + attr.valueList[mLevel])*1.25f);
        else
            SetMaxHpAndCurrentHp((float)(attr.baseAttrbute.baseHP + attr.valueList[mLevel]));
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
    /// 检测传入子弹是否能反弹
    /// </summary>
    /// <param name="baseBullet"></param>
    /// <returns></returns>
    public bool CanThrought(BaseBullet baseBullet)
    {
        // 判断子弹是否已被反弹
        if (baseBullet.GetTagCount(StringManager.BulletRebound) > 0)
            return false;
        // 判断子弹类型能否反弹
        foreach (var item in canReboundBulletStyleList)
        {
            if (item == baseBullet.style)
                return true;
        }
        return false;
    }

    /// <summary>
    /// 寻找被重定向投掷物的目标算法（先左向找最左的，再右向找最左的）
    /// </summary>
    /// <returns></returns>
    public BaseUnit FindRedirectThrowingObjectTarget(BaseBullet b)
    {
        // 先寻找有效单位
        BaseUnit target = MouseManager.FindTheMostLeftTarget(b.mMasterBaseUnit, float.MinValue, b.transform.position.x, b.GetRowIndex());
        if (target != null)
        {
            return target;
        }
        else
        {
            // 如果没目标那就再找一次，这次从右侧找
            target = MouseManager.FindTheMostLeftTarget(b.mMasterBaseUnit, b.transform.position.x, float.MaxValue, b.GetRowIndex());
            return target;
        }
    }

    /// <summary>
    /// 重定向投掷物
    /// </summary>
    public void RedirectThrowingObject(BaseBullet b)
    {
        // 判断子弹是否已被重定向过，如果已被重定向过了则不能再重定向
        if (b.GetTagCount(StringManager.BulletRebound) > 0)
            return;
        // 添加重定向任务，重定向至子弹左侧所有可攻击单位中最靠左侧的单位
        {
            // 先寻找有效单位
            BaseUnit target = FindRedirectThrowingObjectTarget(b);
            if (target != null)
            {
                // 弹回去！
                PitcherManager.AddDefaultFlyTask(b, b.transform.position, target, true, false);
            }
            //else
            //{
            //    // 这要是还没有的话，那就原地向上弹然后碎掉吧
            //    PitcherManager.AddDefaultFlyTask(b, b.transform.position, b.transform.position, true, false);
            //}
            b.AddTag(StringManager.BulletRebound); // 为子弹打上已反弹的标记，防止多次反弹
        }
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
                // 子弹反向
                bullet.SetRotate(-bullet.GetRotate());
                bullet.AddTag(StringManager.BulletRebound); // 为子弹打上已反弹的标记，防止多次反弹
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
