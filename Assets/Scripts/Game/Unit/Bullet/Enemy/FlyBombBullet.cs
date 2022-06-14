using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ֱ��������ը��
/// </summary>
public class FlyBombBullet : BaseBullet
{
    private float acc;
    private float targetY;
    private int currentRowIndex;
    public override void MInit()
    {
        base.MInit();
    }

    public override void MUpdate()
    {
        base.MUpdate();
        if (isDeathState)
            return;
        mVelocity += acc;
        // �����ض��߶�Ҳ���Ա�
        if (transform.position.y < targetY)
        {
            TakeDamage(null);
        }
    }

    /// <summary>
    /// ��ʼ���ٶ�
    /// </summary>
    /// <param name="v"></param>
    /// <param name="acc"></param>
    public void InitVelocity(float v, float acc, float targetY, int currentRowIndex)
    {
        SetVelocity(v);
        this.acc = acc;
        this.targetY = targetY;
        this.currentRowIndex = currentRowIndex;
    }

    /// <summary>
    /// ����Χ���AOE����Ч��
    /// </summary>
    /// <param name="baseUnit"></param>
    public override void TakeDamage(BaseUnit baseUnit)
    {
        // ԭ�ز���һ����ըЧ��
        GameObject instance = GameManager.Instance.GetGameObjectResource(FactoryType.GameFactory, "AreaEffect/BombAreaEffect");
        BombAreaEffectExecution bombEffect = instance.GetComponent<BombAreaEffectExecution>();
        bombEffect.Init(this.mMasterBaseUnit, 900, GetRowIndex(), 1, 1, 0, 0, true, false);
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

        KillThis();
    }

    /// <summary>
    /// ��ȡ��ǰ��λ�������±�
    /// </summary>
    /// <returns></returns>
    public override int GetRowIndex()
    {
        return currentRowIndex;
    }
}
