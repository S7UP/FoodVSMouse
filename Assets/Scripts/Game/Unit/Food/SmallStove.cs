using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// С��¯
/// </summary>
public class SmallStove : FoodUnit
{
    private int timer;
    private FloatModifier floatModifier;
    private int fireCount; // ������

    // �����࿨Ƭ��û�й����������ڼ����ﲻ��д�����Ϣ�����޷�����
    public override void MInit()
    {
        fireCount = 0;
        timer = 0;
        floatModifier = null;
        base.MInit();
        SetLevel(12);
    }

    /// <summary>
    /// ���ݵȼ���͵ȼ������¶�Ӧ����
    /// </summary>
    public override void UpdateAttributeByLevel()
    {
        // ���ԭ���Ĵ��ڣ�Ҫ���Ƴ�ԭ����
        if (floatModifier != null)
            GameController.Instance.RemoveCostResourceModifier("Fire", floatModifier);
        if (mShape > 1)
            fireCount = 2;
        else
            fireCount = 1;
        // �����Ǽ�������µ�  �㷨Ϊ ������*44/������룩/60֡
        floatModifier = new FloatModifier((float)(fireCount * 44) / attr.valueList[mLevel] / 60);
        // �ӻ�ȥ
        GameController.Instance.AddCostResourceModifier("Fire", floatModifier);
    }

    // ����ǰ�����֡��Ч��
    public override void BeforeDeath()
    {
        if (floatModifier != null)
            GameController.Instance.RemoveCostResourceModifier("Fire", floatModifier);
        base.BeforeDeath();
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
            e.transform.SetParent(GameController.Instance.uICanvasGo.transform);
            e.transform.localScale = Vector3.one;
            e.transform.position = transform.position;
            e.transform.Find("Img_AddFireEffect").Find("Text").GetComponent<Text>().text = "+"+ replyCount;
            // ʵ�ʻظ�
            GameController.Instance.AddFireResource(replyCount);
        }
        timer++;
    }
}
