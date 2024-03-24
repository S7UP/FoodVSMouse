namespace GlobalData
{
    /// <summary>
    /// 全局变量存储器的管理者（单例，生命周期自游戏运行起至游戏结束，包含所有要存储的全局变量）
    /// </summary>
    public class Manager
    {
        #region 单例模式的处理（用Manager.Instance引用）
        private Manager()
        {

        }

        public static Manager Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new Manager();
                return _Instance;
            }
        }
        private static Manager _Instance;
        #endregion

        public EditStage mEditStage = new EditStage(); // 关于关卡编辑模块，也是单例
    }
}

