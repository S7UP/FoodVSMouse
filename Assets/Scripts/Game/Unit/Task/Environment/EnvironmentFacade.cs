/// <summary>
/// ����Ч���˿�
/// </summary>
namespace Environment
{
    public class EnvironmentFacade
    {
        /// <summary>
        /// ��ӱ�������Debuff
        /// </summary>
        public static void AddIceDebuff(BaseUnit u, float value)
        {
            if (!u.IsAlive())
                return;
            // ��BOSS��Ч
            if (u is MouseUnit && (u as MouseUnit).IsBoss())
                return;
            // ��������Ч
            if (u is CharacterUnit)
                return;
            // �����߱������˵�Ŀ����Ч
            if (u.NumericBox.GetBoolNumericValue(StringManager.IgnoreFrozen))
                return;

            ITask t = u.taskController.GetTask("IceTask");
            if (t == null)
            {
                t = new IceTask(u, value);
                u.taskController.AddUniqueTask("IceTask", t);
            }
            else
            {
                (t as IceTask).AddValue(value);
            }

            u.actionPointController.TriggerAction("AfterAddIceDebuff");
        }

        /// <summary>
        /// ��ȡĿ��ı�������Debuff
        /// </summary>
        public static ITask GetIceDebuff(BaseUnit u)
        {
            if (!u.IsAlive())
                return null;

            return u.taskController.GetTask("IceTask");
        }

        /// <summary>
        /// ���һ����������BUFF
        /// </summary>
        public static void AddFogBuff(BaseUnit unit)
        {
            // ��ȡĿ������Ψһ������
            FogTask t = null;
            if (unit.GetTask("FogTask") == null)
            {
                t = new FogTask(unit);
                unit.AddUniqueTask("FogTask", t);
            }
            else
            {
                t = unit.GetTask("FogTask") as FogTask;
                t.AddCount();
            }
        }

        /// <summary>
        /// �Ƴ�һ����������BUFF
        /// </summary>
        /// <param name="unit"></param>
        public static void RemoveFogBuff(BaseUnit unit)
        {
            // ��ȡĿ������Ψһ���������
            if (unit.GetTask("FogTask") != null)
            {
                FogTask t = unit.GetTask("FogTask") as FogTask;
                t.DecCount();
            }
        }
    }

}
