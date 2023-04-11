using System.Collections.Generic;
/// <summary>
/// 樱桃反弹布丁
/// </summary>
public class CherryPuddingFoodUnit : FoodUnit
{
    /// <summary>
    /// 可以被反弹的子弹类型表
    /// </summary>
    public static List<BulletStyle> canReboundBulletStyleList = new List<BulletStyle>() {
        BulletStyle.Normal, BulletStyle.NoStrengthenNormal
    };

    public override void MInit()
    {
        base.MInit();
        CreateReboundArea();
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
    /// 检测传入子弹是否能反弹
    /// </summary>
    /// <param name="baseBullet"></param>
    /// <returns></returns>
    public bool CanThrought(BaseBullet b)
    {
        // 自身不被控制
        if (isFrozenState)
            return false;
        // 子弹得是友方子弹
        if (!(b is AllyBullet))
            return false;
        // 判断子弹是否已被反弹
        if (b.GetTagCount(StringManager.BulletRebound) > 0)
            return false;
        // 判断子弹类型能否反弹
        foreach (var item in canReboundBulletStyleList)
        {
            if (item == b.style)
                return true;
        }
        return false;
    }

    public override bool CanBeSelectedAsTarget(BaseUnit otherUnit)
    {
        // 在自身为定身状态下，不被友方投掷选为攻击目标
        if(otherUnit is FoodUnit)
        {
            FoodUnit f = otherUnit as FoodUnit;
            if (GetNoCountUniqueStatus(StringManager.Stun) != null && PitcherManager.IsPitcher(f))
                return false;
        }
        return base.CanBeSelectedAsTarget(otherUnit);
    }

    /// <summary>
    /// 寻找被重定向投掷物的目标算法（先左向找最左的，再右向找最左的）
    /// </summary>
    /// <returns></returns>
    public BaseUnit FindRedirectThrowingObjectTarget(BaseBullet b)
    {
        // 若自身被定身了则不重定向投掷物
        if (GetNoCountUniqueStatus(StringManager.Stun)!=null)
            return null;

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
    /// 创建反弹子弹检测圈
    /// </summary>
    private void CreateReboundArea()
    {
        RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(transform.position, 0.5f, 0.5f, "ItemCollideBullet");
        r.isAffectBullet = true;
        r.SetOnBulletEnterAction(OnCollision);
        r.SetOnBulletStayAction(OnCollision);
        {
            CustomizationTask t = new CustomizationTask();
            t.AddTaskFunc(delegate {
                if(this.IsAlive())
                {
                    r.transform.position = transform.position;
                    return false;
                }
                else
                {
                    return true;
                }
            });
            t.AddOnExitAction(delegate {
                r.MDestory();
            });
            r.AddTask(t);
        }
        GameController.Instance.AddAreaEffectExecution(r);
    }

    public void OnCollision(BaseBullet b)
    {
        if (CanThrought(b)) // 检测子弹能否穿过
        {
            // 子弹反向
            b.SetRotate(-b.GetRotate());
            b.AddTag(StringManager.BulletRebound); // 为子弹打上已反弹的标记，防止多次反弹
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
