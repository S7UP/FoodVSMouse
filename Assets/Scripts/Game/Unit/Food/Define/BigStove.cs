using S7P.Numeric;

using UnityEngine;
/// <summary>
/// ���¯
/// </summary>
public class BigStove : FoodUnit
{
    private int timer;
    private FloatModifier floatModifier = new FloatModifier(0);
    private int fireCount; // ������
    private float lastProductivity; // ��һ֡����Ч��

    // �����࿨Ƭ��û�й����������ڼ����ﲻ��д�����Ϣ�����޷�����
    public override void MInit()
    {
        fireCount = 0;
        timer = 0;
        lastProductivity = 0;
        base.MInit();
    }

    /// <summary>
    /// ���ݵȼ���͵ȼ������¶�Ӧ����
    /// </summary>
    public override void UpdateAttributeByLevel()
    {
        // ���ԭ���Ĵ��ڣ�Ҫ���Ƴ�ԭ����
        GameController.Instance.RemoveCostResourceModifier("Fire", floatModifier);
        // �����Ǽ�������µ�  �㷨Ϊ ����Ч�ʣ���������*������*44/������룩/60֡
        switch (mShape)
        {
            case 0:
                fireCount = 2;
                break;
            case 1:
                fireCount = 3;
                break;
            default:
                fireCount = 4;
                break;
        }
        floatModifier.Value = GetCurrentProductivity();
        lastProductivity = floatModifier.Value;
        // �ӻ�ȥ
        GameController.Instance.AddCostResourceModifier("Fire", floatModifier);
    }

    // ����ǰ�����֡��Ч��
    public override void BeforeDeath()
    {
        GameController.Instance.RemoveCostResourceModifier("Fire", floatModifier);
        base.BeforeDeath();
    }

    /// <summary>
    /// ���Ƴ�һ�Σ��Է�ֹǿ���Ƴ�Ч����������������ж�bug
    /// </summary>
    public override void AfterDeath()
    {
        base.AfterDeath();
        GameController.Instance.RemoveCostResourceModifier("Fire", floatModifier);
    }


    public override void MUpdate()
    {
        if (GetCurrentProductivity() != lastProductivity)
        {
            UpdateAttributeByLevel();
        }
        base.MUpdate();
    }

    public override void OnIdleState()
    {
        // ��60֡ʱ�ظ�������*44��
        if (timer == 60)
        {
            float replyCount = Mathf.Min(1, mCurrentAttackSpeed) * mCurrentAttack / 10 * fireCount * 44;
            SmallStove.CreateAddFireEffect(transform.position, replyCount);
        }
        timer++;
        base.OnIdleState();
    }

    /// <summary>
    /// ��ȡ��ǰ��������ÿ֡�ظ�����
    /// </summary>
    /// <returns></returns>
    public float GetCurrentProductivity()
    {
        if (isFrozenState)
            return 0;
        else
            return Mathf.Min(1, mCurrentAttackSpeed) * mCurrentAttack / 10 * (float)(fireCount * 44) / attr.valueList[mLevel] / 60;
    }
}
