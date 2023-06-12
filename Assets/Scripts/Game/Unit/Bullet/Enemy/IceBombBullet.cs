using Environment;
using System;
using UnityEngine;

/// <summary>
/// �������ƶ�ʽ���ӵ�
/// </summary>
public class IceBombBullet : ParabolaBullet
{
    /// <summary>
    /// ����Χ���AOE����Ч��
    /// </summary>
    /// <param name="baseUnit"></param>
    public override void TakeDamage(BaseUnit baseUnit)
    {
        // ԭ�ز���һ����ըЧ��
        Vector2 pos = Vector2.zero;
        if (baseUnit != null && baseUnit.IsValid())
        {
            // �����λ���ڣ����ڵ�λλ�ñ�ը
            pos = baseUnit.transform.position;
        }
        else
        {
            // ����λ�ڸ��������ı�ը
            pos = MapManager.GetGridLocalPosition(GetColumnIndex(), GetRowIndex());
        }
        RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(pos, 3, 3, "BothCollide");
        r.SetInstantaneous();
        r.isAffectFood = true;
        r.isAffectMouse = true;
        r.isAffectCharacter = true;
        Action<BaseUnit> action = (u) => {
            EnvironmentFacade.AddIceDebuff(u, 100);
        };
        r.SetOnEnemyEnterAction(action);
        r.SetOnFoodEnterAction(action);
        r.SetOnCharacterEnterAction(action);
        GameController.Instance.AddAreaEffectExecution(r);

        ExecuteHitAction(baseUnit);
        KillThis();
    }

    public override void OnTriggerEnter2D(Collider2D collision)
    {
        
    }

    public override void OnTriggerStay2D(Collider2D collision)
    {
        
    }

    public override void OnTriggerExit2D(Collider2D collision)
    {

    }
}
