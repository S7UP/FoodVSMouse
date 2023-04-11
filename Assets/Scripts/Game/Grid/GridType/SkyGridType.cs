using UnityEngine;
using S7P.Numeric;
/// <summary>
/// �߿յؿ�
/// </summary>
public class SkyGridType : BaseGridType
{
    private const string TaskName = "SkyTask";
    public const string NoAffect = "NoAffectSky";

    /// <summary>
    /// Ϊĳ����λ�����ȫ�������Ӱ���Ч��
    /// </summary>
    /// <param name="unit"></param>
    public static void AddNoAffectBySky(BaseUnit unit, BoolModifier boolModifier)
    {
        unit.NumericBox.AddDecideModifierToBoolDict(NoAffect, boolModifier); 
        unit.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreDropFromSky, boolModifier); 
    }

    /// <summary>
    /// Ϊĳ����λ�Ƴ���ȫ�������Ӱ���Ч��
    /// </summary>
    /// <param name="unit"></param>
    public static void RemoveNoAffectBySky(BaseUnit unit, BoolModifier boolModifier)
    {
        unit.NumericBox.RemoveDecideModifierToBoolDict(NoAffect, boolModifier); 
        unit.NumericBox.RemoveDecideModifierToBoolDict(StringManager.IgnoreDropFromSky, boolModifier); 
    }

    public static bool IsIgnoreDrop(BaseUnit unit)
    {
        return unit.NumericBox.GetBoolNumericValue(StringManager.IgnoreDropFromSky);
    }

    public override void OnCollision(Collider2D collision)
    {
        if (collision.tag.Equals("Barrier"))
        {
            BaseUnit u = collision.GetComponent<BaseUnit>();
            if (!unitList.Contains(u) && IsMeetingEnterCondition(u))
            {
                unitList.Add(u);
                OnUnitEnter(u);
            }
        }else
            base.OnCollision(collision);
    }

    public override void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag.Equals("Barrier"))
        {
            BaseUnit u = collision.GetComponent<BaseUnit>();
            if (unitList.Contains(u))
            {
                unitList.Remove(u);
                OnUnitExit(u);
            }
        } else
            base.OnTriggerExit2D(collision);
    }

    /// <summary>
    /// �Ƿ������������
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetingEnterCondition(BaseUnit unit)
    {
        // BOSS��λ���ӵ���Ч��
        if(unit is MouseUnit)
        {
            MouseUnit m = unit as MouseUnit;
            if (m.IsBoss() || !MouseManager.IsGeneralMouse(unit))
                return false;
        }

        // ���ﵥλҲ���ܸ�Ӱ��
        if(unit is CharacterUnit)
        {
            return false;
        }

        // �վ��͵���Ҳ����Ӱ��
        return unit.GetHeight()==0;
    }

    /// <summary>
    /// ���е�λ�������ʱʩ�Ӹ���λ��Ч��
    /// </summary>
    /// <param name="unit"></param>
    public override void OnUnitEnter(BaseUnit unit)
    {
        SkyTask t;
        if (unit.GetTask(TaskName) == null)
        {
            t = new SkyTask(unit);
            unit.AddUniqueTask(TaskName, t);
        }
        else
        {
            t = unit.GetTask(TaskName) as SkyTask;
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
            SkyTask t = unit.GetTask(TaskName) as SkyTask;
            t.DecCount();
        }
        else
        {
            Debug.LogWarning("Ϊʲô�ж�������û���߿�������߿գ�");
        }
    }


    /// <summary>
    /// �߿�����
    /// </summary>
    private class SkyTask : ITask
    {
        private int count; // ����ĸ߿ո�����
        private BaseUnit unit;

        public SkyTask(BaseUnit unit)
        {
            this.unit = unit;
        }

        public void OnEnter()
        {
            count = 1;
        }

        public void OnUpdate()
        {
            // ���Ŀ�� û�б����� �� ������ˤ�� ��ôֱ��ˤ����
            if((!unit.NumericBox.IntDict.ContainsKey(StringManager.BearInSky) || unit.NumericBox.IntDict[StringManager.BearInSky].Value <= 0)
                && !IsIgnoreDrop(unit) && !CottonCandy.IsBearing(unit) && !UnitManager.IsFlying(unit))
            {
                unit.ExecuteDrop();
            }
        }

        public bool IsMeetingExitCondition()
        {
            return count <= 0;
        }

        public void OnExit()
        {

        }

        // �Զ��巽��
        public void AddCount()
        {
            count++;
        }

        public void DecCount()
        {
            count--;
        }
    }
}
