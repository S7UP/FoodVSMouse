using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// �޵е��ϰ�
/// </summary>
public class InvincibilityBarrier :��BaseBarrier 
{
    private int leftTime; // ����ʱ��

    public override void MInit()
    {
        base.MInit();
        mType = (int)ItemNameTypeMap.PigBarrier;
        // ����޵еı�ǩ
        NumericBox.AddDecideModifierToBoolDict(StringManager.Invincibility, new BoolModifier(true));
    }

    public override void OnEnable()
    {
        base.OnEnable();
        leftTime = -1;
    }

    /// <summary>
    /// ���ô��ʱ��
    /// </summary>
    public void SetLeftTime(int time)
    {
        leftTime = time;
    }

    public override void MUpdate()
    {
        base.MUpdate();
        // ʣ��ʱ�����0����ոö������һ��ʼ������ó�-1�������Զ����ʧ
        leftTime--;
        if (leftTime == 0)
        {
            DeathEvent();
        }
    }
}
