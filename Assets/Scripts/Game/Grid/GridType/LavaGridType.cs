using UnityEngine;
/// <summary>
/// �ҽ�����
/// </summary>
public class LavaGridType : BaseGridType
{
    private const string TaskName = "LavaTask";

    /// <summary>
    /// �Ƿ������������
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetingEnterCondition(BaseUnit unit)
    {
        // BOSS��λ���ӵ���Ч��
        if(unit is MouseUnit)
        {
            MouseUnit m = unit as MouseUnit;
            if (m.IsBoss())
                return false;
        }
        return unit.GetHeight()<=0;
    }

    /// <summary>
    /// ���е�λ�������ʱʩ�Ӹ���λ��Ч��
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
                    Debug.LogWarning("�ҽ����������ֵĶ���");
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
    /// ���е�λ���ڵ���ʱ��������λ��Ч��
    /// </summary>
    /// <param name="unit"></param>
    public override void OnUnitStay(BaseUnit unit)
    {

    }

    /// <summary>
    /// ���е�λ�뿪����ʱʩ�Ӹ���λ��Ч��
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
            Debug.LogWarning("Ϊʲô�ж�������û���ҽ�������ҽ���");
        }
    }


    /// <summary>
    /// �ҽ�BUFF����
    /// </summary>
    private class LavaTask : ITask
    {
        // ����Ч��
        private FloatModifier attackModifier0 = new FloatModifier(100);
        private FloatModifier attackSpeedModifier0 = new FloatModifier(100);
        private FloatModifier moveSpeedModifier0 = new FloatModifier(100);

        // ����Ч��
        private FloatModifier attackSpeedModifier1 = new FloatModifier(50);
        private FloatModifier moveSpeedModifier1 = new FloatModifier(100);

        // ����
        private BoolModifier IgnoreModifier = new BoolModifier(true);

        private const int interval = 60;
        private int timeLeft;
        private int timer;

        private int count; // ������ҽ���
        private bool hasVehicle; // �Ƿ��ؾ߳���
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
            if(!hasVehicle && CottonCandy.IsBearing(unit))
            {
                // ���Ŀ�������ؾߵ�״̬�±��޻����أ���ת��Ϊ��������
                ChangeToVehicleMode();
            }else if(hasVehicle && !CottonCandy.IsBearing(unit))
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
                        if (unit.GetCurrentHp() > 15)
                            new DamageAction(CombatAction.ActionType.BurnDamage, null, unit, 1.0f).ApplyAction();
                    }
                    else
                    {
                        // ���ؾ�ʱ��ÿ���ܵ��൱��X%���������ֵ������Դ�Ļҽ��˺�������X = 2 + 0.3*Ŀ���Ѵ����ҽ���ʱ�䣨�룩
                        new DamageAction(CombatAction.ActionType.BurnDamage, null, unit, 0.01f*(2 + 0.3f * timer / 60) * unit.mMaxHp).ApplyAction();
                    }
                    timeLeft = interval;
                }
                
                // �����ؾ�ʱ�������ҽ����ʱ��ᱻ����
                if(hasVehicle)
                    timer=0;
                else
                    timer++;
            }
            else
            {
                // �������ҽ�DEBUFFʱ�����ҽ����ʱ��Ҳ�ᱻ����
                timer = 0;
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
            // �ܵ�һ���൱��20%����ʧ����ֵ���˺�
            if (!unit.NumericBox.GetBoolNumericValue(StringManager.IgnoreLavaDeBuff))
            {
                new DamageAction(CombatAction.ActionType.CauseDamage, null, unit, 0.2f*unit.GetLostHp()).ApplyAction();
            }
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
    }
}
