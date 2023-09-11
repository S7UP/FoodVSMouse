using S7P.Numeric;

using System;
using System.Collections.Generic;
/// <summary>
/// 在战斗场景中建造老鼠的工厂
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
        // 设置基础属性
        m.SetAttribute(MouseManager.GetAttribute(type, shape));
        // 不同难度下对属性的补正处理
        {
            m.NumericBox.Attack.AddPctAddModifier(new FloatModifier((NumberManager.GetEnemyAttackRate()-1) * 100)); // 攻击力
            m.NumericBox.AttackSpeed.AddPctAddModifier(new FloatModifier((NumberManager.GetEnemyAttackSpeedRate()-1) * 100)); // 攻击速度
            m.NumericBox.MoveSpeed.AddPctAddModifier(new FloatModifier((NumberManager.GetEnemyMoveSpeedRate()-1) * 100)); // 移动速度
            m.NumericBox.SkillSpeed.AddModifier(new FloatModifier(NumberManager.GetEnemySkillSpeedRate())); // 技能速度
        }
        // 特殊加工
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
