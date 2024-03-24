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
            // 对人物无效
            if (u is CharacterUnit)
                return;
            // 对免疫冰冻损伤的目标无效
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
        /// 获取目标的冰冻损伤Debuff
        /// </summary>
        public static ITask GetIceDebuff(BaseUnit u)
        {
            if (!u.IsAlive())
                return null;

            return u.taskController.GetTask("IceTask");
        }

        /// <summary>
        /// 添加一层迷雾隐匿BUFF
        /// </summary>
        public static void AddFogBuff(BaseUnit unit)
        {
            // 获取目标身上唯一的任务
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
        /// 移除一层迷雾隐匿BUFF
        /// </summary>
        /// <param name="unit"></param>
        public static void RemoveFogBuff(BaseUnit unit)
        {
            // 获取目标身上唯一的荫蔽任务
            if (unit.GetTask("FogTask") != null)
            {
                FogTask t = unit.GetTask("FogTask") as FogTask;
                t.DecCount();
            }
        }
    }

}
