using System;

using UnityEngine;

public class WaterGridType : BaseGridType
{
    private const string TaskName = "WaterTask";
    public const string NoDrop = "��ˮ�ﲻ�½�"; // �д˱�ǵĵ�λ��ˮ�ﲻ�����������½��Ķ�������

    /// <summary>
    /// �Ƿ������������
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetingEnterCondition(BaseUnit unit)
    {
        // BOSS��λ���ӵ���Ч��
        if (unit is MouseUnit)
        {
            MouseUnit m = unit as MouseUnit;
            if (m.IsBoss())
                return false;
        }
        return unit.GetHeight() <= 0;
    }

    /// <summary>
    /// ���е�λ�������ʱʩ�Ӹ���λ��Ч��
    /// </summary>
    /// <param name="baseUnit"></param>
    public override void OnUnitEnter(BaseUnit unit)
    {
        WaterTask t;
        if (unit.GetTask(TaskName) == null)
        {
            switch (unit.tag)
            {
                case "Food":
                    t = new WaterTask(UnitType.Food, unit); break;
                case "Mouse":
                    t = new WaterTask(UnitType.Mouse, unit); break;
                case "Character":
                    t = new WaterTask(UnitType.Character, unit); break;
                case "Item":
                    t = new WaterTask(UnitType.Item, unit); break;
                default:
                    Debug.LogWarning("ˮ���������ֵĶ���");
                    t = new WaterTask(UnitType.Food, unit); break;
            }
            unit.AddUniqueTask(TaskName, t);
        }
        else
        {
            t = unit.GetTask(TaskName) as WaterTask;
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
            WaterTask t = unit.GetTask(TaskName) as WaterTask;
            t.DecCount();
        }
        else
        {
            Debug.LogWarning("Ϊʲô�ж�������û��ˮ�������ˮ��");
        }
    }

    /// <summary>
    /// �ж�ĳ����λ�Ƿ���ˮ��Χ��
    /// </summary>
    /// <returns></returns>
    public static bool IsInWater(BaseUnit unit)
    {
        return unit.GetTask(TaskName) != null;
    }

    /// <summary>
    /// ˮ��BUFF����
    /// </summary>
    private class WaterTask : ITask
    {
        private FloatModifier slowDownFloatModifier = new FloatModifier(-40); // ��ǰ�ṩ����Ч����������
        private FloatModifier decAttackSpeedModifier = new FloatModifier(-50); // ������Ч��������
        // ��ͼ�仯ϵ��
        private FloatModifier EnterWaterSpriteOffsetY = new FloatModifier(0); // ��ˮʱ����ͼY��ƫ����
        private float offsetY;
        private float offsetYEnd;
        private float cutRate;
        private float cutRateEnd;
        private int currentTime = 0;
        private int totalTime = 30;
        private Action<float> SetCutRateFunc;
        private float currentCutRate;
        // private float descendGridCount; // �½�����
        private float minPos; // �½���͸߶�
        private float maxPos; // ������߸߶�
        private float sprite_pivot_y;
        private float sprite_rect_height;
        // ÿ���˺�ϵ��
        private const int TotalTime = 60;
        private int triggerDamageTimeLeft; // �����˺�ʣ��ʱ�� ��Ϊ-1����Զ�������˺�

        private int count; // �����ˮ����
        private bool hasVehicle; // �Ƿ��ؾ߳���
        private BaseUnit unit;
        private bool isDieInWater;
        //private UnitType type;

