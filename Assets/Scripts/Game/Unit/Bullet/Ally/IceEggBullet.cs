using UnityEngine;
/// <summary>
/// ������
/// </summary>
public class IceEggBullet : ParabolaBullet
{
    public override void MInit()
    {
        base.MInit();
        SetCanAttackFood(false);
        SetCanAttackMouse(true);
    }

    /// <summary>
    /// ����Χ���AOE����Ч��
    /// </summary>
    /// <param name="baseUnit"></param>
    public override void TakeDamage(BaseUnit baseUnit)
    {
        // ԭ�ز���һ������Ч��
        IceSlowDownAreaEffectExecution iceSlowEffect = IceSlowDownAreaEffectExecution.GetInstance();
        iceSlowEffect.Init(this.mMasterBaseUnit, 120, GetRowIndex(), 3, 3, -0.5f, 0, false, true); // �ڶ�������Ϊ����ʱ�䣨֡��
        iceSlowEffect.SetAffectHeight(0); // ֻ�е��浥λ��Ӱ��
        if (baseUnit != null && baseUnit.IsValid())
        {
            // �����λ���ڣ����ڵ�λλ�ñ�ը�����ҵ�λ�ܵ�1.5%�������ֵ�빥�����нϴ����˺�����BOSS��Ϊ������
            float dmg = Mathf.Max(0.015f*baseUnit.mMaxHp, GetDamage());
            if (baseUnit is MouseUnit)
            {
                MouseUnit m = baseUnit as MouseUnit;
                if (m.IsBoss())
                {
                    dmg = GetDamage();
                }
            }
            new DamageAction(CombatAction.ActionType.CauseDamage, this.mMasterBaseUnit, baseUnit, dmg).ApplyAction();
            iceSlowEffect.transform.position = baseUnit.transform.position;
        }
        else
        {
            // ����λ�ڸ��������ı�ը
            iceSlowEffect.transform.position = MapManager.GetGridLocalPosition(GetColumnIndex(), GetRowIndex());
        }
        GameController.Instance.AddAreaEffectExecution(iceSlowEffect);
        //GameController.Instance.AddAreaEffectExecution(dmgEffect);
        ExecuteHitAction(baseUnit);
        KillThis();
    }

    //public override void OnTriggerEnter2D(Collider2D collision)
    //{
        
    //}

    //public override void OnTriggerStay2D(Collider2D collision)
    //{
        
    //}

    //public override void OnTriggerExit2D(Collider2D collision)
    //{

    //}
}
