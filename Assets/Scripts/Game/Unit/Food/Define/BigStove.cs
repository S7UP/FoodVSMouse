/// <summary>
/// ���¯
/// </summary>
public class BigStove : FoodUnit
{
    private int timer;
    private FloatModifier floatModifier = new FloatModifier(0);
    private int fireCount; // ������
    private float lastAttack; // ��һ֡����Ч��

    // �����࿨Ƭ��û�й����������ڼ����ﲻ��д�����Ϣ�����޷�����
    public override void MInit()
    {
        fireCount = 0;
        timer = 0;
        lastAttack = 0;
        base.MInit();
        // �������������ԣ�������������Ч�ʣ�1.0����100%������Ч��
        NumericBox.Attack.SetBase(1.0f);
    }

    /// <summary>
    /// ���ݵȼ���͵ȼ������¶�Ӧ����
    /// </summary>
    public override void UpdateAttributeByLevel()
    {
        // ���ԭ���Ĵ��ڣ�Ҫ���Ƴ�ԭ����
        GameController.Instance.RemoveCostResourceModifier("Fire", floatModifier);
        // �����Ǽ�������µ�  �㷨Ϊ ����Ч�ʣ���������*������*44/������룩/60֡
        if (mShape > 0)
            fireCount = 3;
        else
            fireCount = 2;
        floatModifier.Value = mCurrentAttack*(float)(fireCount*44)/attr.valueList[mLevel]/60;
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
        if (mCurrentAttack != lastAttack)
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
            float replyCount = mCurrentAttack * fireCount * 44;
            SmallStove.CreateAddFireEffect(transform.position, replyCount);
        }
        timer++;
        base.OnIdleState();
    }
}
