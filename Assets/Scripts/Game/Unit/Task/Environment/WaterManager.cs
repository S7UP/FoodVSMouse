using S7P.Numeric;

using System;

using UnityEngine;
namespace Environment
{
    public class WaterManager
    {
        private const string Tag = "WoodenDiskCandyVehicle";


        /// <summary>
        /// ��ȡһ��ˮ��ע�⣬ֻ�ǻ�ȡ����δ����GameController��
        /// </summary>
        public static RetangleAreaEffectExecution GetWaterArea(Vector2 pos, Vector2 size)
        {
            RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(pos, size, "BothCollide");
            r.isAffectFood = true;
            r.isAffectMouse = true;
            r.isAffectCharacter = true;

            // �����ѷ���ˮ����
            {
                Func<BaseUnit, bool> OnAllyEnterCondiFunc = (u) => {
                    return u.aliveTime > 5 && u.GetHeight() <= 0;
                };
                r.AddFoodEnterConditionFunc(OnAllyEnterCondiFunc);
                r.AddCharacterEnterConditionFunc(OnAllyEnterCondiFunc);
            }

            // ����з���ˮ����
            {
                Func<MouseUnit, bool> OnEnemyEnterCondiFunc = (u) => {
                    return u.aliveTime > 5 && !u.IsBoss() && u.GetHeight() <= 0 && !UnitManager.IsFlying(u);
                };
                r.AddEnemyEnterConditionFunc(OnEnemyEnterCondiFunc);
            }

            // �����ѷ���з���ˮʱ�ķ���
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
                                Debug.LogWarning("ˮ���������ֵĶ���");
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

            // �����ѷ���з���ˮʱ�ķ���
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
        /// ��ȡˮ�ؾ߼������
        /// </summary>
        public static RetangleAreaEffectExecution GetVehicleArea(Vector2 pos, Vector2 size, FloatModifier heightMod)
        {
            // ͨ�ñ���
            S7P.Numeric.IntModifier VehicleModifier = new S7P.Numeric.IntModifier(1);

            RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(pos, size, "BothCollide");
            r.isAffectFood = true;
            r.isAffectMouse = true;
            r.isAffectCharacter = true;

            // ����ɽ����ؾߵ�����
            {
                Func<BaseUnit, bool> EnterVehicleCondi = (u) => {
                    return u.GetHeight() == 0;
                };
                r.AddFoodEnterConditionFunc(EnterVehicleCondi);
                r.AddEnemyEnterConditionFunc(EnterVehicleCondi);
                r.AddCharacterEnterConditionFunc(EnterVehicleCondi);
            }

            // ��������ؾߵ��¼�
            {
                Action<BaseUnit> OnEnterVehicleAction = (u) => {
                    // ʹĿ���ˮ�ؾ߳�����+1
                    if (!u.NumericBox.IntDict.ContainsKey(Tag))
                        u.NumericBox.IntDict.Add(Tag, new IntNumeric());
                    u.NumericBox.IntDict[Tag].AddAddModifier(VehicleModifier);
                    // ʹĿ���ˮ�������߶�+�����ֵ
                    if (!u.NumericBox.FloatDict.ContainsKey("WaterVehicleHeight"))
                        u.NumericBox.FloatDict.Add("WaterVehicleHeight", new FloatNumeric());
                    u.NumericBox.FloatDict["WaterVehicleHeight"].AddAddModifier(heightMod);
                };
                r.SetOnFoodEnterAction(OnEnterVehicleAction);
                r.SetOnEnemyEnterAction(OnEnterVehicleAction);
                r.SetOnCharacterEnterAction(OnEnterVehicleAction);
            }

            // �����뿪�ؾߵ��¼�
            {
                Action<BaseUnit> OnExitVehicleAction = (u) => {
                    // ʹĿ���ľ���ӳ�����-1
                    if (!u.NumericBox.IntDict.ContainsKey(Tag))
                        u.NumericBox.IntDict.Add(Tag, new IntNumeric());
                    u.NumericBox.IntDict[Tag].RemoveAddModifier(VehicleModifier);
                    // ʹĿ���ˮ�������߶�-�����ֵ
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

        #region �������õķ���
        /// <summary>
        /// Ŀ���Ƿ�ˮ�ؾ߳���
        /// </summary>
        /// <param name="unit"></param>
        /// <returns></returns>
        public static bool IsBearing(BaseUnit unit)
        {
            // �Ƿ��г���tag�ҳ������������0
            return unit.NumericBox.IntDict.ContainsKey(Tag) && unit.NumericBox.IntDict[Tag].Value > 0;
        }
        #endregion
    }
}
