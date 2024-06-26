namespace ResourceLoading.Mouse
{
    public class NonMainstreamMouse : IMouse
    {
        private int type = 27;

        protected override void O_Load(int shape)
        {
            for (int i = 0; i < MouseManager.GetAttribute(type, shape).mHertRateList.Count; i++)
                GameManager.Instance.GetRuntimeAnimatorController("Mouse/" + type + "/" + shape + "/" + i);
            GameManager.Instance.LoadGameObjectResource(FactoryType.GameFactory, "Mouse/" + type);
        }

        protected override void O_UnLoad(int shape)
        {
            for (int i = 0; i < MouseManager.GetAttribute(type, shape).mHertRateList.Count; i++)
                GameManager.Instance.UnLoadRuntimeAnimatorController("Mouse/" + type + "/" + shape + "/" + i);
            GameManager.Instance.UnLoadGameObjectResource(FactoryType.GameFactory, "Mouse/" + type);
        }
    }
}