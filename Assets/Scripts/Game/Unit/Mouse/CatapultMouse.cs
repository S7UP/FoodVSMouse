using System.Collections;
using System.Collections.Generic;
using TMPro;

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
        // 如果有可以攻击的目标，则停下来等待下一次攻击，否则前进
        if (IsHasTarget() || (targetUnit!=null && targetUnit.IsAlive()))
            SetActionState(new IdleState(this));
        else
            SetActionState(new MoveState(this));
        UpdateBlockState(); // 更新阻挡状态
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
    /// 执行具体的攻击，位于伤害判定为真之后
    /// </summary>
    public override void ExecuteDamage()
    {
        // 阻挡优先级大于远程攻击优先级
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
            //// 空中滑翔时无判定
            //m.CloseCollision();
            ////m.PlayFlyClip();
            //// 使用抛物线移动状态
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
            //// 空中滑翔时无判定
            //m.CloseCollision();
            ////m.PlayFlyClip();
            //// 使用抛物线移动状态
            //ParabolaMoveState s = new ParabolaMoveState(m, 24.0f, 1.2f, m.transform.position, u.transform.position, false);
            //s.SetExitAction(delegate {
            //    m.OpenCollision();
            //});
            //m.SetActionState(s);
        }
    }
}
