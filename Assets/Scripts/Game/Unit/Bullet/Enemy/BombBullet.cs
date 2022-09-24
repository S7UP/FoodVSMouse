using UnityEngine;

/// <summary>
/// �ݻ�ը�����������ƶ�ʽ���ӵ�
/// </summary>
public class BombBullet : ParabolaBullet
{
    /// <summary>
    /// ����Χ���AOE����Ч��
    /// </summary>
    /// <param name="baseUnit"></param>
    public override void TakeDamage(BaseUnit baseUnit)
    {
        // ԭ�ز���һ����ըЧ��
        BombAreaEffectExecution bombEffect = BombAreaEffectExecution.GetInstance();
        bombEffect.Init(this.mMasterBaseUnit, 900, GetRowIndex(), 3, 3, 0, 0, true, false);
        if (baseUnit != null && baseUnit.IsValid())
        {
            // �����λ���ڣ����ڵ�λλ�ñ�ը
            bombEffect.transform.position = baseUnit.transform.position;
        }
        else
        {
            // ����λ�ڸ��������ı�ը
            bombEffect.transform.position = MapManager.GetGridLocalPosition(GetColumnIndex(), GetRowIndex());
        }
        GameController.Instance.AddAreaEffectExecution(bombEffect);
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
