using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// ���¯
/// </summary>
public class BigStove : FoodUnit
{
    private int timer;
    private FloatModifier floatModifier;
    private int fireCount; // ������

    // �����࿨Ƭ��û�й����������ڼ����ﲻ��д�����Ϣ�����޷�����
    public override void MInit()
    {
        floatModifier = null;
        fireCount = 0;
        timer = 0;
        base.MInit();
    }

    /// <summary>
    /// ���ݵȼ���͵ȼ������¶�Ӧ����
    /// </summary>
    public override void UpdateAttributeByLevel()
    {
        // ���ԭ���Ĵ��ڣ�Ҫ���Ƴ�ԭ����
        if (floatModifier != null)
            GameController.Instance.RemoveCostResourceModifier("Fire", floatModifier);
        // �����Ǽ�������µ�  �㷨Ϊ ������*44/������룩/60֡
        if (mShape > 0)
            fireCount = 3;
        else
            fireCount = 2;
        floatModifier = new FloatModifier((float)(fireCount*44)/attr.valueList[mLevel]/60);
        // �ӻ�ȥ
        GameController.Instance.AddCostResourceModifier("Fire", floatModifier);
    }

    // ����ǰ�����֡��Ч��
    public override void AfterDeath()
    {
        base.AfterDeath();
        if (floatModifier != null)
            GameController.Instance.RemoveCostResourceModifier("Fire", floatModifier);
    }


    public override void MUpdate()
    {
        base.MUpdate();
        // ��60֡ʱ�ظ�������*44��
        if (timer == 60)
        {
            int replyCount = fireCount * 44;
            // ��ʾ�ظ���������Ч
            BaseEffect e = GameManager.Instance.GetGameObjectResource(FactoryType.GameFactory, "Effect/Emp_AddFireEffect").GetComponent<BaseEffect>();
            GameController.Instance.AddEffect(e);
            e.transform.SetParent(GameManager.Instance.GetUICanvas().transform);
            e.transform.localScale = Vector3.one;
            e.transform.position = transform.position;
            e.transform.Find("Img_AddFireEffect").Find("Text").GetComponent<Text>().text = "+" + replyCount;
            // ʵ�ʻظ�
            GameController.Instance.AddFireResource(replyCount);
        }
        timer++;
    }
}
