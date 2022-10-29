using UnityEngine;
using System;
/// <summary>
/// 跟踪型子弹
/// </summary>
public class TrackingBullets : BaseBullet
{
    private BaseUnit target; // 索敌目标
    private bool isSearchEnemy; // 是否以敌人单位作为索敌目标
    private bool isSearchAlly; // 是否以友方单位作用索敌目标
    private float currentRotate; // 当前朝向角度（角度制）
    private Func<BaseUnit, BaseUnit, bool> CompareFunc; // 比较的方法，第一个参数为当前target，第二个参数为新传入参与比较的target

    ////////////////////////////////////////////////////////////////以下为供外界调用方法/////////////////////////////////////////////
    
    /// <summary>
    /// 设置是否以敌人单位作为索敌目标
    /// </summary>
    public void SetSearchEnemyEnable(bool enable)
    {
        isSearchEnemy = enable;
    }

    /// <summary>
    /// 设置是否以友方单位作为索敌目标
    /// </summary>
    public void SetSearchAllyEnable(bool enable)
    {
        isSearchAlly = enable;
    }

    /// <summary>
    /// 设置索敌时的比较方法
    /// </summary>
    /// <param name="CompareFunc">第一个参数为当前target，第二个参数为新传入参与比较的target</param>
    public void SetCompareFunc(Func<BaseUnit, BaseUnit, bool> CompareFunc)
    {
        this.CompareFunc = CompareFunc;
    }

    ////////////////////////////////////////////////////////////////以下为重写方法/////////////////////////////////////////////

    /// <summary>
    /// 初始化
    /// </summary>
    public override void MInit()
    {
        base.MInit();
        target = null;
        isSearchEnemy = false;
        isSearchAlly = false;
        currentRotate = 0;
        CompareFunc = null;
    }

    /// <summary>
    /// 飞行途中不断执行索敌
    /// </summary>
    public override void OnFlyState()
    {
        CheckTargetValid();
        SearchTarget();
        if (target != null)
        {
            mRotate = (target.transform.position - transform.position).normalized;
        }
        base.OnFlyState();
    }

    /// <summary>
    /// 只允许与目标产生碰撞
    /// </summary>
    /// <param name="unit"></param>
    /// <returns></returns>
    public override bool CanHit(BaseUnit unit)
    {
        return base.CanHit(unit) && (target == null || target == unit);
    }

    /// <summary>
    /// 改变方向
    /// </summary>
    /// <param name="v"></param>
    public override void SetRotate(Vector2 v)
    {
        base.SetRotate(v);
        currentRotate = 180*Mathf.Acos(mRotate.x)/Mathf.PI;
    }


    ////////////////////////////////////////////////////////////////以下为私有方法/////////////////////////////////////////////


    private bool UnitCanHit(BaseUnit u)
    {
        // 只要有一个不满足条件就返回false
        foreach (var func in u.CanHitFuncList)
        {
            if (!func(u, this))
                return false;
        }
        return true;
    }

    /// <summary>
    /// 索敌方法
    /// </summary>
    private void SearchTarget()
    {
        if (CompareFunc == null || target!=null)
            return;

        if(isSearchEnemy)
            foreach (var item in GameController.Instance.GetEachEnemy())
            {
                if(UnitCanHit(item) && CompareFunc(target, item))
                {
                    target = item;
                }
            }

        if (isSearchAlly)
            foreach (var item in GameController.Instance.GetEachEnemy())
            {
                if (UnitCanHit(item) && CompareFunc(target, item))
                {
                    target = item;
                }
            }
    }

    /// <summary>
    /// 检测target是否有效
    /// </summary>
    private void CheckTargetValid()
{
        if(target == null || !target.IsAlive() || !target.CanBeSelectedAsTarget() || target.GetHeight() != mHeight || !UnitCanHit(target))
        {
            target = null;
        }
    }

}
