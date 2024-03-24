using UnityEngine;

public class CaoNiMaMouse : MouseUnit
{
    public override void OnAttackStateEnter()
    {
        BaseUnit target = GetCurrentTarget();
        if (target != null && target.IsAlive() && mCurrentAttack * 10 < target.mMaxHp)
        {
            SetActionState(new MoveState(this));

            // Ìø2¸ñ
            float dist = 2 * MapManager.gridWidth;
            CustomizationTask t = TaskManager.GetParabolaTask(this, dist / 60, dist / 2, transform.position, transform.position + (Vector3)moveRotate * dist, false, true);
            t.AddOnEnterAction(delegate {
                DisableMove(true);
            });
            t.AddOnExitAction(delegate
            {
                DisableMove(false);
            });
            AddTask(t);
        }
        else
        {
            base.OnAttackStateEnter();
        }
    }
}
