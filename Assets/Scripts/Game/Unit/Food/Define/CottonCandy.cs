using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// �޻���
/// </summary>
public class CottonCandy : FoodUnit
{
    private const string Tag = "CottonCandyVehicle";
    private BoolModifier IgnoreLavaDeBuff = new BoolModifier(true); // �����ҽ�������
    private List<BaseUnit> unitList = new List<BaseUnit>(); // ��Ӱ��ĵ�λ��
    private IntModifier VehicleModifier = new IntModifier(1); // �޻��ؾ���������

    public override void MInit()
    {
        unitList.Clear();
        base.MInit();
        // �����ҽ�����Ч��
        NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreLavaDeBuff, IgnoreLavaDeBuff);
        typeAndShapeToLayer = -1;
        // ����ҽ�������Ч
        EffectManager.AddLavaEffectToUnit(this);
    }

    /// <summary>
    /// �޻��ĽӴ��ж�����һЩ
    /// </summary>
    public override void SetCollider2DParam()
    {
        mBoxCollider2D.offset = new Vector2(0, 0);
        mBoxCollider2D.size = new Vector2(0.55f * MapManager.gridWidth, 0.55f * MapManager.gridHeight);
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
           
            // ֻ�и߶�Ϊ0�ĵ�λ�ܱ��޻�����
            if (!unitList.Contains(unit) && unit.GetHeight() == 0)
            {
                // ʹĿ����޻��ǳ�����+1
                if(!unit.NumericBox.IntDict.ContainsKey(Tag))
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
            // ʹĿ����޻��ǳ�����-1
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
        // �Ƴ���Ч
        EffectManager.RemoveLavaEffectFromUnit(this);
        // ʹ���л����ڵĵ�λִ��һ���뿪������¼�
        for (int i = 0; i < unitList.Count; i++)
        {
            OnUnitExit(unitList[0]);
        }
        base.AfterDeath();
    }

    /// <summary>
    /// Ŀ���Ƿ��޻��ǳ���
    /// </summary>
    /// <param name="unit"></param>
    /// <returns></returns>
    public static bool IsBearing(BaseUnit unit)
    {
        // �Ƿ��г���tag�ҳ������������0
        return unit.NumericBox.IntDict.ContainsKey(Tag) && unit.NumericBox.IntDict[Tag].Value > 0;
    }
}