        public WaterTask(UnitType type, BaseUnit unit)
        {
            //this.type = type;
            this.unit = unit;
            // �޸���ͼY����ü��ٷֱȵķ���
            SetCutRateFunc = (f) => {
                if (unit.GetSpriteRendererList() == null)
                {
                    unit.GetSpriteRenderer().material.SetFloat("_CutRateY", f);
                }
                else
                {
                    foreach (var item in unit.GetSpriteRendererList())
                    {
                        item.material.SetFloat("_CutRateY", f);
                    }
                }
                currentCutRate = f;
            };
            // Ŀ���������³��ĸ߶�
            switch (type)
            {
                case UnitType.Food:
                    minPos = 0f;
                    maxPos = 0.2f* MapManager.gridWidth;
                    break;
                case UnitType.Mouse:
                    minPos = -0.4f*MapManager.gridWidth;
                    maxPos = 0.2f * MapManager.gridWidth;
                    break;
                case UnitType.Item:
                    minPos = 0f;
                    maxPos = 0.2f * MapManager.gridWidth;
                    break;
                case UnitType.Character:
                    minPos = 0f;
                    maxPos = 0.2f * MapManager.gridWidth;
                    break;
                default:
                    break;
            }
            Initial();

            Sprite sprite;
            if (unit.GetSpriteList() == null)
                sprite = unit.GetSpirte();
            else
                sprite = unit.GetSpriteList()[0];
            if (sprite != null)
            {
                sprite_pivot_y = sprite.pivot.y;
                sprite_rect_height = sprite.rect.height;
            }
        }

        private void Initial()
        {
            offsetY = 0;
            offsetYEnd = 0;
            cutRate = 0;
            cutRateEnd = 0;
            currentTime = 0;
            totalTime = 30;
            SetCutRateFunc(cutRate);
        }

        public void OnEnter()
        {
            count = 1;
            // ��������ؾߣ�Ȼ������л������ؾ�ģʽ��������ʼ״̬�������ؾ�״̬
            if (WoodenDisk.IsBearing(unit))
                ChangeToNoVehicleMode();
            else
                ChangeToVehicleMode();
            // ChangeToNoVehicleMode();
        }

        public void OnUpdate()
        {
            if (!unit.IsAlive())
            {
                PlayDieInWater();
            }
            // �ж���ˮ����������ˮ�ﻹ�����ؾ���
            else if (count== 0 || (!hasVehicle && WoodenDisk.IsBearing(unit)))
            {
                // ���Ŀ�� �Ӵ���ˮ����Ϊ0 ���� �����ؾߵ�״̬�±�ľ���ӳ��أ���ת��Ϊ�������
                ChangeToVehicleMode();
            }
            else if (count > 0 && hasVehicle && !WoodenDisk.IsBearing(unit))
            {
                // ���Ŀ�� �Ӵ���ˮ��������0 �� �����ؾߵ�״̬�²����κ�ľ���ӳ��أ���ת��Ϊˮʴ
                ChangeToNoVehicleMode();
            }

            // �˺��ж������Ŀ��Ӵ���ˮ�򳬹�0��û���ؾ��Ҳ�����ˮʴ
            if(count >0 && !hasVehicle && !unit.NumericBox.GetBoolNumericValue(StringManager.IgnoreWaterGridState))
            {
                // ����Դ�ĳ����˺� ��ÿ����� 4%����ʧ����ֵ�˺�����СֵΪ2����
                if (triggerDamageTimeLeft == 0)
                {
                    new DamageAction(CombatAction.ActionType.CauseDamage, null, unit, Mathf.Max(2, unit.GetLostHp() * 0.04f)).ApplyAction();
                    triggerDamageTimeLeft = TotalTime;
                }
                else if (triggerDamageTimeLeft > 0)
                    triggerDamageTimeLeft--;
            }


            // ��������
            if (currentTime < totalTime && !unit.NumericBox.GetBoolNumericValue(NoDrop))
            {
                currentTime++;
                float r = ((float)currentTime) / totalTime;
                unit.RemoveSpriteOffsetY(EnterWaterSpriteOffsetY);
                EnterWaterSpriteOffsetY.Value = offsetY + (offsetYEnd - offsetY) * r;
                unit.AddSpriteOffsetY(EnterWaterSpriteOffsetY);
                SetCutRateFunc(Mathf.Max(0, cutRate + (cutRateEnd - cutRate) * r));
            }
        }

        /// <summary>
        /// �뿪�����������Ϊû��ˮ���Ҷ����������ϰ�
        /// </summary>
        /// <returns></returns>
        public bool IsMeetingExitCondition()
        {
            return count <= 0 && currentTime >= totalTime;
        }

