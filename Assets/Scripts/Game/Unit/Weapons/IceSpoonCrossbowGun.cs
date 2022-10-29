using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// ������ǹ
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
    /// �ж��Ƿ�����Ч�Ĺ���Ŀ��
    /// </summary>
    /// <returns></returns>
    public override bool IsHasTarget()
    {
        // ��������
        List<BaseUnit> list = new List<BaseUnit>();
        // ɸѡ���߶�Ϊ0�Ŀ�ѡȡ��λ
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
    /// ִ�о���Ĺ�����λ���˺��ж�Ϊ��֮��
    /// </summary>
    public override void ExecuteDamage()
    {
        //SearchTargetPosition();
        float d = Mathf.Abs(targetPosition.x - master.transform.position.x);
        
        if (d < MapManager.gridWidth)
        {
            // ����С��һ��ֱ�����������ȥ��
            float v = TransManager.TranToStandardVelocity(MapManager.gridWidth / 30f);
            IceEggBullet iceEggBullet = GameController.Instance.CreateBullet(master, master.transform.position, Vector2.right, BulletStyle.IceEgg) as IceEggBullet;
            iceEggBullet.SetAttribute(v, false, 0.2f, iceEggBullet.transform.position, targetPosition, GetRowIndex());
            iceEggBullet.SetDamage(20);
        }
        else
        {
            // ������90֡��½���ٶȹ̶��߶�Ͷ��
            float v = TransManager.TranToStandardVelocity(d / 90f);
            IceEggBullet iceEggBullet = GameController.Instance.CreateBullet(master, master.transform.position, Vector2.right, BulletStyle.IceEgg) as IceEggBullet;
            iceEggBullet.SetAttribute(v, false, 2.0f, iceEggBullet.transform.position, targetPosition, GetRowIndex());
            iceEggBullet.SetDamage(20);
        }
    }

    
    /// <summary>
    /// Ѱ��Ŀ��
    /// </summary>
    public void SearchTargetPosition()
    {
        List<BaseUnit> list = new List<BaseUnit>();
        // ɸѡ���߶�Ϊ0�Ŀ�ѡȡ��λ
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
