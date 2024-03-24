
using System;

using UnityEngine;
namespace Environment
{
    public class LavaManager
    {
        private const string Tag = "CottonCandyVehicle";


        /// <summary>
        /// 获取一个岩浆地（注意，只是获取，并未加入GameController）
        /// </summary>
        public static RetangleAreaEffectExecution GetLavaArea(Vector2 pos, Vector2 size)
        {
            RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(pos, size, "BothCollide");
            r.isAffectFood = true;
            r.isAffectMouse = true;
            r.isAffectCharacter = true;

            // 定义友方入条件
            {
                Func<BaseUnit, bool> OnAllyEnterCondiFunc = (u) => {
                    return u.GetHeight() <= 0;
                };
                r.AddFoodEnterConditionFunc(OnAllyEnterCondiFunc);
                r.AddCharacterEnterConditionFunc(OnAllyEnterCondiFunc);
            }

            // 定义敌方入条件
            {
                Func<MouseUnit, bool> OnEnemyEnterCondiFunc = (u) => {
                    return !u.IsBoss() && u.GetHeight() <= 0 && !UnitManager.IsFlying(u);
                };
                r.AddEnemyEnterConditionFunc(OnEnemyEnterCondiFunc);
            }

            // 定义友方与敌方入时的方法
            {
                Action<BaseUnit> OnEnterAction = (u) => {
                    LavaTask t;
                    if (u.GetTask("LavaTask") == null)
                    {
                        t = new LavaTask(u);
                        u.AddUniqueTask("LavaTask", t);
                    }
                    else
                    {
                        t = u.GetTask("LavaTask") as LavaTask;
                        t.AddCount();
                    }
                };
                r.SetOnFoodEnterAction(OnEnterAction);
                r.SetOnEnemyEnterAction(OnEnterAction);
                r.SetOnCharacterEnterAction(OnEnterAction);
            }

            // 定义友方与敌方出水时的方法
            {
                Action<BaseUnit> OnExitAction = (u) =>
                {
                    if (u.GetTask("LavaTask") != null)
                    {
                        LavaTask t = u.GetTask("LavaTask") as LavaTask;
                        t.DecCount();
                    }
                };

                r.SetOnFoodExitAction(OnExitAction);
                r.SetOnEnemyExitAction(OnExitAction);
                r.SetOnCharacterExitAction(OnExitAction);
            }
            return r;
        }


        /// <summary>
        /// 获取载具检测区域
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
                    return u.GetHeight() == 0;
                };
                r.AddFoodEnterConditionFunc(EnterVehicleCondi);
                r.AddEnemyEnterConditionFunc(EnterVehicleCondi);
                r.AddCharacterEnterConditionFunc(EnterVehicleCondi);
            }

            // 定义进入载具的事件
            {
                Action<BaseUnit> OnEnterVehicleAction = (u) => {
                    // 使目标的载具承载数+1
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
                    // 使目标的载具承载数-1
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
        /// 目标是否被岩浆载具承载
        /// </summary>
        /// <param name="unit"></param>
        /// <returns></returns>
        public static bool IsBearing(BaseUnit unit)
        {
            // 是否含有承载tag且承载数必须大于0
            return unit.NumericBox.IntDict.ContainsKey(Tag) && unit.NumericBox.IntDict[Tag].Value > 0;
        }
        #endregion
    }
}
