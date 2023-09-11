using S7P.Numeric;

using UnityEngine;
/// <summary>
/// 岩浆任务
/// </summary>
namespace Environment
{
    class LavaTask : ITask
    {
        // 岩浆抗性属性关键字
        public const string LavaRate = "LavaRate";
        public const string OnEnterLava = "OnEnterLava";
        public const string OnStayLava = "OnStayLava";
        public const string OnExitLava = "OnExitLava";

        // 高温效果
        private FloatModifier attackModifier0 = new FloatModifier(50);
        private FloatModifier attackSpeedModifier0 = new FloatModifier(100);
        private FloatModifier moveSpeedModifier0 = new FloatModifier(150);

        // 恒温效果
        private FloatModifier attackSpeedModifier1 = new FloatModifier(50);
        private FloatModifier moveSpeedModifier1 = new FloatModifier(50);

        // 免疫
        private BoolModifier IgnoreModifier = new BoolModifier(true);

        private const int interval = 15;
        private int timeLeft;

        private int count; // 进入的岩浆数
        private bool hasVehicle; // 是否被载具承载
        private BaseUnit unit;

        public LavaTask(BaseUnit unit)
        {
            this.unit = unit;
            timeLeft = 0;
        }

        #region 继承的方法
        public void OnEnter()
        {
            unit.actionPointController.TriggerAction(OnEnterLava);

            if (!unit.IsAlive())
                return;

            count = 1;
            // 先设成有载具，然后调用切换成无载具模式，这样初始状态就是无载具状态
            hasVehicle = true;
            ChangeToNoVehicleMode();
            // 立即移除当前的控制效果
            StatusManager.RemoveAllSettleDownDebuff(unit);
            // 获得冰冻类debuff与定身类debuff免疫效果
            unit.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreFrozen, IgnoreModifier);
            unit.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreSlowDown, IgnoreModifier);
            unit.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreStun, IgnoreModifier);
        }

        public void OnUpdate()
        {
            unit.actionPointController.TriggerAction(OnStayLava);

            if (!unit.IsAlive())
                return;

            if (!hasVehicle && SkyManager.IsBearing(unit))
            {
                // 如果目标在无载具的状态下被棉花承载，则转变为恒温灼烧
                ChangeToVehicleMode();
            }
            else if (hasVehicle && !SkyManager.IsBearing(unit))
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
                    if (!hasVehicle)
                    {
                        // 无载具时，每0.25秒受到相当于2.5%的最大生命值的无来源的灰烬伤害 对人物最多会把生命值下降至100点
                        if(unit is CharacterUnit)
                        {
                            new DamageAction(CombatAction.ActionType.BurnDamage, null, unit, Mathf.Min(GetUnitLavaRate(unit) * 0.025f * unit.mMaxHp, unit.GetCurrentHp() - 10)).ApplyAction();
                        }
                        else
                            new DamageAction(CombatAction.ActionType.BurnDamage, null, unit, GetUnitLavaRate(unit) * 0.025f * unit.mMaxHp).ApplyAction();
                    }
                    timeLeft = interval;
                }
            }
        }

        public bool IsMeetingExitCondition()
{
            return count <= 0 || !unit.IsAlive();
        }

        public void OnExit()
        {
            unit.actionPointController.TriggerAction(OnExitLava);
            // 移除全部属性效果
            unit.NumericBox.Attack.RemovePctAddModifier(attackModifier0);
            unit.NumericBox.AttackSpeed.RemovePctAddModifier(attackSpeedModifier0);
            unit.NumericBox.MoveSpeed.RemovePctAddModifier(moveSpeedModifier0);
            unit.NumericBox.AttackSpeed.RemovePctAddModifier(attackSpeedModifier1);
            unit.NumericBox.MoveSpeed.RemovePctAddModifier(moveSpeedModifier1);
            // 移除冰冻类debuff与定身类debuff免疫效果
            unit.NumericBox.RemoveDecideModifierToBoolDict(StringManager.IgnoreFrozen, IgnoreModifier);
            unit.NumericBox.RemoveDecideModifierToBoolDict(StringManager.IgnoreSlowDown, IgnoreModifier);
            unit.NumericBox.RemoveDecideModifierToBoolDict(StringManager.IgnoreStun, IgnoreModifier);
            // 移除岩浆灼烧效果
            EffectManager.RemoveLavaEffectFromUnit(unit);
        }
        #endregion

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

        #region 静态的方法
        /// <summary>
        /// 获取目标的岩浆伤害倍率
        /// </summary>
        /// <param name="unit"></param>
        /// <returns></returns>
        public static float GetUnitLavaRate(BaseUnit unit)
        {
            if (unit == null || !unit.IsAlive())
                return 0;
            FloatCollectorComponent c = CollectorComponentManager.GetFloatCollectorComponent(unit.mComponentController);
            if (c.HasCollector(LavaRate))
                return c.GetCollector(LavaRate).MulValue;
            return 1;
        }

        public static void AddUnitLavaRate(BaseUnit unit, FloatModifier mod)
        {
            if (unit == null || !unit.IsAlive())
                return;
            FloatCollectorComponent c = CollectorComponentManager.GetFloatCollectorComponent(unit.mComponentController);
            if (!c.HasCollector(LavaRate))
                c.AddCollector(LavaRate, new FloatModifierCollector());
            c.GetCollector(LavaRate).AddModifier(mod);
        }

        public static void RemoveUnitLavaRate(BaseUnit unit, FloatModifier mod)
        {
            if (unit == null || !unit.IsAlive())
                return;
            FloatCollectorComponent c = CollectorComponentManager.GetFloatCollectorComponent(unit.mComponentController);
            if (!c.HasCollector(LavaRate))
                c.AddCollector(LavaRate, new FloatModifierCollector());
            c.GetCollector(LavaRate).RemoveModifier(mod);
        }

        public void ShutDown()
        {
            
        }

        public bool IsClearWhenDie()
        {
            return true;
        }
        #endregion
    }
}