using UnityEngine;
/// <summary>
/// 岩浆地形
/// </summary>
public class LavaGridType : BaseGridType
{
    private const string TaskName = "LavaTask";

    /// <summary>
    /// 是否满足进入条件
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetingEnterCondition(BaseUnit unit)
    {
        // BOSS单位无视地形效果
        if(unit is MouseUnit)
        {
            MouseUnit m = unit as MouseUnit;
            if (m.IsBoss())
                return false;
        }
        return unit.GetHeight()<=0;
    }

    /// <summary>
    /// 当有单位进入地形时施加给单位的效果
    /// </summary>
    /// <param name="unit"></param>
    public override void OnUnitEnter(BaseUnit unit)
    {
        LavaTask t;
        if (unit.GetTask(TaskName) == null)
        {
            switch (unit.tag)
            {
                case "Food":
                    t = new LavaTask(UnitType.Food, unit); break;
                case "Mouse":
                    t = new LavaTask(UnitType.Mouse, unit); break;
                case "Character":
                    t = new LavaTask(UnitType.Character, unit); break;
                default:
                    Debug.LogWarning("岩浆里进入了奇怪的东西");
                    t = new LavaTask(UnitType.Food, unit); break;
            }
            unit.AddUniqueTask(TaskName, t);
        }
        else
        {
            t = unit.GetTask(TaskName) as LavaTask;
            t.AddCount();
        }
    }

    /// <summary>
    /// 当有单位处于地形时持续给单位的效果
    /// </summary>
    /// <param name="unit"></param>
    public override void OnUnitStay(BaseUnit unit)
    {

    }

    /// <summary>
    /// 当有单位离开地形时施加给单位的效果
    /// </summary>
    /// <param name="unit"></param>
    public override void OnUnitExit(BaseUnit unit)
    {
        if (unit.GetTask(TaskName) != null)
        {
            LavaTask t = unit.GetTask(TaskName) as LavaTask;
            t.DecCount();
        }
        else
        {
            Debug.LogWarning("为什么有东西可以没带岩浆任务出岩浆？");
        }
    }


    /// <summary>
    /// 岩浆BUFF任务
    /// </summary>
    private class LavaTask : ITask
    {
        // 高温效果
        private FloatModifier attackModifier0 = new FloatModifier(100);
        private FloatModifier attackSpeedModifier0 = new FloatModifier(100);
        private FloatModifier moveSpeedModifier0 = new FloatModifier(100);

        // 恒温效果
        private FloatModifier attackSpeedModifier1 = new FloatModifier(50);
        private FloatModifier moveSpeedModifier1 = new FloatModifier(100);

        // 免疫
        private BoolModifier IgnoreModifier = new BoolModifier(true);

        private const int interval = 60;
        private int timeLeft;
        private int timer;

        private int count; // 进入的岩浆数
        private bool hasVehicle; // 是否被载具承载
        private BaseUnit unit;
        private UnitType type;

        public LavaTask(UnitType type, BaseUnit unit)
        {
            this.type = type;
            this.unit = unit;
            timeLeft = 0;
            timer = 0;
        }

