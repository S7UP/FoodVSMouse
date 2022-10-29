
using UnityEngine;
/// <summary>
/// Ͷʯ��������
/// </summary>
public class CatapultMouse : MouseUnit
{
    private BaseUnit targetUnit; // ���赲̬�¹���Ŀ��

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
    /// ִ�о���Ĺ�����λ���˺��ж�Ϊ��֮��
    /// </summary>
    public override void ExecuteDamage()
    {
        RuntimeAnimatorController run = GameManager.Instance.GetRuntimeAnimatorController("Bullet/6/" + mShape);

        // �赲���ȼ�����Զ�̹������ȼ�
        if (IsHasTarget())
        {
            BaseUnit u = GetCurrentTarget();
            EnemyBullet b = EnemyBullet.GetInstance(run, this, mCurrentAttack);
            // �޸Ĺ������ȼ�������Ͷ���������ȹ���������Ķ���
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
            // �޸Ĺ������ȼ�������Ͷ���������ȹ���������Ķ���
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
