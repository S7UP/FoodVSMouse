namespace GlobalData
{
    /// <summary>
    /// ȫ�ֱ����洢���Ĺ����ߣ�������������������Ϸ����������Ϸ��������������Ҫ�洢��ȫ�ֱ�����
    /// </summary>
    public class Manager
    {
        #region ����ģʽ�Ĵ�����Manager.Instance���ã�
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

        public EditStage mEditStage = new EditStage(); // ���ڹؿ��༭ģ�飬Ҳ�ǵ���
    }
}

