
using UnityEngine;
/// <summary>
/// 投石车类老鼠
/// </summary>
public class CatapultMouse : MouseUnit
{
    private BaseUnit targetUnit; // 非阻挡态下攻击目标

    public override void AfterGeneralAttack()
    {
        mAttackFlag = true;
        UpdateBlockState(); // 更新阻挡状态
        // 如果有可以攻击的目标，则停下来等待下一次攻击，否则前进
        if (IsHasTarget() || (targetUnit!=null && targetUnit.IsAlive()))
            SetActionState(new IdleState(this));
        else
            SetActionState(new MoveState(this));
    }

    public override void OnIdleState()
    {
        // 没目标了就走了
        if(!IsMeetGeneralAttackCondition())
            SetActionState(new MoveState(this));
    }

    /// <summary>
    /// 是否满足普通攻击的条件
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetGeneralAttackCondition()
    {
        targetUnit = null;
        // 攻击有两种情况：
        // 1、被阻挡了 && 阻挡对象是有效的 -> 攻击阻挡者
        // 2、未被阻挡 && 自身位置已超过右一列中心 && 本行存在可攻击对象 -> 攻击最靠左侧的可攻击对象
        if (IsHasTarget())
            return true;
        else
        {
            if(transform.position.x < MapManager.GetColumnX(MapController.xColumn - 1))
            {
                targetUnit = FoodManager.GetSpecificRowFarthestLeftCanTargetedAlly(GetRowIndex(), transform.position.x);
                if (targetUnit != null)
                {
                    return true;
                }
                return false;
            }
        }
        return false;
    }

    /// <summary>
    /// 执行具体的攻击，位于伤害判定为真之后
    /// </summary>
    public override void ExecuteDamage()
    {
        RuntimeAnimatorController run = GameManager.Instance.GetRuntimeAnimatorController("Bullet/6/" + mShape);

        // 阻挡优先级大于远程攻击优先级
        if (IsHasTarget())
        {
            BaseUnit u = GetCurrentTarget();
            EnemyBullet b = EnemyBullet.GetInstance(run, this, mCurrentAttack);
            // 修改攻击优先级，这种投掷攻击优先攻击护罩里的东西
            b.GetTargetFunc = (unit) => {
                BaseGrid g = unit.GetGrid();
                if (g != null)
                {
                    return g.GetThrowHighestAttackPriorityUnitInclude();
                }
                return unit;
            };
            TaskManager.AddParabolaTask(b, TransManager.TranToVelocity(24f), 0.25f, transform.position, new Vector2(u.transform.position.x, transform.position.y), false);
            GameController.Instance.AddBullet(b);

        }
        else if(targetUnit != null &&  targetUnit.IsAlive())
        {
            float v = TransManager.TranToStandardVelocity(Mathf.Abs(targetUnit.transform.position.x - transform.position.x)/90f);
            EnemyBullet b = EnemyBullet.GetInstance(run, this, mCurrentAttack);
            // 修改攻击优先级，这种投掷攻击优先攻击护罩里的东西
            b.GetTargetFunc = (unit) => {
                BaseGrid g = unit.GetGrid();
                if (g != null)
                {
                    return g.GetThrowHighestAttackPriorityUnitInclude();
                }
                return unit;
            };
            TaskManager.AddParabolaTask(b, TransManager.TranToVelocity(v), 2.0f, transform.position, new Vector2(targetUnit.transform.position.x, transform.position.y), false);
            GameController.Instance.AddBullet(b);
        }
    }
}