        public void OnEnter()
        {
            count = 1;
            // 先设成有载具，然后调用切换成无载具模式，这样初始状态就是无载具状态
            hasVehicle = true;
            // ChangeToNoVehicleMode();
            // 立即移除当前的控制效果和冰冻减速效果
            StatusManager.RemoveAllSettleDownDebuff(unit);
            StatusManager.RemoveAllFrozenDebuff(unit);
            // 获得冰冻类debuff与定身类debuff免疫效果
            unit.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreFrozen, IgnoreModifier);
            unit.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreFrozenSlowDown, IgnoreModifier);
            unit.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreStun, IgnoreModifier);
        }

        public void OnUpdate()
        {
            if(!hasVehicle && CottonCandy.IsBearing(unit))
            {
                // 如果目标在无载具的状态下被棉花承载，则转变为恒温灼烧
                ChangeToVehicleMode();
            }else if(hasVehicle && !CottonCandy.IsBearing(unit))
            {
                // 如果目标在有载具的状态下不被任何棉花承载，则转变为高温灼烧
                ChangeToNoVehicleMode();
            }

            // 每秒伤害结算
            if (!unit.NumericBox.GetBoolNumericValue(StringManager.IgnoreLavaDeBuff))
            {
                if (timeLeft > 0)
                    timeLeft--;
                else
                {
                    if (hasVehicle)
                    {
                        // 有载具时，当目标生命高于15点时每秒受到1点无来源的灰烬伤害
                        if (unit.GetCurrentHp() > 15)
                            new DamageAction(CombatAction.ActionType.BurnDamage, null, unit, 1.0f).ApplyAction();
                    }
                    else
                    {
                        // 无载具时，每秒受到相当于X%的最大生命值的无来源的灰烬伤害，其中X = 2 + 0.3*目标已待在岩浆的时间（秒）
                        new DamageAction(CombatAction.ActionType.BurnDamage, null, unit, 0.01f*(2 + 0.3f * timer / 60) * unit.mMaxHp).ApplyAction();
                    }
                    timeLeft = interval;
                }
                
                // 当有载具时，待在岩浆里的时间会被重置
                if(hasVehicle)
                    timer=0;
                else
                    timer++;
            }
            else
            {
                // 当免疫岩浆DEBUFF时待在岩浆里的时间也会被重置
                timer = 0;
            }
        }

        public bool IsMeetingExitCondition()
        {
            return count <= 0;
        }

        public void OnExit()
        {
            // 移除全部属性效果
            unit.NumericBox.Attack.RemovePctAddModifier(attackModifier0);
            unit.NumericBox.AttackSpeed.RemovePctAddModifier(attackSpeedModifier0);
            unit.NumericBox.MoveSpeed.RemovePctAddModifier(moveSpeedModifier0);
            unit.NumericBox.AttackSpeed.RemovePctAddModifier(attackSpeedModifier1);
            unit.NumericBox.MoveSpeed.RemovePctAddModifier(moveSpeedModifier1);
            // 移除冰冻类debuff与定身类debuff免疫效果
            unit.NumericBox.RemoveDecideModifierToBoolDict(StringManager.IgnoreFrozen, IgnoreModifier);
            unit.NumericBox.RemoveDecideModifierToBoolDict(StringManager.IgnoreFrozenSlowDown, IgnoreModifier);
            unit.NumericBox.RemoveDecideModifierToBoolDict(StringManager.IgnoreStun, IgnoreModifier);
            // 移除岩浆灼烧效果
            EffectManager.RemoveLavaEffectFromUnit(unit);
            // 受到一次相当于20%已损失生命值的伤害
            if (!unit.NumericBox.GetBoolNumericValue(StringManager.IgnoreLavaDeBuff))
            {
                new DamageAction(CombatAction.ActionType.CauseDamage, null, unit, 0.2f*unit.GetLostHp()).ApplyAction();
            }
        }

        // 自定义方法
        public void AddCount()
        {
            count++;
        }

        public void DecCount()
        {
            count--;
        }

        /// <summary>
        /// 切换成无载具模式
        /// </summary>
        private void ChangeToNoVehicleMode()
        {
            if (hasVehicle)
            {
                hasVehicle = false;
                // 移除恒温效果
                unit.NumericBox.AttackSpeed.RemovePctAddModifier(attackSpeedModifier1);
                unit.NumericBox.MoveSpeed.RemovePctAddModifier(moveSpeedModifier1);
                // 添加高温效果
                unit.NumericBox.Attack.AddPctAddModifier(attackModifier0);
                unit.NumericBox.AttackSpeed.AddPctAddModifier(attackSpeedModifier0);
                unit.NumericBox.MoveSpeed.AddPctAddModifier(moveSpeedModifier0);
                // 添加岩浆灼烧效果
                EffectManager.AddLavaEffectToUnit(unit);
            }
        }

        /// <summary>
        /// 切换成载具模式
        /// </summary>
        private void ChangeToVehicleMode()
        {
            if (!hasVehicle)
            {
                hasVehicle = true;
                // 移除高温效果
                unit.NumericBox.Attack.RemovePctAddModifier(attackModifier0);
                unit.NumericBox.AttackSpeed.RemovePctAddModifier(attackSpeedModifier0);
                unit.NumericBox.MoveSpeed.RemovePctAddModifier(moveSpeedModifier0);
                // 添加恒温效果
                unit.NumericBox.AttackSpeed.AddPctAddModifier(attackSpeedModifier1);
                unit.NumericBox.MoveSpeed.AddPctAddModifier(moveSpeedModifier1);
                // 移除岩浆灼烧效果
                EffectManager.RemoveLavaEffectFromUnit(unit);
            }
        }
    }
}
