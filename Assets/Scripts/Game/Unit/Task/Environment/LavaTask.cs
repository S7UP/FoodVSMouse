using S7P.Numeric;
/// <summary>
/// �ҽ�����
/// </summary>
namespace Environment
{
    class LavaTask : ITask
    {
        // �ҽ��������Թؼ���
        public const string LavaRate = "LavaRate";

        // ����Ч��
        private FloatModifier attackModifier0 = new FloatModifier(50);
        private FloatModifier attackSpeedModifier0 = new FloatModifier(100);
        private FloatModifier moveSpeedModifier0 = new FloatModifier(150);

        // ����Ч��
        private FloatModifier attackSpeedModifier1 = new FloatModifier(50);
        private FloatModifier moveSpeedModifier1 = new FloatModifier(50);

        // ����
        private BoolModifier IgnoreModifier = new BoolModifier(true);

        private const int interval = 15;
        private int timeLeft;

        private int count; // ������ҽ���
        private bool hasVehicle; // �Ƿ��ؾ߳���
        private BaseUnit unit;

        public LavaTask(BaseUnit unit)
        {
            this.unit = unit;
            timeLeft = 0;
        }

        public void OnEnter()
        {
            count = 1;
            // ��������ؾߣ�Ȼ������л������ؾ�ģʽ��������ʼ״̬�������ؾ�״̬
            hasVehicle = true;
            // ChangeToNoVehicleMode();
            // �����Ƴ���ǰ�Ŀ���Ч���ͱ�������Ч��
            StatusManager.RemoveAllSettleDownDebuff(unit);
            StatusManager.RemoveAllFrozenDebuff(unit);
            // ��ñ�����debuff�붨����debuff����Ч��
            unit.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreFrozen, IgnoreModifier);
            unit.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreFrozenSlowDown, IgnoreModifier);
            unit.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreStun, IgnoreModifier);
        }

        public void OnUpdate()
        {
            if (!hasVehicle && CottonCandy.IsBearing(unit))
            {
                // ���Ŀ�������ؾߵ�״̬�±��޻����أ���ת��Ϊ��������
                ChangeToVehicleMode();
            }
            else if (hasVehicle && !CottonCandy.IsBearing(unit))
            {
                // ���Ŀ�������ؾߵ�״̬�²����κ��޻����أ���ת��Ϊ��������
                ChangeToNoVehicleMode();
            }

            // ÿ���˺�����
            if (!unit.NumericBox.GetBoolNumericValue(StringManager.IgnoreLavaDeBuff))
            {
                if (timeLeft > 0)
                    timeLeft--;
                else
                {
                    if (hasVehicle)
                    {
                        // ���ؾ�ʱ����Ŀ����������15��ʱÿ���ܵ�1������Դ�Ļҽ��˺�
                        //if (unit.GetCurrentHp() > 15)
                        //    new DamageAction(CombatAction.ActionType.BurnDamage, null, unit, GetUnitLavaRate(unit) * 1.0f).ApplyAction();
                    }
                    else
                    {
                        // ���ؾ�ʱ��ÿ0.25���ܵ��൱��5%���������ֵ������Դ�Ļҽ��˺�
                        new DamageAction(CombatAction.ActionType.BurnDamage, null, unit, GetUnitLavaRate(unit) * 0.025f * unit.mMaxHp).ApplyAction();
                    }
                    timeLeft = interval;
                }
            }
        }

        public bool IsMeetingExitCondition()
        {
            return count <= 0;
        }

        public void OnExit()
        {
            // �Ƴ�ȫ������Ч��
            unit.NumericBox.Attack.RemovePctAddModifier(attackModifier0);
            unit.NumericBox.AttackSpeed.RemovePctAddModifier(attackSpeedModifier0);
            unit.NumericBox.MoveSpeed.RemovePctAddModifier(moveSpeedModifier0);
            unit.NumericBox.AttackSpeed.RemovePctAddModifier(attackSpeedModifier1);
            unit.NumericBox.MoveSpeed.RemovePctAddModifier(moveSpeedModifier1);
            // �Ƴ�������debuff�붨����debuff����Ч��
            unit.NumericBox.RemoveDecideModifierToBoolDict(StringManager.IgnoreFrozen, IgnoreModifier);
            unit.NumericBox.RemoveDecideModifierToBoolDict(StringManager.IgnoreFrozenSlowDown, IgnoreModifier);
            unit.NumericBox.RemoveDecideModifierToBoolDict(StringManager.IgnoreStun, IgnoreModifier);
            // �Ƴ��ҽ�����Ч��
            EffectManager.RemoveLavaEffectFromUnit(unit);
        }

        // �Զ��巽��
        public void AddCount()
        {
            count++;
        }

        public void DecCount()
        {
            count--;
        }

        /// <summary>
        /// �л������ؾ�ģʽ
        /// </summary>
        private void ChangeToNoVehicleMode()
        {
            if (hasVehicle)
            {
                hasVehicle = false;
                // �Ƴ�����Ч��
                unit.NumericBox.AttackSpeed.RemovePctAddModifier(attackSpeedModifier1);
                unit.NumericBox.MoveSpeed.RemovePctAddModifier(moveSpeedModifier1);
                // ��Ӹ���Ч��
                unit.NumericBox.Attack.AddPctAddModifier(attackModifier0);
                unit.NumericBox.AttackSpeed.AddPctAddModifier(attackSpeedModifier0);
                unit.NumericBox.MoveSpeed.AddPctAddModifier(moveSpeedModifier0);
                // ����ҽ�����Ч��
                EffectManager.AddLavaEffectToUnit(unit);
            }
        }

        /// <summary>
        /// �л����ؾ�ģʽ
        /// </summary>
        private void ChangeToVehicleMode()
        {
            if (!hasVehicle)
            {
                hasVehicle = true;
                // �Ƴ�����Ч��
                unit.NumericBox.Attack.RemovePctAddModifier(attackModifier0);
                unit.NumericBox.AttackSpeed.RemovePctAddModifier(attackSpeedModifier0);
                unit.NumericBox.MoveSpeed.RemovePctAddModifier(moveSpeedModifier0);
                // ��Ӻ���Ч��
                unit.NumericBox.AttackSpeed.AddPctAddModifier(attackSpeedModifier1);
                unit.NumericBox.MoveSpeed.AddPctAddModifier(moveSpeedModifier1);
                // �Ƴ��ҽ�����Ч��
                EffectManager.RemoveLavaEffectFromUnit(unit);
            }
        }

        /// <summary>
        /// ��ȡĿ����ҽ��˺�����
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
    }
}