
using Environment;

using S7P.Numeric;

using UnityEngine;

public class WaterGridType : BaseGridType
{
    private const string TaskName = "WaterTask";

    /// <summary>
    /// Ϊĳ����λ�����ȫ����ˮӰ���Ч��
    /// </summary>
    /// <param name="unit"></param>
    public static void AddNoAffectByWater(BaseUnit unit, BoolModifier boolModifier)
    {
        unit.NumericBox.AddDecideModifierToBoolDict(WaterTask.NoDrop, boolModifier); // �����ˮ�ﲻ�½�
        unit.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreWaterGridState, boolModifier); // ����ˮʴ
    }

    /// <summary>
    /// Ϊĳ����λ�Ƴ���ȫ����ˮӰ���Ч��
    /// </summary>
    /// <param name="unit"></param>
    public static void RemoveNoAffectByWater(BaseUnit unit, BoolModifier boolModifier)
    {
        unit.NumericBox.RemoveDecideModifierToBoolDict(WaterTask.NoDrop, boolModifier); // �����ˮ�ﲻ�½�
        unit.NumericBox.RemoveDecideModifierToBoolDict(StringManager.IgnoreWaterGridState, boolModifier); // ����ˮʴ
    }

    /// <summary>
    /// �Ƿ������������
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetingEnterCondition(BaseUnit unit)
    {
        if (unit.aliveTime <= 5)
            return false;
        // BOSS��λ���ӵ���Ч��
        if (unit is MouseUnit)
        {
            MouseUnit m = unit as MouseUnit;
            if (m.IsBoss())
                return false;
        }
        return unit.GetHeight() <= 0;
    }

    /// <summary>
    /// ���е�λ�������ʱʩ�Ӹ���λ��Ч��
    /// </summary>
    /// <param name="baseUnit"></param>
    public override void OnUnitEnter(BaseUnit unit)
    {
        WaterTask t;
        if (unit.GetTask(TaskName) == null)
        {
            switch (unit.tag)
            {
                case "Food":
                    t = new WaterTask(UnitType.Food, unit); break;
                case "Mouse":
                    t = new WaterTask(UnitType.Mouse, unit); break;
                case "Character":
                    t = new WaterTask(UnitType.Character, unit); break;
                case "Item":
                    t = new WaterTask(UnitType.Item, unit); break;
                default:
                    Debug.LogWarning("ˮ���������ֵĶ���");
                    t = new WaterTask(UnitType.Food, unit); break;
            }
            unit.AddUniqueTask(TaskName, t);
        }
        else
        {
            t = unit.GetTask(TaskName) as WaterTask;
            t.AddCount();
        }
    }

    /// <summary>
    /// ���е�λ���ڵ���ʱ��������λ��Ч��
    /// </summary>
    /// <param name="unit"></param>
    public override void OnUnitStay(BaseUnit unit)
    {

    }

    /// <summary>
    /// ���е�λ�뿪����ʱʩ�Ӹ���λ��Ч��
    /// </summary>
    /// <param name="unit"></param>
    public override void OnUnitExit(BaseUnit unit)
    {
        if (unit.GetTask(TaskName) != null)
        {
            WaterTask t = unit.GetTask(TaskName) as WaterTask;
            t.DecCount();
        }
    }

    /// <summary>
    /// �ж�ĳ����λ�Ƿ���ˮ��Χ��
    /// </summary>
    /// <returns></returns>
    public static bool IsInWater(BaseUnit unit)
    {
        return unit.GetTask(TaskName) != null;
    }
}
