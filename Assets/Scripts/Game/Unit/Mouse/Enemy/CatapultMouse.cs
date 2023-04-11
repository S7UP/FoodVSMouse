
using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// 投石车类老鼠
/// </summary>
public class CatapultMouse : MouseUnit
{
    private BaseUnit targetUnit; // 非阻挡态下攻击目标
    private RuntimeAnimatorController[] RunArray;

    public override void Awake()
    {
        if(RunArray == null)
        {
            RunArray = new RuntimeAnimatorController[3];
            for (int i = 0; i < RunArray.Length; i++)
            {
                RunArray[i] = GameManager.Instance.GetRuntimeAnimatorController("Mouse/15/" + i + "/Bullet");
            }
        }
        base.Awake();
    }

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
                targetUnit = FoodManager.GetSpecificRowFarthestLeftCanTargetedAlly(GetRowIndex(), transform.position.x, true);
                if (targetUnit != null)
                {
                    UpdateTargetUnit();
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
        float dmg = mCurrentAttack;
        GameManager.Instance.audioSourceManager.PlayEffectMusic("Basketball");

        // 阻挡优先级大于远程攻击优先级
        if (IsHasTarget())
        {
            BaseUnit u = GetCurrentTarget();
            EnemyBullet b = EnemyBullet.GetInstance(RunArray[mShape], this, 0);
            b.AddHitAction((b, u) =>
            {
                BaseGrid g = u.GetGrid();
                if (g != null)
                {
                    BaseUnit target = g.GetThrowHighestAttackPriorityUnitInclude(this);
                    new DamageAction(CombatAction.ActionType.CauseDamage, this, target, dmg).ApplyAction();
                }
            });
            TaskManager.AddParabolaTask(b, TransManager.TranToVelocity(24f), 0.25f, transform.position, u, false);
            GameController.Instance.AddBullet(b);

        }
        else if(targetUnit != null &&  targetUnit.IsAlive())
        {
            UpdateTargetUnit();
            float v = TransManager.TranToStandardVelocity(Mathf.Abs(targetUnit.transform.position.x - transform.position.x)/90f);
            EnemyBullet b = EnemyBullet.GetInstance(RunArray[mShape], this, 0);
            b.AddHitAction((b, u) =>
            {
                if (u == null)
                    return;
                BaseGrid g = u.GetGrid();
                if (g != null)
                {
                    BaseUnit target = g.GetThrowHighestAttackPriorityUnitInclude(this);
                    new DamageAction(CombatAction.ActionType.CauseDamage, this, target, dmg).ApplyAction();
                }
            });
            TaskManager.AddParabolaTask(b, TransManager.TranToVelocity(v), 2.0f, transform.position, targetUnit, false);
            GameController.Instance.AddBullet(b);
        }
    }

    public override void OnDieStateEnter()
    {
        GameManager.Instance.audioSourceManager.PlayEffectMusic("Explosion");
        base.OnDieStateEnter();
    }

    private void UpdateTargetUnit()
    {
        if (targetUnit != null && targetUnit.IsAlive())
        {
            Queue<FoodInGridType> queue = new Queue<FoodInGridType>();
            queue.Enqueue(FoodInGridType.Bomb);
            queue.Enqueue(FoodInGridType.Default);
            queue.Enqueue(FoodInGridType.Shield);
            targetUnit = targetUnit.GetGrid().GetHighestAttackPriorityFoodUnit(queue, this);
        }
    }
}
