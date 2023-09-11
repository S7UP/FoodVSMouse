using UnityEngine;
using UnityEngine.UI;
using S7P.Numeric;
/// <summary>
/// С��¯
/// </summary>
public class SmallStove : FoodUnit
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
        if (mShape > 1)
            fireCount = 2;
        else
            fireCount = 1;
        // �����Ǽ�������µ�  �㷨Ϊ ����Ч�ʣ���������*������*44/������룩/60֡
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

    public override void OnIdleState()
    {
        // ��60֡ʱ�ظ�������*44��
        if (timer == 60)
        {
            float replyCount = Mathf.Min(1, mCurrentAttackSpeed) * mCurrentAttack / 10 * fireCount * 44;
            CreateAddFireEffect(transform.position, replyCount);
        }
        timer++;
        base.OnIdleState();
    }

    /// <summary>
    /// ����һ�����ӻ������Ч
    /// </summary>
    public static void CreateAddFireEffect(Vector2 position, float replyCount)
    {
        // ��ʾ�ظ���������Ч
        BaseEffect e = BaseEffect.GetInstance("Emp_AddFireEffect");
        GameController.Instance.AddEffect(e);
        e.transform.SetParent(GameManager.Instance.GetUICanvas().transform);
        e.transform.localScale = Vector3.one;
        e.transform.position = position;
        Text text = e.transform.Find("Text").GetComponent<Text>();
        text.text = "+" + (int)replyCount;
        text.color = new Color(1, 0.4627f, 0);
        // ʵ�ʻظ�
        GameController.Instance.AddFireResource(replyCount);
        GameManager.Instance.audioSourceController.PlayEffectMusic("Points");
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
