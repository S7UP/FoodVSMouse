using System.Collections.Generic;

using S7P.Numeric;

using UnityEngine;
/// <summary>
/// ľ����
/// </summary>
public class WoodenDisk : FoodUnit
{
    private BoolModifier IgnoreWaterModifier = new BoolModifier(true); // ����ˮʴ������

    // �����±��е���������ľ�������ò���Ч
    private List<MouseNameTypeMap> noEffectMouseList = new List<MouseNameTypeMap>() 
    { 
        MouseNameTypeMap.SubmarineMouse,
        MouseNameTypeMap.RowboatMouse,
    };


    public override void MInit()
    {
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
        RetangleAreaEffectExecution r = Environment.WaterManager.GetVehicleArea(transform.position, new Vector2(MapManager.gridWidth, MapManager.gridHeight), new S7P.Numeric.FloatModifier(0.2f * MapManager.gridHeight));
        // �������һ�����˽����������ľ���Ӳ��ܳ����ض�����
        r.AddEnemyEnterConditionFunc((u) => {
            return !noEffectMouseList.Contains((MouseNameTypeMap)u.mType);
        });
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

    public override void UpdateAttributeByLevel()
    {
        SetMaxHpAndCurrentHp((float)(attr.baseAttrbute.baseHP + attr.valueList[mLevel]));
    }

    public override void LoadSkillAbility()
    {
        
    }
}
