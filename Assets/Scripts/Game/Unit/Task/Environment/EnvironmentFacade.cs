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
        }


    }

}
