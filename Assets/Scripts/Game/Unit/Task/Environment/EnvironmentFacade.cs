/// <summary>
/// 环境效果端口
/// </summary>
namespace Environment
{
    public class EnvironmentFacade
    {
        /// <summary>
        /// 添加冰冻损伤Debuff
        /// </summary>
        public static void AddIceDebuff(BaseUnit u, float value)
        {
            if (!u.IsAlive())
                return;
            // 对BOSS无效
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
