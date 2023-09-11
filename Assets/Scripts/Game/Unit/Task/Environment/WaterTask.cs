using S7P.Numeric;

using System;

using UnityEngine;

namespace Environment
{
    public class WaterTask : BaseTask
    {
        // 水抗性属性关键字
        public const string WaterRate = "WaterRate";

        public const string NoDrop = "NoDropWater"; // 有此标记的单位在水里不会有上升和下降的动画表现
        public const string OnEnterWater = "OnEnterWater";
        public const string OnStayWater = "OnStayWater";
        public const string OnExitWater = "OnExitWater";

        private FloatModifier slowDownFloatModifier = new FloatModifier(-40); // 当前提供减速效果的修饰器
        private FloatModifier decAttackSpeedModifier = new FloatModifier(-25); // 减攻速效果修饰器
                                                                               // 贴图变化系列
        private FloatModifier EnterWaterSpriteOffsetY = new FloatModifier(0); // 下水时的贴图Y总偏移量
        private float offsetY;
        private float offsetYEnd;
        private float cutRate;
        private float cutRateEnd;
        private int currentTime = 0;
        private int totalTime = 30;
        private Action<float> SetCutRateFunc;
        private float currentCutRate;
        // private float descendGridCount; // 下降格数
        private float minPos; // 下降最低高度
        private float sprite_pivot_y;
        private float sprite_rect_height;
        // 每秒伤害系列
        private const int TotalTime = 60;
        private int triggerDamageTimeLeft; // 触发伤害剩余时间 若为-1则永远不触发伤害

        private int count; // 进入的水域数
        private bool hasVehicle; // 是否被载具承载
        private BaseUnit unit;
        private bool isDieInWater;

        private BaseEffect eff; // 冰雕特效
        private float gobal_alpha; // 全局透明度
        //private UnitType type;

