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
        /// ��ȡһ������ע�⣬ֻ�ǻ�ȡ����δ����GameController��
        /// </summary>
        public static RetangleAreaEffectExecution GetSkyArea(Vector2 pos, Vector2 size)
        {
            RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(pos, size, "BothCollide");
            r.isAffectFood = true;
            r.isAffectMouse = true;

            // �����ѷ��ȿ�����
            {
                Func<BaseUnit, bool> OnAllyEnterCondiFunc = (u) => {
                    return u.GetHeight() <= 0;
                };
                r.AddFoodEnterConditionFunc(OnAllyEnterCondiFunc);
            }

            // ����з��ȿ�����
            {
                Func<MouseUnit, bool> OnEnemyEnterCondiFunc = (u) => {
                    return !u.IsBoss() && u.GetHeight() <= 0;
                };
                r.AddEnemyEnterConditionFunc(OnEnemyEnterCondiFunc);
            }

            // �����ѷ���з��ȿ�ʱ�ķ���
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

            // �����ѷ���з��뿪����ʱ�ķ���
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
        /// ��ȡ���ؾ߼������
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
                    return u.GetHeight() <= 0;
                };
                r.AddFoodEnterConditionFunc(EnterVehicleCondi);
                r.AddEnemyEnterConditionFunc(EnterVehicleCondi);
                r.AddCharacterEnterConditionFunc(EnterVehicleCondi);
            }

            // ��������ؾߵ��¼�
            {
                Action<BaseUnit> OnEnterVehicleAction = (u) => {
                    // ʹĿ��Ŀ��ؾ߳�����+1
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
                    // ʹĿ��Ŀ��ؾ߳�����-1
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
        /// Ŀ���Ƿ񱻿��ؾ߳���
        /// </summary>
        /// <param name="unit"></param>
        /// <returns></returns>
        public static bool IsBearing(BaseUnit unit)
        {
            // �Ƿ��г���tag�ҳ������������0
            return unit.NumericBox.IntDict.ContainsKey(Tag) && unit.NumericBox.IntDict[Tag].Value > 0;
        }

        /// <summary>
        /// Ϊĳ����λ�����ȫ�������Ӱ���Ч��
        /// </summary>
        /// <param name="unit"></param>
        public static void AddNoAffectBySky(BaseUnit unit, BoolModifier boolModifier)
        {
            unit.NumericBox.AddDecideModifierToBoolDict(NoAffect, boolModifier);
            unit.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreDropFromSky, boolModifier);
        }

        /// <summary>
        /// Ϊĳ����λ�Ƴ���ȫ�������Ӱ���Ч��
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
