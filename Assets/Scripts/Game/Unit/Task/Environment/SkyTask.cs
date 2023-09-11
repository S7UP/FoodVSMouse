namespace Environment
{
    /// <summary>
    /// 高空任务
    /// </summary>
    public class SkyTask : ITask
    {
        private int count; // 进入的高空格子数
        private BaseUnit unit;

        public SkyTask(BaseUnit unit)
        {
            this.unit = unit;
        }

        public void OnEnter()
        {
            count = 1;
        }

        public void OnUpdate()
        {
            // 如果目标 没有被空载 且 不免疫摔落 那么直接摔死吧
            if ((!unit.NumericBox.IntDict.ContainsKey(StringManager.BearInSky) || unit.NumericBox.IntDict[StringManager.BearInSky].Value <= 0)
                && !SkyManager.IsIgnoreDrop(unit) && !SkyManager.IsBearing(unit) && !UnitManager.IsFlying(unit))
            {
                unit.ExecuteDrop();
            }
        }

        public bool IsMeetingExitCondition()
        {
            return count <= 0;
        }

        public void OnExit()
        {

        }

        // 自定义方法
        public void AddCount()
        {
            count++;
        }

        public void DecCount()
        {
            count--;
        }

        public void ShutDown()
        {

        }

        public bool IsClearWhenDie()
        {
            return true;
        }
    }

}