        public WaterTask(UnitType type, BaseUnit unit)
        {
            gobal_alpha = 0.5f;

            //this.type = type;
            this.unit = unit;
            // 修改贴图Y方向裁剪百分比的方法
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
            // 目标上升和下沉的高度
            switch (type)
            {
                case UnitType.Food:
                    minPos = 0f;
                    break;
                case UnitType.Mouse:
                    minPos = -0.4f * MapManager.gridWidth;
                    break;
                case UnitType.Item:
                    minPos = 0f;
                    break;
                case UnitType.Character:
                    minPos = 0f;
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

        #region 继承的方法
        protected override void O_OnEnter()
        {
            unit.actionPointController.TriggerAction(OnEnterWater);

            count = 1;
            // 先设成有载具，然后调用切换成无载具模式，这样初始状态就是无载具状态
            if (Environment.WaterManager.IsBearing(unit))
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
            // 判断在水域中是泡在水里还是在载具上
            else if (count == 0 || (!hasVehicle && WaterManager.IsBearing(unit)) || UnitManager.IsFlying(unit))
            {
                // 如果目标 接触的水域数为0 或者 在无载具的状态下被木盘子承载 或者滞空状态 则转变为正常情况
                ChangeToVehicleMode();
            }
            else if (count > 0 && hasVehicle && !WaterManager.IsBearing(unit) && !UnitManager.IsFlying(unit))
            {
                // 如果目标 接触的水域数超过0 且 在有载具的状态下不被任何木盘子承载 且不滞空 则转变为水蚀
                ChangeToNoVehicleMode();
            }

            // 伤害判定，如果目标接触的水域超过0且没有载具且不免疫水蚀
            if (count > 0 && !hasVehicle && !unit.NumericBox.GetBoolNumericValue(StringManager.IgnoreWaterGridState))
            {
                // 无来源的持续伤害 （每秒造成 4%已损失生命值伤害（最小值为2））
                if (triggerDamageTimeLeft == 0)
                {
                    new DamageAction(CombatAction.ActionType.RealDamage, null, unit, Mathf.Max(2, unit.GetLostHp() * 0.04f)* GetUnitWaterRate(unit)).ApplyAction();
                    triggerDamageTimeLeft = TotalTime;
                }
                else if (triggerDamageTimeLeft > 0)
                    triggerDamageTimeLeft--;
            }


            // 动画表现
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
        /// 离开该任务的条件为没有水域且动画表现已上岸
        /// </summary>
        /// <returns></returns>
        protected override bool O_IsMeetingCondition()
        {
            return count <= 0 && currentTime >= totalTime;
        }

        protected override void O_OnExit()
        {
            unit.actionPointController.TriggerAction(OnExitWater);
            // 移除减移速 和 减攻速 以及升高降低效果
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


        // 自定义方法
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

        #region 私有方法
        /// <summary>
        /// 切换成无载具模式
        /// </summary>
        private void ChangeToNoVehicleMode()
        {
            if (hasVehicle)
            {
                GameManager.Instance.audioSourceController.PlayEffectMusic("EnterWater");
                hasVehicle = false;
                // BUFF效果改变
                triggerDamageTimeLeft = TotalTime;

                if (!unit.NumericBox.GetBoolNumericValue(StringManager.IgnoreWaterGridState))
                {
                    // 移除减移速 和 减攻速
                    unit.NumericBox.MoveSpeed.RemoveFinalPctAddModifier(slowDownFloatModifier);
                    unit.NumericBox.AttackSpeed.RemoveFinalPctAddModifier(decAttackSpeedModifier);
                    // 添加水地形减速效果减攻速
                    unit.NumericBox.MoveSpeed.AddFinalPctAddModifier(slowDownFloatModifier);
                    unit.NumericBox.AttackSpeed.AddFinalPctAddModifier(decAttackSpeedModifier);
                }


                // 以下为处理动画表现
                // 检测目标是否有下水接口，如果有则额外调用对应方法
                if (typeof(IInWater).IsAssignableFrom(unit.GetType()))
                {
                    IInWater InWater = (IInWater)unit;
                    InWater.OnEnterWater();
                    // 也要沉下去
                    currentTime = 0;
                    offsetY = EnterWaterSpriteOffsetY.Value;
                    offsetYEnd = 0;
                    cutRate = 0; // 裁剪高度
                    cutRateEnd = 0;
                }
                else
                {
                    // 沉下去！
                    currentTime = 0;
                    offsetY = EnterWaterSpriteOffsetY.Value;
                    offsetYEnd = minPos;
                    cutRate = currentCutRate; // 裁剪高度
                    cutRateEnd = (sprite_pivot_y - TransManager.WorldToTex(offsetYEnd)) / sprite_rect_height;
                    EffectManager.AddWaterWaveEffectToUnit(unit, new Vector2(0, -offsetYEnd)); // 添加下水特效
                }
            }
        }

        /// <summary>
        /// 切换成载具模式
        /// </summary>
        private void ChangeToVehicleMode()
        {
            if (!hasVehicle)
            {
                GameManager.Instance.audioSourceController.PlayEffectMusic("EnterWater");
                hasVehicle = true;
                // BUFF效果改变
                if (!unit.NumericBox.GetBoolNumericValue(StringManager.IgnoreWaterGridState))
                {
                    // 移除减移速 和 减攻速
                    unit.NumericBox.MoveSpeed.RemoveFinalPctAddModifier(slowDownFloatModifier);
                    unit.NumericBox.AttackSpeed.RemoveFinalPctAddModifier(decAttackSpeedModifier);
                }

                // 检测目标是否有下水接口，如果有则额外调用对应方法
                if (typeof(IInWater).IsAssignableFrom(unit.GetType()))
                {
                    IInWater InWater = (IInWater)unit;
                    InWater.OnExitWater();
                    // 也要浮起来
                    currentTime = 0;
                    offsetY = EnterWaterSpriteOffsetY.Value;
                    offsetYEnd = (unit.NumericBox.FloatDict.ContainsKey("WaterVehicleHeight") ? unit.NumericBox.FloatDict["WaterVehicleHeight"].AddCollector.MaxValue : 0);
                    cutRate = currentCutRate; // 裁剪高度
                    cutRateEnd = 0;
                }
                else
                {
                    // 浮起来！
                    currentTime = 0;
                    offsetY = EnterWaterSpriteOffsetY.Value;
                    offsetYEnd = (unit.NumericBox.FloatDict.ContainsKey("WaterVehicleHeight") ? unit.NumericBox.FloatDict["WaterVehicleHeight"].AddCollector.MaxValue : 0);
                    cutRate = currentCutRate; // 裁剪高度
                    cutRateEnd = 0;
                    EffectManager.RemoveWaterWaveEffectFromUnit(unit);
                }
            }
        }

        private void CreateWaterEffect()
        {
            eff = BaseEffect.CreateInstance(unit.GetSpriteRenderer().sprite);
            eff.spriteRenderer.sortingLayerName = unit.GetSpriteRenderer().sortingLayerName;
            eff.spriteRenderer.sortingOrder = unit.GetSpriteRenderer().sortingOrder - 1;
            eff.spriteRenderer.material = GameManager.Instance.GetMaterial("Water");
            eff.spriteRenderer.material.SetFloat("_Alpha", 1);
            GameController.Instance.AddEffect(eff);
            unit.mEffectController.AddEffectToDict("Water_Body", eff, Vector2.zero);
        }

        private void RemoveWaterEffect()
        {
            eff.ExecuteDeath();
        }

        private void PlayDieInWater()
        {
            // 如果这家伙是非水上接口单位 且 在水域 且 没有载具的时候 且 死了 就沉下去
            if (!typeof(IInWater).IsAssignableFrom(unit.GetType()) && !isDieInWater && count > 0 && !hasVehicle)
            {
                currentTime = 0;
                totalTime = 120;
                offsetY = EnterWaterSpriteOffsetY.Value;
                offsetYEnd = -MapManager.gridHeight;  // 下降1格
                cutRate = currentCutRate; // 裁剪高度
                cutRateEnd = (sprite_pivot_y - TransManager.WorldToTex(offsetYEnd)) / sprite_rect_height;
                SetCutRateFunc(cutRate);
            }
            isDieInWater = true; // 只执行一次
        }
        #endregion

        #region 静态方法
        /// <summary>
        /// 获取目标的岩浆伤害倍率
        /// </summary>
        /// <param name="unit"></param>
        /// <returns></returns>
        public static float GetUnitWaterRate(BaseUnit unit)
        {
            if (unit == null || !unit.IsAlive())
                return 0;
            FloatCollectorComponent c = CollectorComponentManager.GetFloatCollectorComponent(unit.mComponentController);
            if (c.HasCollector(WaterRate))
                return c.GetCollector(WaterRate).MulValue;
            return 1;
        }

        public static void AddUnitWaterRate(BaseUnit unit, FloatModifier mod)
        {
            if (unit == null || !unit.IsAlive())
                return;
            FloatCollectorComponent c = CollectorComponentManager.GetFloatCollectorComponent(unit.mComponentController);
            if (!c.HasCollector(WaterRate))
                c.AddCollector(WaterRate, new FloatModifierCollector());
            c.GetCollector(WaterRate).AddModifier(mod);
        }

        public static void RemoveUnitWaterRate(BaseUnit unit, FloatModifier mod)
        {
            if (unit == null || !unit.IsAlive())
                return;
            FloatCollectorComponent c = CollectorComponentManager.GetFloatCollectorComponent(unit.mComponentController);
            if (!c.HasCollector(WaterRate))
                c.AddCollector(WaterRate, new FloatModifierCollector());
            c.GetCollector(WaterRate).RemoveModifier(mod);
        }
        #endregion
    }

}
