using System;

using UnityEngine;

public class WaterGridType : BaseGridType
{
    private const string TaskName = "WaterTask";
    public const string NoDrop = "在水里不下降"; // 有此标记的单位在水里不会有上升和下降的动画表现

    /// <summary>
    /// 是否满足进入条件
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetingEnterCondition(BaseUnit unit)
    {
        // BOSS单位无视地形效果
        if (unit is MouseUnit)
        {
            MouseUnit m = unit as MouseUnit;
            if (m.IsBoss())
                return false;
        }
        return unit.GetHeight() <= 0;
    }

    /// <summary>
    /// 当有单位进入地形时施加给单位的效果
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
                    Debug.LogWarning("水里进入了奇怪的东西");
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
            WaterTask t = unit.GetTask(TaskName) as WaterTask;
            t.DecCount();
        }
        else
        {
            Debug.LogWarning("为什么有东西可以没带水域任务出水域？");
        }
    }

    /// <summary>
    /// 判断某个单位是否在水域范围内
    /// </summary>
    /// <returns></returns>
    public static bool IsInWater(BaseUnit unit)
    {
        return unit.GetTask(TaskName) != null;
    }

    /// <summary>
    /// 水域BUFF任务
    /// </summary>
    private class WaterTask : ITask
    {
        private FloatModifier slowDownFloatModifier = new FloatModifier(-40); // 当前提供减速效果的修饰器
        private FloatModifier decAttackSpeedModifier = new FloatModifier(-50); // 减攻速效果修饰器
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
        private float maxPos; // 上升最高高度
        private float sprite_pivot_y;
        private float sprite_rect_height;
        // 每秒伤害系列
        private const int TotalTime = 60;
        private int triggerDamageTimeLeft; // 触发伤害剩余时间 若为-1则永远不触发伤害

        private int count; // 进入的水域数
        private bool hasVehicle; // 是否被载具承载
        private BaseUnit unit;
        private bool isDieInWater;
        //private UnitType type;

        public WaterTask(UnitType type, BaseUnit unit)
        {
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
                currentCutRate = f;
            };
            // 目标上升和下沉的高度
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
            // 先设成有载具，然后调用切换成无载具模式，这样初始状态就是无载具状态
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
            // 判断在水域中是泡在水里还是在载具上
            else if (count== 0 || (!hasVehicle && WoodenDisk.IsBearing(unit)))
            {
                // 如果目标 接触的水域数为0 或者 在无载具的状态下被木盘子承载，则转变为正常情况
                ChangeToVehicleMode();
            }
            else if (count > 0 && hasVehicle && !WoodenDisk.IsBearing(unit))
            {
                // 如果目标 接触的水域数超过0 且 在有载具的状态下不被任何木盘子承载，则转变为水蚀
                ChangeToNoVehicleMode();
            }

            // 伤害判定，如果目标接触的水域超过0且没有载具且不免疫水蚀
            if(count >0 && !hasVehicle && !unit.NumericBox.GetBoolNumericValue(StringManager.IgnoreWaterGridState))
            {
                // 无来源的持续伤害 （每秒造成 4%已损失生命值伤害（最小值为2））
                if (triggerDamageTimeLeft == 0)
                {
                    new DamageAction(CombatAction.ActionType.CauseDamage, null, unit, Mathf.Max(2, unit.GetLostHp() * 0.04f)).ApplyAction();
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
        }

        /// <summary>
        /// 离开该任务的条件为没有水域且动画表现已上岸
        /// </summary>
        /// <returns></returns>
        public bool IsMeetingExitCondition()
        {
            return count <= 0 && currentTime >= totalTime;
        }

        public void OnExit()
        {
            // 移除减移速 和 减攻速
            unit.NumericBox.MoveSpeed.RemoveFinalPctAddModifier(slowDownFloatModifier);
            unit.NumericBox.AttackSpeed.RemoveFinalPctAddModifier(decAttackSpeedModifier);
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
                    EffectManager.AddWaterWaveEffectToUnit(unit); // 添加下水特效
                    // 沉下去！
                    currentTime = 0;
                    offsetY = EnterWaterSpriteOffsetY.Value;
                    offsetYEnd = minPos;
                    cutRate = currentCutRate; // 裁剪高度
                    cutRateEnd = (sprite_pivot_y - TransManager.WorldToTex(offsetYEnd)) / sprite_rect_height;
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
                    offsetYEnd = maxPos;
                    cutRate = currentCutRate; // 裁剪高度
                    cutRateEnd = 0;
                }
                else
                {
                    // 浮起来！
                    currentTime = 0;
                    offsetY = EnterWaterSpriteOffsetY.Value;
                    offsetYEnd = maxPos;
                    cutRate = currentCutRate; // 裁剪高度
                    cutRateEnd = 0;
                    EffectManager.RemoveWaterWaveEffectFromUnit(unit);
                }
            }
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
    }
}
