using UnityEngine;
using S7P.Numeric;
/// <summary>
/// �Ʊ���
/// </summary>
public class CupLight : FoodUnit
{
    private int timer;
    private FloatModifier floatModifier = new FloatModifier(0);
    private float fireValue; // ���������ֵ��34-44)
    private int growTimeLeft; // �ɳ�ʣ��ʱ��
    private int fireCount; // ������
    private float lastProductivity; // ��һ֡����Ч��

    // �����࿨Ƭ��û�й����������ڼ����ﲻ��д�����Ϣ�����޷�����
    public override void MInit()
    {
        fireCount = 0;
        timer = 0;
        lastProductivity = 0;
        fireValue = 34;
        growTimeLeft = 1;
        base.MInit();
        // ʣ��ɳ�ʱ��
        growTimeLeft = GetGrowTime(mShape, mLevel);
    }

    /// <summary>
    /// ���ݵȼ���͵ȼ������¶�Ӧ����
    /// </summary>
    public override void UpdateAttributeByLevel()
    {
        // ���ԭ���Ĵ��ڣ�Ҫ���Ƴ�ԭ����
        GameController.Instance.RemoveCostResourceModifier("Fire", floatModifier);
        if (mShape > 1)
            fireCount = 2;
        else
            fireCount = 1;
        // �����Ǽ�������µ�  �㷨Ϊ ����Ч�ʣ���������*������*fireValue/������룩/60֡
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

    public override void OnIdleStateEnter()
    {
        if(growTimeLeft > 0)
            animatorController.Play("Idle", true);
        else
            animatorController.Play("Idle2", true);
    }

    public override void OnIdleState()
    {
        // ��60֡ʱ�ظ�������*fireValue��
        if (timer == 60)
        {
            float replyCount = Mathf.Min(1, mCurrentAttackSpeed) * mCurrentAttack / 10 * fireCount * fireValue;
            SmallStove.CreateAddFireEffect(transform.position, replyCount);
        }
        timer++;
        growTimeLeft--;
        if (growTimeLeft == 0)
            SetActionState(new TransitionState(this));
        base.OnIdleState();
    }

    public override void OnTransitionStateEnter()
    {
        fireValue = 44;
        animatorController.Play("Grow");
        GameManager.Instance.audioSourceController.PlayEffectMusic("Grow");
    }

    public override void OnTransitionState()
    {
        if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
        {
            growTimeLeft = 0;
            SetActionState(new IdleState(this));
        }
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
            return Mathf.Min(1, mCurrentAttackSpeed) * mCurrentAttack/10 * (float)(fireCount * fireValue) / attr.valueList[mLevel] / 60;
    }

    /// <summary>
    /// ��ȡ�ɳ�ʱ��
    /// </summary>
    /// <param name="shape"></param>
    /// <param name="level"></param>
    /// <returns></returns>
    public static int GetGrowTime(int shape, int level)
    {
        FoodUnit.Attribute attr = GameManager.Instance.attributeManager.GetFoodUnitAttribute((int)FoodNameTypeMap.CupLight, shape);
        if (shape < 2)
            return Mathf.CeilToInt(90*attr.valueList[level]);
        else
            return Mathf.CeilToInt(15*attr.valueList[level]);
    }
}
