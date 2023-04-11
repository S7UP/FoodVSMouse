using System.Collections.Generic;

using S7P.Numeric;

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
        // ����ˤ��
        SkyGridType.AddNoAffectBySky(this, new BoolModifier(true));
        typeAndShapeToLayer = -1;
        // ����ҽ�������Ч
        // EffectManager.AddLavaEffectToUnit(this);
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

    //private void OnUnitEnter(BaseUnit unit)
    //{
    //    // ΪĿ�����һ��<�߿ճ���>��ǩ
    //    if (!unit.NumericBox.IntDict.ContainsKey(StringManager.BearInSky))
    //        unit.NumericBox.IntDict.Add(StringManager.BearInSky, new IntNumeric());
    //    unit.NumericBox.IntDict[StringManager.BearInSky].AddAddModifier(BearInSkyModifier);
    //    // ����������ֱ�����أ����ѣ�
    //    if (unitList.Count >= maxBearCount)
    //    {
    //        isBreak = true;
    //        recoverTimeLeft = 60 * 24;
    //        Hide();
    //    }
    //}

    //private void OnUnitExit(BaseUnit unit)
    //{
    //    // �Ƴ�Ŀ��һ��<�߿ճ���>��ǩ
    //    if (unit.NumericBox.IntDict.ContainsKey(StringManager.BearInSky))
    //        unit.NumericBox.IntDict[StringManager.BearInSky].RemoveAddModifier(BearInSkyModifier);
    //}

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

    /// <summary>
    /// �����������
    /// </summary>
    private void CreateCheckArea()
    {
        RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(transform.position, 1, 1, "BothCollide");
        r.isAffectFood = true;
        r.isAffectMouse = true;
        r.isAffectCharacter = true;
        r.SetOnFoodEnterAction(OnCollision);
        r.SetOnEnemyEnterAction(OnCollision);
        r.SetOnCharacterEnterAction(OnCollision);
        r.SetOnFoodStayAction(OnCollision);
        r.SetOnEnemyStayAction(OnCollision);
        r.SetOnCharacterStayAction(OnCollision);
        r.SetOnFoodExitAction(OnUnitExit);
        r.SetOnEnemyExitAction(OnUnitExit);
        r.SetOnCharacterExitAction(OnUnitExit);
        GameController.Instance.AddAreaEffectExecution(r);

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
        t.AddOnExitAction(delegate {
            r.ExecuteRecycle();
        });
        r.AddTask(t);
    }

    private void OnCollision(BaseUnit unit)
    {
        // ֻ�и߶�Ϊ0�ĵ�λ�ܱ��޻�����
        if (!unitList.Contains(unit) && unit.GetHeight() == 0)
        {
            // ʹĿ����޻��ǳ�����+1
            if (!unit.NumericBox.IntDict.ContainsKey(Tag))
                unit.NumericBox.IntDict.Add(Tag, new IntNumeric());
            unit.NumericBox.IntDict[Tag].AddAddModifier(VehicleModifier);
            // ��Ŀ���������Լ����صı���
            unitList.Add(unit);
        }
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

    public override void AfterDeath()
    {
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
