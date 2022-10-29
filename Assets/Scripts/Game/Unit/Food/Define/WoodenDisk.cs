using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// ľ����
/// </summary>
public class WoodenDisk : FoodUnit, IInWater
{
    private const string TaskKey = "ľ�����ṩ������Ч��";
    private BoolModifier IgnoreWaterModifier = new BoolModifier(true); // ����ˮʴ������
    private List<BaseUnit> unitList = new List<BaseUnit>(); // ��Ӱ��ĵ�λ�� 

    private const string Tag = "WoodenDiskCandyVehicle";
    private BoolModifier IgnoreLavaDeBuff = new BoolModifier(true); // �����ҽ�������
    private IntModifier VehicleModifier = new IntModifier(1); // �޻��ؾ���������



    // �����±��е���������ľ�������ò���Ч
    private List<MouseNameTypeMap> noEffectMouseList = new List<MouseNameTypeMap>() 
    { 
        MouseNameTypeMap.SubmarineMouse,
        MouseNameTypeMap.RowboatMouse,
    };


    public override void MInit()
    {
        unitList.Clear();
        base.MInit();
        // ����ˮʴЧ��
        NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreWaterGridState, IgnoreWaterModifier);
        typeAndShapeToLayer = -1;
        // ���ˮ����Ч��
        EffectManager.AddWaterWaveEffectToUnit(this);
    }

    public override void UpdateAttributeByLevel()
    {
        SetMaxHpAndCurrentHp((float)(attr.baseAttrbute.baseHP + attr.valueList[mLevel]));
    }

    public override void LoadSkillAbility()
    {
        
    }

    public override void MUpdate()
    {
        // �޳��Ѿ���Ч�ĵ�λ
        List<BaseUnit> delList = new List<BaseUnit>();
        foreach (var item in unitList)
        {
            if (!item.IsValid())
                delList.Add(item);
        }
        foreach (var item in delList)
        {
            unitList.Remove(item);
        }
        base.MUpdate();
    }

    private void OnCollision(Collider2D collision)
    {
        if(collision.tag.Equals("Food") || collision.tag.Equals("Mouse") || collision.tag.Equals("Item") || collision.tag.Equals("Character"))
        {
            BaseUnit unit = collision.GetComponent<BaseUnit>();
            // ���Ŀ���Ƿ�����Ч
            if ((unit is MouseUnit && noEffectMouseList.Contains((MouseNameTypeMap)unit.mType)) || unit.GetHeight()!=0)
                return;

            if (!unitList.Contains(unit))
            {
                // ʹĿ���ľ���ӳ�����+1
                if (!unit.NumericBox.IntDict.ContainsKey(Tag))
                    unit.NumericBox.IntDict.Add(Tag, new IntNumeric());
                unit.NumericBox.IntDict[Tag].AddAddModifier(VehicleModifier);
                // ��Ŀ���������Լ����صı���
                unitList.Add(unit);
            } 
        }
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        OnCollision(collision);
    }

    public void OnTriggerStay2D(Collider2D collision)
    {
        OnCollision(collision);
    }

    private void OnUnitExit(BaseUnit unit)
    {
        if (unitList.Contains(unit))
        {
            // ʹĿ���ľ���ӳ�����-1
            if (!unit.NumericBox.IntDict.ContainsKey(Tag))
                unit.NumericBox.IntDict.Add(Tag, new IntNumeric());
            unit.NumericBox.IntDict[Tag].RemoveAddModifier(VehicleModifier);
            // ��Ŀ������Ƴ��Լ����صı���
            unitList.Remove(unit);
        }
    }

    public void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag.Equals("Food") || collision.tag.Equals("Mouse") || collision.tag.Equals("Item") || collision.tag.Equals("Character"))
        {
            BaseUnit unit = collision.GetComponent<BaseUnit>();
            OnUnitExit(unit);
        }
    }

    public override void AfterDeath()
    {
        // �Ƴ�ˮ����Ч��
        EffectManager.RemoveWaterWaveEffectFromUnit(this);

        // �Ƴ����л����ڵĵ�λ���ϵ��ɸ����Ӳ���������ˮʴ��ǩ
        for (int i = 0; i < unitList.Count; i++)
        {
            OnUnitExit(unitList[0]);
        }
        base.AfterDeath();
    }

    public void OnEnterWater()
    {
        
    }

    public void OnStayWater()
    {
        
    }

    public void OnExitWater()
    {
        
    }

    /// <summary>
    /// ��ȡĿ���������ڵ���������
    /// </summary>
    /// <param name="unit"></param>
    /// <returns></returns>
    public static ITask GetFloatTask(BaseUnit unit)
    {
        return unit.GetTask(TaskKey);
    }

    /// <summary>
    /// Ŀ���Ƿ�ľ���ӳ���
    /// </summary>
    /// <param name="unit"></param>
    /// <returns></returns>
    public static bool IsBearing(BaseUnit unit)
    {
        // �Ƿ��г���tag�ҳ������������0
        return unit.NumericBox.IntDict.ContainsKey(Tag) && unit.NumericBox.IntDict[Tag].Value > 0;
    }
}
