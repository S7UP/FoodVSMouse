using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// 冰勺弩枪
/// </summary>
public class IceSpoonCrossbowGun : BaseWeapons
{
    private BaseUnit target;
    private Vector3 targetPosition;

    public override void MInit()
    {
        base.MInit();
        //targetPosition = null;
    }

    /// <summary>
    /// 判断是否有有效的攻击目标
    /// </summary>
    /// <returns></returns>
    public override bool IsHasTarget()
    {
        // 单行索敌
        List<BaseUnit> list = new List<BaseUnit>();
        // 筛选出高度为0的可选取单位
        foreach (var item in GameController.Instance.GetSpecificRowEnemyList(GetRowIndex()))
        {
            if (item.GetHeight() == 0 && item.CanBeSelectedAsTarget())
                list.Add(item);
        }
        if (list.Count > 0)
        {
            bool flag = false;
            foreach (var item in list)
            {
                if(item.transform.position.x > master.transform.position.x)
                {
                    flag = true;
                    target = item;
                    SearchTargetPosition();
                    break;
                }
            }
            return flag;
        }
        return false;
    }

    /// <summary>
    /// 执行具体的攻击，位于伤害判定为真之后
    /// </summary>
    public override void ExecuteDamage()
    {
        //SearchTargetPosition();
        float d = Mathf.Abs(targetPosition.x - master.transform.position.x);
        
        if (d < MapManager.gridWidth)
        {
            // 距离小于一格直接脸对脸怼上去！
            float v = TransManager.TranToStandardVelocity(MapManager.gridWidth / 30f);
            IceEggBullet iceEggBullet = GameController.Instance.CreateBullet(master, master.transform.position, Vector2.right, BulletStyle.IceEgg) as IceEggBullet;
            iceEggBullet.SetAttribute(v, false, 0.2f, iceEggBullet.transform.position, targetPosition, GetRowIndex());
            iceEggBullet.SetDamage(20);
        }
        else
        {
            // 否则以90帧着陆的速度固定高度投掷
            float v = TransManager.TranToStandardVelocity(d / 90f);
            IceEggBullet iceEggBullet = GameController.Instance.CreateBullet(master, master.transform.position, Vector2.right, BulletStyle.IceEgg) as IceEggBullet;
            iceEggBullet.SetAttribute(v, false, 2.0f, iceEggBullet.transform.position, targetPosition, GetRowIndex());
            iceEggBullet.SetDamage(20);
        }
    }

    
    /// <summary>
    /// 寻找目标
    /// </summary>
    public void SearchTargetPosition()
    {
        List<BaseUnit> list = new List<BaseUnit>();
        // 筛选出高度为0的可选取单位
        foreach (var item in GameController.Instance.GetSpecificRowEnemyList(GetRowIndex()))
        {
            if (item.GetHeight() == 0 && item.CanBeSelectedAsTarget())
                list.Add(item);
        }
        if (list.Count <= 0)
            return;
        targetPosition = target.transform.position;
        foreach (var item in list)
        {
            if(item.transform.position.x < targetPosition.x && item.transform.position.x > master.transform.position.x)
            {
                targetPosition = item.transform.position;
            }
        }
    }
}