        public void OnExit()
        {
            // �Ƴ������� �� ������
            unit.NumericBox.MoveSpeed.RemoveFinalPctAddModifier(slowDownFloatModifier);
            unit.NumericBox.AttackSpeed.RemoveFinalPctAddModifier(decAttackSpeedModifier);
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
                // BUFFЧ���ı�
                triggerDamageTimeLeft = TotalTime;

                if (!unit.NumericBox.GetBoolNumericValue(StringManager.IgnoreWaterGridState))
                {
                    // �Ƴ������� �� ������
                    unit.NumericBox.MoveSpeed.RemoveFinalPctAddModifier(slowDownFloatModifier);
                    unit.NumericBox.AttackSpeed.RemoveFinalPctAddModifier(decAttackSpeedModifier);
                    // ���ˮ���μ���Ч��������
                    unit.NumericBox.MoveSpeed.AddFinalPctAddModifier(slowDownFloatModifier);
                    unit.NumericBox.AttackSpeed.AddFinalPctAddModifier(decAttackSpeedModifier);
                }


                // ����Ϊ����������
                // ���Ŀ���Ƿ�����ˮ�ӿڣ�������������ö�Ӧ����
                if (typeof(IInWater).IsAssignableFrom(unit.GetType()))
                {
                    IInWater InWater = (IInWater)unit;
                    InWater.OnEnterWater();
                    // ҲҪ����ȥ
                    currentTime = 0;
                    offsetY = EnterWaterSpriteOffsetY.Value;
                    offsetYEnd = 0;
                    cutRate = 0; // �ü��߶�
                    cutRateEnd = 0;
                }
                else
                {
                    EffectManager.AddWaterWaveEffectToUnit(unit); // �����ˮ��Ч
                    // ����ȥ��
                    currentTime = 0;
                    offsetY = EnterWaterSpriteOffsetY.Value;
                    offsetYEnd = minPos;
                    cutRate = currentCutRate; // �ü��߶�
                    cutRateEnd = (sprite_pivot_y - TransManager.WorldToTex(offsetYEnd)) / sprite_rect_height;
                }
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
                // BUFFЧ���ı�
                if (!unit.NumericBox.GetBoolNumericValue(StringManager.IgnoreWaterGridState))
                {
                    // �Ƴ������� �� ������
                    unit.NumericBox.MoveSpeed.RemoveFinalPctAddModifier(slowDownFloatModifier);
                    unit.NumericBox.AttackSpeed.RemoveFinalPctAddModifier(decAttackSpeedModifier);
                }

                // ���Ŀ���Ƿ�����ˮ�ӿڣ�������������ö�Ӧ����
                if (typeof(IInWater).IsAssignableFrom(unit.GetType()))
                {
                    IInWater InWater = (IInWater)unit;
                    InWater.OnExitWater();
                    // ҲҪ������
                    currentTime = 0;
                    offsetY = EnterWaterSpriteOffsetY.Value;
                    offsetYEnd = maxPos;
                    cutRate = currentCutRate; // �ü��߶�
                    cutRateEnd = 0;
                }
                else
                {
                    // ��������
                    currentTime = 0;
                    offsetY = EnterWaterSpriteOffsetY.Value;
                    offsetYEnd = maxPos;
                    cutRate = currentCutRate; // �ü��߶�
                    cutRateEnd = 0;
                    EffectManager.RemoveWaterWaveEffectFromUnit(unit);
                }
            }
        }

        private void PlayDieInWater()
        {
            // �����һ��Ƿ�ˮ�Ͻӿڵ�λ �� ��ˮ�� �� û���ؾߵ�ʱ�� �� ���� �ͳ���ȥ
            if (!typeof(IInWater).IsAssignableFrom(unit.GetType()) && !isDieInWater && count > 0 && !hasVehicle)
            {
                currentTime = 0;
                totalTime = 120;
                offsetY = EnterWaterSpriteOffsetY.Value;
                offsetYEnd = -MapManager.gridHeight;  // �½�1��
                cutRate = currentCutRate; // �ü��߶�
                cutRateEnd = (sprite_pivot_y - TransManager.WorldToTex(offsetYEnd)) / sprite_rect_height;
                SetCutRateFunc(cutRate);
            }
            isDieInWater = true; // ִֻ��һ��
        }
    }
}
