using S7P.Numeric;

using System;

using UnityEngine;
using UnityEngine.Timeline;

namespace Environment
{
    public class WaterTask : BaseTask
    {
        public const string NoDrop = "NoDropWater"; // �д˱�ǵĵ�λ��ˮ�ﲻ�����������½��Ķ�������
        public const string OnEnterWater = "OnEnterWater";
        public const string OnStayWater = "OnStayWater";
        public const string OnExitWater = "OnExitWater";

        private FloatModifier slowDownFloatModifier = new FloatModifier(-40); // ��ǰ�ṩ����Ч����������
        private FloatModifier decAttackSpeedModifier = new FloatModifier(-25); // ������Ч��������
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

        private SpriteGo eff; // ��ˮ��Ч
        private float gobal_alpha; // ȫ��͸����

        private Action<BaseUnit> OnMasterDestoryAction;
        //private UnitType type;

        public WaterTask(UnitType type, BaseUnit unit)
        {
            gobal_alpha = 0.5f;

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
                if(eff != null)
                    eff.spriteRenderer.material.SetFloat("_CutRateY", f);
                currentCutRate = f;
            };
            // Ŀ���������³��ĸ߶�
            switch (type)
            {
                case UnitType.Food:
                    minPos = 0f;
                    maxPos = 0.2f * MapManager.gridWidth;
                    break;
                case UnitType.Mouse:
                    minPos = -0.4f * MapManager.gridWidth;
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

            offsetY = 0;
            offsetYEnd = 0;
            cutRate = 0;
            cutRateEnd = 0;
            currentTime = 0;
            totalTime = 30;

            CreateWaterEffect();


            SetCutRateFunc(cutRate);

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

        #region �̳еķ���
        protected override void O_OnEnter()
        {
            unit.actionPointController.TriggerAction(OnEnterWater);

            count = 1;
            // ��������ؾߣ�Ȼ������л������ؾ�ģʽ��������ʼ״̬�������ؾ�״̬
            if (WoodenDisk.IsBearing(unit))
                ChangeToNoVehicleMode();
            else
                ChangeToVehicleMode();
        }

        protected override void O_OnUpdate()
        {
            unit.actionPointController.TriggerAction(OnStayWater);

            if (!unit.IsAlive())
            {
                PlayDieInWater();
            }
            // �ж���ˮ����������ˮ�ﻹ�����ؾ���
            else if (count == 0 || (!hasVehicle && WoodenDisk.IsBearing(unit)) || UnitManager.IsFlying(unit))
            {
                // ���Ŀ�� �Ӵ���ˮ����Ϊ0 ���� �����ؾߵ�״̬�±�ľ���ӳ��� �����Ϳ�״̬ ��ת��Ϊ�������
                ChangeToVehicleMode();
            }
            else if (count > 0 && hasVehicle && !WoodenDisk.IsBearing(unit) && !UnitManager.IsFlying(unit))
            {
                // ���Ŀ�� �Ӵ���ˮ��������0 �� �����ؾߵ�״̬�²����κ�ľ���ӳ��� �Ҳ��Ϳ� ��ת��Ϊˮʴ
                ChangeToNoVehicleMode();
            }

            // �˺��ж������Ŀ��Ӵ���ˮ�򳬹�0��û���ؾ��Ҳ�����ˮʴ
            if (count > 0 && !hasVehicle && !unit.NumericBox.GetBoolNumericValue(StringManager.IgnoreWaterGridState))
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

            if (eff != null)
            {
                eff.spriteRenderer.sortingLayerName = unit.GetSpriteRenderer().sortingLayerName;
                eff.spriteRenderer.sortingOrder = unit.GetSpriteRenderer().sortingOrder - 1;
                eff.spriteRenderer.sprite = unit.GetSpirte();
                eff.spriteRenderer.material.SetFloat("_Alpha", gobal_alpha);
            }
        }

        /// <summary>
        /// �뿪�����������Ϊû��ˮ���Ҷ����������ϰ�
        /// </summary>
        /// <returns></returns>
        protected override bool O_IsMeetingCondition()
        {
            return count <= 0 && currentTime >= totalTime;
        }

        protected override void O_OnExit()
        {
            unit.actionPointController.TriggerAction(OnExitWater);
            // �Ƴ������� �� ������ �Լ����߽���Ч��
            unit.NumericBox.MoveSpeed.RemoveFinalPctAddModifier(slowDownFloatModifier);
            unit.NumericBox.AttackSpeed.RemoveFinalPctAddModifier(decAttackSpeedModifier);
            unit.RemoveSpriteOffsetY(EnterWaterSpriteOffsetY);

            RemoveWaterEffect();
        }

        protected override bool O_IsClearWhenDie()
        {
            return false;
        }
        #endregion


        // �Զ��巽��
        public void AddCount()
        {
            count++;
        }

        public void DecCount()
        {
            count--;
            if (count == 0 && !hasVehicle)
            {
                ChangeToVehicleMode();
            }
        }

        /// <summary>
        /// �л������ؾ�ģʽ
        /// </summary>
        private void ChangeToNoVehicleMode()
        {
            if (hasVehicle)
            {
                GameManager.Instance.audioSourceManager.PlayEffectMusic("EnterWater");
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
                    // ����ȥ��
                    currentTime = 0;
                    offsetY = EnterWaterSpriteOffsetY.Value;
                    offsetYEnd = minPos;
                    cutRate = currentCutRate; // �ü��߶�
                    cutRateEnd = (sprite_pivot_y - TransManager.WorldToTex(offsetYEnd)) / sprite_rect_height;
                    EffectManager.AddWaterWaveEffectToUnit(unit, new Vector2(0, -offsetYEnd)); // �����ˮ��Ч
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
                GameManager.Instance.audioSourceManager.PlayEffectMusic("EnterWater");
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

        private void CreateWaterEffect()
        {
            eff = SpriteGo.GetInstance();
            eff.spriteRenderer.sortingLayerName = unit.GetSpriteRenderer().sortingLayerName;
            eff.spriteRenderer.sortingOrder = unit.GetSpriteRenderer().sortingOrder - 1;
            eff.spriteRenderer.material = GameManager.Instance.GetMaterial("Water");
            eff.spriteRenderer.material.SetFloat("_Alpha", 1);
            eff.transform.SetParent(unit.GetSpriteRenderer().transform);
            eff.transform.localPosition = Vector2.zero;

            OnMasterDestoryAction = delegate { eff.MDestory(); };
            unit.AddOnDestoryAction(OnMasterDestoryAction);
        }

        private void RemoveWaterEffect()
        {
            if (OnMasterDestoryAction != null)
            {
                OnMasterDestoryAction(unit);
                unit.RemoveOnDestoryAction(OnMasterDestoryAction);
                eff = null;
                OnMasterDestoryAction = null;
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
