
using System;

using UnityEngine;
namespace Environment
{
    public class LavaManager
    {
        private const string Tag = "CottonCandyVehicle";


        /// <summary>
        /// ��ȡһ���ҽ��أ�ע�⣬ֻ�ǻ�ȡ����δ����GameController��
        /// </summary>
        public static RetangleAreaEffectExecution GetLavaArea(Vector2 pos, Vector2 size)
        {
            RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(pos, size, "BothCollide");
            r.isAffectFood = true;
            r.isAffectMouse = true;
            r.isAffectCharacter = true;

            // �����ѷ�������
            {
                Func<BaseUnit, bool> OnAllyEnterCondiFunc = (u) => {
                    return u.GetHeight() <= 0;
                };
                r.AddFoodEnterConditionFunc(OnAllyEnterCondiFunc);
                r.AddCharacterEnterConditionFunc(OnAllyEnterCondiFunc);
            }

            // ����з�������
            {
                Func<MouseUnit, bool> OnEnemyEnterCondiFunc = (u) => {
                    return !u.IsBoss() && u.GetHeight() <= 0 && !UnitManager.IsFlying(u);
                };
                r.AddEnemyEnterConditionFunc(OnEnemyEnterCondiFunc);
            }

            // �����ѷ���з���ʱ�ķ���
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

            // �����ѷ���з���ˮʱ�ķ���
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
        /// ��ȡ�ؾ߼������
        /// </summary>
        public static RetangleAreaEffectExecution GetVehicleArea(Vector2 pos, Vector2 size)
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
                    // ʹĿ����ؾ߳�����+1
                    if (!u.NumericBox.IntDict.ContainsKey(Tag))
                        u.NumericBox.IntDict.Add(Tag, new IntNumeric());
                    u.NumericBox.IntDict[Tag].AddAddModifier(VehicleModifier);
                };
                r.SetOnFoodEnterAction(OnEnterVehicleAction);
                r.SetOnEnemyEnterAction(OnEnterVehicleAction);
                r.SetOnCharacterEnterAction(OnEnterVehicleAction);
            }

            // �����뿪�ؾߵ��¼�
            {
                Action<BaseUnit> OnExitVehicleAction = (u) => {
                    // ʹĿ����ؾ߳�����-1
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

        #region �������õķ���
        /// <summary>
        /// Ŀ���Ƿ��ҽ��ؾ߳���
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
