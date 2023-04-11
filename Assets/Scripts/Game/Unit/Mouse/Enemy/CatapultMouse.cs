
using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// Ͷʯ��������
/// </summary>
public class CatapultMouse : MouseUnit
{
    private BaseUnit targetUnit; // ���赲̬�¹���Ŀ��
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
        UpdateBlockState(); // �����赲״̬
        // ����п��Թ�����Ŀ�꣬��ͣ�����ȴ���һ�ι���������ǰ��
        if (IsHasTarget() || (targetUnit!=null && targetUnit.IsAlive()))
            SetActionState(new IdleState(this));
        else
            SetActionState(new MoveState(this));
    }

    public override void OnIdleState()
    {
        // ûĿ���˾�����
        if(!IsMeetGeneralAttackCondition())
            SetActionState(new MoveState(this));
    }

    /// <summary>
    /// �Ƿ�������ͨ����������
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetGeneralAttackCondition()
    {
        targetUnit = null;
        // ���������������
        // 1�����赲�� && �赲��������Ч�� -> �����赲��
        // 2��δ���赲 && ����λ���ѳ�����һ������ && ���д��ڿɹ������� -> ��������Ŀɹ�������
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
    /// ִ�о���Ĺ�����λ���˺��ж�Ϊ��֮��
    /// </summary>
    public override void ExecuteDamage()
    {
        float dmg = mCurrentAttack;
        GameManager.Instance.audioSourceManager.PlayEffectMusic("Basketball");

        // �赲���ȼ�����Զ�̹������ȼ�
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
