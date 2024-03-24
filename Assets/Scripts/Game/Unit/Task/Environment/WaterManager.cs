using S7P.Numeric;

using System;

using UnityEngine;
namespace Environment
{
    public class WaterManager
    {
        private const string Tag = "WoodenDiskCandyVehicle";


        /// <summary>
        /// 获取一个水域（注意，只是获取，并未加入GameController）
        /// </summary>
        public static RetangleAreaEffectExecution GetWaterArea(Vector2 pos, Vector2 size)
        {
            RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(pos, size, "BothCollide");
            r.isAffectFood = true;
            r.isAffectMouse = true;
            r.isAffectCharacter = true;

            // 定义友方入水条件
            {
                Func<BaseUnit, bool> OnAllyEnterCondiFunc = (u) => {
                    return u.aliveTime > 5 && u.GetHeight() <= 0;
                };
                r.AddFoodEnterConditionFunc(OnAllyEnterCondiFunc);
                r.AddCharacterEnterConditionFunc(OnAllyEnterCondiFunc);
            }

            // 定义敌方入水条件
            {
                Func<MouseUnit, bool> OnEnemyEnterCondiFunc = (u) => {
                    return u.aliveTime > 5 && !u.IsBoss() && u.GetHeight() <= 0 && !UnitManager.IsFlying(u);
                };
                r.AddEnemyEnterConditionFunc(OnEnemyEnterCondiFunc);
            }

            // 定义友方与敌方入水时的方法
            {
                Action<BaseUnit> OnEnterAction = (u) => {
                    WaterTask t;
                    if (u.GetTask("WaterTask") == null)
                    {
                        switch (u.tag)
                        {
                            case "Food":
                                t = new WaterTask(UnitType.Food, u); break;
                            case "Mouse":
                                t = new WaterTask(UnitType.Mouse, u); break;
                            case "Character":
                                t = new WaterTask(UnitType.Character, u); break;
                            case "Item":
                                t = new WaterTask(UnitType.Item, u); break;
                            default:
                                Debug.LogWarning("水里进入了奇怪的东西");
                                t = new WaterTask(UnitType.Food, u); break;
                        }
                        u.AddUniqueTask("WaterTask", t);
                    }
                    else
                    {
                        t = u.GetTask("WaterTask") as WaterTask;
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
                    if (u.GetTask("WaterTask") != null)
                    {
                        WaterTask t = u.GetTask("WaterTask") as WaterTask;
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
        /// 获取水载具检测区域
        /// </summary>
        public static RetangleAreaEffectExecution GetVehicleArea(Vector2 pos, Vector2 size, FloatModifier heightMod)
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
                    // 使目标的水载具承载数+1
                    if (!u.NumericBox.IntDict.ContainsKey(Tag))
                        u.NumericBox.IntDict.Add(Tag, new IntNumeric());
                    u.NumericBox.IntDict[Tag].AddAddModifier(VehicleModifier);
                    // 使目标的水承载最大高度+传入的值
                    if (!u.NumericBox.FloatDict.ContainsKey("WaterVehicleHeight"))
                        u.NumericBox.FloatDict.Add("WaterVehicleHeight", new FloatNumeric());
                    u.NumericBox.FloatDict["WaterVehicleHeight"].AddAddModifier(heightMod);
                };
                r.SetOnFoodEnterAction(OnEnterVehicleAction);
                r.SetOnEnemyEnterAction(OnEnterVehicleAction);
                r.SetOnCharacterEnterAction(OnEnterVehicleAction);
            }

            // 定义离开载具的事件
            {
                Action<BaseUnit> OnExitVehicleAction = (u) => {
                    // 使目标的木盘子承载数-1
                    if (!u.NumericBox.IntDict.ContainsKey(Tag))
                        u.NumericBox.IntDict.Add(Tag, new IntNumeric());
                    u.NumericBox.IntDict[Tag].RemoveAddModifier(VehicleModifier);
                    // 使目标的水承载最大高度-传入的值
                    if (!u.NumericBox.FloatDict.ContainsKey("WaterVehicleHeight"))
                        u.NumericBox.FloatDict.Add("WaterVehicleHeight", new FloatNumeric());
                    u.NumericBox.FloatDict["WaterVehicleHeight"].RemoveAddModifier(heightMod);
                };
                r.SetOnFoodExitAction(OnExitVehicleAction);
                r.SetOnEnemyExitAction(OnExitVehicleAction);
                r.SetOnCharacterExitAction(OnExitVehicleAction);
            }

            return r;
        }

        #region 供外界调用的方法
        /// <summary>
        /// 目标是否被水载具承载
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
