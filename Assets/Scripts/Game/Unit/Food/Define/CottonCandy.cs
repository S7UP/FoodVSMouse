
using S7P.Numeric;

using UnityEngine;
/// <summary>
/// �޻���
/// </summary>
public class CottonCandy : FoodUnit
{
    //private const string Tag = "CottonCandyVehicle";
    private BoolModifier IgnoreLavaDeBuff = new BoolModifier(true); // �����ҽ�������
    //private List<BaseUnit> unitList = new List<BaseUnit>(); // ��Ӱ��ĵ�λ��
    //private IntModifier VehicleModifier = new IntModifier(1); // �޻��ؾ���������

    public override void MInit()
    {
        //unitList.Clear();
        base.MInit();
        // �����ҽ�����Ч��
        NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreLavaDeBuff, IgnoreLavaDeBuff);
        // ����ˤ��
        Environment.SkyManager.AddNoAffectBySky(this, new BoolModifier(true));
        typeAndShapeToLayer = -1;
        // �������ؼ������
        CreateCheckArea();
        // ���ý��ܱ����������赲�����˺�
        AddCanBlockFunc((self, other) =>
        {
            return false;
        });
        AddCanHitFunc((self, bullet) => {
            return false;
        });
        AddCanBeSelectedAsTargetFunc((self, other) => {
            return false;
        });
    }

    public override void UpdateAttributeByLevel()
    {
        SetMaxHpAndCurrentHp((float)(attr.baseAttrbute.baseHP + attr.valueList[mLevel]));
    }

    public override void LoadSkillAbility()
    {
        
    }

    //public override void MUpdate()
    //{
    //    // �޳��Ѿ���Ч�ĵ�λ
    //    List<BaseUnit> delList = new List<BaseUnit>();
    //    foreach (var item in unitList)
    //    {
    //        if (!item.IsValid())
    //            delList.Add(item);
    //    }
    //    foreach (var item in delList)
    //    {
    //        unitList.Remove(item);
    //    }
    //    base.MUpdate();
    //}

    /// <summary>
    /// �����������
    /// </summary>
    private void CreateCheckArea()
    {
        RetangleAreaEffectExecution r = Environment.SkyManager.GetVehicleArea(transform.position, new Vector2(0.75f*MapManager.gridWidth, 0.75f*MapManager.gridHeight));
        GameController.Instance.AddAreaEffectExecution(r);

        // �����߼�
        CustomizationTask t = new CustomizationTask();
        t.AddTaskFunc(delegate {
            if (IsAlive())
            {
                r.transform.position = transform.position;
                return false;
            }
            else
                return true;
        });
        r.AddTask(t);

        AddOnDestoryAction(delegate {
            r.MDestory();
        });
    }

    //private void OnCollision(BaseUnit unit)
    //{
    //    // ֻ�и߶�Ϊ0�ĵ�λ�ܱ��޻�����
    //    if (!unitList.Contains(unit) && unit.GetHeight() == 0)
    //    {
    //        // ʹĿ����޻��ǳ�����+1
    //        if (!unit.NumericBox.IntDict.ContainsKey(Tag))
    //            unit.NumericBox.IntDict.Add(Tag, new IntNumeric());
    //        unit.NumericBox.IntDict[Tag].AddAddModifier(VehicleModifier);
    //        // ��Ŀ���������Լ����صı���
    //        unitList.Add(unit);
    //    }
    //}

    //private void OnUnitExit(BaseUnit unit)
    //{
    //    if (unitList.Contains(unit))
    //    {
    //        // ʹĿ����޻��ǳ�����-1
    //        if (!unit.NumericBox.IntDict.ContainsKey(Tag))
    //            unit.NumericBox.IntDict.Add(Tag, new IntNumeric());
    //        unit.NumericBox.IntDict[Tag].RemoveAddModifier(VehicleModifier);
    //        // ��Ŀ������Ƴ��Լ����صı���
    //        unitList.Remove(unit);
    //    }
    //}

    //public override void AfterDeath()
    //{
    //    // ʹ���л����ڵĵ�λִ��һ���뿪������¼�
    //    for (int i = 0; i < unitList.Count; i++)
    //    {
    //        OnUnitExit(unitList[0]);
    //    }
    //    base.AfterDeath();
    //}
}
