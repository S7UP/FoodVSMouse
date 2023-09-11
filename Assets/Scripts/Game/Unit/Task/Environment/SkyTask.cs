namespace Environment
{
    /// <summary>
    /// �߿�����
    /// </summary>
    public class SkyTask : ITask
    {
        private int count; // ����ĸ߿ո�����
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
            // ���Ŀ�� û�б����� �� ������ˤ�� ��ôֱ��ˤ����
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

        // �Զ��巽��
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
