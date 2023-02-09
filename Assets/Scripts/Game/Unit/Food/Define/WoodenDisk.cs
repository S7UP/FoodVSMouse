using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// ľ����
/// </summary>
public class WoodenDisk : FoodUnit
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
        WaterGridType.AddNoAffectByWater(this, IgnoreWaterModifier);
        typeAndShapeToLayer = -1;
        // �������ؼ������
        CreateCheckArea();
        // ���ý��ܱ����������赲�����˺�
        AddCanBlockFunc((self, other) =>
        {
            if(other is MouseUnit)
            {
                if (noEffectMouseList.Contains((MouseNameTypeMap)other.mType))
                    return true;
            }
            return false;
        });
        AddCanHitFunc((self, bullet) => {
            return false;
        });
        AddCanBeSelectedAsTargetFunc((self, other) => {
            if (other is MouseUnit)
            {
                if (noEffectMouseList.Contains((MouseNameTypeMap)other.mType))
                    return true;
            }
            return false;
        });
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
        t.OnExitFunc = delegate {
            r.ExecuteRecycle();
        };
        r.AddTask(t);
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

    private void OnCollision(BaseUnit u)
    {
        // ���Ŀ���Ƿ�����Ч
        if ((u is MouseUnit && noEffectMouseList.Contains((MouseNameTypeMap)u.mType)) || u.GetHeight() != 0)
            return;

        if (!unitList.Contains(u))
        {
            // ʹĿ���ľ���ӳ�����+1
            if (!u.NumericBox.IntDict.ContainsKey(Tag))
                u.NumericBox.IntDict.Add(Tag, new IntNumeric());
            u.NumericBox.IntDict[Tag].AddAddModifier(VehicleModifier);
            // ��Ŀ���������Լ����صı���
            unitList.Add(u);
        }
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

    public override void AfterDeath()
    {
        // �Ƴ�ˮ����Ч��
        // EffectManager.RemoveWaterWaveEffectFromUnit(this);

        // �Ƴ����л����ڵĵ�λ���ϵ��ɸ����Ӳ���������ˮʴ��ǩ
        for (int i = 0; i < unitList.Count; i++)
        {
            OnUnitExit(unitList[0]);
        }
        base.AfterDeath();
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
