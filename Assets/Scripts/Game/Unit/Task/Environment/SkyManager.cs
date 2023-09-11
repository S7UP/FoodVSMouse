using S7P.Numeric;

using System;

using UnityEngine;
namespace Environment
{
    public class SkyManager
    {
        private const string Tag = "CottonCandyVehicle";
        private const string TaskName = "SkyTask";
        public const string NoAffect = "NoAffectSky";

        /// <summary>
        /// 获取一个空域（注意，只是获取，并未加入GameController）
        /// </summary>
        public static RetangleAreaEffectExecution GetSkyArea(Vector2 pos, Vector2 size)
        {
            RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(pos, size, "BothCollide");
            r.isAffectFood = true;
            r.isAffectMouse = true;

            // 定义友方踩空条件
            {
                Func<BaseUnit, bool> OnAllyEnterCondiFunc = (u) => {
                    return u.GetHeight() <= 0;
                };
                r.AddFoodEnterConditionFunc(OnAllyEnterCondiFunc);
            }

            // 定义敌方踩空条件
            {
                Func<MouseUnit, bool> OnEnemyEnterCondiFunc = (u) => {
                    return !u.IsBoss() && u.GetHeight() <= 0;
                };
                r.AddEnemyEnterConditionFunc(OnEnemyEnterCondiFunc);
            }

            // 定义友方与敌方踩空时的方法
            {
                Action<BaseUnit> OnEnterAction = (u) => {
                    SkyTask t;
                    if (u.GetTask(TaskName) == null)
                    {
                        t = new SkyTask(u);
                        u.AddUniqueTask(TaskName, t);
                    }
                    else
                    {
                        t = u.GetTask(TaskName) as SkyTask;
                        t.AddCount();
                    }
                };
                r.SetOnFoodEnterAction(OnEnterAction);
                r.SetOnEnemyEnterAction(OnEnterAction);
            }

            // 定义友方与敌方离开空域时的方法
            {
                Action<BaseUnit> OnExitAction = (u) =>
                {
                    if (u.GetTask(TaskName) != null)
                    {
                        SkyTask t = u.GetTask(TaskName) as SkyTask;
                        t.DecCount();
                    }
                };
                r.SetOnFoodExitAction(OnExitAction);
                r.SetOnEnemyExitAction(OnExitAction);
            }
            return r;
        }

        /// <summary>
        /// 获取空载具检测区域
        /// </summary>
        public static RetangleAreaEffectExecution GetVehicleArea(Vector2 pos, Vector2 size)
        {
            // 通用变量
            S7P.Numeric.IntModifier VehicleModifier = new S7P.Numeric.IntModifier(1);

            RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(pos, size, "BothCollide");
            r.isAffectFood = true;
            r.isAffectMouse = true;
            r.isAffectCharacter = true;

            // 定义可进入载具的条件
            {
                Func<BaseUnit, bool> EnterVehicleCondi = (u) => {
                    return u.GetHeight() <= 0;
                };
                r.AddFoodEnterConditionFunc(EnterVehicleCondi);
                r.AddEnemyEnterConditionFunc(EnterVehicleCondi);
                r.AddCharacterEnterConditionFunc(EnterVehicleCondi);
            }

            // 定义进入载具的事件
            {
                Action<BaseUnit> OnEnterVehicleAction = (u) => {
                    // 使目标的空载具承载数+1
                    if (!u.NumericBox.IntDict.ContainsKey(Tag))
                        u.NumericBox.IntDict.Add(Tag, new IntNumeric());
                    u.NumericBox.IntDict[Tag].AddAddModifier(VehicleModifier);
                };
                r.SetOnFoodEnterAction(OnEnterVehicleAction);
                r.SetOnEnemyEnterAction(OnEnterVehicleAction);
                r.SetOnCharacterEnterAction(OnEnterVehicleAction);
            }

            // 定义离开载具的事件
            {
                Action<BaseUnit> OnExitVehicleAction = (u) => {
                    // 使目标的空载具承载数-1
                    if (!u.NumericBox.IntDict.ContainsKey(Tag))
                        u.NumericBox.IntDict.Add(Tag, new IntNumeric());
                    u.NumericBox.IntDict[Tag].RemoveAddModifier(VehicleModifier);
                };
                r.SetOnFoodExitAction(OnExitVehicleAction);
                r.SetOnEnemyExitAction(OnExitVehicleAction);
                r.SetOnCharacterExitAction(OnExitVehicleAction);
            }

            return r;
        }


        #region 供外界调用的方法
        /// <summary>
        /// 目标是否被空载具承载
        /// </summary>
        /// <param name="unit"></param>
        /// <returns></returns>
        public static bool IsBearing(BaseUnit unit)
        {
            // 是否含有承载tag且承载数必须大于0
            return unit.NumericBox.IntDict.ContainsKey(Tag) && unit.NumericBox.IntDict[Tag].Value > 0;
        }

        /// <summary>
        /// 为某个单位添加完全不受天空影响的效果
        /// </summary>
        /// <param name="unit"></param>
        public static void AddNoAffectBySky(BaseUnit unit, BoolModifier boolModifier)
        {
            unit.NumericBox.AddDecideModifierToBoolDict(NoAffect, boolModifier);
            unit.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreDropFromSky, boolModifier);
        }

        /// <summary>
        /// 为某个单位移除完全不受天空影响的效果
        /// </summary>
        /// <param name="unit"></param>
        public static void RemoveNoAffectBySky(BaseUnit unit, BoolModifier boolModifier)
        {
            unit.NumericBox.RemoveDecideModifierToBoolDict(NoAffect, boolModifier);
            unit.NumericBox.RemoveDecideModifierToBoolDict(StringManager.IgnoreDropFromSky, boolModifier);
        }

        public static bool IsIgnoreDrop(BaseUnit unit)
        {
            return unit.NumericBox.GetBoolNumericValue(StringManager.IgnoreDropFromSky);
        }
        #endregion
    }
}
