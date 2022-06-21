using System.Collections;
using System.Collections.Generic;
using TMPro;

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
        // ����п��Թ�����Ŀ�꣬��ͣ�����ȴ���һ�ι���������ǰ��
        if (IsHasTarget() || (targetUnit!=null && targetUnit.IsAlive()))
            SetActionState(new IdleState(this));
        else
            SetActionState(new MoveState(this));
        UpdateBlockState(); // �����赲״̬
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
                List<BaseUnit> list = GameController.Instance.GetSpecificRowAllyList(GetRowIndex());
                float temp_x = transform.position.x;
                foreach (var item in list)
                {
                    if (item.transform.position.x < temp_x && item.IsAlive())
                    {
                        temp_x = item.transform.position.x;
                        targetUnit = item;
                    }
                }
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
        // �赲���ȼ�����Զ�̹������ȼ�
        if (IsHasTarget())
        {
            BaseUnit u = GetCurrentTarget();
            ParabolaBullet b = (ParabolaBullet)GameController.Instance.CreateBullet(this, transform.position, Vector2.left, BulletStyle.CatapultMouseBullet);
            b.SetAttribute(24.0f, true, 0.25f, transform.position, new Vector2(u.transform.position.x, transform.position.y), u.GetRowIndex());
            b.SetCanAttackFood(true);
            b.SetDamage(mBaseAttack);
            Animator ani = b.transform.Find("SpriteGo").GetComponent<Animator>();
            ani.runtimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Bullet/6/"+mShape);
            //PandaMouse m = GameController.Instance.CreateMouseUnit(GetRowIndex(), new BaseEnemyGroup.EnemyInfo() { type = 19, shape = mShape }).GetComponent<PandaMouse>();
            //m.transform.position = new Vector3(transform.position.x, u.transform.position.y, transform.position.z);
            ////m.transform.position = master.transform.position;
            //// ���л���ʱ���ж�
            //m.CloseCollision();
            ////m.PlayFlyClip();
            //// ʹ���������ƶ�״̬
            //ParabolaMoveState s = new ParabolaMoveState(m, 24.0f, 1.2f, m.transform.position, u.transform.position, false);
            //s.SetExitAction(delegate {
            //    m.OpenCollision();
            //});
            //m.SetActionState(s);
        }
        else if(targetUnit != null &&  targetUnit.IsAlive())
        {
            //BaseUnit u = targetUnit;
            float v = TransManager.TranToStandardVelocity(Mathf.Abs(targetUnit.transform.position.x - transform.position.x)/90f);
            ParabolaBullet b = (ParabolaBullet)GameController.Instance.CreateBullet(this, transform.position, Vector2.left, BulletStyle.CatapultMouseBullet);
            b.SetAttribute(v, true, 2.0f, new Vector2(transform.position.x, transform.position.y), new Vector2(targetUnit.transform.position.x, transform.position.y), targetUnit.GetRowIndex());
            b.SetCanAttackFood(true);
            b.SetDamage(mBaseAttack);
            Animator ani = b.transform.Find("SpriteGo").GetComponent<Animator>();
            ani.runtimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Bullet/6/" + mShape);
            //PandaMouse m = GameController.Instance.CreateMouseUnit(GetRowIndex(), new BaseEnemyGroup.EnemyInfo() { type = 19, shape = mShape }).GetComponent<PandaMouse>();
            //m.transform.position = new Vector3(transform.position.x, u.transform.position.y, transform.position.z);
            ////m.transform.position = master.transform.position;
            //// ���л���ʱ���ж�
            //m.CloseCollision();
            ////m.PlayFlyClip();
            //// ʹ���������ƶ�״̬
            //ParabolaMoveState s = new ParabolaMoveState(m, 24.0f, 1.2f, m.transform.position, u.transform.position, false);
            //s.SetExitAction(delegate {
            //    m.OpenCollision();
            //});
            //m.SetActionState(s);
        }
    }
}
