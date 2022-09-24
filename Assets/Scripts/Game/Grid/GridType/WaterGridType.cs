using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterGridType : BaseGridType
{
    /// <summary>
    /// ���е�λ�������ʱʩ�Ӹ���λ��Ч��
    /// </summary>
    /// <param name="baseUnit"></param>
    public override void OnUnitEnter(BaseUnit baseUnit)
    {
        if (baseUnit.mHeight <= 0)
        {
            float descendGridCount;
            if (baseUnit is FoodUnit)
                descendGridCount = 0f;
            else if (baseUnit is MouseUnit)
                descendGridCount = 0.4f;
            else if (baseUnit is BaseItem)
                descendGridCount = 0f;
            else if (baseUnit is CharacterUnit)
                descendGridCount = 0f;
            else
                descendGridCount = 0;
            // ΪĿ���޲����ӽ�ˮ��Ψһ״̬��֮������ж��߼�ת������״̬����
            baseUnit.AddUniqueStatusAbility(StringManager.WaterGridState, new WaterStatusAbility(baseUnit, descendGridCount));
        }
    }

    /// <summary>
    /// ���е�λ���ڵ���ʱ��������λ��Ч��
    /// </summary>
    /// <param name="baseUnit"></param>
    public override void OnUnitStay(BaseUnit baseUnit)
    {

    }

    /// <summary>
    /// ���е�λ�뿪����ʱʩ�Ӹ���λ��Ч��
    /// </summary>
    /// <param name="baseUnit"></param>
    public override void OnUnitExit(BaseUnit baseUnit)
    {
        baseUnit.RemoveUniqueStatusAbility(StringManager.WaterGridState);
    }
}
