using S7P.Numeric;

using System;
using System.Collections.Generic;
/// <summary>
/// ��ս�������н�������Ĺ���
/// </summary>
public class MouseFactory : IGameControllerMember
{
    private static MouseFactory _Instance;
    public static MouseFactory Instance { get { if (_Instance == null) _Instance = new MouseFactory(); return _Instance; } }

    private List<Action<MouseUnit>> ProcessActionList = new List<Action<MouseUnit>>();

    private MouseFactory()
    {

    }

    public void MInit()
    {
        ProcessActionList.Clear();
    }

    public void MUpdate()
    {
        
    }

    public void MPause()
    {
        
    }

    public void MPauseUpdate()
    {
        
    }

    public void MResume()
    {
        
    }

    public void MDestory()
    {
        
    }

    public MouseUnit GetMouse(int type, int shape)
    {
        MouseUnit m = GameManager.Instance.GetGameObjectResource(FactoryType.GameFactory, "Mouse/" + type).GetComponent<MouseUnit>();
        m.mType = type;
        m.mShape = shape;
        m.MInit();
        // ���û�������
        m.SetAttribute(MouseManager.GetAttribute(type, shape));
        // ��ͬ�Ѷ��¶����ԵĲ�������
        {
            m.NumericBox.Attack.AddPctAddModifier(new FloatModifier((NumberManager.GetEnemyAttackRate()-1) * 100)); // ������
            m.NumericBox.AttackSpeed.AddPctAddModifier(new FloatModifier((NumberManager.GetEnemyAttackSpeedRate()-1) * 100)); // �����ٶ�
            m.NumericBox.MoveSpeed.AddPctAddModifier(new FloatModifier((NumberManager.GetEnemyMoveSpeedRate()-1) * 100)); // �ƶ��ٶ�
            m.NumericBox.SkillSpeed.AddModifier(new FloatModifier(NumberManager.GetEnemySkillSpeedRate())); // �����ٶ�
        }
        // ����ӹ�
        Process(m);
        return m;
    }

    private void Process(MouseUnit u)
    {
        foreach (var action in ProcessActionList)
            action(u);
    }

    public void AddProcessAction(Action<MouseUnit> action)
    {
        ProcessActionList.Add(action);
    }

    public void RemoveProcessAction(Action<MouseUnit> action)
    {
        ProcessActionList.Remove(action);
    }
}
