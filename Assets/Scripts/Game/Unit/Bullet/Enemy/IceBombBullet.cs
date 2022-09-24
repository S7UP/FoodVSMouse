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
        IceAreaEffectExecution iceEffect = IceAreaEffectExecution.GetInstance();
        iceEffect.Init(this.mMasterBaseUnit, 600, GetRowIndex(), 3, 3, -0.5f, 0, true, false); // �ڶ�������Ϊ����ʱ�䣨֡��
        iceEffect.isAffectCharacter = true; // ������Ч
        if (baseUnit != null && baseUnit.IsValid())
        {
            // �����λ���ڣ����ڵ�λλ�ñ�ը
            iceEffect.transform.position = baseUnit.transform.position;
        }
        else
        {
            // ����λ�ڸ��������ı�ը
            iceEffect.transform.position = MapManager.GetGridLocalPosition(GetColumnIndex(), GetRowIndex());
        }
        GameController.Instance.AddAreaEffectExecution(iceEffect);
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
